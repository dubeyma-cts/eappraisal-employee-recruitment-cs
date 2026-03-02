using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAuthService _auth;

    public AdminController(IAuthService auth) => _auth = auth;

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers() => Ok(await _auth.GetAllUsersAsync());

    [HttpPost("users/{userId}/unlock")]
    public async Task<IActionResult> UnlockUser(string userId)
    {
        var result = await _auth.UnlockUserAsync(userId, User.Identity?.Name ?? "system");
        return result ? Ok() : NotFound();
    }

    [HttpPost("users/{userId}/lock")]
    public async Task<IActionResult> LockUser(string userId)
    {
        var result = await _auth.LockUserAsync(userId, User.Identity?.Name ?? "system");
        return result ? Ok() : NotFound();
    }
}
