using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TicketFlow.BLL.DTOs.Auth;
using TicketFlow.BLL.Services.Interfaces;
using TicketFlow.DAL.Models;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace TicketFlow.BLL.Services.Classes
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IPhoneVerificationService _phoneVerificationService;
        private readonly IMapper _mapper;

        public AuthService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IConfiguration configuration,
            IEmailService emailService,
            IPhoneVerificationService phoneVerificationService,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
            _phoneVerificationService = phoneVerificationService;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return new AuthResponseDto { IsSuccess = false, Message = "Email address is already in use." };

            var user = _mapper.Map<AppUser>(dto);

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponseDto { IsSuccess = false, Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var emailBody = $@"
            <h2>Welcome to TicketFlow, {user.FullName}!</h2>
            <p>Your user ID is: <b>{user.Id}</b></p>
            <p>Please use the following confirmation token to verify your email:</p>
            <p><code>{confirmationToken}</code></p>";

            await _emailService.SendEmailAsync(user.Email, "TicketFlow - Confirm Your Email", emailBody);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "User registered successfully. A confirmation token has been sent to your email."
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password." };

            if (!user.IsActive)
                return new AuthResponseDto { IsSuccess = false, Message = "Your account has been deactivated." };

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var jwtToken = GenerateJwtToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Login successful.",
                Token = jwtToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                Roles = roles
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
            if (principal == null)
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid access token." };

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null ||
                !user.IsActive ||
                user.RefreshToken != dto.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid or expired refresh token." };
            }

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var newJwtToken = GenerateJwtToken(user, roles);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Token refreshed successfully.",
                Token = newJwtToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                Roles = roles
            };
        }

        public async Task<bool> RevokeTokenAsync(RevokeTokenRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return false;

            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == dto.RefreshToken);
            if (user == null)
                return false;

            // 1. Clear refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            // 2. Change SecurityStamp -> invalidates all existing JWT access tokens immediately
            await _userManager.UpdateSecurityStampAsync(user);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<AuthResponseDto> ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
                return new AuthResponseDto { IsSuccess = false, Message = "User not found." };

            var result = await _userManager.ConfirmEmailAsync(user, dto.Token);
            if (!result.Succeeded)
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid or expired confirmation token." };

            return new AuthResponseDto { IsSuccess = true, Message = "Email confirmed successfully." };
        }

        public async Task<AuthResponseDto> ConfirmPhoneNumberAsync(ConfirmPhoneDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
                return new AuthResponseDto { IsSuccess = false, Message = "User not found." };

            // Verify Firebase ID Token passed in dto.Code
            var (isValid, verifiedPhone) = await _phoneVerificationService.VerifyFirebaseTokenAsync(dto.Code);
            if (!isValid || string.IsNullOrEmpty(verifiedPhone))
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid Firebase phone verification token." };

            user.PhoneNumber = verifiedPhone;
            user.PhoneNumberConfirmed = true;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return new AuthResponseDto { IsSuccess = false, Message = "Failed to update phone verification status." };

            return new AuthResponseDto { IsSuccess = true, Message = "Phone number verified successfully via Firebase." };
        }

        public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var body = $@"
                <h3>Reset Your TicketFlow Password</h3>
                <p>Use the following token to reset your password:</p>
                <p><code>{token}</code></p>";

                await _emailService.SendEmailAsync(user.Email!, "TicketFlow - Password Reset Request", body);
            }

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "If the email is registered, a password reset token has been sent."
            };
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid request." };

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponseDto { IsSuccess = false, Message = errors };
            }

            return new AuthResponseDto { IsSuccess = true, Message = "Password has been reset successfully." };
        }

        public async Task<AuthResponseDto> ResendConfirmationEmailAsync(ResendConfirmationEmailDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null && !user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var body = $@"
                <h3>Confirm Your Email</h3>
                <p>Your user ID is: <b>{user.Id}</b></p>
                <p>Use the token below to confirm your account:</p>
                <p><code>{token}</code></p>";

                await _emailService.SendEmailAsync(user.Email!, "TicketFlow - Confirmation Email", body);
            }

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "If the account exists and requires confirmation, a new email has been sent."
            };
        }

        public async Task<AuthResponseDto> ResendPhoneOtpAsync(ResendPhoneOtpDto dto)
        {
            return await Task.FromResult(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Firebase handles SMS OTP resends directly on the client application."
            });
        }

        public async Task<AuthResponseDto> AssignRoleAsync(AssignRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
                return new AuthResponseDto { IsSuccess = false, Message = "User not found." };

            if (!await _roleManager.RoleExistsAsync(dto.RoleName))
                return new AuthResponseDto { IsSuccess = false, Message = "Role does not exist." };

            var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
            if (!result.Succeeded)
                return new AuthResponseDto { IsSuccess = false, Message = "Role assignment failed or user already has this role." };

            return new AuthResponseDto { IsSuccess = true, Message = $"Role '{dto.RoleName}' assigned successfully." };
        }

        private string GenerateJwtToken(AppUser user, List<string> roles)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("SecurityStamp", user.SecurityStamp ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var secret = _configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret missing in configuration.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var secret = _configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret missing in configuration.");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        public async Task<bool> ValidateSecurityStampAsync(Guid userId, string securityStamp)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || !user.IsActive)
                return false;

            return user.SecurityStamp == securityStamp;
        }
    }
}