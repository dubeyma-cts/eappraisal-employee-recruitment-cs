using eAppraisal.Domain.DTOs;

namespace eAppraisal.Domain.Interfaces;

public interface IAppraisalWorkflowService
{
    Task<AppraisalDto> AssignAsync(int employeeId, int cycleId, string assignedBy);
    Task FinalizeAsync(FinalizeAppraisalDto dto, string changedBy);
}
