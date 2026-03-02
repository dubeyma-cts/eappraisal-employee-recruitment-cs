using Microsoft.EntityFrameworkCore;
using eAppraisal.Application.Contracts;
using eAppraisal.Domain.DTOs;
using eAppraisal.Domain.Entities;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Application.Services;

public class CycleService : ICycleService
{
    private readonly IAppDbContext _db;

    public CycleService(IAppDbContext db) => _db = db;

    public async Task<List<CycleDto>> GetAllAsync()
    {
        return await _db.AppraisalCycles.Select(c => new CycleDto
        {
            Id = c.Id,
            Name = c.Name,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            State = c.State
        }).ToListAsync();
    }

    public async Task<List<CycleDto>> GetOpenAsync()
    {
        return await _db.AppraisalCycles
            .Where(c => c.State == "Open")
            .Select(c => new CycleDto
            {
                Id = c.Id,
                Name = c.Name,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                State = c.State
            }).ToListAsync();
    }

    public async Task<CycleDto> CreateAsync(CycleDto dto)
    {
        var entity = new AppraisalCycle
        {
            Name = dto.Name,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            State = dto.State
        };
        _db.AppraisalCycles.Add(entity);
        await _db.SaveChangesAsync();
        return new CycleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            State = entity.State
        };
    }

    public async Task<CycleDto?> GetByIdAsync(int id)
    {
        var c = await _db.AppraisalCycles.FindAsync(id);
        if (c == null) return null;
        return new CycleDto
        {
            Id = c.Id,
            Name = c.Name,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            State = c.State
        };
    }

    public async Task<CycleDto?> UpdateAsync(CycleDto dto)
    {
        var entity = await _db.AppraisalCycles.FindAsync(dto.Id);
        if (entity == null) return null;

        entity.Name = dto.Name;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.State = dto.State;

        await _db.SaveChangesAsync();
        return new CycleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            State = entity.State
        };
    }

    public async Task<List<EmployeeDto>> GetUnassignedEmployeesAsync(List<int> openCycleIds)
    {
        var assignedEmployeeIds = await _db.Appraisals
            .Where(a => openCycleIds.Contains(a.CycleId))
            .Select(a => a.EmployeeId)
            .Distinct()
            .ToListAsync();

        return await _db.Employees
            .Include(e => e.Manager)
            .Where(e => !assignedEmployeeIds.Contains(e.Id))
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Department = e.Department,
                ManagerId = e.ManagerId,
                ManagerName = e.Manager != null ? e.Manager.FirstName + " " + e.Manager.LastName : null
            }).ToListAsync();
    }
}
