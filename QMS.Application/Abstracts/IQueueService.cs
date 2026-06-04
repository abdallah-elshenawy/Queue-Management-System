using QMS.Application.DTOs.Service;
using QMS.Application.Responses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.Abstracts
{
    public interface IQueueService
    {
        Task<ApiResponse<List<GetService>>> GetAllAsync();
        Task<ApiResponse<GetService>> GetByIdAsync(int id);
        Task<ApiResponse<string>> CreateServiceAsync(CreateService service);
        Task<ApiResponse<string>> UpdateServiceAsync(UpdateService service, int id);
        Task<ApiResponse<string>> DeleteServiceAsync(int id);
    }
}
