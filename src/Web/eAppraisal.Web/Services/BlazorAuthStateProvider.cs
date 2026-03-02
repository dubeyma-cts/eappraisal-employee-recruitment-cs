using eAppraisal.Shared.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace eAppraisal.Web.Services;

/// <summary>
/// Per-circuit (Scoped) auth state for Interactive Server Blazor.
/// On login, the principal is set for the lifetime of the connection.
/// Refreshing the page resets state — user must log in again (acceptable for demo).
/// </summary>
public class BlazorAuthStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _user = new(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(_user));

    public void Login(string username, string role, int? employeeId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),
        };
        if (employeeId.HasValue)
            claims.Add(new Claim("employee_id", employeeId.Value.ToString()));

        _user = new ClaimsPrincipal(new ClaimsIdentity(claims, "cookie"));
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));
    }

    public void Logout()
    {
        _user = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));
    }

    public string? CurrentUsername => _user.Identity?.Name;
    public string? CurrentRole     => _user.FindFirst(ClaimTypes.Role)?.Value;
    public int?    CurrentEmployeeId
    {
        get
        {
            var v = _user.FindFirst("employee_id")?.Value;
            return int.TryParse(v, out var id) ? id : null;
        }
    }

    public bool IsInRole(string role) => CurrentRole == role;
    public bool IsHR       => IsInRole(AppRoles.HR);
    public bool IsManager  => IsInRole(AppRoles.Manager);
    public bool IsEmployee => IsInRole(AppRoles.Employee);
    public bool IsAdmin    => IsInRole(AppRoles.ITAdmin);
}
