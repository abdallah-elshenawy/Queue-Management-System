using AutoMapper;
using QMS.Application.DTOs.Service;
using QMS.Application.Responses;
using QMS.Domain.Models;
using QMS.Application.Abstracts;
using QMS.Domain.IRepositories;

namespace QMS.Application.Services
{
    public class QueueService : IQueueService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public QueueService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }
        public async Task<ApiResponse<List<GetService>>> GetAllAsync()
        {
            var services = await unitOfWork.ServiceRepo.GetAllAsync();
            var getServices = mapper.Map<List<GetService>>(services);
            return ApiResponse<List<GetService>>.SuccessResponse(getServices);
        }

        public async Task<ApiResponse<GetService>> GetByIdAsync(int id)
        {
            var service = await unitOfWork.ServiceRepo.GetByIdAsync(id);
            if (service == null)
                return ApiResponse<GetService>.FailResponse("Service not found");

            var getService = mapper.Map<GetService>(service);   
            return ApiResponse<GetService>.SuccessResponse(getService);
        }
        public async Task<ApiResponse<string>> CreateServiceAsync(CreateService createService)
        {
            var service = mapper.Map<Service>(createService);
            await unitOfWork.ServiceRepo.AddAsync(service);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Service created successfully");
        }
        public async Task<ApiResponse<string>> UpdateServiceAsync(UpdateService updateService, int id)
        {
            var service = await unitOfWork.ServiceRepo.GetByIdAsync(id);
            if (service == null)
                return ApiResponse<string>.FailResponse("Service not found");

            mapper.Map(updateService, service);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Service updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteServiceAsync(int id)
        {
            var service = await unitOfWork.ServiceRepo.GetByIdAsync(id);
            if (service == null)
                return ApiResponse<string>.FailResponse("Service not found");


            unitOfWork.ServiceRepo.Delete(service);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Service deleted successfully");
        }
    }
}
