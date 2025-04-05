using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xprema.Managment.Application.Contracts.Common;
using Xprema.Managment.Application.Contracts.Tasks.Dtos;
using Xprema.Managment.Domain.ProcedureArea;

namespace Xprema.Managment.Application.Contracts.Tasks;

/// <summary>
/// Service interface for FlowTask operations
/// </summary>
public interface IFlowTaskAppService
{
    /// <summary>
    /// Gets a task by id
    /// </summary>
    Task<TaskDto> GetAsync(Guid id);
    
    /// <summary>
    /// Gets a list of all tasks
    /// </summary>
    Task<List<TaskDto>> GetListAsync();
    
    /// <summary>
    /// Creates a new task
    /// </summary>
    Task<TaskDto> CreateAsync(CreateUpdateTaskDto input);
    
    /// <summary>
    /// Updates an existing task
    /// </summary>
    Task<TaskDto> UpdateAsync(Guid id, CreateUpdateTaskDto input);
    
    /// <summary>
    /// Deletes a task
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Gets tasks with paging and filtering
    /// </summary>
    Task<PagedResultDto<TaskDto>> GetPagedListAsync(GetTaskListInput input);
    
    /// <summary>
    /// Gets detailed task with steps and timeline
    /// </summary>
    Task<TaskDto> GetWithDetailsAsync(Guid id);
    
    /// <summary>
    /// Updates task status
    /// </summary>
    Task<TaskDto> UpdateStatusAsync(Guid id, StepType status, string? comments = null);
    
    /// <summary>
    /// Assigns task to a user
    /// </summary>
    Task<TaskDto> AssignAsync(Guid id, Guid assignedToId);
    
    /// <summary>
    /// Adds a step to a task
    /// </summary>
    Task<TaskStepDto> AddStepAsync(CreateUpdateTaskStepDto input);
    
    /// <summary>
    /// Adds a participant to a task
    /// </summary>
    Task<TaskParticipantDto> AddParticipantAsync(Guid taskId, Guid participantId, string? role = null);
    
    /// <summary>
    /// Gets tasks by procedure id
    /// </summary>
    Task<List<TaskDto>> GetByProcedureAsync(Guid procedureId);
    
    /// <summary>
    /// Gets tasks by assigned user id
    /// </summary>
    Task<List<TaskDto>> GetByAssignedToAsync(Guid assignedToId);
} 