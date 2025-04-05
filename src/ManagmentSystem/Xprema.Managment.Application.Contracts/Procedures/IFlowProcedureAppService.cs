using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xprema.Managment.Application.Contracts.Common;
using Xprema.Managment.Application.Contracts.Procedures.Dtos;

namespace Xprema.Managment.Application.Contracts.Procedures;

/// <summary>
/// Service interface for FlowProcedure operations
/// </summary>
public interface IFlowProcedureAppService
{
    /// <summary>
    /// Gets a procedure by id
    /// </summary>
    Task<FlowProcedureDto> GetAsync(Guid id);
    
    /// <summary>
    /// Gets a list of all procedures
    /// </summary>
    Task<List<FlowProcedureDto>> GetListAsync();
    
    /// <summary>
    /// Creates a new procedure
    /// </summary>
    Task<FlowProcedureDto> CreateAsync(CreateUpdateFlowProcedureDto input);
    
    /// <summary>
    /// Updates an existing procedure
    /// </summary>
    Task<FlowProcedureDto> UpdateAsync(Guid id, CreateUpdateFlowProcedureDto input);
    
    /// <summary>
    /// Deletes a procedure
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Gets procedures with paging and filtering
    /// </summary>
    Task<PagedResultDto<FlowProcedureDto>> GetPagedListAsync(GetFlowProcedureListInput input);
    
    /// <summary>
    /// Gets detailed procedure with steps
    /// </summary>
    Task<FlowProcedureDto> GetWithDetailsAsync(Guid id);
    
    /// <summary>
    /// Activates or deactivates a procedure
    /// </summary>
    Task<FlowProcedureDto> SetActivationAsync(Guid id, bool isActive);
} 