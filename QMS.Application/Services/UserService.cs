using System.Linq.Expressions;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using QMS.Application.DTOs.User;
using QMS.Application.Responses;
using QMS.Domain.Enums;
using QMS.Domain.Models;
using QMS.Application.Abstracts;
using QMS.Domain.IRepositories;

namespace QMS.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IConfiguration config;
        private readonly ITokenService tokenService;
        private readonly IUtilityService utilityService;
        private readonly IFileService fileService;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration config,
            ITokenService tokenService, IUtilityService utilityService, IFileService fileService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.config = config;
            this.tokenService = tokenService;
            this.utilityService = utilityService;
            this.fileService = fileService;
        }

        // ── Shared validation helper to avoid repeating the three uniqueness checks ──────────────
        private async Task<string?> GetDuplicateUserError(string email, string username, string nationalNumber)
        {
            if (await unitOfWork.UserRepo.IsEmailExist(email))
                return $"The email '{email}' is already in use.";

            if (await unitOfWork.UserRepo.IsUsernameExist(username))
                return $"The username '{username}' is already in use.";

            if (await unitOfWork.UserRepo.IsNationalNumberExist(nationalNumber))
                return $"The national number '{nationalNumber}' is already in use.";

            return null;
        }

        public async Task<ApiResponse<string>> RegisterAdminAsync(CreateUser createAdmin)
        {
            var duplicateError = await GetDuplicateUserError(createAdmin.Email, createAdmin.Username, createAdmin.NationalNumber);
            if (duplicateError != null)
                return ApiResponse<string>.FailResponse(duplicateError);

            if (!utilityService.TryValidatePassword(createAdmin.Password, out string error))
                return ApiResponse<string>.FailResponse(error);

            User user = mapper.Map<User>(createAdmin);
            utilityService.MappingFullName(user, createAdmin.FullName);
            user.Role = Role.Admin;
            // FIX: HashPassword now returns the hash instead of using a broken out-param
            user.PasswordHash = utilityService.HashPassword(createAdmin.Password);

            await unitOfWork.UserRepo.AddAsync(user);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Admin registered successfully.");
        }

        public async Task<ApiResponse<string>> RegisterCustomerAsync(CreateUser createCustomer)
        {
            var duplicateError = await GetDuplicateUserError(createCustomer.Email, createCustomer.Username, createCustomer.NationalNumber);
            if (duplicateError != null)
                return ApiResponse<string>.FailResponse(duplicateError);

            if (!utilityService.TryValidatePassword(createCustomer.Password, out string error))
                return ApiResponse<string>.FailResponse(error);

            User user = mapper.Map<User>(createCustomer);
            utilityService.MappingFullName(user, createCustomer.FullName);
            user.Role = Role.Customer;
            user.PasswordHash = utilityService.HashPassword(createCustomer.Password);

            CustomerInfo customerInfo = new CustomerInfo { User = user };

            await unitOfWork.UserRepo.AddAsync(user);
            await unitOfWork.CustomerRepo.AddAsync(customerInfo);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Customer registered successfully.");
        }

        public async Task<ApiResponse<string>> RegisterCounterAsync(CreateCounter createCounter)
        {
            var duplicateError = await GetDuplicateUserError(createCounter.Email, createCounter.Username, createCounter.NationalNumber);
            if (duplicateError != null)
                return ApiResponse<string>.FailResponse(duplicateError);

            if (!utilityService.TryValidatePassword(createCounter.Password, out string error))
                return ApiResponse<string>.FailResponse(error);

            User user = mapper.Map<User>(createCounter);
            utilityService.MappingFullName(user, createCounter.FullName);
            user.Role = Role.Counter;
            user.PasswordHash = utilityService.HashPassword(createCounter.Password);

            EmployeeInfo employeeInfo = mapper.Map<EmployeeInfo>(createCounter);
            employeeInfo.User = user;

            await unitOfWork.UserRepo.AddAsync(user);
            await unitOfWork.EmployeeRepo.AddAsync(employeeInfo);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Counter employee registered successfully.");
        }

        public async Task<ApiResponse<string>> RegisterDoorVerifierAsync(CreateDoorVerifier createDoorVerifier)
        {
            var duplicateError = await GetDuplicateUserError(createDoorVerifier.Email, createDoorVerifier.Username, createDoorVerifier.NationalNumber);
            if (duplicateError != null)
                return ApiResponse<string>.FailResponse(duplicateError);

            if (!utilityService.TryValidatePassword(createDoorVerifier.Password, out string error))
                return ApiResponse<string>.FailResponse(error);

            User user = mapper.Map<User>(createDoorVerifier);
            utilityService.MappingFullName(user, createDoorVerifier.FullName);
            user.Role = Role.DoorVerifier;
            user.PasswordHash = utilityService.HashPassword(createDoorVerifier.Password);

            EmployeeInfo employeeInfo = mapper.Map<EmployeeInfo>(createDoorVerifier);
            employeeInfo.User = user;

            await unitOfWork.UserRepo.AddAsync(user);
            await unitOfWork.EmployeeRepo.AddAsync(employeeInfo);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Door verifier registered successfully.");
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginUser loginUser)
        {
            User user = await unitOfWork.UserRepo.GetByEmailAsync(loginUser.Email);

            // FIX: also reject inactive (soft-deleted) accounts
            if (user == null || !user.IsActive == true || !unitOfWork.UserRepo.CheckPassword(loginUser.Password, user.PasswordHash))
                return ApiResponse<LoginResponse>.FailResponse("Invalid email or password.");

            string accessToken = tokenService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());

            RefreshToken refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = tokenService.GenerateRefreshToken(),
            };
            await unitOfWork.RefreshTokenRepo.AddAsync(refreshToken);
            await unitOfWork.SaveChangesAsync();

            return ApiResponse<LoginResponse>.SuccessResponse(new LoginResponse
            {
                AccessToken = accessToken,
                AccessTokenExpirationInSeconds = config.GetValue<int>("Jwt:TokenValidityInMinutes") * 60,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiresAt
            });
        }

        public async Task<ApiResponse<RefreshTokenResponse>> RefreshTokenAsync(string token)
        {
            RefreshToken refreshToken = await unitOfWork.RefreshTokenRepo.GetByTokenAsync(token);

            if (refreshToken == null || refreshToken.ExpiresAt <= DateTime.UtcNow)
                return ApiResponse<RefreshTokenResponse>.FailResponse("Invalid or expired refresh token.");

            // Reuse detection: if the token was already revoked someone is replaying a stolen token.
            // Revoke ALL tokens for this user to force a fresh login.
            if (refreshToken.IsRevoked)
            {
                await unitOfWork.RefreshTokenRepo.RevokeAllUserTokensAsync(refreshToken.UserId);
                await unitOfWork.SaveChangesAsync();
                return ApiResponse<RefreshTokenResponse>.FailResponse("Refresh token already used. Please log in again.");
            }

            await unitOfWork.RefreshTokenRepo.RevokeRefreshTokenAsync(refreshToken);

            User user = await unitOfWork.UserRepo.GetByIdAsync(refreshToken.UserId);
            if (user == null)
                return ApiResponse<RefreshTokenResponse>.FailResponse("User not found.");

            string newAccessToken = tokenService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
            RefreshToken newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = tokenService.GenerateRefreshToken(),
            };

            await unitOfWork.RefreshTokenRepo.AddAsync(newRefreshToken);
            await unitOfWork.SaveChangesAsync();

            return ApiResponse<RefreshTokenResponse>.SuccessResponse(new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            });
        }

        public async Task<ApiResponse<string>> LogoutAsync(string token)
        {
            RefreshToken refreshToken = await unitOfWork.RefreshTokenRepo.GetByTokenAsync(token);
            if (refreshToken == null)
                return ApiResponse<string>.FailResponse("Refresh token not found.");

            await unitOfWork.RefreshTokenRepo.RevokeRefreshTokenAsync(refreshToken);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Logged out successfully.");
        }

        public async Task<ApiResponse<GetUser>> GetByIdAsync(int id, bool includeInfo = false)
        {
            var includes = new List<Expression<Func<User, object>>>();
            if (includeInfo)
            {
                includes.Add(u => u.CustomerInfo);
                includes.Add(u => u.EmployeeInfo);
            }

            User user = await unitOfWork.UserRepo.GetByIdAsync(id, includes.ToArray());
            if (user == null)
                return ApiResponse<GetUser>.FailResponse("User not found.");

            GetUser getUser = mapper.Map<GetUser>(user);
            return ApiResponse<GetUser>.SuccessResponse(getUser);
        }

        public async Task<ApiResponse<List<GetUser>>> GetAllAsync(bool includeInfo = false)
        {
            var includes = new List<Expression<Func<User, object>>>();
            if (includeInfo)
            {
                includes.Add(u => u.CustomerInfo);
                includes.Add(u => u.EmployeeInfo);
            }

            var users = await unitOfWork.UserRepo.GetAllAsync(includes.ToArray());
            List<GetUser> getUsers = mapper.Map<List<GetUser>>(users);
            return ApiResponse<List<GetUser>>.SuccessResponse(getUsers);
        }

        public async Task<ApiResponse<List<GetUser>>> GetByRoleAsync(Role role, bool includeInfo = false)
        {
            var users = await unitOfWork.UserRepo.GetByRoleAsync(role, includeInfo);
            List<GetUser> getUsers = mapper.Map<List<GetUser>>(users);
            return ApiResponse<List<GetUser>>.SuccessResponse(getUsers);
        }

        public async Task<ApiResponse<GetUser>> GetProfileAsync(Claim claimId)
        {
            if (!int.TryParse(claimId?.Value, out int userId))
                return ApiResponse<GetUser>.FailResponse("You don't have access.");

            User user = await unitOfWork.UserRepo.GetByIdAsync(userId,
                u => u.CustomerInfo, u => u.EmployeeInfo);

            if (user == null)
                return ApiResponse<GetUser>.FailResponse("User not found.");

            return ApiResponse<GetUser>.SuccessResponse(mapper.Map<GetUser>(user));
        }

        public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePassword changePassword, Claim claimId)
        {
            if (!int.TryParse(claimId?.Value, out int userId))
                return ApiResponse<string>.FailResponse("You don't have access.");

            User user = await unitOfWork.UserRepo.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<string>.FailResponse("User not found.");

            if (!unitOfWork.UserRepo.CheckPassword(changePassword.OldPassword, user.PasswordHash))
                return ApiResponse<string>.FailResponse("Old password is incorrect.");

            if (!utilityService.TryValidatePassword(changePassword.NewPassword, out string error))
                return ApiResponse<string>.FailResponse(error);

            // FIX: was hashing changePassword.OldPassword instead of NewPassword
            user.PasswordHash = utilityService.HashPassword(changePassword.NewPassword);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Password changed successfully.");
        }

        public async Task<ApiResponse<string>> UpladeImageAsync(IFormFile imageData, Claim claimId)
        {
            if (!int.TryParse(claimId?.Value, out int userId))
                return ApiResponse<string>.FailResponse("You don't have access.");

            User user = await unitOfWork.UserRepo.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<string>.FailResponse("User not found.");

            if (imageData == null || imageData.Length == 0)
                return ApiResponse<string>.FailResponse("No image file provided.");

            string imageUrl = await fileService.SaveUserImageAsync(imageData);
            user.ImageUrl = imageUrl;
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Image uploaded successfully.");
        }

        public async Task<ApiResponse<string>> DeleteAsync(Claim claimId)
        {
            if (!int.TryParse(claimId?.Value, out int userId))
                return ApiResponse<string>.FailResponse("You don't have access.");

            User user = await unitOfWork.UserRepo.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<string>.FailResponse("User not found.");

            // Soft-delete. A background job should hard-delete inactive accounts after 30 days.
            user.IsActive = false;
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse(
                "Your account has been deactivated. It will be permanently deleted after 30 days.");
        }

        public async Task<ApiResponse<string>> UpdateAsync(UpdateUser updateUser, Claim claimId)
        {
            if (!int.TryParse(claimId?.Value, out int userId))
                return ApiResponse<string>.FailResponse("You don't have access.");

            User user = await unitOfWork.UserRepo.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<string>.FailResponse("User not found.");

            mapper.Map(updateUser, user);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Profile updated successfully.");
        }

        public async Task<ApiResponse<string>> UpdateEmployeeAsync(UpdateEmployee updateEmployee, int id)
        {
            // FIX: EmployeeInfo's PK is UserId, not Id — GetByIdAsync(id) used EF.Property<int>("Id")
            // which always returned null. Use GetAsync(predicate, includes) instead.
            EmployeeInfo employeeInfo = await unitOfWork.EmployeeRepo.GetAsync(
                e => e.UserId == id, e => e.User);

            if (employeeInfo == null)
                return ApiResponse<string>.FailResponse("Employee not found.");

            mapper.Map(updateEmployee, employeeInfo);
            unitOfWork.EmployeeRepo.Update(employeeInfo);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("Employee updated successfully.");
        }
    }
}