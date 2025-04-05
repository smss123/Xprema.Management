using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xprema.Managment.Domain.ProcedureArea;
using Xprema.Managment.Domain.TaskArea;
using Xprema.Managment.EntityFrameworkCore;

namespace Xprema.Managment.Application.Tasks;

/// <summary>
/// Service for managing task workflow transitions
/// </summary>
public class TaskWorkflowService
{
    private readonly ManagmentDbContext _dbContext;

    public TaskWorkflowService(ManagmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Advances a task to the next step in its workflow
    /// </summary>
    public async Task<FlowTask> AdvanceToNextStepAsync(Guid taskId, Guid userId, string? comments = null)
    {
        var task = await _dbContext.FlowTasks
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == taskId);
            
        if (task == null)
        {
            throw new Exception($"Task with id {taskId} not found");
        }
        
        // Get current step
        var currentStep = task.Steps.FirstOrDefault(s => s.StepNumber == task.CurrentStepNumber);
        if (currentStep == null)
        {
            throw new Exception($"Current step not found for task {taskId}");
        }
        
        // Check if current step is completed
        if (currentStep.Status != StepType.Completed)
        {
            throw new Exception("Current step must be completed before advancing to next step");
        }
        
        // Get next step
        var nextStep = task.Steps.FirstOrDefault(s => s.StepNumber == task.CurrentStepNumber + 1);
        if (nextStep == null)
        {
            // No more steps, mark task as completed
            task.Status = StepType.Completed;
            task.CompletedDate = DateTime.Now;
            
            // Record in timeline
            await AddTimelineEventAsync(task.Id, userId, TimelineEventType.TaskCompleted, 
                "Task completed", task.Status, StepType.Completed, comments);
                
            await _dbContext.SaveChangesAsync();
            return task;
        }
        
        // Advance to next step
        task.CurrentStepNumber++;
        nextStep.Status = StepType.OnProgress;
        nextStep.StartDate = DateTime.Now;
        
        // If first step, mark task as started
        if (task.Status == StepType.Initialized)
        {
            task.Status = StepType.OnProgress;
            task.StartDate = DateTime.Now;
            
            // Record in timeline
            await AddTimelineEventAsync(task.Id, userId, TimelineEventType.TaskStarted, 
                "Task started", StepType.Initialized, StepType.OnProgress, comments);
        }
        
        // Record step transition in timeline
        await AddTimelineEventAsync(task.Id, userId, TimelineEventType.StepStarted, 
            $"Started step {nextStep.StepNumber}: {nextStep.StepName}", 
            null, StepType.OnProgress, comments, nextStep.Id);
            
        await _dbContext.SaveChangesAsync();
        
        return task;
    }

    /// <summary>
    /// Completes a task step
    /// </summary>
    public async Task<FlowTaskStep> CompleteStepAsync(Guid taskStepId, Guid userId, string? comments = null)
    {
        var step = await _dbContext.FlowTaskSteps
            .Include(s => s.Task)
            .FirstOrDefaultAsync(s => s.Id == taskStepId);
            
        if (step == null)
        {
            throw new Exception($"Task step with id {taskStepId} not found");
        }
        
        // Check if step is in progress
        if (step.Status != StepType.OnProgress)
        {
            throw new Exception("Step must be in progress before it can be completed");
        }
        
        // Mark step as completed
        step.Status = StepType.Completed;
        step.CompletedDate = DateTime.Now;
        
        // Record in timeline
        await AddTimelineEventAsync(step.Task.Id, userId, TimelineEventType.StepCompleted, 
            $"Completed step {step.StepNumber}: {step.StepName}", 
            StepType.OnProgress, StepType.Completed, comments, step.Id);
            
        await _dbContext.SaveChangesAsync();
        
        return step;
    }

    /// <summary>
    /// Updates task status
    /// </summary>
    public async Task<FlowTask> UpdateTaskStatusAsync(Guid taskId, StepType newStatus, Guid userId, string? comments = null)
    {
        var task = await _dbContext.FlowTasks
            .FirstOrDefaultAsync(t => t.Id == taskId);
            
        if (task == null)
        {
            throw new Exception($"Task with id {taskId} not found");
        }
        
        // Check if status is actually changing
        if (task.Status == newStatus)
        {
            return task;
        }
        
        // Handle special statuses
        var oldStatus = task.Status;
        task.Status = newStatus;
        
        if (newStatus == StepType.Completed && !task.CompletedDate.HasValue)
        {
            task.CompletedDate = DateTime.Now;
        }
        else if (newStatus == StepType.OnProgress && !task.StartDate.HasValue)
        {
            task.StartDate = DateTime.Now;
        }
        
        // Record in timeline
        await AddTimelineEventAsync(task.Id, userId, TimelineEventType.StatusChanged, 
            $"Status changed from {oldStatus} to {newStatus}", 
            oldStatus, newStatus, comments);
            
        await _dbContext.SaveChangesAsync();
        
        return task;
    }

    /// <summary>
    /// Add timeline event
    /// </summary>
    private async Task<TaskTimeline> AddTimelineEventAsync(
        Guid taskId, 
        Guid userId,
        TimelineEventType eventType, 
        string description, 
        StepType? oldStatus = null, 
        StepType? newStatus = null, 
        string? comments = null,
        Guid? taskStepId = null)
    {
        var timeline = new TaskTimeline
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            TaskStepId = taskStepId,
            EventDate = DateTime.Now,
            EventDescription = description,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedById = userId,
            Comments = comments,
            EventType = eventType,
            CreationTime = DateTime.Now,
            CreatorId = userId
        };
        
        await _dbContext.TaskTimelines.AddAsync(timeline);
        
        return timeline;
    }
} 