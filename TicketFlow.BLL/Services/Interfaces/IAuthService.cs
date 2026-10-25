using TicketFlow.BLL.DTOs.Auth;

namespace TicketFlow.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
        Task<AuthResponseDto> ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<AuthResponseDto> ConfirmPhoneNumberAsync(ConfirmPhoneDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
        Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto dto);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto);
        Task<AuthResponseDto> ResendConfirmationEmailAsync(ResendConfirmationEmailDto dto);
        Task<AuthResponseDto> ResendPhoneOtpAsync(ResendPhoneOtpDto dto);
        Task<bool> RevokeTokenAsync(RevokeTokenRequestDto dto);
        Task<AuthResponseDto> AssignRoleAsync(AssignRoleDto dto);
        Task<bool> ValidateSecurityStampAsync(Guid userId, string securityStamp);
    }
}