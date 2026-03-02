namespace eAppraisal.Domain.Interfaces;

public interface IPolicyMaskingEngine
{
    string? MaskPan(string? pan);
    bool CanViewFullPan(string? role);
    string? ApplyMasking(string? pan, string? role);
}
