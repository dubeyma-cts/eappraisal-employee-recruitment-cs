namespace eAppraisal.Domain.Interfaces;

public interface IConfigurationService
{
    string? GetSetting(string key);
    string GetConnectionString(string name);
    T GetSection<T>(string sectionName) where T : class, new();
}
