using AutoMapper;
using Microsoft.AspNetCore.Identity;
using TicketFlow.BLL.DTOs.Account;
using TicketFlow.BLL.DTOs.Auth;
using TicketFlow.BLL.Services.Interfaces;
using TicketFlow.DAL.Models;

namespace TicketFlow.BLL.Services.Classes
{
    public class AccountService :IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;


        public AccountService(UserManager<AppUser> userManager,IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return null;

            var profileDto = _mapper.Map<UserProfileDto>(user);
            profileDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return profileDto;
        }

        public async Task<AuthResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new AuthResponseDto { IsSuccess = false, Message = "User profile not found." };

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponseDto { IsSuccess = false, Message = errors };
            }

            return new AuthResponseDto { IsSuccess = true, Message = "Profile updated successfully." };
        }

        public async Task<AuthResponseDto> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new AuthResponseDto { IsSuccess = false, Message = "User account not found." };

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponseDto { IsSuccess = false, Message = errors };
            }

            return new AuthResponseDto { IsSuccess = true, Message = "Password changed successfully." };
        }
    }
}