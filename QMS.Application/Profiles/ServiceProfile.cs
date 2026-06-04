using AutoMapper;
using QMS.Application.DTOs.Service;
using QMS.Domain.Models;

namespace QMS.Application.Profiles
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<Service, GetService>();
            CreateMap<CreateService, Service>();
            // FIX: was CreateMap<UpdateBranch, Service>() — completely wrong source type.
            CreateMap<UpdateService, Service>();
        }
    }
}