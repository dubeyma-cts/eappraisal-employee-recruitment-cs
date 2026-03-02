using eAppraisal.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace eAppraisal.Infrastructure.CrossCutting;

public class InMemoryFeatureFlagService : IFeatureFlagService
{
    private readonly IConfiguration _configuration;

    public InMemoryFeatureFlagService(IConfiguration configuration) => _configuration = configuration;

    public bool IsEnabled(string featureName)
    {
        var value = _configuration[$"FeatureFlags:{featureName}"];
        return bool.TryParse(value, out var result) && result;
    }

    public string? GetValue(string featureName)
    {
        return _configuration[$"FeatureFlags:{featureName}"];
    }
}
