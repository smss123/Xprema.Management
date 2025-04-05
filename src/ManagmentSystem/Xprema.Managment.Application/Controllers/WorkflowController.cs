using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Xprema.Managment.Application.Contracts.Tasks;
using Xprema.Managment.Application.Contracts.Tasks.Dtos;
using Xprema.Managment.Application.Tasks;

namespace Xprema.Managment.Application.Controllers;

[ApiController]
[Route("api/workflow")]
public class WorkflowController : ControllerBase
{
    private readonly TaskWorkflowService _workflowService;
    private readonly IFlowTaskAppService _taskAppService;
    private readonly IMapper _mapper;

    public WorkflowController(
        TaskWorkflowService workflowService, 
        IFlowTaskAppService taskAppService,
        IMapper mapper)
    {
        _workflowService = workflowService;
        _taskAppService = taskAppService;
        _mapper = mapper;
    }

    /// <summary>
    /// Advances a task to its next step in the workflow
    /// </summary>
    [HttpPost("tasks/{taskId}/advance")]
    public async Task<ActionResult<TaskDto>> AdvanceToNextStepAsync(Guid taskId, [FromBody] WorkflowActionRequest request)
    {
        try
        {
            // In a real application, you would get the user ID from the authenticated user
            var userId = request.UserId ?? Guid.NewGuid();
            
            var task = await _workflowService.AdvanceToNextStepAsync(taskId, userId, request.Comments);
            return Ok(_mapper.Map<TaskDto>(task));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Completes a task step
    /// </summary>
    [HttpPost("tasks/steps/{stepId}/complete")]
    public async Task<ActionResult<TaskStepDto>> CompleteStepAsync(Guid stepId, [FromBody] WorkflowActionRequest request)
    {
        try
        {
            // In a real application, you would get the user ID from the authenticated user
            var userId = request.UserId ?? Guid.NewGuid();
            
            var step = await _workflowService.CompleteStepAsync(stepId, userId, request.Comments);
            return Ok(_mapper.Map<TaskStepDto>(step));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

/// <summary>
/// Request for workflow actions
/// </summary>
public class WorkflowActionRequest
{
    /// <summary>
    /// Optional user ID performing the action (for demo purposes, in real app this would come from auth)
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Optional comments for the action
    /// </summary>
    public string? Comments { get; set; }
} 