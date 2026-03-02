namespace eAppraisal.Domain.Interfaces;

public interface IFeatureFlagService
{
    bool IsEnabled(string featureName);
    string? GetValue(string featureName);
}
