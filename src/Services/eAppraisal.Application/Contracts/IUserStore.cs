using eAppraisal.Domain.DTOs;

namespace eAppraisal.Application.Contracts;

public interface IUserStore
{
    Task<AuthResultDto> AuthenticateAsync(string email, string password, string? ipAddress);
    Task SignOutAsync();
    Task<List<UserDto>> GetAllUsersAsync();
    Task<bool> UnlockUserAsync(string userId, string unlockedBy);
    Task<bool> LockUserAsync(string userId, string lockedBy);
    Task<(bool Success, string? Error, int? EmployeeId)> CreateUserAsync(string email, string fullName, string password, string role);
}
