using eAppraisal.Application.Contracts;
using eAppraisal.Domain.DTOs;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserStore _userStore;
    private readonly IAuditService _audit;

    public AuthService(IUserStore userStore, IAuditService audit)
    {
        _userStore = userStore;
        _audit = audit;
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto, string? ipAddress)
    {
        var result = await _userStore.AuthenticateAsync(dto.Email, dto.Password, ipAddress);

        var action = result.Success ? "LoginSuccess" : "LoginFailed";
        await _audit.LogAsync("User", null, action, dto.Email, ipAddress, null,
            new { result.ErrorMessage });

        return result;
    }

    public async Task LogoutAsync(string? actorEmail)
    {
        await _userStore.SignOutAsync();
        await _audit.LogAsync("User", null, "Logout", actorEmail, null, null);
    }

    public Task<List<UserDto>> GetAllUsersAsync() => _userStore.GetAllUsersAsync();

    public async Task<bool> UnlockUserAsync(string userId, string unlockedBy)
    {
        var result = await _userStore.UnlockUserAsync(userId, unlockedBy);
        if (result)
            await _audit.LogAsync("User", null, "AccountUnlocked", unlockedBy, null, null, new { userId });
        return result;
    }

    public async Task<bool> LockUserAsync(string userId, string lockedBy)
    {
        var result = await _userStore.LockUserAsync(userId, lockedBy);
        if (result)
            await _audit.LogAsync("User", null, "AccountLocked", lockedBy, null, null, new { userId });
        return result;
    }

    public async Task<(bool Success, string? Error)> RegisterUserAsync(string email, string fullName, string password, string role)
    {
        var result = await _userStore.CreateUserAsync(email, fullName, password, role);
        if (result.Success)
            await _audit.LogAsync("User", result.EmployeeId, "Registered", email, null, null, new { role });
        return (result.Success, result.Error);
    }
}
