using Microsoft.AspNetCore.Mvc;
using eAppraisal.Domain.DTOs;
using eAppraisal.Web.Services;

namespace eAppraisal.Web.Controllers;

public class ManagerController : Controller
{
    private readonly ApiClient _api;

    public ManagerController(ApiClient api) => _api = api;

    private bool IsAuthenticated() => !string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"));

    public IActionResult Index()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        return View();
    }

    public async Task<IActionResult> MyAppraisals()
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId") ?? 0;
            var appraisals = await _api.GetAsync<List<AppraisalDto>>($"api/appraisals/by-manager/{empId}");
            return View(appraisals ?? new List<AppraisalDto>());
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    public async Task<IActionResult> ViewEmployee(int id)
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
    public async Task<IActionResult> EnterComments(int id)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var detail = await _api.GetAsync<AppraisalDetailDto>($"api/appraisals/{id}");
            if (detail == null) return NotFound();
            var model = new ManagerCommentDto
            {
                AppraisalId = id,
                Achievements = detail.Achievements ?? "",
                Gaps = detail.Gaps,
                Suggestions = detail.Suggestions
            };
            ViewBag.EmployeeName = detail.EmployeeName;
            return View(model);
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpPost]
    public async Task<IActionResult> EnterComments(ManagerCommentDto model)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);
        await _api.PostAsync("api/appraisals/comment", model);
        return RedirectToAction("MyAppraisals");
    }

    [HttpGet]
    public async Task<IActionResult> Finalize(int id)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        try
        {
            var detail = await _api.GetAsync<AppraisalDetailDto>($"api/appraisals/{id}");
            if (detail == null) return NotFound();
            var model = new FinalizeAppraisalDto
            {
                AppraisalId = id,
                IsPromoted = detail.IsPromoted,
                Basic = detail.Basic,
                DA = detail.DA,
                HRA = detail.HRA,
                FoodAllowance = detail.FoodAllowance,
                PF = detail.PF,
                NextAppraisalDate = detail.NextAppraisalDate
            };
            ViewBag.Detail = detail;
            return View(model);
        }
        catch { return RedirectToAction("Login", "Account"); }
    }

    [HttpPost]
    public async Task<IActionResult> Finalize(FinalizeAppraisalDto model)
    {
        if (!IsAuthenticated()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);
        await _api.PostAsync("api/appraisals/finalize", model);
        return RedirectToAction("MyAppraisals");
    }
}
