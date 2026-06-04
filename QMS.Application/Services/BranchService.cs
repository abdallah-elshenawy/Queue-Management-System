using AutoMapper;
using QMS.Application.Abstracts;
using QMS.Application.DTOs.Branch;
using QMS.Application.Responses;
using QMS.Domain.IRepositories;
using QMS.Domain.Models;
using System.Linq.Expressions;

namespace QMS.Application.Services
{
    public class BranchService : IBranchService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public BranchService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<ApiResponse<List<GetBranch>>> GetAllAsync(bool includeInfo = false)
        {
            var includes = new List<Expression<Func<Branch, object>>>();
            if (includeInfo)
            {
                includes.Add(b => b.Employees);
                includes.Add(b => b.Tickets);
            }

            var branches = await unitOfWork.BranchRepo.GetAllAsync(includes.ToArray());
            var getBranches = mapper.Map<List<GetBranch>>(branches);
            return ApiResponse<List<GetBranch>>.SuccessResponse(getBranches);
        }

        public async Task<ApiResponse<GetBranch>> GetByIdAsync(int id, bool includeInfo = false)
        {
            var includes = new List<Expression<Func<Branch, object>>>();
            if (includeInfo)
            {
                includes.Add(b => b.Employees);
                includes.Add(b => b.Tickets);
            }

            Branch branch = await unitOfWork.BranchRepo.GetByIdAsync(id, includes.ToArray());
            if (branch == null)
                return ApiResponse<GetBranch>.FailResponse("Branch not found.");

            return ApiResponse<GetBranch>.SuccessResponse(mapper.Map<GetBranch>(branch));
        }

        public async Task<ApiResponse<string>> CreateAsync(CreateBranch createBranch)
        {
            Branch branch = mapper.Map<Branch>(createBranch);

            // FIX: old code did AddAsync(DisplayScreen { BranchId = branch.Id }) BEFORE saving,
            // so branch.Id was still 0 (EF hadn't assigned the DB identity yet).
            // Use the navigation property so EF resolves the FK after the INSERT.
            branch.Display = new DisplayScreen { IsActive = true };

            await unitOfWork.BranchRepo.AddAsync(branch);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Branch created successfully.");
        }

        public async Task<ApiResponse<string>> UpdateAsync(UpdateBranch updateBranch, int id)
        {
            Branch branch = await unitOfWork.BranchRepo.GetByIdAsync(id);
            if (branch == null)
                return ApiResponse<string>.FailResponse("Branch not found.");

            mapper.Map(updateBranch, branch);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Branch updated successfully.");
        }

        public async Task<ApiResponse<string>> ToggleStatusAsync(int id)
        {
            Branch branch = await unitOfWork.BranchRepo.GetByIdAsync(id);
            if (branch == null)
                return ApiResponse<string>.FailResponse("Branch not found.");

            branch.IsActive = !branch.IsActive;
            await unitOfWork.SaveChangesAsync();

            string status = branch.IsActive == true ? "activated" : "deactivated";
            return ApiResponse<string>.SuccessResponse($"Branch {status} successfully.");
        }

        public async Task<ApiResponse<string>> HardDeleteAsync(int id)
        {
            Branch branch = await unitOfWork.BranchRepo.GetByIdAsync(id);
            if (branch == null)
                return ApiResponse<string>.FailResponse("Branch not found.");

            unitOfWork.BranchRepo.Delete(branch);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Branch deleted successfully.");
        }
    }
}