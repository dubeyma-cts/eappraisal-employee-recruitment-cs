using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eAppraisal.Domain.DTOs;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CyclesController : ControllerBase
{
    private readonly ICycleService _cycles;

    public CyclesController(ICycleService cycles) => _cycles = cycles;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _cycles.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _cycles.GetByIdAsync(id);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("open")]
    public async Task<IActionResult> GetOpen() => Ok(await _cycles.GetOpenAsync());

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CycleDto dto)
    {
        dto.Id = id;
        var result = await _cycles.UpdateAsync(dto);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CycleDto dto)
    {
        var result = await _cycles.CreateAsync(dto);
        return Ok(result);
    }

    [HttpPost("unassigned-employees")]
    public async Task<IActionResult> GetUnassignedEmployees([FromBody] List<int> openCycleIds)
    {
        return Ok(await _cycles.GetUnassignedEmployeesAsync(openCycleIds));
    }
}
