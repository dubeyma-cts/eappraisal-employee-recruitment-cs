namespace eAppraisal.Domain.Interfaces;

public interface IHrisPayrollService
{
    Task<object?> GetEmployeePayrollAsync(int employeeId);
    Task<bool> SyncEmployeeAsync(int employeeId, string fullName, string email, string department);
}
