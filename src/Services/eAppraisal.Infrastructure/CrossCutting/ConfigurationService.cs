using eAppraisal.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace eAppraisal.Infrastructure.CrossCutting;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;

    public ConfigurationService(IConfiguration configuration) => _configuration = configuration;

    public string? GetSetting(string key) => _configuration[key];

    public string GetConnectionString(string name) => _configuration.GetConnectionString(name) ?? string.Empty;

    public T GetSection<T>(string sectionName) where T : class, new()
    {
        var section = new T();
        _configuration.GetSection(sectionName).Bind(section);
        return section;
    }
}
