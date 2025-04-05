using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xprema.Managment.Application.Contracts.Common;
using Xprema.Managment.Application.Contracts.Tasks;
using Xprema.Managment.Application.Contracts.Tasks.Dtos;
using Xprema.Managment.Domain.ProcedureArea;

namespace Xprema.Managment.Application.Controllers;

[ApiController]
[Route("api/tasks")]
public class FlowTaskController : ControllerBase
{
    private readonly IFlowTaskAppService _taskAppService;

    public FlowTaskController(IFlowTaskAppService taskAppService)
    {
        _taskAppService = taskAppService;
    }

    /// <summary>
    /// Gets a task by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetAsync(Guid id)
    {
        try
        {
            var task = await _taskAppService.GetAsync(id);
            return Ok(task);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Gets a list of all tasks
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<TaskDto>>> GetListAsync()
    {
        var tasks = await _taskAppService.GetListAsync();
        return Ok(tasks);
    }

    /// <summary>
    /// Gets detailed task with steps and timeline
    /// </summary>
    [HttpGet("{id}/details")]
    public async Task<ActionResult<TaskDto>> GetWithDetailsAsync(Guid id)
    {
        try
        {
            var task = await _taskAppService.GetWithDetailsAsync(id);
            return Ok(task);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Gets tasks with paging and filtering
    /// </summary>
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResultDto<TaskDto>>> GetPagedListAsync([FromQuery] GetTaskListInput input)
    {
        var tasks = await _taskAppService.GetPagedListAsync(input);
        return Ok(tasks);
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateAsync([FromBody] CreateUpdateTaskDto input)
    {
        try
        {
            var task = await _taskAppService.CreateAsync(input);
            return CreatedAtAction(nameof(GetAsync), new { id = task.Id }, task);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing task
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TaskDto>> UpdateAsync(Guid id, [FromBody] CreateUpdateTaskDto input)
    {
        try
        {
            var task = await _taskAppService.UpdateAsync(id, input);
            return Ok(task);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a task
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        try
        {
            await _taskAppService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Updates task status
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TaskDto>> UpdateStatusAsync(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        try
        {
            var task = await _taskAppService.UpdateStatusAsync(id, request.Status, request.Comments);
            return Ok(task);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Assigns task to a user
    /// </summary>
    [HttpPatch("{id}/assign")]
    public async Task<ActionResult<TaskDto>> AssignAsync(Guid id, [FromBody] AssignTaskRequest request)
    {
        try
        {
            var task = await _taskAppService.AssignAsync(id, request.AssignedToId);
            return Ok(task);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Adds a step to a task
    /// </summary>
    [HttpPost("{id}/steps")]
    public async Task<ActionResult<TaskStepDto>> AddStepAsync(Guid id, [FromBody] CreateUpdateTaskStepDto input)
    {
        if (id != input.TaskId)
        {
            return BadRequest("Task ID in URL must match TaskId in request body");
        }

        try
        {
            var step = await _taskAppService.AddStepAsync(input);
            return CreatedAtAction(nameof(GetWithDetailsAsync), new { id = input.TaskId }, step);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Adds a participant to a task
    /// </summary>
    [HttpPost("{id}/participants")]
    public async Task<ActionResult<TaskParticipantDto>> AddParticipantAsync(Guid id, [FromBody] AddParticipantRequest request)
    {
        try
        {
            var participant = await _taskAppService.AddParticipantAsync(id, request.ParticipantId, request.Role);
            return CreatedAtAction(nameof(GetWithDetailsAsync), new { id = id }, participant);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets tasks by procedure id
    /// </summary>
    [HttpGet("by-procedure/{procedureId}")]
    public async Task<ActionResult<List<TaskDto>>> GetByProcedureAsync(Guid procedureId)
    {
        var tasks = await _taskAppService.GetByProcedureAsync(procedureId);
        return Ok(tasks);
    }

    /// <summary>
    /// Gets tasks by assigned user id
    /// </summary>
    [HttpGet("by-assignee/{assignedToId}")]
    public async Task<ActionResult<List<TaskDto>>> GetByAssignedToAsync(Guid assignedToId)
    {
        var tasks = await _taskAppService.GetByAssignedToAsync(assignedToId);
        return Ok(tasks);
    }
}

/// <summary>
/// Request for updating task status
/// </summary>
public class UpdateTaskStatusRequest
{
    /// <summary>
    /// New status for the task
    /// </summary>
    public StepType Status { get; set; }
    
    /// <summary>
    /// Optional comments for the status change
    /// </summary>
    public string? Comments { get; set; }
}

/// <summary>
/// Request for assigning a task to a user
/// </summary>
public class AssignTaskRequest
{
    /// <summary>
    /// User ID to assign the task to
    /// </summary>
    public Guid AssignedToId { get; set; }
}

/// <summary>
/// Request for adding a participant to a task
/// </summary>
public class AddParticipantRequest
{
    /// <summary>
    /// Participant user ID
    /// </summary>
    public Guid ParticipantId { get; set; }
    
    /// <summary>
    /// Optional role for the participant
    /// </summary>
    public string? Role { get; set; }
} 