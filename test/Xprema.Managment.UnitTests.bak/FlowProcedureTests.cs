using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Xprema.Managment.Application.Procedures;
using Xprema.Managment.Domain.ProcedureArea;

namespace Xprema.Managment.UnitTests;

public class FlowProcedureTests
{
    [Fact]
    public void FlowProcedure_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var procedureId = Guid.NewGuid();
        var procedureName = "Test Procedure";
        var description = "Test Description";
        
        // Act
        var procedure = new FlowProcedure
        {
            Id = procedureId,
            ProcedureName = procedureName,
            Description = description,
            IsSystem = false,
            IsActive = true
        };
        
        // Assert
        procedure.Should().NotBeNull();
        procedure.Id.Should().Be(procedureId);
        procedure.ProcedureName.Should().Be(procedureName);
        procedure.Description.Should().Be(description);
        procedure.IsSystem.Should().BeFalse();
        procedure.IsActive.Should().BeTrue();
    }
    
    [Fact]
    public void FlowProcedure_WithSteps_ShouldHaveCorrectStepCount()
    {
        // Arrange
        var procedure = new FlowProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureName = "Procedure With Steps",
            Description = "A procedure with multiple steps",
            IsSystem = false,
            IsActive = true
        };
        
        var steps = new List<FlowProcedureStep>
        {
            new FlowProcedureStep
            {
                Id = Guid.NewGuid(),
                ProcedureId = procedure.Id,
                Step = 1,
                IsSystem = false
            },
            new FlowProcedureStep
            {
                Id = Guid.NewGuid(),
                ProcedureId = procedure.Id,
                Step = 2,
                IsSystem = false
            },
            new FlowProcedureStep
            {
                Id = Guid.NewGuid(),
                ProcedureId = procedure.Id,
                Step = 3,
                IsSystem = false
            }
        };
        
        procedure.Steps = steps;
        
        // Act & Assert
        procedure.Steps.Should().NotBeNull();
        procedure.Steps.Should().HaveCount(3);
        procedure.Steps.Select(s => s.Step).Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }
    
    [Fact]
    public void FlowProcedure_WhenDeactivated_ShouldUpdateIsActiveProperty()
    {
        // Arrange
        var procedure = new FlowProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureName = "Active Procedure",
            Description = "An initially active procedure",
            IsSystem = false,
            IsActive = true
        };
        
        // Act
        procedure.IsActive = false;
        
        // Assert
        procedure.IsActive.Should().BeFalse();
    }
}
