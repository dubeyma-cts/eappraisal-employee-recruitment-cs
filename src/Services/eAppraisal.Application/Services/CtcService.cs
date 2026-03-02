using Microsoft.EntityFrameworkCore;
using eAppraisal.Application.Contracts;
using eAppraisal.Domain.DTOs;
using eAppraisal.Domain.Entities;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Application.Services;

public class CtcService : ICtcService
{
    private readonly IAppDbContext _db;

    public CtcService(IAppDbContext db) => _db = db;

    public async Task CreateOrUpdateSnapshotAsync(int appraisalId, FinalizeAppraisalDto dto)
    {
        var appraisal = await _db.Appraisals
            .Include(a => a.CtcSnapshot)
            .FirstOrDefaultAsync(a => a.Id == appraisalId)
            ?? throw new KeyNotFoundException($"Appraisal {appraisalId} not found");

        if (appraisal.CtcSnapshot == null)
        {
            appraisal.CtcSnapshot = new CtcSnapshot
            {
                AppraisalId = appraisalId,
                IsPromoted = dto.IsPromoted,
                Basic = dto.Basic,
                DA = dto.DA,
                HRA = dto.HRA,
                FoodAllowance = dto.FoodAllowance,
                PF = dto.PF,
                NextAppraisalDate = dto.NextAppraisalDate,
                ApprovedAt = DateTime.UtcNow
            };
        }
        else
        {
            appraisal.CtcSnapshot.IsPromoted = dto.IsPromoted;
            appraisal.CtcSnapshot.Basic = dto.Basic;
            appraisal.CtcSnapshot.DA = dto.DA;
            appraisal.CtcSnapshot.HRA = dto.HRA;
            appraisal.CtcSnapshot.FoodAllowance = dto.FoodAllowance;
            appraisal.CtcSnapshot.PF = dto.PF;
            appraisal.CtcSnapshot.NextAppraisalDate = dto.NextAppraisalDate;
            appraisal.CtcSnapshot.ApprovedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }
}
