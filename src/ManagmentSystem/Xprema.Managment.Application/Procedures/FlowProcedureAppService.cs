using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Xprema.Managment.Application.Contracts.Common;
using Xprema.Managment.Application.Contracts.Procedures;
using Xprema.Managment.Application.Contracts.Procedures.Dtos;
using Xprema.Managment.Domain.ProcedureArea;
using Xprema.Managment.EntityFrameworkCore;

namespace Xprema.Managment.Application.Procedures;

/// <summary>
/// Implementation of IFlowProcedureAppService
/// </summary>
public class FlowProcedureAppService : IFlowProcedureAppService
{
    private readonly ManagmentDbContext _dbContext;
    private readonly IMapper _mapper;

    public FlowProcedureAppService(ManagmentDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<FlowProcedureDto> GetAsync(Guid id)
    {
        var procedure = await _dbContext.FlowProcedures
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (procedure == null)
        {
            throw new Exception($"Procedure with id {id} not found");
        }
        
        return _mapper.Map<FlowProcedure, FlowProcedureDto>(procedure);
    }

    /// <inheritdoc/>
    public async Task<List<FlowProcedureDto>> GetListAsync()
    {
        var procedures = await _dbContext.FlowProcedures
            .ToListAsync();
            
        return _mapper.Map<List<FlowProcedure>, List<FlowProcedureDto>>(procedures);
    }

    /// <inheritdoc/>
    public async Task<FlowProcedureDto> CreateAsync(CreateUpdateFlowProcedureDto input)
    {
        var procedure = _mapper.Map<CreateUpdateFlowProcedureDto, FlowProcedure>(input);
        
        // Set audit fields
        procedure.Id = Guid.NewGuid();
        procedure.CreationTime = DateTime.Now;
        
        await _dbContext.FlowProcedures.AddAsync(procedure);
        
        // Create steps if provided
        if (input.Steps != null && input.Steps.Any())
        {
            foreach (var stepDto in input.Steps)
            {
                var step = _mapper.Map<CreateUpdateFlowProcedureStepDto, FlowProcedureStep>(stepDto);
                step.Id = Guid.NewGuid();
                step.ProcedureId = procedure.Id;
                
                await _dbContext.FlowProcedureSteps.AddAsync(step);
            }
        }
        
        await _dbContext.SaveChangesAsync();
        
        return _mapper.Map<FlowProcedure, FlowProcedureDto>(procedure);
    }

    /// <inheritdoc/>
    public async Task<FlowProcedureDto> UpdateAsync(Guid id, CreateUpdateFlowProcedureDto input)
    {
        var procedure = await _dbContext.FlowProcedures
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (procedure == null)
        {
            throw new Exception($"Procedure with id {id} not found");
        }
        
        // Check concurrency
        if (input.ConcurrencyStamp != null && 
            procedure.ConcurrencyStamp != input.ConcurrencyStamp)
        {
            throw new Exception("The procedure has been modified by another user");
        }
        
        // Update fields
        _mapper.Map(input, procedure);
        
        // Set audit fields
        procedure.LastModificationTime = DateTime.Now;
        procedure.ConcurrencyStamp = Guid.NewGuid().ToString();
        
        // Update steps if provided
        if (input.Steps != null && input.Steps.Any())
        {
            // Get existing steps for this procedure
            var existingStepIds = await _dbContext.FlowProcedureSteps
                .Where(s => s.ProcedureId == id)
                .Select(s => s.Id)
                .ToListAsync();
                
            // Process each step from input
            foreach (var stepDto in input.Steps)
            {
                if (stepDto.Id.HasValue && stepDto.Id.Value != Guid.Empty)
                {
                    // Update existing step
                    var step = await _dbContext.FlowProcedureSteps
                        .FirstOrDefaultAsync(s => s.Id == stepDto.Id.Value);
                        
                    if (step != null)
                    {
                        _mapper.Map(stepDto, step);
                        step.ProcedureId = id;
                        existingStepIds.Remove(step.Id);
                    }
                }
                else
                {
                    // Create new step
                    var step = _mapper.Map<CreateUpdateFlowProcedureStepDto, FlowProcedureStep>(stepDto);
                    step.Id = Guid.NewGuid();
                    step.ProcedureId = id;
                    
                    await _dbContext.FlowProcedureSteps.AddAsync(step);
                }
            }
            
            // Remove steps that were not included in the update
            if (existingStepIds.Any())
            {
                var stepsToRemove = await _dbContext.FlowProcedureSteps
                    .Where(s => existingStepIds.Contains(s.Id))
                    .ToListAsync();
                    
                _dbContext.FlowProcedureSteps.RemoveRange(stepsToRemove);
            }
        }
        
        await _dbContext.SaveChangesAsync();
        
        return _mapper.Map<FlowProcedure, FlowProcedureDto>(procedure);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        var procedure = await _dbContext.FlowProcedures
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (procedure == null)
        {
            throw new Exception($"Procedure with id {id} not found");
        }
        
        // Soft delete
        procedure.IsDeleted = true;
        procedure.DeletionTime = DateTime.Now;
        
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<FlowProcedureDto>> GetPagedListAsync(GetFlowProcedureListInput input)
    {
        // Create query
        var query = _dbContext.FlowProcedures.AsQueryable();
        
        // Apply filters
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(p => p.ProcedureName.Contains(input.Filter));
        }
        
        if (input.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == input.IsActive.Value);
        }
        
        if (input.IsSystem.HasValue)
        {
            query = query.Where(p => p.IsSystem == input.IsSystem.Value);
        }
        
        // Get total count
        var totalCount = await query.CountAsync();
        
        // Apply sorting
        if (input.Sorting == "ProcedureName")
        {
            query = query.OrderBy(p => p.ProcedureName);
        }
        else if (input.Sorting == "CreationTime")
        {
            query = query.OrderByDescending(p => p.CreationTime);
        }
        else
        {
            query = query.OrderBy(p => p.ProcedureName);
        }
        
        // Apply paging
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
        
        // Include details if requested
        if (input.IncludeDetails)
        {
            query = query.Include(p => p.Steps);
        }
        
        // Execute query
        var procedures = await query.ToListAsync();
        
        // Map to DTOs
        var dtos = _mapper.Map<List<FlowProcedure>, List<FlowProcedureDto>>(procedures);
        
        return new PagedResultDto<FlowProcedureDto>(totalCount, dtos);
    }

    /// <inheritdoc/>
    public async Task<FlowProcedureDto> GetWithDetailsAsync(Guid id)
    {
        var procedure = await _dbContext.FlowProcedures
            .Include(p => p.Steps)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (procedure == null)
        {
            throw new Exception($"Procedure with id {id} not found");
        }
        
        return _mapper.Map<FlowProcedure, FlowProcedureDto>(procedure);
    }

    /// <inheritdoc/>
    public async Task<FlowProcedureDto> SetActivationAsync(Guid id, bool isActive)
    {
        var procedure = await _dbContext.FlowProcedures
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (procedure == null)
        {
            throw new Exception($"Procedure with id {id} not found");
        }
        
        procedure.IsActive = isActive;
        procedure.LastModificationTime = DateTime.Now;
        procedure.ConcurrencyStamp = Guid.NewGuid().ToString();
        
        await _dbContext.SaveChangesAsync();
        
        return _mapper.Map<FlowProcedure, FlowProcedureDto>(procedure);
    }
} 