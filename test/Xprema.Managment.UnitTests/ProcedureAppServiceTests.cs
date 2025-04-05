using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xprema.Managment.Application.Contracts.Procedures.Dtos;
using Xprema.Managment.Application.Procedures;
using Xprema.Managment.Domain.ProcedureArea;
using Xprema.Managment.EntityFrameworkCore;
using Xunit;

namespace Xprema.Managment.UnitTests;

public class ProcedureAppServiceTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ManagmentDbContext> _mockDbContext;
    private readonly Mock<DbSet<FlowProcedure>> _mockProcedureDbSet;
    private readonly List<FlowProcedure> _procedures;

    public ProcedureAppServiceTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockDbContext = new Mock<ManagmentDbContext>();
        _mockProcedureDbSet = new Mock<DbSet<FlowProcedure>>();
        
        // Sample test data
        _procedures = new List<FlowProcedure>
        {
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
            }
        };
    }

    private void SetupMockDbSet()
    {
        var queryable = _procedures.AsQueryable();
        
        _mockProcedureDbSet.As<IQueryable<FlowProcedure>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _mockProcedureDbSet.As<IQueryable<FlowProcedure>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _mockProcedureDbSet.As<IQueryable<FlowProcedure>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _mockProcedureDbSet.As<IQueryable<FlowProcedure>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        
        _mockDbContext.Setup(c => c.FlowProcedures).Returns(_mockProcedureDbSet.Object);
    }
    
    [Fact]
    public async Task GetAsync_ShouldReturnMappedProcedure_WhenProcedureExists()
    {
        // Arrange
        var procedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a");
        var procedure = _procedures.First(p => p.Id == procedureId);
        var procedureDto = new FlowProcedureDto
        {
            Id = procedure.Id,
            ProcedureName = procedure.ProcedureName,
            Description = procedure.Description,
            IsSystem = procedure.IsSystem,
            IsActive = procedure.IsActive
        };
        
        SetupMockDbSet();
        
        _mockProcedureDbSet.Setup(m => m.FindAsync(procedureId))
            .ReturnsAsync(procedure);
        
        _mockMapper.Setup(m => m.Map<FlowProcedureDto>(procedure))
            .Returns(procedureDto);
        
        var service = new FlowProcedureAppService(_mockDbContext.Object, _mockMapper.Object);
        
        // Act
        var result = await service.GetAsync(procedureId);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(procedureId);
        result.ProcedureName.Should().Be("Test Procedure 1");
        
        _mockProcedureDbSet.Verify(m => m.FindAsync(procedureId), Times.Once);
        _mockMapper.Verify(m => m.Map<FlowProcedureDto>(procedure), Times.Once);
    }
    
    [Fact]
    public async Task GetAsync_ShouldThrowException_WhenProcedureDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        SetupMockDbSet();
        
        _mockProcedureDbSet.Setup(m => m.FindAsync(nonExistentId))
            .ReturnsAsync((FlowProcedure)null);
        
        var service = new FlowProcedureAppService(_mockDbContext.Object, _mockMapper.Object);
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.GetAsync(nonExistentId));
        
        _mockProcedureDbSet.Verify(m => m.FindAsync(nonExistentId), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnNewProcedure()
    {
        // Arrange
        var input = new CreateUpdateFlowProcedureDto
        {
            ProcedureName = "New Procedure",
            Description = "New Description",
            IsSystem = false,
            IsActive = true
        };
        
        var newProcedure = new FlowProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureName = input.ProcedureName,
            Description = input.Description,
            IsSystem = input.IsSystem,
            IsActive = input.IsActive,
            CreationTime = DateTime.Now,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        
        var newProcedureDto = new FlowProcedureDto
        {
            Id = newProcedure.Id,
            ProcedureName = newProcedure.ProcedureName,
            Description = newProcedure.Description,
            IsSystem = newProcedure.IsSystem,
            IsActive = newProcedure.IsActive
        };
        
        SetupMockDbSet();
        
        _mockMapper.Setup(m => m.Map<FlowProcedure>(input))
            .Returns(newProcedure);
        
        _mockMapper.Setup(m => m.Map<FlowProcedureDto>(newProcedure))
            .Returns(newProcedureDto);
        
        _mockProcedureDbSet.Setup(m => m.Add(newProcedure))
            .Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<FlowProcedure>)null);
        
        _mockDbContext.Setup(m => m.SaveChangesAsync(default))
            .ReturnsAsync(1);
        
        var service = new FlowProcedureAppService(_mockDbContext.Object, _mockMapper.Object);
        
        // Act
        var result = await service.CreateAsync(input);
        
        // Assert
        result.Should().NotBeNull();
        result.ProcedureName.Should().Be("New Procedure");
        result.Description.Should().Be("New Description");
        
        _mockMapper.Verify(m => m.Map<FlowProcedure>(input), Times.Once);
        _mockProcedureDbSet.Verify(m => m.Add(newProcedure), Times.Once);
        _mockDbContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        _mockMapper.Verify(m => m.Map<FlowProcedureDto>(newProcedure), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndReturnUpdatedProcedure()
    {
        // Arrange
        var procedureId = Guid.Parse("c7d9e4b1-1d8c-4b0c-af5e-a1fe8f4f860a");
        var procedure = _procedures.First(p => p.Id == procedureId);
        var concurrencyStamp = procedure.ConcurrencyStamp;
        
        var input = new CreateUpdateFlowProcedureDto
        {
            ProcedureName = "Updated Procedure",
            Description = "Updated Description",
            IsSystem = procedure.IsSystem,
            IsActive = procedure.IsActive,
            ConcurrencyStamp = concurrencyStamp
        };
        
        var updatedProcedureDto = new FlowProcedureDto
        {
            Id = procedure.Id,
            ProcedureName = input.ProcedureName,
            Description = input.Description,
            IsSystem = input.IsSystem,
            IsActive = input.IsActive
        };
        
        SetupMockDbSet();
        
        _mockProcedureDbSet.Setup(m => m.FindAsync(procedureId))
            .ReturnsAsync(procedure);
        
        _mockMapper.Setup(m => m.Map(input, procedure))
            .Returns(procedure);
        
        _mockMapper.Setup(m => m.Map<FlowProcedureDto>(procedure))
            .Returns(updatedProcedureDto);
        
        _mockDbContext.Setup(m => m.SaveChangesAsync(default))
            .ReturnsAsync(1);
        
        var service = new FlowProcedureAppService(_mockDbContext.Object, _mockMapper.Object);
        
        // Act
        var result = await service.UpdateAsync(procedureId, input);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(procedureId);
        result.ProcedureName.Should().Be("Updated Procedure");
        result.Description.Should().Be("Updated Description");
        
        _mockProcedureDbSet.Verify(m => m.FindAsync(procedureId), Times.Once);
        _mockMapper.Verify(m => m.Map(input, procedure), Times.Once);
        _mockDbContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        _mockMapper.Verify(m => m.Map<FlowProcedureDto>(procedure), Times.Once);
    }
} 