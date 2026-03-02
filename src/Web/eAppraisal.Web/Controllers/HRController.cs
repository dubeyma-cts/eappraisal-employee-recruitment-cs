using Microsoft.AspNetCore.Mvc;
using eAppraisal.Domain.DTOs;
using eAppraisal.Web.Services;

namespace eAppraisal.Web.Controllers;

public class HRController : Controller
{
    private readonly ApiClient _api;

    public HRController(ApiClient api) => _api = api;

    private bool IsAuthenticated() => !string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"));

    public IActionResult Index()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        return View();
    }

    public async Task<IActionResult> Employees()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var employees = await _api.GetAsync<List<EmployeeDto>>("api/employees");
            return View(employees ?? new List<EmployeeDto>());
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpGet]
    public async Task<IActionResult> CreateEmployee()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var employees = await _api.GetAsync<List<EmployeeDto>>("api/employees") ?? new();
            ViewBag.Managers = employees;
            return View(new EmployeeDto { DateOfJoining = DateTime.Today });
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee(EmployeeDto model, bool createLogin, string? loginPassword, string? loginRole)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid)
        {
            var employees = await _api.GetAsync<List<EmployeeDto>>("api/employees") ?? new();
            ViewBag.Managers = employees;
            return View(model);
        }
        await _api.PostAsync("api/employees", model);

        // Optionally create a login account for the new employee
        if (createLogin && !string.IsNullOrEmpty(loginPassword))
        {
            var fullName = $"{model.FirstName} {model.LastName}".Trim();
            var role = string.IsNullOrEmpty(loginRole) ? "Employee" : loginRole;
            var response = await _api.PostAsync("api/auth/register", new
            {
                Email = model.Email,
                FullName = fullName,
                Password = loginPassword,
                Role = role
            });

            if (!response.IsSuccessStatusCode)
            {
                TempData["Warning"] = "Employee created, but login account creation failed. You can create it later from Admin panel.";
                return RedirectToAction("Employees");
            }

            TempData["Success"] = $"Employee and login account created successfully for {model.Email}.";
        }

        return RedirectToAction("Employees");
    }

    [HttpGet]
    public async Task<IActionResult> EditEmployee(int id)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var emp = await _api.GetAsync<EmployeeDto>($"api/employees/{id}");
            if (emp == null) return NotFound();
            var employees = await _api.GetAsync<List<EmployeeDto>>("api/employees") ?? new();
            ViewBag.Managers = employees.Where(e => e.Id != id).ToList();
            return View(emp);
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpPost]
    public async Task<IActionResult> EditEmployee(EmployeeDto model)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid)
        {
            var employees = await _api.GetAsync<List<EmployeeDto>>("api/employees") ?? new();
            ViewBag.Managers = employees.Where(e => e.Id != model.Id).ToList();
            return View(model);
        }
        await _api.PutAsync($"api/employees/{model.Id}", model);
        return RedirectToAction("Employees");
    }

    public async Task<IActionResult> Cycles()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var cycles = await _api.GetAsync<List<CycleDto>>("api/cycles");
            return View(cycles ?? new List<CycleDto>());
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpGet]
    public IActionResult CreateCycle()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        return View(new CycleDto { StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(3), State = "Open" });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCycle(CycleDto model)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);
        await _api.PostAsync("api/cycles", model);
        return RedirectToAction("Cycles");
    }

    [HttpGet]
    public async Task<IActionResult> EditCycle(int id)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var cycle = await _api.GetAsync<CycleDto>($"api/cycles/{id}");
            if (cycle == null) return NotFound();
            return View(cycle);
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpPost]
    public async Task<IActionResult> EditCycle(CycleDto model)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);
        await _api.PutAsync($"api/cycles/{model.Id}", model);
        return RedirectToAction("Cycles");
    }

    public async Task<IActionResult> UpcomingAppraisals()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var openCycles = await _api.GetAsync<List<CycleDto>>("api/cycles/open") ?? new();
            var openCycleIds = openCycles.Select(c => c.Id).ToList();
            var unassigned = openCycleIds.Any()
                ? await _api.PostAsync<List<int>, List<EmployeeDto>>("api/cycles/unassigned-employees", openCycleIds) ?? new()
                : new List<EmployeeDto>();
            ViewBag.OpenCycles = openCycles;
            return View(unassigned);
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpPost]
    public async Task<IActionResult> AssignAppraisal(int employeeId, int cycleId)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        await _api.PostAsync("api/appraisals/assign", new { employeeId, cycleId });
        return RedirectToAction("UpcomingAppraisals");
    }
}
