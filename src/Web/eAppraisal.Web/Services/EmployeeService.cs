using eAppraisal.Shared.Contracts;
using eAppraisal.Shared.Data;
using eAppraisal.Shared.Masking;
using eAppraisal.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace eAppraisal.Web.Services;

public class EmployeeService(AppDbContext db, BlazorAuthStateProvider authState)
{
    public async Task<List<EmployeeDto>> GetAllAsync() =>
        (await db.Employees
            .Include(e => e.ReportsTo)
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync())
        .Select(ToDto).ToList();

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var e = await db.Employees.Include(x => x.ReportsTo)
                        .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        return e is null ? null : ToDto(e);
    }

    public async Task<(bool ok, string msg)> CreateAsync(CreateEmployeeRequest req)
    {
        if (await db.Employees.AnyAsync(e => e.Email == req.Email))
            return (false, "An employee with this email already exists.");

        var emp = new Employee
        {
            Name = req.Name, Address = req.Address, City = req.City,
            PhoneNumber = req.PhoneNumber, Mobile = req.Mobile, Email = req.Email,
            DateOfBirth = req.DateOfBirth, Gender = req.Gender,
            MaritalStatus = req.MaritalStatus, DateOfJoining = req.DateOfJoining,
            PassportNo = req.PassportNo,
            PanNo = req.PanNo.Trim().ToUpperInvariant(),
            WorkExperienceYears = req.WorkExperienceYears,
            ReportsToId = req.ReportsToId, Department = req.Department, CTC = req.CTC
        };
        db.Employees.Add(emp);
        db.AuditLogs.Add(new AuditLog
        {
            Actor = authState.CurrentUsername!, Role = authState.CurrentRole!,
            Action = "EMPLOYEE_CREATED",
            Details = $"Employee created: {emp.Name} ({emp.Department})"
        });
        await db.SaveChangesAsync();
        return (true, $"Employee '{emp.Name}' added successfully.");
    }

    public async Task<List<EmployeeDto>> GetManagersAsync() =>
        (await db.Users
            .Where(u => u.Role == eAppraisal.Shared.Auth.AppRoles.Manager && u.EmployeeId != null)
            .Include(u => u.Employee)
            .ToListAsync())
        .Where(u => u.Employee is not null)
        .Select(u => ToDto(u.Employee!))
        .ToList();

    private static EmployeeDto ToDto(Employee e) => new(
        e.Id, e.Name, e.Email, e.Mobile, e.Department, e.City,
        PanMaskingService.MaskPan(e.PanNo), e.ReportsToId, e.ReportsTo?.Name,
        e.CTC, e.WorkExperienceYears, e.DateOfJoining, e.IsActive);
}
