using Microsoft.EntityFrameworkCore;
using eAppraisal.Application.Contracts;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Application.Services;

public class EligibilityRulesService : IEligibilityRulesService
{
    private readonly IAppDbContext _db;

    public EligibilityRulesService(IAppDbContext db) => _db = db;

    public async Task<(bool IsValid, string? Error)> ValidateAssignmentAsync(int employeeId, int cycleId)
    {
        var employee = await _db.Employees.FindAsync(employeeId);
        if (employee == null)
            return (false, $"Employee {employeeId} not found");
        if (!employee.ManagerId.HasValue)
            return (false, "Employee has no manager assigned");

        var alreadyAssigned = await _db.Appraisals
            .AnyAsync(a => a.EmployeeId == employeeId && a.CycleId == cycleId);
        if (alreadyAssigned)
            return (false, "Employee is already assigned to this cycle");

        var cycle = await _db.AppraisalCycles.FindAsync(cycleId);
        if (cycle == null)
            return (false, $"Cycle {cycleId} not found");
        if (cycle.State != "Open")
            return (false, "Cycle is not open");

        return (true, null);
    }

    public async Task<(bool IsValid, string? Error)> ValidateCommentAsync(int appraisalId)
    {
        var appraisal = await _db.Appraisals
            .Include(a => a.ManagerComment)
            .FirstOrDefaultAsync(a => a.Id == appraisalId);
        if (appraisal == null)
            return (false, $"Appraisal {appraisalId} not found");
        if (appraisal.ManagerComment?.IsLocked == true)
            return (false, "Comments are locked after finalization");
        return (true, null);
    }

    public async Task<(bool IsValid, string? Error)> ValidateFeedbackAsync(int appraisalId)
    {
        var appraisal = await _db.Appraisals.FindAsync(appraisalId);
        if (appraisal == null)
            return (false, $"Appraisal {appraisalId} not found");
        if (appraisal.Status == "Final")
            return (false, "Cannot modify feedback on finalized appraisal");
        return (true, null);
    }

    public async Task<(bool IsValid, string? Error)> ValidateFinalizationAsync(int appraisalId)
    {
        var appraisal = await _db.Appraisals.FindAsync(appraisalId);
        if (appraisal == null)
            return (false, $"Appraisal {appraisalId} not found");
        if (appraisal.Status == "Final")
            return (false, "Appraisal already finalized");
        return (true, null);
    }
}
