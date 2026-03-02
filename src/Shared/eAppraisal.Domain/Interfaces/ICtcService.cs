using eAppraisal.Domain.DTOs;

namespace eAppraisal.Domain.Interfaces;

public interface ICtcService
{
    Task CreateOrUpdateSnapshotAsync(int appraisalId, FinalizeAppraisalDto dto);
}
