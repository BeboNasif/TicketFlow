using TicketFlow.BLL.DTOs.Account;
using TicketFlow.BLL.DTOs.Auth;

namespace TicketFlow.BLL.Services.Interfaces
{
    public interface IAccountService
    {
        Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
        Task<AuthResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
        Task<AuthResponseDto> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);

    }
}