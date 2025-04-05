using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xprema.Managment.Application.Actions;
using Xprema.Managment.Application.Contracts.Actions.Dtos;
using Xprema.Managment.Domain.ActionArea;
using Xprema.Managment.EntityFrameworkCore;
using Xprema.Managment.Tests.Application;
using Xunit;

namespace Xprema.Managment.Tests.Application.Actions;

public class ActionAppServiceTests : ApplicationTestBase
{
    private readonly ActionAppService _actionAppService;
    private readonly ManagmentDbContext _dbContext;
    private readonly IMapper _mapper;

    public ActionAppServiceTests()
    {
        _dbContext = GetDbContext();
        _mapper = GetMapper();
        _actionAppService = new ActionAppService(_dbContext, _mapper);
        
        // Seed database with test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Clear existing data
        _dbContext.Actions.RemoveRange(_dbContext.Actions);
        _dbContext.SaveChanges();
        
        // Add test actions
        _dbContext.Actions.AddRange(
            new Action
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-4a5b-9c8d-1e2f3a4b5c6d"),
                Name = "Test Action 1",
                Description = "Test Description 1",
                IsSystem = false,
                IsActive = true,
                CreationTime = DateTime.Now.AddDays(-10),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new Action
            {
                Id = Guid.Parse("b2c3d4e5-f6a7-5b6c-0d1e-2f3a4b5c6d7e"),
                Name = "Test Action 2",
                Description = "Test Description 2",
                IsSystem = true,
                IsActive = true,
                CreationTime = DateTime.Now.AddDays(-5),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new Action
            {
                Id = Guid.Parse("c3d4e5f6-a7b8-6c7d-1e2f-3a4b5c6d7e8f"),
                Name = "Test Action 3",
                Description = "Test Description 3",
                IsSystem = false,
                IsActive = false,
                CreationTime = DateTime.Now.AddDays(-1),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        );
        
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnAction_WhenActionExists()
    {
        // Arrange
        var actionId = Guid.Parse("a1b2c3d4-e5f6-4a5b-9c8d-1e2f3a4b5c6d");
        
        // Act
        var result = await _actionAppService.GetAsync(actionId);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(actionId);
        result.Name.Should().Be("Test Action 1");
    }

    [Fact]
    public async Task GetAsync_ShouldThrowException_WhenActionDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _actionAppService.GetAsync(nonExistentId));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnAllActions()
    {
        // Act
        var result = await _actionAppService.GetListAsync();
        
        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateNewAction()
    {
        // Arrange
        var input = new CreateUpdateActionDto
        {
            Name = "New Test Action",
            Description = "New Test Description",
            IsSystem = false,
            IsActive = true
        };
        
        // Act
        var result = await _actionAppService.CreateAsync(input);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("New Test Action");
        
        // Verify it was added to database
        var savedAction = await _dbContext.Actions.FindAsync(result.Id);
        savedAction.Should().NotBeNull();
        savedAction!.Name.Should().Be("New Test Action");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingAction()
    {
        // Arrange
        var actionId = Guid.Parse("a1b2c3d4-e5f6-4a5b-9c8d-1e2f3a4b5c6d");
        var action = await _dbContext.Actions.FindAsync(actionId);
        var concurrencyStamp = action!.ConcurrencyStamp;
        
        var input = new CreateUpdateActionDto
        {
            Name = "Updated Action Name",
            Description = "Updated Description",
            IsSystem = action.IsSystem,
            IsActive = action.IsActive,
            ConcurrencyStamp = concurrencyStamp
        };
        
        // Act
        var result = await _actionAppService.UpdateAsync(actionId, input);
        
        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Action Name");
        result.Description.Should().Be("Updated Description");
        
        // Verify it was updated in database
        var updatedAction = await _dbContext.Actions.FindAsync(actionId);
        updatedAction.Should().NotBeNull();
        updatedAction!.Name.Should().Be("Updated Action Name");
        updatedAction.ConcurrencyStamp.Should().NotBe(concurrencyStamp);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteAction()
    {
        // Arrange
        var actionId = Guid.Parse("a1b2c3d4-e5f6-4a5b-9c8d-1e2f3a4b5c6d");
        
        // Act
        await _actionAppService.DeleteAsync(actionId);
        
        // Assert
        var deletedAction = await _dbContext.Actions
            .IgnoreQueryFilters() // To get soft-deleted entities
            .FirstOrDefaultAsync(a => a.Id == actionId);
        
        deletedAction.Should().NotBeNull();
        deletedAction!.IsDeleted.Should().BeTrue();
        deletedAction.DeletionTime.Should().NotBeNull();
        
        // Verify it's not returned in regular queries
        var actions = await _actionAppService.GetListAsync();
        actions.Should().NotContain(a => a.Id == actionId);
    }

    [Fact]
    public async Task SetActivationAsync_ShouldUpdateActionActivation()
    {
        // Arrange
        var actionId = Guid.Parse("a1b2c3d4-e5f6-4a5b-9c8d-1e2f3a4b5c6d");
        var originalAction = await _dbContext.Actions.FindAsync(actionId);
        var originalIsActive = originalAction!.IsActive;
        
        // Act
        var result = await _actionAppService.SetActivationAsync(actionId, !originalIsActive);
        
        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().Be(!originalIsActive);
        
        // Verify it was updated in database
        var updatedAction = await _dbContext.Actions.FindAsync(actionId);
        updatedAction.Should().NotBeNull();
        updatedAction!.IsActive.Should().Be(!originalIsActive);
    }

    [Fact]
    public async Task GetPagedListAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var input = new GetActionListInput
        {
            MaxResultCount = 2,
            SkipCount = 0,
            Sorting = "Name"
        };
        
        // Act
        var result = await _actionAppService.GetPagedListAsync(input);
        
        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPagedListAsync_ShouldFilterByIsActive()
    {
        // Arrange
        var input = new GetActionListInput
        {
            MaxResultCount = 10,
            SkipCount = 0,
            IsActive = true
        };
        
        // Act
        var result = await _actionAppService.GetPagedListAsync(input);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(a => a.IsActive);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPagedListAsync_ShouldFilterByIsSystem()
    {
        // Arrange
        var input = new GetActionListInput
        {
            MaxResultCount = 10,
            SkipCount = 0,
            IsSystem = true
        };
        
        // Act
        var result = await _actionAppService.GetPagedListAsync(input);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().OnlyContain(a => a.IsSystem);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPagedListAsync_ShouldFilterBySearchText()
    {
        // Arrange
        var input = new GetActionListInput
        {
            MaxResultCount = 10,
            SkipCount = 0,
            Filter = "Test Action 1"
        };
        
        // Act
        var result = await _actionAppService.GetPagedListAsync(input);
        
        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Test Action 1");
    }
} 