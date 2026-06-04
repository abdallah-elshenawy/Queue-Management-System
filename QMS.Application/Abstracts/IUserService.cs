using AutoMapper;
using Microsoft.AspNetCore.Http;
using QMS.Application.DTOs.User;
using QMS.Application.Responses;
using QMS.Domain.Enums;
using QMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.Abstracts
{
    public interface IUserService
    {
        Task<ApiResponse<string>> RegisterAdminAsync(CreateUser createAdmin);     // it's also used for admin registration
        Task<ApiResponse<string>> RegisterCustomerAsync(CreateUser createCustomer);
        Task<ApiResponse<string>> RegisterCounterAsync(CreateCounter createCounter);
        Task<ApiResponse<string>> RegisterDoorVerifierAsync(CreateDoorVerifier createDoorVerifier);
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginUser loginUser);
        Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync(string token);
        Task<ApiResponse<string>> LogoutAsync(string token);
        Task<ApiResponse<GetUser>> GetByIdAsync(int id, bool includeInfo = false);
        Task<ApiResponse<List<GetUser>>> GetAllAsync(bool includeInfo = false);
        Task<ApiResponse<GetUser>> GetProfileAsync(Claim claimId);
        Task<ApiResponse<string>> ChangePasswordAsync(ChangePassword changePassword, Claim claimId);
        Task<ApiResponse<List<GetUser>>> GetByRoleAsync(Role role, bool includeInfo = false);
        Task<ApiResponse<string>> UpladeImageAsync(IFormFile imageData, Claim claimId);
        Task<ApiResponse<string>> DeleteAsync(Claim claimId);
        Task<ApiResponse<string>> UpdateAsync(UpdateUser updateUser, Claim claimId);
        Task<ApiResponse<string>> UpdateEmployeeAsync(UpdateEmployee updateEmployee, int id);
    }
}
