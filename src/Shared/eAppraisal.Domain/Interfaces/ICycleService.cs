using eAppraisal.Domain.DTOs;

namespace eAppraisal.Domain.Interfaces;

public interface ICycleService
{
    Task<List<CycleDto>> GetAllAsync();
    Task<List<CycleDto>> GetOpenAsync();
    Task<CycleDto> CreateAsync(CycleDto dto);
    Task<CycleDto?> GetByIdAsync(int id);
    Task<CycleDto?> UpdateAsync(CycleDto dto);
    Task<List<EmployeeDto>> GetUnassignedEmployeesAsync(List<int> openCycleIds);
}
