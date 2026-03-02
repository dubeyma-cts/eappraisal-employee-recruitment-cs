using eAppraisal.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace eAppraisal.Infrastructure.ExternalServices;

public class StubHrisPayrollService : IHrisPayrollService
{
    private readonly ILogger<StubHrisPayrollService> _logger;

    public StubHrisPayrollService(ILogger<StubHrisPayrollService> logger) => _logger = logger;

    public Task<object?> GetEmployeePayrollAsync(int employeeId)
    {
        _logger.LogInformation("[HRIS Stub] GetEmployeePayroll for EmployeeId: {EmployeeId}", employeeId);
        var mockPayroll = new
        {
            EmployeeId = employeeId,
            PayGrade = "L3",
            BasicSalary = 50000m,
            LastRevisedDate = DateTime.UtcNow.AddMonths(-6),
            Source = "StubHrisPayrollService"
        };
        return Task.FromResult<object?>(mockPayroll);
    }

    public Task<bool> SyncEmployeeAsync(int employeeId, string fullName, string email, string department)
    {
        _logger.LogInformation("[HRIS Stub] SyncEmployee: {EmployeeId} - {FullName} ({Email}, {Department})", employeeId, fullName, email, department);
        return Task.FromResult(true);
    }
}
