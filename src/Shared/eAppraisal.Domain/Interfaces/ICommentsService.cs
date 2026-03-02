using eAppraisal.Domain.DTOs;

namespace eAppraisal.Domain.Interfaces;

public interface ICommentsService
{
    Task SaveCommentAsync(ManagerCommentDto dto, string changedBy);
    Task SaveFeedbackAsync(EmployeeFeedbackDto dto, string changedBy);
    Task LockCommentsAsync(int appraisalId);
}
