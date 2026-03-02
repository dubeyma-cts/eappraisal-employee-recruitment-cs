using eAppraisal.Domain.DTOs;

namespace eAppraisal.Domain.Interfaces;

public interface IReportingService
{
    Task<List<AppraisalDto>> GetByManagerAsync(int managerEmpId);
    Task<AppraisalDetailDto?> GetDetailAsync(int appraisalId);
    Task<List<AppraisalDto>> GetByStatusAsync(string status, string? role, int? employeeId);
    Task<List<AppraisalDto>> GetByEmployeeAsync(int employeeId);
}
