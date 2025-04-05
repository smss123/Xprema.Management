using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xprema.Managment.Application.Contracts.Common;
using Xprema.Managment.Application.Contracts.Procedures;
using Xprema.Managment.Application.Contracts.Procedures.Dtos;

namespace Xprema.Managment.Application.Controllers;

[ApiController]
[Route("api/procedures")]
public class FlowProcedureController : ControllerBase
{
    private readonly IFlowProcedureAppService _procedureAppService;

    public FlowProcedureController(IFlowProcedureAppService procedureAppService)
    {
        _procedureAppService = procedureAppService;
    }

    /// <summary>
    /// Gets a procedure by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FlowProcedureDto>> GetAsync(Guid id)
    {
        try
        {
            var procedure = await _procedureAppService.GetAsync(id);
            return Ok(procedure);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Gets a list of all procedures
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<FlowProcedureDto>>> GetListAsync()
    {
        var procedures = await _procedureAppService.GetListAsync();
        return Ok(procedures);
    }

    /// <summary>
    /// Gets detailed procedure with steps
    /// </summary>
    [HttpGet("{id}/details")]
    public async Task<ActionResult<FlowProcedureDto>> GetWithDetailsAsync(Guid id)
    {
        try
        {
            var procedure = await _procedureAppService.GetWithDetailsAsync(id);
            return Ok(procedure);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Gets procedures with paging and filtering
    /// </summary>
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResultDto<FlowProcedureDto>>> GetPagedListAsync([FromQuery] GetFlowProcedureListInput input)
    {
        var procedures = await _procedureAppService.GetPagedListAsync(input);
        return Ok(procedures);
    }

    /// <summary>
    /// Creates a new procedure
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FlowProcedureDto>> CreateAsync([FromBody] CreateUpdateFlowProcedureDto input)
    {
        try
        {
            var procedure = await _procedureAppService.CreateAsync(input);
            return CreatedAtAction(nameof(GetAsync), new { id = procedure.Id }, procedure);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing procedure
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<FlowProcedureDto>> UpdateAsync(Guid id, [FromBody] CreateUpdateFlowProcedureDto input)
    {
        try
        {
            var procedure = await _procedureAppService.UpdateAsync(id, input);
            return Ok(procedure);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a procedure
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        try
        {
            await _procedureAppService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Activates or deactivates a procedure
    /// </summary>
    [HttpPatch("{id}/activation")]
    public async Task<ActionResult<FlowProcedureDto>> SetActivationAsync(Guid id, [FromBody] bool isActive)
    {
        try
        {
            var procedure = await _procedureAppService.SetActivationAsync(id, isActive);
            return Ok(procedure);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
} 