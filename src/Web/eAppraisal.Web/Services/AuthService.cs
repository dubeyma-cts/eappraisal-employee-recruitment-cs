using eAppraisal.Shared.Contracts;
using eAppraisal.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace eAppraisal.Web.Services;

public record LoginResult(bool Success, string Message, string? Role = null, int? EmployeeId = null);

public class AuthService(AppDbContext db, BlazorAuthStateProvider authState)
{
    private const int MaxAttempts = 3;

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user is null)
            return new(false, "Invalid username or password.");

        if (user.IsLocked)
            return new(false, "Your account is locked. Please contact IT Admin.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxAttempts)
            {
                user.IsLocked = true;
                db.AuditLogs.Add(new eAppraisal.Shared.Models.AuditLog
                {
                    Actor = username, Role = user.Role,
                    Action = "ACCOUNT_LOCKED",
                    Details = $"Account locked after {MaxAttempts} failed attempts (Web)"
                });
            }
            await db.SaveChangesAsync();
            int remaining = Math.Max(0, MaxAttempts - user.FailedLoginAttempts);
            return user.IsLocked
                ? new(false, "Account locked after too many failed attempts. Contact IT Admin.")
                : new(false, $"Invalid password. {remaining} attempt(s) remaining.");
        }

        user.FailedLoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;
        db.AuditLogs.Add(new eAppraisal.Shared.Models.AuditLog
        {
            Actor = username, Role = user.Role, Action = "LOGIN",
            Details = "Web login successful"
        });
        await db.SaveChangesAsync();

        authState.Login(username, user.Role, user.EmployeeId);
        return new(true, "Login successful.", user.Role, user.EmployeeId);
    }

    public void Logout()
    {
        db.AuditLogs.Add(new eAppraisal.Shared.Models.AuditLog
        {
            Actor = authState.CurrentUsername ?? "unknown", Role = authState.CurrentRole ?? "",
            Action = "LOGOUT", Details = "Web logout"
        });
        db.SaveChanges();
        authState.Logout();
    }

    public async Task<bool> UnlockUserAsync(string username)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null) return false;
        user.IsLocked = false;
        user.FailedLoginAttempts = 0;
        db.AuditLogs.Add(new eAppraisal.Shared.Models.AuditLog
        {
            Actor = authState.CurrentUsername ?? "ITAdmin", Role = authState.CurrentRole ?? "",
            Action = "ACCOUNT_UNLOCKED", Details = $"Unlocked: {username}"
        });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserSummaryDto>> GetUsersAsync() =>
        await db.Users
            .OrderBy(u => u.Role).ThenBy(u => u.Username)
            .Select(u => new UserSummaryDto(
                u.Id, u.Username, u.Role, u.IsLocked,
                u.FailedLoginAttempts, u.LastLoginAt))
            .ToListAsync();
}
