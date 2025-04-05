using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xprema.Managment.Application.Contracts.Procedures.Dtos;
using Xprema.Managment.Domain.ProcedureArea;
using Xprema.Managment.EntityFrameworkCore;
using Xprema.Managment.Tests.Api;
using Xunit;

namespace Xprema.Managment.Tests.Api;

public class ProcedureControllerTests : ApiTestBase
{
    public ProcedureControllerTests()
    {
        // Additional setup if needed
    }

    protected override void SeedDatabase(ManagmentDbContext context)
    {
        // Seed test procedures
        context.FlowProcedures.AddRange(
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
        context.FlowProcedureSteps.AddRange(
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
        
        context.SaveChanges();
    }
    
    [Fact]
    public async Task GetAll_ShouldReturnAllProcedures()
    {
        // Act
        var response = await Client.GetAsync("/api/procedures");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var procedures = await response.Content.ReadFromJsonAsync<List<FlowProcedureDto>>();
        procedures.Should().NotBeNull();
        procedures.Should().HaveCount(3);
    }
    
    [Fact]
    public async Task Get_ShouldReturnProcedure_WhenProcedureExists()
    {
        // Arrange
        var procedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a");
        
        // Act
        var response = await Client.GetAsync($"/api/procedures/{procedureId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var procedure = await response.Content.ReadFromJsonAsync<FlowProcedureDto>();
        procedure.Should().NotBeNull();
        procedure!.Id.Should().Be(procedureId);
        procedure.ProcedureName.Should().Be("Test Procedure 1");
    }
    
    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenProcedureDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        // Act
        var response = await Client.GetAsync($"/api/procedures/{nonExistentId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Create_ShouldCreateNewProcedure()
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
        var response = await Client.PostAsJsonAsync("/api/procedures", input);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var procedure = await response.Content.ReadFromJsonAsync<FlowProcedureDto>();
        procedure.Should().NotBeNull();
        procedure!.Id.Should().NotBeEmpty();
        procedure.ProcedureName.Should().Be("New Test Procedure");
        
        // Verify it was added to database
        var dbContext = GetService<ManagmentDbContext>();
        var savedProcedure = await dbContext.FlowProcedures.FindAsync(procedure.Id);
        savedProcedure.Should().NotBeNull();
        savedProcedure!.ProcedureName.Should().Be("New Test Procedure");
    }
    
    [Fact]
    public async Task Update_ShouldUpdateExistingProcedure()
    {
        // Arrange
        var procedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a");
        var dbContext = GetService<ManagmentDbContext>();
        var procedure = await dbContext.FlowProcedures.FindAsync(procedureId);
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
        var response = await Client.PutAsJsonAsync($"/api/procedures/{procedureId}", input);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updatedProcedure = await response.Content.ReadFromJsonAsync<FlowProcedureDto>();
        updatedProcedure.Should().NotBeNull();
        updatedProcedure!.ProcedureName.Should().Be("Updated Procedure Name");
        updatedProcedure.Description.Should().Be("Updated Description");
        
        // Verify it was updated in database
        var savedProcedure = await dbContext.FlowProcedures.FindAsync(procedureId);
        savedProcedure.Should().NotBeNull();
        savedProcedure!.ProcedureName.Should().Be("Updated Procedure Name");
        savedProcedure.ConcurrencyStamp.Should().NotBe(concurrencyStamp);
    }
    
    [Fact]
    public async Task Delete_ShouldSoftDeleteProcedure()
    {
        // Arrange
        var procedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a");
        
        // Act
        var response = await Client.DeleteAsync($"/api/procedures/{procedureId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify it was soft-deleted
        var dbContext = GetService<ManagmentDbContext>();
        var deletedProcedure = await dbContext.FlowProcedures
            .FindAsync(procedureId);
        
        deletedProcedure.Should().BeNull(); // Should be null when using default query filters
        
        // Check with IgnoreQueryFilters to verify it was soft-deleted
        var softDeletedProcedure = await dbContext.FlowProcedures
            .IgnoreQueryFilters()
            .FindAsync(procedureId);
        
        softDeletedProcedure.Should().NotBeNull();
        softDeletedProcedure!.IsDeleted.Should().BeTrue();
        softDeletedProcedure.DeletionTime.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetWithDetails_ShouldReturnProcedureWithSteps()
    {
        // Arrange
        var procedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a");
        
        // Act
        var response = await Client.GetAsync($"/api/procedures/{procedureId}/details");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var procedure = await response.Content.ReadFromJsonAsync<FlowProcedureWithDetailsDto>();
        procedure.Should().NotBeNull();
        procedure!.Id.Should().Be(procedureId);
        procedure.Steps.Should().NotBeNull();
        procedure.Steps.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task SetActivation_ShouldUpdateProcedureActivation()
    {
        // Arrange
        var procedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a");
        var dbContext = GetService<ManagmentDbContext>();
        var originalProcedure = await dbContext.FlowProcedures.FindAsync(procedureId);
        var originalIsActive = originalProcedure!.IsActive;
        
        // Act
        var response = await Client.PatchAsync(
            $"/api/procedures/{procedureId}/activation/{!originalIsActive}", 
            null);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updatedProcedure = await response.Content.ReadFromJsonAsync<FlowProcedureDto>();
        updatedProcedure.Should().NotBeNull();
        updatedProcedure!.IsActive.Should().Be(!originalIsActive);
        
        // Verify it was updated in database
        var savedProcedure = await dbContext.FlowProcedures.FindAsync(procedureId);
        savedProcedure.Should().NotBeNull();
        savedProcedure!.IsActive.Should().Be(!originalIsActive);
    }
    
    [Fact]
    public async Task GetPaged_ShouldReturnPagedResults()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            ["MaxResultCount"] = "2",
            ["SkipCount"] = "0",
            ["Sorting"] = "ProcedureName"
        };
        
        var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={p.Value}"));
        
        // Act
        var response = await Client.GetAsync($"/api/procedures/paged?{queryString}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResultDto<FlowProcedureDto>>();
        pagedResult.Should().NotBeNull();
        pagedResult!.TotalCount.Should().Be(3);
        pagedResult.Items.Should().HaveCount(2);
    }
} 