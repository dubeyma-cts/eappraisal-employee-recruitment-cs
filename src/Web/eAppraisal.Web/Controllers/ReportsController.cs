using Microsoft.AspNetCore.Mvc;
using eAppraisal.Domain.DTOs;
using eAppraisal.Web.Services;

namespace eAppraisal.Web.Controllers;

public class ReportsController : Controller
{
    private readonly ApiClient _api;

    public ReportsController(ApiClient api) => _api = api;

    public IActionResult Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole")))
            return RedirectToAction("Login", "Account");
        return View();
    }

    public async Task<IActionResult> Upcoming()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole")))
            return RedirectToAction("Login", "Account");

        try
        {
            var role = HttpContext.Session.GetString("UserRole");
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            var appraisals = await _api.GetAsync<List<AppraisalDto>>($"api/appraisals/by-status?status=Draft&role={role}&employeeId={empId}");
            return View(appraisals ?? new());
        }
        catch
        {
            return RedirectToAction("Login", "Account");
        }
    }

    public async Task<IActionResult> InProcess()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole")))
            return RedirectToAction("Login", "Account");

        try
        {
            var role = HttpContext.Session.GetString("UserRole");
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            var list1 = await _api.GetAsync<List<AppraisalDto>>($"api/appraisals/by-status?status=ManagerCommented&role={role}&employeeId={empId}") ?? new();
            var list2 = await _api.GetAsync<List<AppraisalDto>>($"api/appraisals/by-status?status=EmployeeFeedback&role={role}&employeeId={empId}") ?? new();
            return View(list1.Concat(list2).ToList());
        }
        catch
        {
            return RedirectToAction("Login", "Account");
        }
    }

    public async Task<IActionResult> Completed()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole")))
            return RedirectToAction("Login", "Account");

        try
        {
            var role = HttpContext.Session.GetString("UserRole");
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            var appraisals = await _api.GetAsync<List<AppraisalDto>>($"api/appraisals/by-status?status=Final&role={role}&employeeId={empId}");
            return View(appraisals ?? new());
        }
        catch
        {
            return RedirectToAction("Login", "Account");
        }
    }
}
