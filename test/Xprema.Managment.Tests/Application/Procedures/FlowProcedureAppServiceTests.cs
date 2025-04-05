using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xprema.Managment.Application.Contracts.Procedures.Dtos;
using Xprema.Managment.Application.Procedures;
using Xprema.Managment.Domain.ProcedureArea;
using Xprema.Managment.EntityFrameworkCore;
using Xprema.Managment.Tests.Application;
using Xunit;
using FluentAssertions;

namespace Xprema.Managment.Tests.Application.Procedures;

public class FlowProcedureAppServiceTests : ApplicationTestBase
{
    private readonly FlowProcedureAppService _procedureAppService;
    private readonly ManagmentDbContext _dbContext;
    private readonly IMapper _mapper;

    public FlowProcedureAppServiceTests()
    {
        _dbContext = GetDbContext();
        _mapper = GetMapper();
        _procedureAppService = new FlowProcedureAppService(_dbContext, _mapper);
        
        // Seed database with test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Clear existing data
        _dbContext.FlowProcedures.RemoveRange(_dbContext.FlowProcedures);
        _dbContext.SaveChanges();
        
        // Add test procedures
        _dbContext.FlowProcedures.AddRange(
            new FlowProcedure
            {
                Id = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a"),
                ProcedureName = "Test Procedure 1",
                Description = "Test Description 1",
                IsSystem = false,
                IsActive = true,
                CreationTime = DateTime.Now.AddDays(-10),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new FlowProcedure
            {
                Id = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860b"),
                ProcedureName = "Test Procedure 2",
                Description = "Test Description 2",
                IsSystem = true,
                IsActive = true,
                CreationTime = DateTime.Now.AddDays(-5),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new FlowProcedure
            {
                Id = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860c"),
                ProcedureName = "Test Procedure 3",
                Description = "Test Description 3",
                IsSystem = false,
                IsActive = false,
                CreationTime = DateTime.Now.AddDays(-1),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        );
        
        // Add test procedure steps
        _dbContext.FlowProcedureSteps.AddRange(
            new FlowProcedureStep
            {
                Id = Guid.Parse("d8f9e5b2-2e9d-5c1d-bf6f-b2fe9f5f971a"),
                ProcedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a"),
                Step = 1,
                IsSystem = false
            },
            new FlowProcedureStep
            {
                Id = Guid.Parse("d8f9e5b2-2e9d-5c1d-bf6f-b2fe9f5f971b"),
                ProcedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a"),
                Step = 2,
                IsSystem = false
            }
        );
        
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnProcedure_WhenProcedureExists()
    {
        // Arrange
        var procedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a");
        
        // Act
        var result = await _procedureAppService.GetAsync(procedureId);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(procedureId);
        result.ProcedureName.Should().Be("Test Procedure 1");
    }

    [Fact]
    public async Task GetAsync_ShouldThrowException_WhenProcedureDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _procedureAppService.GetAsync(nonExistentId));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnAllProcedures()
    {
        // Act
        var result = await _procedureAppService.GetListAsync();
        
        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateNewProcedure()
    {
        // Arrange
        var input = new CreateUpdateFlowProcedureDto
        {
            ProcedureName = "New Test Procedure",
            Description = "New Test Description",
            IsSystem = false,
            IsActive = true
        };
        
        // Act
        var result = await _procedureAppService.CreateAsync(input);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.ProcedureName.Should().Be("New Test Procedure");
        
        // Verify it was added to database
        var savedProcedure = await _dbContext.FlowProcedures.FindAsync(result.Id);
        savedProcedure.Should().NotBeNull();
        savedProcedure!.ProcedureName.Should().Be("New Test Procedure");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingProcedure()
    {
        // Arrange
        var procedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a");
        var procedure = await _dbContext.FlowProcedures.FindAsync(procedureId);
        var concurrencyStamp = procedure!.ConcurrencyStamp;
        
        var input = new CreateUpdateFlowProcedureDto
        {
            ProcedureName = "Updated Procedure Name",
            Description = "Updated Description",
            IsSystem = procedure.IsSystem,
            IsActive = procedure.IsActive,
            ConcurrencyStamp = concurrencyStamp
        };
        
        // Act
        var result = await _procedureAppService.UpdateAsync(procedureId, input);
        
        // Assert
        result.Should().NotBeNull();
        result.ProcedureName.Should().Be("Updated Procedure Name");
        result.Description.Should().Be("Updated Description");
        
        // Verify it was updated in database
        var updatedProcedure = await _dbContext.FlowProcedures.FindAsync(procedureId);
        updatedProcedure.Should().NotBeNull();
        updatedProcedure!.ProcedureName.Should().Be("Updated Procedure Name");
        updatedProcedure.ConcurrencyStamp.Should().NotBe(concurrencyStamp);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteProcedure()
    {
        // Arrange
        var procedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a");
        
        // Act
        await _procedureAppService.DeleteAsync(procedureId);
        
        // Assert
        var deletedProcedure = await _dbContext.FlowProcedures
            .IgnoreQueryFilters() // To get soft-deleted entities
            .FirstOrDefaultAsync(p => p.Id == procedureId);
        
        deletedProcedure.Should().NotBeNull();
        deletedProcedure!.IsDeleted.Should().BeTrue();
        deletedProcedure.DeletionTime.Should().NotBeNull();
        
        // Verify it's not returned in regular queries
        var procedures = await _procedureAppService.GetListAsync();
        procedures.Should().NotContain(p => p.Id == procedureId);
    }

    [Fact]
    public async Task GetWithDetailsAsync_ShouldReturnProcedureWithSteps()
    {
        // Arrange
        var procedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a");
        
        // Act
        var result = await _procedureAppService.GetWithDetailsAsync(procedureId);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(procedureId);
        result.Steps.Should().NotBeNull();
        result.Steps.Should().HaveCount(2);
    }

    [Fact]
    public async Task SetActivationAsync_ShouldUpdateProcedureActivation()
    {
        // Arrange
        var procedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a");
        var originalProcedure = await _dbContext.FlowProcedures.FindAsync(procedureId);
        var originalIsActive = originalProcedure!.IsActive;
        
        // Act
        var result = await _procedureAppService.SetActivationAsync(procedureId, !originalIsActive);
        
        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().Be(!originalIsActive);
        
        // Verify it was updated in database
        var updatedProcedure = await _dbContext.FlowProcedures.FindAsync(procedureId);
        updatedProcedure.Should().NotBeNull();
        updatedProcedure!.IsActive.Should().Be(!originalIsActive);
    }

    [Fact]
    public async Task GetPagedListAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var input = new GetFlowProcedureListInput
        {
            MaxResultCount = 2,
            SkipCount = 0,
            Sorting = "ProcedureName"
        };
        
        // Act
        var result = await _procedureAppService.GetPagedListAsync(input);
        
        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPagedListAsync_ShouldFilterByIsActive()
    {
        // Arrange
        var input = new GetFlowProcedureListInput
        {
            MaxResultCount = 10,
            SkipCount = 0,
            IsActive = true
        };
        
        // Act
        var result = await _procedureAppService.GetPagedListAsync(input);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(p => p.IsActive);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPagedListAsync_ShouldFilterByIsSystem()
    {
        // Arrange
        var input = new GetFlowProcedureListInput
        {
            MaxResultCount = 10,
            SkipCount = 0,
            IsSystem = true
        };
        
        // Act
        var result = await _procedureAppService.GetPagedListAsync(input);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(p => p.IsSystem);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPagedListAsync_ShouldFilterBySearchText()
    {
        // Arrange
        var input = new GetFlowProcedureListInput
        {
            MaxResultCount = 10,
            SkipCount = 0,
            Filter = "Test Procedure 1"
        };
        
        // Act
        var result = await _procedureAppService.GetPagedListAsync(input);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().ProcedureName.Should().Be("Test Procedure 1");
    }
} 