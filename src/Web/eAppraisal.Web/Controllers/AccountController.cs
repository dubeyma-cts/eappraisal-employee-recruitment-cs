using Microsoft.AspNetCore.Mvc;
using eAppraisal.Domain.DTOs;
using eAppraisal.Web.Services;

namespace eAppraisal.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api) => _api = api;

    public IActionResult Index() => RedirectToAction("Login");

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var result = await _api.LoginAsync<LoginDto, AuthResultDto>("api/auth/login", model);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("", result?.ErrorMessage ?? "Login failed");
                return View(model);
            }

            // Store user info in session
            HttpContext.Session.SetString("UserEmail", model.Email);
            HttpContext.Session.SetString("UserRole", result.Role ?? "");
            HttpContext.Session.SetString("UserFullName", result.FullName ?? "");
            if (result.EmployeeId.HasValue)
                HttpContext.Session.SetInt32("EmployeeId", result.EmployeeId.Value);

            return result.Role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "HR" => RedirectToAction("Index", "HR"),
                "Manager" => RedirectToAction("Index", "Manager"),
                "Employee" => RedirectToAction("Index", "Employee"),
                _ => RedirectToAction("Login")
            };
        }
        catch
        {
            ModelState.AddModelError("", "Unable to connect to the API server.");
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        try { await _api.PostEmptyAsync("api/auth/logout"); } catch { }
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}

public class ErrorResponse
{
    public string? Message { get; set; }
}
