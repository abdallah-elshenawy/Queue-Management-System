using AutoMapper;
using QMS.Application.DTOs.Branch;
using QMS.Application.DTOs.Ticket;
using QMS.Application.DTOs.User;
using QMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.Profiles
{
    public class BranchProfile : Profile
    { 
        public BranchProfile()
        {
            CreateMap<CreateBranch, Branch>();

            CreateMap<Branch, GetBranch>()
                .ForMember(dest => dest.Employees, opt => opt.MapFrom(src => src.Employees))
                .ForMember(dest => dest.Tickets, opt => opt.MapFrom(src => src.Tickets));

            CreateMap<EmployeeInfo, GetEmployee>();
            CreateMap<Ticket, GetTicket>();

            CreateMap<UpdateBranch, Branch>();
        }
    }
}
