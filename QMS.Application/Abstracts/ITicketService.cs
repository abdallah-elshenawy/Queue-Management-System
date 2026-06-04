using QMS.Application.DTOs.Branch;
using QMS.Application.DTOs.Ticket;
using QMS.Application.Responses;
using QMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.Abstracts
{
    public interface ITicketService
    {
        Task<ApiResponse<List<GetTicket>>> GetAllAsync();
        Task<ApiResponse<GetTicket>> GetByIdAsync(int id);
        Task<ApiResponse<string>> CreateAsync(CreateTicket createTicket, Claim claimId);
        Task<ApiResponse<string>> UpdateAsync(UpdateTicket updateTicket, int id, Claim claimId);
        Task<ApiResponse<string>> DeleteAsync(int id);
        Task<ApiResponse<string>> CallNextTicketAsync(Claim claimEmpId);
        Task<ApiResponse<string>> TicketVerifyingAsync(Claim claimDoorId, string qrCode);
        Task<ApiResponse<string>> CompleteTicketAsync(Claim claimDoorId, int ticketId);
    }
}
