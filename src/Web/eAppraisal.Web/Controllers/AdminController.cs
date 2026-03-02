using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using eAppraisal.Domain.DTOs;
using eAppraisal.Web.Services;

namespace eAppraisal.Web.Controllers;

public class AdminController : Controller
{
    private readonly ApiClient _api;

    public AdminController(ApiClient api) => _api = api;

    private bool IsAuthenticated() => !string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"));

    public IActionResult Index()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        return View();
    }

    public async Task<IActionResult> Users()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var users = await _api.GetAsync<List<UserDto>>("api/admin/users");
            return View(users ?? new List<UserDto>());
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpGet]
    public IActionResult CreateUser()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);

        try
        {
            var response = await _api.PostAsync("api/auth/register", new
            {
                model.Email,
                model.FullName,
                model.Password,
                model.Role
            });
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var error = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(json,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ModelState.AddModelError("", error?.Message ?? "User creation failed.");
                return View(model);
            }

            TempData["Success"] = $"User '{model.Email}' created successfully with role '{model.Role}'.";
            return RedirectToAction("Users");
        }
        catch
        {
            ModelState.AddModelError("", "Unable to connect to the API server.");
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UnlockUser(string userId)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        await _api.PostEmptyAsync($"api/admin/users/{userId}/unlock");
        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> LockUser(string userId)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        await _api.PostEmptyAsync($"api/admin/users/{userId}/lock");
        return RedirectToAction("Users");
    }
}

public class CreateUserViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Employee";
}
