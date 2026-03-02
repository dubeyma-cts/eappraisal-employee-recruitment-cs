using Microsoft.EntityFrameworkCore;
using eAppraisal.Application.Contracts;
using eAppraisal.Domain.DTOs;
using eAppraisal.Domain.Entities;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Application.Services;

public class AppraisalWorkflowService : IAppraisalWorkflowService
{
    private readonly IAppDbContext _db;
    private readonly ICtcService _ctc;
    private readonly ICommentsService _comments;
    private readonly IAuditService _audit;
    private readonly INotificationService _notification;

    public AppraisalWorkflowService(
        IAppDbContext db,
        ICtcService ctc,
        ICommentsService comments,
        IAuditService audit,
        INotificationService notification)
    {
        _db = db;
        _ctc = ctc;
        _comments = comments;
        _audit = audit;
        _notification = notification;
    }

    public async Task<AppraisalDto> AssignAsync(int employeeId, int cycleId, string assignedBy)
    {
        var employee = await _db.Employees.FindAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee {employeeId} not found");

        var appraisal = new Appraisal
        {
            EmployeeId = employeeId,
            CycleId = cycleId,
            ManagerEmployeeId = employee.ManagerId ?? throw new InvalidOperationException("Employee has no manager assigned"),
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Appraisals.Add(appraisal);
        await _db.SaveChangesAsync();

        _db.StageHistories.Add(new StageHistory
        {
            AppraisalId = appraisal.Id,
            FromStage = "None",
            ToStage = "Draft",
            ChangedBy = assignedBy,
            ChangedAt = DateTime.UtcNow,
            Reason = "Appraisal assigned"
        });
        await _db.SaveChangesAsync();

        await _audit.LogAsync("Appraisal", appraisal.Id, "Assigned", assignedBy, null, null);
        await _notification.QueueAsync(employee.Email ?? "", "AppraisalAssigned", appraisal.Id);

        var result = await _db.Appraisals
            .Include(a => a.Employee).Include(a => a.ManagerEmployee).Include(a => a.Cycle)
            .FirstAsync(a => a.Id == appraisal.Id);

        return new AppraisalDto
        {
            AppraisalId = result.Id,
            EmployeeName = result.Employee?.FullName,
            Department = result.Employee?.Department,
            ManagerName = result.ManagerEmployee?.FullName,
            CycleName = result.Cycle?.Name,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            FinalisedAt = result.FinalisedAt
        };
    }

    public async Task FinalizeAsync(FinalizeAppraisalDto dto, string changedBy)
    {
        var appraisal = await _db.Appraisals
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == dto.AppraisalId)
            ?? throw new KeyNotFoundException($"Appraisal {dto.AppraisalId} not found");

        if (appraisal.Status == "Final")
            throw new InvalidOperationException("Appraisal already finalized");

        // Create CTC snapshot
        await _ctc.CreateOrUpdateSnapshotAsync(dto.AppraisalId, dto);

        // Lock comments (immutability post-finalization)
        await _comments.LockCommentsAsync(dto.AppraisalId);

        // Update appraisal status
        var oldStatus = appraisal.Status;
        appraisal.Status = "Final";
        appraisal.FinalisedAt = DateTime.UtcNow;
        appraisal.NextAppraisalDate = dto.NextAppraisalDate;
        appraisal.UpdatedAt = DateTime.UtcNow;

        _db.StageHistories.Add(new StageHistory
        {
            AppraisalId = appraisal.Id,
            FromStage = oldStatus,
            ToStage = "Final",
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow,
            Reason = "Appraisal finalized by manager"
        });

        await _db.SaveChangesAsync();

        await _audit.LogAsync("Appraisal", appraisal.Id, "Finalized", changedBy, null, null);
        await _notification.QueueAsync(appraisal.Employee?.Email ?? "", "AppraisalFinalized", appraisal.Id);
    }
}
