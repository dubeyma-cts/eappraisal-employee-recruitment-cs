using Microsoft.EntityFrameworkCore;
using eAppraisal.Application.Contracts;
using eAppraisal.Domain.DTOs;
using eAppraisal.Domain.Entities;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Application.Services;

public class ReportingService : IReportingService
{
    private readonly IAppDbContext _db;

    public ReportingService(IAppDbContext db) => _db = db;

    public async Task<List<AppraisalDto>> GetByManagerAsync(int managerEmpId)
    {
        return await _db.Appraisals
            .Include(a => a.Employee).Include(a => a.ManagerEmployee).Include(a => a.Cycle)
            .Where(a => a.ManagerEmployeeId == managerEmpId)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<AppraisalDetailDto?> GetDetailAsync(int appraisalId)
    {
        var a = await _db.Appraisals
            .Include(x => x.Employee).Include(x => x.ManagerEmployee)
            .Include(x => x.Cycle).Include(x => x.ManagerComment)
            .Include(x => x.EmployeeFeedback).Include(x => x.CtcSnapshot)
            .FirstOrDefaultAsync(x => x.Id == appraisalId);

        if (a == null) return null;

        return new AppraisalDetailDto
        {
            AppraisalId = a.Id,
            Status = a.Status,
            CreatedAt = a.CreatedAt,
            FinalisedAt = a.FinalisedAt,
            CycleName = a.Cycle?.Name,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee?.FullName,
            Department = a.Employee?.Department,
            Email = a.Employee?.Email,
            DateOfJoining = a.Employee?.DateOfJoining ?? default,
            ManagerEmployeeId = a.ManagerEmployeeId,
            ManagerName = a.ManagerEmployee?.FullName,
            Achievements = a.ManagerComment?.Achievements,
            Gaps = a.ManagerComment?.Gaps,
            Suggestions = a.ManagerComment?.Suggestions,
            IsCommentLocked = a.ManagerComment?.IsLocked ?? false,
            FeedbackText = a.EmployeeFeedback?.FeedbackText,
            SelfAssessment = a.EmployeeFeedback?.SelfAssessment,
            IsPromoted = a.CtcSnapshot?.IsPromoted ?? false,
            Basic = a.CtcSnapshot?.Basic ?? 0,
            DA = a.CtcSnapshot?.DA ?? 0,
            HRA = a.CtcSnapshot?.HRA ?? 0,
            FoodAllowance = a.CtcSnapshot?.FoodAllowance ?? 0,
            PF = a.CtcSnapshot?.PF ?? 0,
            TotalCTC = a.CtcSnapshot?.TotalCTC ?? 0,
            NextAppraisalDate = a.NextAppraisalDate,
            ApprovedAt = a.CtcSnapshot?.ApprovedAt
        };
    }

    public async Task<List<AppraisalDto>> GetByStatusAsync(string status, string? role, int? employeeId)
    {
        var query = _db.Appraisals
            .Include(a => a.Employee).Include(a => a.ManagerEmployee).Include(a => a.Cycle)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        if (role == "Manager" && employeeId.HasValue)
            query = query.Where(a => a.ManagerEmployeeId == employeeId.Value);
        else if (role == "Employee" && employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);

        return await query.Select(a => MapToDto(a)).ToListAsync();
    }

    public async Task<List<AppraisalDto>> GetByEmployeeAsync(int employeeId)
    {
        return await _db.Appraisals
            .Include(a => a.Employee).Include(a => a.ManagerEmployee).Include(a => a.Cycle)
            .Where(a => a.EmployeeId == employeeId)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    private static AppraisalDto MapToDto(Appraisal a) => new()
    {
        AppraisalId = a.Id,
        EmployeeName = a.Employee?.FullName,
        Department = a.Employee?.Department,
        ManagerName = a.ManagerEmployee?.FullName,
        CycleName = a.Cycle?.Name,
        Status = a.Status,
        CreatedAt = a.CreatedAt,
        FinalisedAt = a.FinalisedAt
    };
}
