using AutoMapper;
using Xprema.Managment.Application.Contracts.Procedures.Dtos;
using Xprema.Managment.Application.Contracts.Tasks.Dtos;
using Xprema.Managment.Domain.ProcedureArea;
using Xprema.Managment.Domain.TaskArea;

namespace Xprema.Managment.Application.Mapping;

/// <summary>
/// AutoMapper profile for mapping between entities and DTOs
/// </summary>
public class ManagmentMappingProfile : Profile
{
    public ManagmentMappingProfile()
    {
        // FlowProcedure mappings
        CreateMap<FlowProcedure, FlowProcedureDto>()
            .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.Steps));
            
        CreateMap<CreateUpdateFlowProcedureDto, FlowProcedure>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
            .ForMember(dest => dest.LastModificationTime, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifierId, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeleterId, opt => opt.Ignore())
            .ForMember(dest => dest.DeletionTime, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.Steps, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore());
            
        // FlowProcedureStep mappings
        CreateMap<FlowProcedureStep, FlowProcedureStepDto>()
            .ForMember(dest => dest.ProcedureComposeName, opt => opt.MapFrom(src => src.ProcedureCompose != null ? src.ProcedureCompose.ProcedureComposeName : null))
            .ForMember(dest => dest.FlowProcedureName, opt => opt.MapFrom(src => src.FlowProcedure != null ? src.FlowProcedure.ProcedureName : null))
            .ForMember(dest => dest.ActionName, opt => opt.MapFrom(src => src.Action != null ? src.Action.ActionName : null));
            
        CreateMap<CreateUpdateFlowProcedureStepDto, FlowProcedureStep>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProcedureCompose, opt => opt.Ignore())
            .ForMember(dest => dest.FlowProcedure, opt => opt.Ignore())
            .ForMember(dest => dest.Action, opt => opt.Ignore());
            
        // FlowTask mappings
        CreateMap<FlowTask, TaskDto>()
            .ForMember(dest => dest.ProcedureName, opt => opt.MapFrom(src => src.Procedure != null ? src.Procedure.ProcedureName : null));
            
        CreateMap<CreateUpdateTaskDto, FlowTask>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Procedure, opt => opt.Ignore())
            .ForMember(dest => dest.ProcedureStep, opt => opt.Ignore())
            .ForMember(dest => dest.Participants, opt => opt.Ignore())
            .ForMember(dest => dest.Steps, opt => opt.Ignore())
            .ForMember(dest => dest.Timeline, opt => opt.Ignore())
            .ForMember(dest => dest.StartDate, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedDate, opt => opt.Ignore());
    }
} 