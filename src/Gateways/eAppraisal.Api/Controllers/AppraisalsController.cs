using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eAppraisal.Domain.DTOs;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppraisalsController : ControllerBase
{
    private readonly IAppraisalWorkflowService _workflow;
    private readonly ICommentsService _comments;
    private readonly IReportingService _reporting;

    public AppraisalsController(
        IAppraisalWorkflowService workflow,
        ICommentsService comments,
        IReportingService reporting)
    {
        _workflow = workflow;
        _comments = comments;
        _reporting = reporting;
    }

    [HttpGet("by-manager/{managerEmpId}")]
    public async Task<IActionResult> GetByManager(int managerEmpId)
    {
        return Ok(await _reporting.GetByManagerAsync(managerEmpId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var detail = await _reporting.GetDetailAsync(id);
        if (detail == null) return NotFound();
        return Ok(detail);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignRequest req)
    {
        var result = await _workflow.AssignAsync(req.EmployeeId, req.CycleId, User.Identity?.Name ?? "system");
        return Ok(result);
    }

    [HttpPost("comment")]
    public async Task<IActionResult> SaveComment([FromBody] ManagerCommentDto dto)
    {
        await _comments.SaveCommentAsync(dto, User.Identity?.Name ?? "system");
        return Ok();
    }

    [HttpPost("feedback")]
    public async Task<IActionResult> SaveFeedback([FromBody] EmployeeFeedbackDto dto)
    {
        await _comments.SaveFeedbackAsync(dto, User.Identity?.Name ?? "system");
        return Ok();
    }

    [HttpPost("finalize")]
    public async Task<IActionResult> Finalize([FromBody] FinalizeAppraisalDto dto)
    {
        await _workflow.FinalizeAsync(dto, User.Identity?.Name ?? "system");
        return Ok();
    }

    [HttpGet("by-status")]
    public async Task<IActionResult> GetByStatus([FromQuery] string status, [FromQuery] string? role, [FromQuery] int? employeeId)
    {
        return Ok(await _reporting.GetByStatusAsync(status, role, employeeId));
    }

    [HttpGet("by-employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        return Ok(await _reporting.GetByEmployeeAsync(employeeId));
    }
}

public class AssignRequest
{
    public int EmployeeId { get; set; }
    public int CycleId { get; set; }
}
