using eAppraisal.Domain.DTOs;

namespace eAppraisal.Domain.Interfaces;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync();
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto> CreateAsync(EmployeeDto dto);
    Task<EmployeeDto> UpdateAsync(EmployeeDto dto);

    /// <summary>
    /// Employee self-edit: allows updating personal fields only (excludes Department and Manager).
    /// </summary>
    Task<EmployeeDto> UpdateProfileAsync(int employeeId, EmployeeDto dto);
}
