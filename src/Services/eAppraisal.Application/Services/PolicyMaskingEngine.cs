using eAppraisal.Domain.Common;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Application.Services;

public class PolicyMaskingEngine : IPolicyMaskingEngine
{
    public string? MaskPan(string? pan) => PanMaskingService.Mask(pan);

    public bool CanViewFullPan(string? role) => PanMaskingService.CanViewFullPan(role);

    public string? ApplyMasking(string? pan, string? role) => PanMaskingService.ForRole(pan, role);
}
