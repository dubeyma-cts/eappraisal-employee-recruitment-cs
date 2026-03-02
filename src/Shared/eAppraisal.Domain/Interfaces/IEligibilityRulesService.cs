namespace eAppraisal.Domain.Interfaces;

public interface IEligibilityRulesService
{
    Task<(bool IsValid, string? Error)> ValidateAssignmentAsync(int employeeId, int cycleId);
    Task<(bool IsValid, string? Error)> ValidateCommentAsync(int appraisalId);
    Task<(bool IsValid, string? Error)> ValidateFeedbackAsync(int appraisalId);
    Task<(bool IsValid, string? Error)> ValidateFinalizationAsync(int appraisalId);
}
