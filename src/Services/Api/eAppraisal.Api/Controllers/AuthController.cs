using eAppraisal.Shared.Auth;
using eAppraisal.Shared.Contracts;
using eAppraisal.Shared.Data;
using eAppraisal.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace eAppraisal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, IConfiguration config) : ControllerBase
{
    private const int MaxFailedAttempts = 3;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await db.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Username == req.Username);

        if (user is null)
            return Unauthorized(new ApiResult(false, "Invalid credentials."));

        if (user.IsLocked)
            return Unauthorized(new ApiResult(false, "Account is locked. Please contact IT Admin."));

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.IsLocked = true;
                db.AuditLogs.Add(new AuditLog
                {
                    Actor = user.Username, Role = user.Role,
                    Action = "ACCOUNT_LOCKED",
                    Details = $"Account locked after {MaxFailedAttempts} failed attempts",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });
            }
            await db.SaveChangesAsync();
            int remaining = Math.Max(0, MaxFailedAttempts - user.FailedLoginAttempts);
            return Unauthorized(new ApiResult(false,
                user.IsLocked
                    ? "Account locked. Contact IT Admin."
                    : $"Invalid credentials. {remaining} attempt(s) remaining."));
        }

        // Successful login
        user.FailedLoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;
        db.AuditLogs.Add(new AuditLog
        {
            Actor = user.Username, Role = user.Role, Action = "LOGIN",
            Details = "Successful login",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await db.SaveChangesAsync();

        var token = GenerateToken(user, config);
        return Ok(new LoginResponse(token, user.Username, user.Role, user.EmployeeId));
    }

    [HttpPost("unlock/{username}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = AppRoles.ITAdmin)]
    public async Task<IActionResult> Unlock(string username)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null) return NotFound(new ApiResult(false, "User not found."));

        user.IsLocked = false;
        user.FailedLoginAttempts = 0;
        db.AuditLogs.Add(new AuditLog
        {
            Actor = User.Identity?.Name ?? "ITAdmin", Role = AppRoles.ITAdmin,
            Action = "ACCOUNT_UNLOCKED", Details = $"Unlocked account: {username}",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await db.SaveChangesAsync();
        return Ok(new ApiResult(true, $"Account {username} has been unlocked."));
    }

    [HttpGet("users")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = AppRoles.ITAdmin)]
    public async Task<IActionResult> GetUsers()
    {
        var users = await db.Users
            .Select(u => new { u.Id, u.Username, u.Role, u.IsLocked, u.FailedLoginAttempts, u.LastLoginAt })
            .ToListAsync();
        return Ok(users);
    }

    private static string GenerateToken(AppUser user, IConfiguration config)
    {
        var key     = config["Jwt:Key"]    ?? "eAppraisal-Demo-SuperSecretKey-2026!";
        var issuer  = config["Jwt:Issuer"] ?? "eAppraisal.Api";
        var expires = DateTime.UtcNow.AddMinutes(15); // 15-min idle timeout

        var claims = new[]
        {
            new Claim(ClaimTypes.Name,            user.Username),
            new Claim(ClaimTypes.Role,            user.Role),
            new Claim("employee_id",              user.EmployeeId?.ToString() ?? ""),
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer, audience: null,
            claims: claims, expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
