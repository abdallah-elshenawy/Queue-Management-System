using AutoMapper;
using QMS.Application.DTOs.DisplayScreen;
using QMS.Application.Responses;
using QMS.Domain.Models;
using QMS.Application.Abstracts;
using QMS.Domain.IRepositories;

namespace QMS.Application.Services
{
    public class DisplayScreenService : IDisplayScreenService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public DisplayScreenService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<ApiResponse<GetDisplayScreen>> GetDisplayData(int branchId)
        {
            Branch branch = await unitOfWork.BranchRepo.GetByIdAsync(branchId);
            if (branch == null)
                return ApiResponse<GetDisplayScreen>.FailResponse("Branch not found.");

            List<DisplayTicket> displayTickets = await unitOfWork.DisplayScreen.GetDisplayData(branchId);

            // FIX: ToListAsync() never returns null — it returns an empty list when there are no rows.
            // An empty display is valid (no tickets have been called yet), so just map and return.
            List<GetDisplayTicket> getDisplayTickets = mapper.Map<List<GetDisplayTicket>>(displayTickets);

            var getDisplayScreen = new GetDisplayScreen
            {
                BranchName = branch.Name,
                DisplayTickets = getDisplayTickets
            };

            return ApiResponse<GetDisplayScreen>.SuccessResponse(getDisplayScreen);
        }
    }
}