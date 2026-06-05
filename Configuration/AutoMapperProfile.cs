using Authentication.DTOs.Agent;
using Authentication.DTOs.Farmer;
using Authentication.DTOs.User;
using Authentication.Models;
using AutoMapper;

namespace Authentication.Configuration;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ─── User ──────────────────────────────────────────────────────────
        CreateMap<User, UserProfileDto>()
            .ForMember(d => d.Role,
                o => o.MapFrom(s => s.Role.ToString()));

        // ─── Farmer ────────────────────────────────────────────────────────
        CreateMap<FarmerProfile, FarmerProfileDto>()
            .ForMember(d => d.FullName,
                o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.Email,
                o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.Farms,
                o => o.MapFrom(s => s.Farms));

        CreateMap<Farm, FarmDto>()
            .ForMember(d => d.Crops,
                o => o.MapFrom(s => s.Crops));

        CreateMap<Crop, CropDto>()
            .ForMember(d => d.Status,
                o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<AddFarmRequestDto, Farm>();
        CreateMap<AddCropRequestDto, Crop>();

        // ─── Agent ─────────────────────────────────────────────────────────
        CreateMap<AgentProfile, AgentProfileDto>()
            .ForMember(d => d.FullName,
                o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.Email,
                o => o.MapFrom(s => s.User.Email));
    }
}