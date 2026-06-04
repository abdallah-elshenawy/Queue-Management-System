using QMS.Application.DTOs.DisplayScreen;
using QMS.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.Abstracts
{
    public interface IDisplayScreenService
    {
        public Task<ApiResponse<GetDisplayScreen>> GetDisplayData(int branchId);
    }
}
