using AutoMapper;
using QMS.Application.DTOs.DisplayScreen;
using QMS.Domain.Models;

namespace QMS.Application.Profiles
{
    public class DisplayScreenProfile : Profile
    {
        public DisplayScreenProfile()
        {
            CreateMap<DisplayTicket, GetDisplayTicket>()
                .ForMember(dest => dest.TicketNumber, opt => opt.MapFrom(src => src.Ticket.TicketNumber));

            CreateMap<DisplayScreen, GetDisplayScreen>()
                // FIX: renamed destination member from getDisplayTickets to DisplayTickets
                .ForMember(dest => dest.DisplayTickets, opt => opt.MapFrom(src => src.DisplayTickets));
        }
    }
}