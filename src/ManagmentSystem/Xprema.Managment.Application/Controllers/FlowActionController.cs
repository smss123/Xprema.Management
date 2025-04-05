using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xprema.Managment.Domain.ActionArea;
using Xprema.Managment.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Xprema.Managment.Application.Controllers;

[ApiController]
[Route("api/actions")]
public class FlowActionController : ControllerBase
{
    private readonly ManagmentDbContext _dbContext;

    public FlowActionController(ManagmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets an action by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FlowAction>> GetAsync(Guid id)
    {
        var action = await _dbContext.FlowActions.FindAsync(id);
        if (action == null)
        {
            return NotFound($"Action with id {id} not found");
        }

        return Ok(action);
    }

    /// <summary>
    /// Gets a list of all actions
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<FlowAction>>> GetListAsync()
    {
        var actions = await _dbContext.FlowActions.ToListAsync();
        return Ok(actions);
    }

    /// <summary>
    /// Creates a new action
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FlowAction>> CreateAsync([FromBody] CreateUpdateFlowActionDto input)
    {
        var action = new FlowAction
        {
            Id = Guid.NewGuid(),
            ActionName = input.ActionName,
            Description = input.Description
        };

        await _dbContext.FlowActions.AddAsync(action);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAsync), new { id = action.Id }, action);
    }

    /// <summary>
    /// Updates an existing action
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<FlowAction>> UpdateAsync(Guid id, [FromBody] CreateUpdateFlowActionDto input)
    {
        var action = await _dbContext.FlowActions.FindAsync(id);
        if (action == null)
        {
            return NotFound($"Action with id {id} not found");
        }

        action.ActionName = input.ActionName;
        action.Description = input.Description;

        await _dbContext.SaveChangesAsync();

        return Ok(action);
    }

    /// <summary>
    /// Deletes an action
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        var action = await _dbContext.FlowActions.FindAsync(id);
        if (action == null)
        {
            return NotFound($"Action with id {id} not found");
        }

        _dbContext.FlowActions.Remove(action);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}

/// <summary>
/// DTO for creating or updating a FlowAction
/// </summary>
public class CreateUpdateFlowActionDto
{
    /// <summary>
    /// Name of the action
    /// </summary>
    public required string ActionName { get; set; }
    
    /// <summary>
    /// Optional description of the action
    /// </summary>
    public string? Description { get; set; }
} 