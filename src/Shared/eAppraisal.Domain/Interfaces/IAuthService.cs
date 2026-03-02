using eAppraisal.Domain.DTOs;

namespace eAppraisal.Domain.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginDto dto, string? ipAddress);
    Task LogoutAsync(string? actorEmail);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<bool> UnlockUserAsync(string userId, string unlockedBy);
    Task<bool> LockUserAsync(string userId, string lockedBy);
    Task<(bool Success, string? Error)> RegisterUserAsync(string email, string fullName, string password, string role);
}
