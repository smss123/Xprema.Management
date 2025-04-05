using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xprema.Managment.Application.Contracts.Procedures;
using Xprema.Managment.Application.Contracts.Procedures.Dtos;

namespace Xprema.Managment.Application.Controllers;

[ApiController]
[Route("api/procedure-composes")]
public class FlowProcedureComposeController : ControllerBase
{
    private readonly IFlowProcedureComposeAppService _composeAppService;

    public FlowProcedureComposeController(IFlowProcedureComposeAppService composeAppService)
    {
        _composeAppService = composeAppService;
    }

    /// <summary>
    /// Gets a procedure compose by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FlowProcedureComposeDto>> GetAsync(Guid id)
    {
        try
        {
            var compose = await _composeAppService.GetAsync(id);
            return Ok(compose);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Gets a list of all procedure composes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<FlowProcedureComposeDto>>> GetListAsync()
    {
        var composes = await _composeAppService.GetListAsync();
        return Ok(composes);
    }

    /// <summary>
    /// Gets detailed procedure compose with steps
    /// </summary>
    [HttpGet("{id}/details")]
    public async Task<ActionResult<FlowProcedureComposeDto>> GetWithDetailsAsync(Guid id)
    {
        try
        {
            var compose = await _composeAppService.GetWithDetailsAsync(id);
            return Ok(compose);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new procedure compose
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FlowProcedureComposeDto>> CreateAsync([FromBody] CreateUpdateFlowProcedureComposeDto input)
    {
        try
        {
            var compose = await _composeAppService.CreateAsync(input);
            return CreatedAtAction(nameof(GetAsync), new { id = compose.Id }, compose);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing procedure compose
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<FlowProcedureComposeDto>> UpdateAsync(Guid id, [FromBody] CreateUpdateFlowProcedureComposeDto input)
    {
        try
        {
            var compose = await _composeAppService.UpdateAsync(id, input);
            return Ok(compose);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a procedure compose
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        try
        {
            await _composeAppService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
} 