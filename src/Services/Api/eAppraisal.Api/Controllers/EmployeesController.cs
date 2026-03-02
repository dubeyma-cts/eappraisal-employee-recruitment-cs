using eAppraisal.Shared.Auth;
using eAppraisal.Shared.Contracts;
using eAppraisal.Shared.Data;
using eAppraisal.Shared.Masking;
using eAppraisal.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eAppraisal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{AppRoles.HR},{AppRoles.ITAdmin},{AppRoles.Manager}")]
    public async Task<IActionResult> GetAll()
    {
        var employees = await db.Employees
            .Include(e => e.ReportsTo)
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();

        return Ok(employees.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var emp = await db.Employees
            .Include(e => e.ReportsTo)
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);

        if (emp is null) return NotFound(new ApiResult(false, "Employee not found."));

        // Employees can only view their own record
        var role       = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var empIdClaim = User.FindFirst("employee_id")?.Value;
        if (role == AppRoles.Employee && empIdClaim != id.ToString())
            return Forbid();

        return Ok(ToDto(emp));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.HR)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest req)
    {
        if (await db.Employees.AnyAsync(e => e.Email == req.Email))
            return Conflict(new ApiResult(false, "Email already registered."));

        var emp = new Employee
        {
            Name              = req.Name,
            Address           = req.Address,
            City              = req.City,
            PhoneNumber       = req.PhoneNumber,
            Mobile            = req.Mobile,
            Email             = req.Email,
            DateOfBirth       = req.DateOfBirth,
            Gender            = req.Gender,
            MaritalStatus     = req.MaritalStatus,
            DateOfJoining     = req.DateOfJoining,
            PassportNo        = req.PassportNo,
            PanNo             = req.PanNo.Trim().ToUpperInvariant(), // stored as-is; display always masked
            WorkExperienceYears = req.WorkExperienceYears,
            ReportsToId       = req.ReportsToId,
            Department        = req.Department,
            CTC               = req.CTC
        };

        db.Employees.Add(emp);
        db.AuditLogs.Add(new AuditLog
        {
            Actor = User.Identity?.Name ?? "HR", Role = AppRoles.HR,
            Action = "EMPLOYEE_CREATED",
            Details = $"Employee created: {emp.Name}, Dept: {emp.Department}"
            // PAN deliberately omitted from audit
        });
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = emp.Id }, ToDto(emp));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.HR)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateEmployeeRequest req)
    {
        var emp = await db.Employees.FindAsync(id);
        if (emp is null) return NotFound(new ApiResult(false, "Employee not found."));

        emp.Name              = req.Name;
        emp.Address           = req.Address;
        emp.City              = req.City;
        emp.PhoneNumber       = req.PhoneNumber;
        emp.Mobile            = req.Mobile;
        emp.Email             = req.Email;
        emp.Department        = req.Department;
        emp.CTC               = req.CTC;
        emp.ReportsToId       = req.ReportsToId;
        emp.WorkExperienceYears = req.WorkExperienceYears;

        db.AuditLogs.Add(new AuditLog
        {
            Actor = User.Identity?.Name ?? "HR", Role = AppRoles.HR,
            Action = "EMPLOYEE_UPDATED",
            Details = $"Employee updated: {id} ({emp.Name})"
        });
        await db.SaveChangesAsync();
        return Ok(ToDto(emp));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static EmployeeDto ToDto(Employee e) => new(
        e.Id, e.Name, e.Email, e.Mobile, e.Department,
        e.City, PanMaskingService.MaskPan(e.PanNo),
        e.ReportsToId, e.ReportsTo?.Name,
        e.CTC, e.WorkExperienceYears, e.DateOfJoining, e.IsActive);
}
