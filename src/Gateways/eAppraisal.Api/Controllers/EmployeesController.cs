using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eAppraisal.Domain.DTOs;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employees;
    private readonly IPolicyMaskingEngine _masking;

    public EmployeesController(IEmployeeService employees, IPolicyMaskingEngine masking)
    {
        _employees = employees;
        _masking = masking;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _employees.GetAllAsync();
        var role = GetUserRole();
        foreach (var e in list)
            e.PanNo = _masking.ApplyMasking(e.PanNo, role);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var emp = await _employees.GetByIdAsync(id);
        if (emp == null) return NotFound();
        emp.PanNo = _masking.ApplyMasking(emp.PanNo, GetUserRole());
        return Ok(emp);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeDto dto)
    {
        var result = await _employees.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeDto dto)
    {
        dto.Id = id;
        var result = await _employees.UpdateAsync(dto);
        return Ok(result);
    }

    [HttpPut("{id}/profile")]
    public async Task<IActionResult> UpdateProfile(int id, [FromBody] EmployeeDto dto)
    {
        var result = await _employees.UpdateProfileAsync(id, dto);
        return Ok(result);
    }

    private string? GetUserRole()
    {
        return User.Claims.FirstOrDefault(c => c.Type == "AppRole")?.Value;
    }
}
