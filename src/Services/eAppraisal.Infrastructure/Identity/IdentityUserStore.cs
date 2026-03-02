using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using eAppraisal.Application.Contracts;
using eAppraisal.Domain.DTOs;
using eAppraisal.Domain.Entities;
using eAppraisal.Infrastructure.Data;

namespace eAppraisal.Infrastructure.Identity;

public class IdentityUserStore : IUserStore
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _db;

    public IdentityUserStore(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    public async Task<AuthResultDto> AuthenticateAsync(string email, string password, string? ipAddress)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return new AuthResultDto { Success = false, ErrorMessage = "Invalid email or password." };

        // Check manual lock or 3-strike lockout
        if (user.IsManuallyLocked || user.FailedLoginAttempts >= 3)
            return new AuthResultDto { Success = false, ErrorMessage = "Account is locked. Contact IT Admin." };

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            user.FailedLoginAttempts++;
            await _userManager.UpdateAsync(user);

            if (user.FailedLoginAttempts >= 3)
                return new AuthResultDto { Success = false, ErrorMessage = "Account locked after 3 failed attempts. Contact IT Admin." };

            return new AuthResultDto { Success = false, ErrorMessage = "Invalid email or password." };
        }

        // Successful login - reset failed attempts
        user.FailedLoginAttempts = 0;
        await _userManager.UpdateAsync(user);

        await _signInManager.SignInAsync(user, isPersistent: false);

        return new AuthResultDto
        {
            Success = true,
            Role = user.AppRole,
            FullName = user.FullName,
            EmployeeId = user.EmployeeId
        };
    }

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _userManager.Users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            FullName = u.FullName,
            AppRole = u.AppRole,
            IsLocked = u.IsManuallyLocked || u.FailedLoginAttempts >= 3,
            FailedLoginAttempts = u.FailedLoginAttempts,
            EmployeeId = u.EmployeeId
        }).ToListAsync();
    }

    public async Task<bool> UnlockUserAsync(string userId, string unlockedBy)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.FailedLoginAttempts = 0;
        user.IsManuallyLocked = false;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<bool> LockUserAsync(string userId, string lockedBy)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.IsManuallyLocked = true;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<(bool Success, string? Error, int? EmployeeId)> CreateUserAsync(string email, string fullName, string password, string role)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null)
            return (false, "An account with this email already exists.", null);

        // Create employee record
        var employee = new Employee
        {
            FirstName = fullName.Split(' ').FirstOrDefault() ?? fullName,
            LastName = fullName.Contains(' ') ? string.Join(' ', fullName.Split(' ').Skip(1)) : "",
            Email = email,
            DateOfJoining = DateTime.UtcNow,
            Department = "Unassigned"
        };
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        // Create Identity user
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            AppRole = role,
            EmployeeId = employee.Id
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            // Rollback employee
            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();

            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return (false, errors, null);
        }

        return (true, null, employee.Id);
    }
}
