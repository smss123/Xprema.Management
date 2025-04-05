using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xprema.Managment.Application.Contracts.Common;
using Xprema.Managment.Application.Contracts.Procedures.Dtos;

namespace Xprema.Managment.Application.Contracts.Procedures;

/// <summary>
/// Service interface for FlowProcedureCompose operations
/// </summary>
public interface IFlowProcedureComposeAppService
{
    /// <summary>
    /// Gets a procedure compose by id
    /// </summary>
    Task<FlowProcedureComposeDto> GetAsync(Guid id);
    
    /// <summary>
    /// Gets a list of all procedure composes
    /// </summary>
    Task<List<FlowProcedureComposeDto>> GetListAsync();
    
    /// <summary>
    /// Creates a new procedure compose
    /// </summary>
    Task<FlowProcedureComposeDto> CreateAsync(CreateUpdateFlowProcedureComposeDto input);
    
    /// <summary>
    /// Updates an existing procedure compose
    /// </summary>
    Task<FlowProcedureComposeDto> UpdateAsync(Guid id, CreateUpdateFlowProcedureComposeDto input);
    
    /// <summary>
    /// Deletes a procedure compose
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Gets detailed procedure compose with steps
    /// </summary>
    Task<FlowProcedureComposeDto> GetWithDetailsAsync(Guid id);
} 