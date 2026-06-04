using AutoMapper;
using QMS.Application.DTOs.User;
using QMS.Domain.Models;

namespace QMS.Application.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<CreateUser, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password));

            CreateMap<CreateCounter, EmployeeInfo>();
            CreateMap<CreateCounter, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password));

            CreateMap<CreateDoorVerifier, EmployeeInfo>();
            CreateMap<CreateDoorVerifier, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password));

            CreateMap<User, GetUser>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.EmpInfo, opt => opt.MapFrom(src => src.EmployeeInfo))
                // FIX: string interpolation with null ThirdName/FourthName produced "John Doe null null".
                // Join only the non-null/non-empty parts instead.
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    string.Join(" ", new[] { src.FirstName, src.SecondName, src.ThirdName, src.FourthName }
                        .Where(n => !string.IsNullOrEmpty(n)))));

            CreateMap<EmployeeInfo, EmpInfoDTO>();
            CreateMap<UpdateUser, User>();
            CreateMap<UpdateEmployee, EmployeeInfo>();
        }
    }
}