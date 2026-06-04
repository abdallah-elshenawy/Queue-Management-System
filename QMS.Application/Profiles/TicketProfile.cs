using AutoMapper;
using QMS.Application.DTOs.Ticket;
using QMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.Profiles
{
    public class TicketProfile : Profile
    {
        public TicketProfile()
        {
            CreateMap<Ticket, GetTicket>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Name))
                .ForMember(dest => dest.BranchName, memberOptions => memberOptions.MapFrom(src => src.Branch.Name));

            CreateMap<CreateTicket, Ticket>();
            CreateMap<UpdateTicket, Ticket>();
        }
    }
}
