using Microsoft.EntityFrameworkCore;
using eAppraisal.Application.Contracts;
using eAppraisal.Domain.DTOs;
using eAppraisal.Domain.Entities;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IAppDbContext _db;

    public EmployeeService(IAppDbContext db) => _db = db;

    public async Task<List<EmployeeDto>> GetAllAsync()
    {
        return await _db.Employees
            .Include(e => e.Manager)
            .Select(e => MapToDto(e))
            .ToListAsync();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var emp = await _db.Employees.Include(e => e.Manager).FirstOrDefaultAsync(e => e.Id == id);
        return emp == null ? null : MapToDto(emp);
    }

    public async Task<EmployeeDto> CreateAsync(EmployeeDto dto)
    {
        var entity = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Address = dto.Address,
            City = dto.City,
            PersonalPhone = dto.PersonalPhone,
            MobileNo = dto.MobileNo,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            MaritalStatus = dto.MaritalStatus,
            DateOfJoining = dto.DateOfJoining,
            PassportNo = dto.PassportNo,
            PanNo = dto.PanNo,
            WorkExperience = dto.WorkExperience,
            Department = dto.Department,
            ManagerId = dto.ManagerId
        };
        _db.Employees.Add(entity);
        await _db.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<EmployeeDto> UpdateAsync(EmployeeDto dto)
    {
        var entity = await _db.Employees.FindAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Employee {dto.Id} not found");
        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.PersonalPhone = dto.PersonalPhone;
        entity.MobileNo = dto.MobileNo;
        entity.Email = dto.Email;
        entity.DateOfBirth = dto.DateOfBirth;
        entity.Gender = dto.Gender;
        entity.MaritalStatus = dto.MaritalStatus;
        entity.DateOfJoining = dto.DateOfJoining;
        entity.PassportNo = dto.PassportNo;
        entity.PanNo = dto.PanNo;
        entity.WorkExperience = dto.WorkExperience;
        entity.Department = dto.Department;
        entity.ManagerId = dto.ManagerId;
        await _db.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<EmployeeDto> UpdateProfileAsync(int employeeId, EmployeeDto dto)
    {
        var entity = await _db.Employees.FindAsync(employeeId)
            ?? throw new KeyNotFoundException($"Employee {employeeId} not found");
        // Employee self-edit: personal fields only (excludes Department, ManagerId)
        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.PersonalPhone = dto.PersonalPhone;
        entity.MobileNo = dto.MobileNo;
        entity.DateOfBirth = dto.DateOfBirth;
        entity.Gender = dto.Gender;
        entity.MaritalStatus = dto.MaritalStatus;
        entity.PassportNo = dto.PassportNo;
        entity.PanNo = dto.PanNo;
        await _db.SaveChangesAsync();
        return MapToDto(entity);
    }

    private static EmployeeDto MapToDto(Employee e) => new()
    {
        Id = e.Id,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Address = e.Address,
        City = e.City,
        PersonalPhone = e.PersonalPhone,
        MobileNo = e.MobileNo,
        Email = e.Email,
        DateOfBirth = e.DateOfBirth,
        Gender = e.Gender,
        MaritalStatus = e.MaritalStatus,
        DateOfJoining = e.DateOfJoining,
        PassportNo = e.PassportNo,
        PanNo = e.PanNo,
        WorkExperience = e.WorkExperience,
        Department = e.Department,
        ManagerId = e.ManagerId,
        ManagerName = e.Manager?.FullName
    };
}
