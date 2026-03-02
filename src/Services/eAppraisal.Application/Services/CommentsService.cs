using Microsoft.EntityFrameworkCore;
using eAppraisal.Application.Contracts;
using eAppraisal.Domain.DTOs;
using eAppraisal.Domain.Entities;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Application.Services;

public class CommentsService : ICommentsService
{
    private readonly IAppDbContext _db;
    private readonly IAuditService _audit;

    public CommentsService(IAppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task SaveCommentAsync(ManagerCommentDto dto, string changedBy)
    {
        var appraisal = await _db.Appraisals
            .Include(a => a.ManagerComment)
            .FirstOrDefaultAsync(a => a.Id == dto.AppraisalId)
            ?? throw new KeyNotFoundException($"Appraisal {dto.AppraisalId} not found");

        if (appraisal.ManagerComment?.IsLocked == true)
            throw new InvalidOperationException("Comments are locked after finalization");

        if (appraisal.ManagerComment == null)
        {
            appraisal.ManagerComment = new Comment
            {
                AppraisalId = dto.AppraisalId,
                Achievements = dto.Achievements,
                Gaps = dto.Gaps,
                Suggestions = dto.Suggestions,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        else
        {
            appraisal.ManagerComment.Achievements = dto.Achievements;
            appraisal.ManagerComment.Gaps = dto.Gaps;
            appraisal.ManagerComment.Suggestions = dto.Suggestions;
            appraisal.ManagerComment.UpdatedAt = DateTime.UtcNow;
        }

        var oldStatus = appraisal.Status;
        if (appraisal.Status == "Draft")
        {
            appraisal.Status = "ManagerCommented";
            appraisal.UpdatedAt = DateTime.UtcNow;
            _db.StageHistories.Add(new StageHistory
            {
                AppraisalId = appraisal.Id,
                FromStage = oldStatus,
                ToStage = "ManagerCommented",
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow,
                Reason = "Manager submitted comments"
            });
        }

        await _db.SaveChangesAsync();
        await _audit.LogAsync("Comment", appraisal.ManagerComment!.Id, "Saved", changedBy, null, null);
    }

    public async Task SaveFeedbackAsync(EmployeeFeedbackDto dto, string changedBy)
    {
        var appraisal = await _db.Appraisals
            .Include(a => a.EmployeeFeedback)
            .FirstOrDefaultAsync(a => a.Id == dto.AppraisalId)
            ?? throw new KeyNotFoundException($"Appraisal {dto.AppraisalId} not found");

        if (appraisal.Status == "Final")
            throw new InvalidOperationException("Cannot modify feedback on finalized appraisal");

        if (appraisal.EmployeeFeedback == null)
        {
            appraisal.EmployeeFeedback = new EmployeeFeedback
            {
                AppraisalId = dto.AppraisalId,
                FeedbackText = dto.FeedbackText,
                SelfAssessment = dto.SelfAssessment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        else
        {
            appraisal.EmployeeFeedback.FeedbackText = dto.FeedbackText;
            appraisal.EmployeeFeedback.SelfAssessment = dto.SelfAssessment;
            appraisal.EmployeeFeedback.UpdatedAt = DateTime.UtcNow;
        }

        var oldStatus = appraisal.Status;
        if (appraisal.Status == "ManagerCommented")
        {
            appraisal.Status = "EmployeeFeedback";
            appraisal.UpdatedAt = DateTime.UtcNow;
            _db.StageHistories.Add(new StageHistory
            {
                AppraisalId = appraisal.Id,
                FromStage = oldStatus,
                ToStage = "EmployeeFeedback",
                ChangedBy = changedBy,
                ChangedAt = DateTime.UtcNow,
                Reason = "Employee submitted feedback"
            });
        }

        await _db.SaveChangesAsync();
        await _audit.LogAsync("Feedback", appraisal.EmployeeFeedback!.Id, "Saved", changedBy, null, null);
    }

    public async Task LockCommentsAsync(int appraisalId)
    {
        var appraisal = await _db.Appraisals
            .Include(a => a.ManagerComment)
            .FirstOrDefaultAsync(a => a.Id == appraisalId);

        if (appraisal?.ManagerComment != null)
        {
            appraisal.ManagerComment.IsLocked = true;
            await _db.SaveChangesAsync();
        }
    }
}
