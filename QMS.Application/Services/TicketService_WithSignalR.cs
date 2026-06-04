using AutoMapper;
using QMS.Application.DTOs.Notifications;
using QMS.Application.DTOs.Ticket;
using QMS.Application.Responses;
using QMS.Domain.Enums;
using QMS.Domain.Models;
using QMS.Application.Abstracts;
using System.Security.Claims;
using QMS.Domain.IRepositories;

namespace QMS.Application.Services
{
    public class TicketService_WithSignalR : ITicketService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IQueueNotificationService notificationService;

        public TicketService_WithSignalR(IUnitOfWork unitOfWork, IMapper mapper,
            IQueueNotificationService notificationService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.notificationService = notificationService;
        }

        public async Task<ApiResponse<List<GetTicket>>> GetAllAsync()
        {
            List<Ticket> tickets = await unitOfWork.TicketRepo.GetAllAsync(
                t => t.Service,
                t => t.Branch);

            return ApiResponse<List<GetTicket>>.SuccessResponse(mapper.Map<List<GetTicket>>(tickets));
        }

        public async Task<ApiResponse<GetTicket>> GetByIdAsync(int id)
        {
            Ticket ticket = await unitOfWork.TicketRepo.GetByIdAsync(id,
                t => t.Service,
                t => t.Branch,
                t => t.Customer);

            if (ticket == null)
                return ApiResponse<GetTicket>.FailResponse("Ticket not found.");

            GetTicket getTicket = mapper.Map<GetTicket>(ticket);

            if (ticket.Customer?.User != null)
            {
                var u = ticket.Customer.User;
                getTicket.CustomerName = string.Join(" ",
                    new[] { u.FirstName, u.SecondName, u.ThirdName, u.FourthName }
                    .Where(n => !string.IsNullOrEmpty(n)));
            }

            return ApiResponse<GetTicket>.SuccessResponse(getTicket);
        }

        public async Task<ApiResponse<string>> CreateAsync(CreateTicket createTicket, Claim claimId)
        {
            if (!int.TryParse(claimId?.Value, out int customerId))
                return ApiResponse<string>.FailResponse("You don't have access.");

            var user = await unitOfWork.UserRepo.GetByIdAsync(customerId);
            if (user == null)
                return ApiResponse<string>.FailResponse("User not found.");

            Ticket ticket = mapper.Map<Ticket>(createTicket);
            ticket.CustomerId = customerId;
            await unitOfWork.TicketRepo.AddAsync(ticket);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse($"Ticket '{ticket.TicketNumber}' created successfully.");
        }

        public async Task<ApiResponse<string>> TicketVerifyingAsync(Claim claimDoorId, string qrCode)
        {
            if (!int.TryParse(claimDoorId?.Value, out _))
                return ApiResponse<string>.FailResponse("You don't have access.");

            var ticket = await unitOfWork.TicketRepo.GetAsync(t => t.QRCodeData == qrCode);
            if (ticket == null)
                return ApiResponse<string>.FailResponse("Invalid QR code.");

            if (ticket.Status == TicketStatus.Completed || ticket.Status == TicketStatus.Serving)
                return ApiResponse<string>.FailResponse("This ticket has already been used.");

            ticket.IsVerifiedAtDoor = true;
            ticket.VerificationTime = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();

            // Notify the branch so counters know this customer is checked in
            await notificationService.NotifyTicketVerifiedAsync(ticket.BranchId, new TicketVerifiedNotification
            {
                TicketNumber = ticket.TicketNumber,
                VerifiedAt = ticket.VerificationTime.Value
            });

            return ApiResponse<string>.SuccessResponse("Ticket verified successfully.");
        }

        public async Task<ApiResponse<string>> CallNextTicketAsync(Claim claimEmpId)
        {
            if (!int.TryParse(claimEmpId?.Value, out int empId))
                return ApiResponse<string>.FailResponse("You don't have access.");

            EmployeeInfo emp = await unitOfWork.EmployeeRepo.GetAsync(e => e.UserId == empId);
            if (emp == null)
                return ApiResponse<string>.FailResponse("Employee not found.");

            if (emp.ServiceId == null)
                return ApiResponse<string>.FailResponse("This employee is not assigned to a service.");

            Ticket nextTicket = await unitOfWork.TicketRepo.GetNextTicket(emp.ServiceId.Value, emp.BranchId);
            if (nextTicket == null)
                return ApiResponse<string>.FailResponse("No waiting tickets for this service.");

            if (nextTicket.IsVerifiedAtDoor == false)
            {
                nextTicket.Status = TicketStatus.Requeued;
                nextTicket.ReservedAt = DateTime.UtcNow;
                await unitOfWork.SaveChangesAsync();
                return ApiResponse<string>.FailResponse("Customer has not been verified at the door. Ticket requeued.");
            }

            nextTicket.Status = TicketStatus.Serving;
            nextTicket.EmployeeId = empId;
            nextTicket.CalledAt = DateTime.UtcNow;

            var display = await unitOfWork.DisplayScreen.GetAsync(d => d.BranchId == emp.BranchId);
            if (display == null)
                return ApiResponse<string>.FailResponse("No display screen found for this branch.");

            await unitOfWork.DisplayTicket.AddAsync(new DisplayTicket
            {
                TicketId = nextTicket.Id,
                DisplayId = display.Id,
                CounterNumber = emp.CounterNumber,
                CalledAt = DateTime.UtcNow
            });

            await unitOfWork.SaveChangesAsync();

            // Load service name for the notification
            var service = await unitOfWork.ServiceRepo.GetByIdAsync(nextTicket.ServiceId);

            // Broadcast to the display screen and all counter clients in this branch
            await notificationService.NotifyTicketCalledAsync(emp.BranchId, new TicketCalledNotification
            {
                TicketNumber = nextTicket.TicketNumber,
                CounterNumber = emp.CounterNumber,
                ServiceName = service?.Name ?? string.Empty,
                CalledAt = nextTicket.CalledAt.Value
            });

            return ApiResponse<string>.SuccessResponse(
                $"Ticket '{nextTicket.TicketNumber}' is now being served at counter {emp.CounterNumber}.");
        }

        public async Task<ApiResponse<string>> CompleteTicketAsync(Claim claimEmpId, int ticketId)
        {
            if (!int.TryParse(claimEmpId?.Value, out int empId))
                return ApiResponse<string>.FailResponse("You don't have access.");

            Ticket ticket = await unitOfWork.TicketRepo.GetByIdAsync(ticketId);
            if (ticket == null)
                return ApiResponse<string>.FailResponse("Ticket not found.");

            if (ticket.EmployeeId != empId)
                return ApiResponse<string>.FailResponse("You can only complete tickets assigned to you.");

            if (ticket.Status != TicketStatus.Serving)
                return ApiResponse<string>.FailResponse("Only a ticket currently being served can be completed.");

            ticket.Status = TicketStatus.Completed;
            ticket.CompletedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse($"Ticket '{ticket.TicketNumber}' completed.");
        }

        public async Task<ApiResponse<string>> UpdateAsync(UpdateTicket updateTicket, int id, Claim claimId)
        {
            if (!int.TryParse(claimId?.Value, out int customerId))
                return ApiResponse<string>.FailResponse("You don't have access.");

            Ticket ticket = await unitOfWork.TicketRepo.GetByIdAsync(id);
            if (ticket == null)
                return ApiResponse<string>.FailResponse("Ticket not found.");

            if (ticket.CustomerId != customerId)
                return ApiResponse<string>.FailResponse("You can only update your own tickets.");

            mapper.Map(updateTicket, ticket);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse($"Ticket '{ticket.TicketNumber}' updated successfully.");
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            Ticket ticket = await unitOfWork.TicketRepo.GetByIdAsync(id);
            if (ticket == null)
                return ApiResponse<string>.FailResponse("Ticket not found.");

            unitOfWork.TicketRepo.Delete(ticket);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse($"Ticket '{ticket.TicketNumber}' deleted successfully.");
        }
    }
}