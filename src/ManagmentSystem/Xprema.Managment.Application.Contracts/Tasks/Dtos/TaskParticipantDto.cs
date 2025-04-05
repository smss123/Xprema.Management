using System;

namespace Xprema.Managment.Application.Contracts.Tasks.Dtos;

/// <summary>
/// DTO for task participants
/// </summary>
public class TaskParticipantDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid ParticipantId { get; set; }
    public string? ParticipantName { get; set; }
    public string? Role { get; set; }
    public DateTime JoinedDate { get; set; }
    public bool IsActive { get; set; }
} 