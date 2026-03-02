using Microsoft.AspNetCore.Mvc;
using eAppraisal.Domain.DTOs;
using eAppraisal.Web.Services;

namespace eAppraisal.Web.Controllers;

public class EmployeeController : Controller
{
    private readonly ApiClient _api;

    public EmployeeController(ApiClient api) => _api = api;

    private bool IsAuthenticated() => !string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"));

    public IActionResult Index()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        return View();
    }

    public async Task<IActionResult> MyProfile()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId") ?? 0;
            var emp = await _api.GetAsync<EmployeeDto>($"api/employees/{empId}");
            return View(emp);
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId") ?? 0;
            var emp = await _api.GetAsync<EmployeeDto>($"api/employees/{empId}");
            return View(emp);
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpPost]
    public async Task<IActionResult> EditProfile(EmployeeDto model)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);
        var empId = HttpContext.Session.GetInt32("EmployeeId") ?? 0;
        await _api.PutAsync($"api/employees/{empId}/profile", model);
        return RedirectToAction("MyProfile");
    }

    public async Task<IActionResult> MyAppraisal()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId") ?? 0;
            var appraisals = await _api.GetAsync<List<AppraisalDto>>($"api/appraisals/by-employee/{empId}");
            return View(appraisals ?? new List<AppraisalDto>());
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpGet]
    public async Task<IActionResult> ViewAppraisal(int id)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var detail = await _api.GetAsync<AppraisalDetailDto>($"api/appraisals/{id}");
            if (detail == null) return NotFound();
            return View(detail);
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpGet]
    public async Task<IActionResult> SubmitFeedback(int id)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var detail = await _api.GetAsync<AppraisalDetailDto>($"api/appraisals/{id}");
            if (detail == null) return NotFound();
            var model = new EmployeeFeedbackDto
            {
                AppraisalId = id,
                FeedbackText = detail.FeedbackText ?? "",
                SelfAssessment = detail.SelfAssessment
            };
            ViewBag.Detail = detail;
            return View(model);
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpPost]
    public async Task<IActionResult> SubmitFeedback(EmployeeFeedbackDto model)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);
        await _api.PostAsync("api/appraisals/feedback", model);
        return RedirectToAction("MyAppraisal");
    }
}
