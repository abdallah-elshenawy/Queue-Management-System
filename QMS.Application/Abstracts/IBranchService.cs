using QMS.Application.DTOs.Branch;
using QMS.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.Abstracts
{
    public interface IBranchService
    {
        Task<ApiResponse<List<GetBranch>>> GetAllAsync(bool includeInfo = false);
        Task<ApiResponse<GetBranch>> GetByIdAsync(int id, bool includeInfo = false);
        Task<ApiResponse<string>> CreateAsync(CreateBranch createBranch);
        Task<ApiResponse<string>> UpdateAsync(UpdateBranch updateBranch, int id);
        Task<ApiResponse<string>> ToggleStatusAsync(int id);
        Task<ApiResponse<string>> HardDeleteAsync(int id);
    }
}
