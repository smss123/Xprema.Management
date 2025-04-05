using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Xprema.Managment.Application.Contracts.Common;
using Xprema.Managment.Application.Contracts.Tasks;
using Xprema.Managment.Application.Contracts.Tasks.Dtos;
using Xprema.Managment.Domain.ProcedureArea;
using Xprema.Managment.Domain.TaskArea;
using Xprema.Managment.EntityFrameworkCore;

namespace Xprema.Managment.Application.Tasks;

/// <summary>
/// Implementation of IFlowTaskAppService
/// </summary>
public class FlowTaskAppService : IFlowTaskAppService
{
    private readonly ManagmentDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly TaskWorkflowService _workflowService;

    public FlowTaskAppService(
        ManagmentDbContext dbContext, 
        IMapper mapper,
        TaskWorkflowService workflowService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _workflowService = workflowService;
    }

    /// <inheritdoc/>
    public async Task<TaskDto> GetAsync(Guid id)
    {
        var task = await _dbContext.FlowTasks
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (task == null)
        {
            throw new Exception($"Task with id {id} not found");
        }
        
        return _mapper.Map<FlowTask, TaskDto>(task);
    }

    /// <inheritdoc/>
    public async Task<List<TaskDto>> GetListAsync()
    {
        var tasks = await _dbContext.FlowTasks
            .ToListAsync();
            
        return _mapper.Map<List<FlowTask>, List<TaskDto>>(tasks);
    }

    /// <inheritdoc/>
    public async Task<TaskDto> CreateAsync(CreateUpdateTaskDto input)
    {
        var task = _mapper.Map<CreateUpdateTaskDto, FlowTask>(input);
        
        // Set audit fields
        task.Id = Guid.NewGuid();
        task.CreatedDate = DateTime.Now;
        
        await _dbContext.FlowTasks.AddAsync(task);
        
        // Create steps if provided
        if (input.Steps != null && input.Steps.Any())
        {
            foreach (var stepDto in input.Steps)
            {
                var step = _mapper.Map<CreateUpdateTaskStepDto, FlowTaskStep>(stepDto);
                step.Id = Guid.NewGuid();
                step.TaskId = task.Id;
                
                await _dbContext.FlowTaskSteps.AddAsync(step);
            }
        }
        
        // Add participants if provided
        if (input.Participants != null && input.Participants.Any())
        {
            foreach (var participantDto in input.Participants)
            {
                var participant = new TaskParticipant
                {
                    Id = Guid.NewGuid(),
                    TaskId = task.Id,
                    ParticipantId = participantDto.ParticipantId,
                    Role = participantDto.Role,
                    JoinedDate = DateTime.Now,
                    IsActive = true
                };
                
                await _dbContext.TaskParticipants.AddAsync(participant);
            }
        }
        
        // Create initial timeline event
        var timeline = new TaskTimeline
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            EventDate = DateTime.Now,
            EventDescription = "Task created",
            NewStatus = StepType.Initialized,
            EventType = TimelineEventType.TaskCreated,
            CreationTime = DateTime.Now
        };
        
        await _dbContext.TaskTimelines.AddAsync(timeline);
        
        await _dbContext.SaveChangesAsync();
        
        return _mapper.Map<FlowTask, TaskDto>(task);
    }

    /// <inheritdoc/>
    public async Task<TaskDto> UpdateAsync(Guid id, CreateUpdateTaskDto input)
    {
        var task = await _dbContext.FlowTasks
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (task == null)
        {
            throw new Exception($"Task with id {id} not found");
        }
        
        // Update fields
        _mapper.Map(input, task);
        
        // Handle status changes through workflow service if status changed
        if (task.Status != input.Status)
        {
            // We assume current user ID here, in real application this would be from current user
            var currentUserId = Guid.NewGuid(); 
            await _workflowService.UpdateTaskStatusAsync(id, input.Status, currentUserId, "Status updated via API");
        }
        
        await _dbContext.SaveChangesAsync();
        
        return _mapper.Map<FlowTask, TaskDto>(task);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        var task = await _dbContext.FlowTasks
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (task == null)
        {
            throw new Exception($"Task with id {id} not found");
        }
        
        // In real implementation, this might be a soft delete
        _dbContext.FlowTasks.Remove(task);
        
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<TaskDto>> GetPagedListAsync(GetTaskListInput input)
    {
        // Create query
        var query = _dbContext.FlowTasks.AsQueryable();
        
        // Apply filters
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(t => t.TaskName.Contains(input.Filter) || 
                                    (t.Description != null && t.Description.Contains(input.Filter)));
        }
        
        if (input.ProcedureId.HasValue)
        {
            query = query.Where(t => t.ProcedureId == input.ProcedureId.Value);
        }
        
        if (input.AssignedToId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == input.AssignedToId.Value);
        }
        
        if (input.Status.HasValue)
        {
            query = query.Where(t => t.Status == input.Status.Value);
        }
        
        if (input.IsHighPriority.HasValue)
        {
            query = query.Where(t => t.IsHighPriority == input.IsHighPriority.Value);
        }
        
        if (input.DueBefore.HasValue)
        {
            query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value <= input.DueBefore.Value);
        }
        
        if (input.CreatedAfter.HasValue)
        {
            query = query.Where(t => t.CreatedDate >= input.CreatedAfter.Value);
        }
        
        // Get total count
        var totalCount = await query.CountAsync();
        
        // Apply sorting
        if (input.Sorting == "CreatedDate")
        {
            query = query.OrderByDescending(t => t.CreatedDate);
        }
        else if (input.Sorting == "DueDate")
        {
            query = query.OrderBy(t => t.DueDate);
        }
        else if (input.Sorting == "TaskName")
        {
            query = query.OrderBy(t => t.TaskName);
        }
        else if (input.Sorting == "Status")
        {
            query = query.OrderBy(t => t.Status);
        }
        else
        {
            query = query.OrderByDescending(t => t.CreatedDate);
        }
        
        // Apply paging
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
        
        // Include details if requested
        if (input.IncludeDetails)
        {
            query = query.Include(t => t.Steps)
                         .Include(t => t.Timeline)
                         .Include(t => t.Participants);
        }
        
        // Execute query
        var tasks = await query.ToListAsync();
        
        // Map to DTOs
        var dtos = _mapper.Map<List<FlowTask>, List<TaskDto>>(tasks);
        
        return new PagedResultDto<TaskDto>(totalCount, dtos);
    }

    /// <inheritdoc/>
    public async Task<TaskDto> GetWithDetailsAsync(Guid id)
    {
        var task = await _dbContext.FlowTasks
            .Include(t => t.Steps)
            .Include(t => t.Timeline)
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (task == null)
        {
            throw new Exception($"Task with id {id} not found");
        }
        
        return _mapper.Map<FlowTask, TaskDto>(task);
    }

    /// <inheritdoc/>
    public async Task<TaskDto> UpdateStatusAsync(Guid id, StepType status, string? comments = null)
    {
        // We assume current user ID here, in real application this would be from current user
        var currentUserId = Guid.NewGuid();
        
        var task = await _workflowService.UpdateTaskStatusAsync(id, status, currentUserId, comments);
        
        return _mapper.Map<FlowTask, TaskDto>(task);
    }

    /// <inheritdoc/>
    public async Task<TaskDto> AssignAsync(Guid id, Guid assignedToId)
    {
        var task = await _dbContext.FlowTasks
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (task == null)
        {
            throw new Exception($"Task with id {id} not found");
        }
        
        // Record old assignee for timeline
        var oldAssigneeId = task.AssignedToId;
        
        // Update assignee
        task.AssignedToId = assignedToId;
        
        // Record in timeline
        var timeline = new TaskTimeline
        {
            Id = Guid.NewGuid(),
            TaskId = id,
            EventDate = DateTime.Now,
            EventDescription = "Task assigned to new user",
            EventType = TimelineEventType.AssigneeChanged,
            CreationTime = DateTime.Now
        };
        
        await _dbContext.TaskTimelines.AddAsync(timeline);
        
        await _dbContext.SaveChangesAsync();
        
        return _mapper.Map<FlowTask, TaskDto>(task);
    }

    /// <inheritdoc/>
    public async Task<TaskStepDto> AddStepAsync(CreateUpdateTaskStepDto input)
    {
        var task = await _dbContext.FlowTasks
            .FirstOrDefaultAsync(t => t.Id == input.TaskId);
            
        if (task == null)
        {
            throw new Exception($"Task with id {input.TaskId} not found");
        }
        
        var step = _mapper.Map<CreateUpdateTaskStepDto, FlowTaskStep>(input);
        step.Id = Guid.NewGuid();
        
        await _dbContext.FlowTaskSteps.AddAsync(step);
        
        // Record in timeline
        var timeline = new TaskTimeline
        {
            Id = Guid.NewGuid(),
            TaskId = input.TaskId,
            TaskStepId = step.Id,
            EventDate = DateTime.Now,
            EventDescription = $"Added step {step.StepNumber}: {step.StepName}",
            EventType = TimelineEventType.CustomEvent,
            CreationTime = DateTime.Now
        };
        
        await _dbContext.TaskTimelines.AddAsync(timeline);
        
        await _dbContext.SaveChangesAsync();
        
        return _mapper.Map<FlowTaskStep, TaskStepDto>(step);
    }

    /// <inheritdoc/>
    public async Task<TaskParticipantDto> AddParticipantAsync(Guid taskId, Guid participantId, string? role = null)
    {
        var task = await _dbContext.FlowTasks
            .FirstOrDefaultAsync(t => t.Id == taskId);
            
        if (task == null)
        {
            throw new Exception($"Task with id {taskId} not found");
        }
        
        var participant = new TaskParticipant
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ParticipantId = participantId,
            Role = role,
            JoinedDate = DateTime.Now,
            IsActive = true
        };
        
        await _dbContext.TaskParticipants.AddAsync(participant);
        
        // Record in timeline
        var timeline = new TaskTimeline
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            EventDate = DateTime.Now,
            EventDescription = $"Added participant to task",
            EventType = TimelineEventType.CustomEvent,
            CreationTime = DateTime.Now
        };
        
        await _dbContext.TaskTimelines.AddAsync(timeline);
        
        await _dbContext.SaveChangesAsync();
        
        return _mapper.Map<TaskParticipant, TaskParticipantDto>(participant);
    }

    /// <inheritdoc/>
    public async Task<List<TaskDto>> GetByProcedureAsync(Guid procedureId)
    {
        var tasks = await _dbContext.FlowTasks
            .Where(t => t.ProcedureId == procedureId)
            .ToListAsync();
            
        return _mapper.Map<List<FlowTask>, List<TaskDto>>(tasks);
    }

    /// <inheritdoc/>
    public async Task<List<TaskDto>> GetByAssignedToAsync(Guid assignedToId)
    {
        var tasks = await _dbContext.FlowTasks
            .Where(t => t.AssignedToId == assignedToId)
            .ToListAsync();
            
        return _mapper.Map<List<FlowTask>, List<TaskDto>>(tasks);
    }
} 