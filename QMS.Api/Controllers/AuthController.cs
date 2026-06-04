using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QMS.Application.DTOs.Ticket;
using QMS.Application.DTOs.Token;
using QMS.Application.DTOs.User;
using QMS.Application.Abstracts;
using QMS.Domain.Enums;
using System.Security.Claims;

namespace QMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly ITicketService ticketService;

        public AuthController(IUserService userService, ITicketService ticketService)
        {
            this.userService = userService;
            this.ticketService = ticketService;
        }

        // FIX: was missing [Authorize(Roles = "DoorVerifier")] — any anonymous caller could hit it
        [HttpPost("door-verifying")]
        [Authorize(Roles = "DoorVerifier")]
        public async Task<IActionResult> DoorVerifying([FromBody] DoorVerifyingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Claim claimDoorId = User.FindFirst(ClaimTypes.NameIdentifier);
            var result = await ticketService.TicketVerifyingAsync(claimDoorId, request.QrCode);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("call-next")]
        [Authorize(Roles = "Counter")]
        public async Task<IActionResult> CallNext()
        {
            Claim empClaimId = User.FindFirst(ClaimTypes.NameIdentifier);
            var result = await ticketService.CallNextTicketAsync(empClaimId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("complete/{id:int}")]
        [Authorize(Roles = "Counter")]
        public async Task<IActionResult> Complete(int id)
        {
            Claim empClaimId = User.FindFirst(ClaimTypes.NameIdentifier);
            var result = await ticketService.CompleteTicketAsync(empClaimId, id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("register-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterAdmin(CreateUser createUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await userService.RegisterAdminAsync(createUser);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("register-counter")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterCounter(CreateCounter createCounter)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await userService.RegisterCounterAsync(createCounter);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("register-door-verifier")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterDoorVerifier(CreateDoorVerifier createDoorVerifier)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await userService.RegisterDoorVerifierAsync(createDoorVerifier);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer(CreateUser createCustomer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await userService.RegisterCustomerAsync(createCustomer);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUser loginUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await userService.LoginAsync(loginUser);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequest request)
        {
            var result = await userService.RefreshTokenAsync(request.Token);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // FIX: was [HttpPost] with no route — mapped to POST /api/auth which is confusing
        // and clashed conceptually with login. Added explicit "logout" route.
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(string token)
        {
            var result = await userService.LogoutAsync(token);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id, bool includeInfo = false)
        {
            var result = await userService.GetByIdAsync(id, includeInfo);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("get-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(bool includeInfo = false)
        {
            var result = await userService.GetAllAsync(includeInfo);
            return Ok(result);
        }

        [HttpGet("get-by-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByRole(Role role, bool includeInfo = false)
        {
            var result = await userService.GetByRoleAsync(role, includeInfo);
            return Ok(result);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            Claim claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            var result = await userService.GetProfileAsync(claimId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        // FIX: was missing [Authorize] — unauthenticated callers got a NullReferenceException
        // because User.FindFirst() returned null
        [HttpPatch("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePassword changePassword)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Claim claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            var result = await userService.ChangePasswordAsync(changePassword, claimId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // FIX: was missing [Authorize] — same NullReferenceException risk
        [HttpPatch("upload-image")]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile imageData)
        {
            Claim claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            var result = await userService.UpladeImageAsync(imageData, claimId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // FIX: route was [HttpDelete("{id:int}")] but the method signature had no int parameter —
        // the route variable was silently ignored. Since this is a self-delete (from claim), no id needed.
        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> Delete()
        {
            Claim claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            var result = await userService.DeleteAsync(claimId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPatch("update-user")]
        [Authorize]
        public async Task<IActionResult> Update(UpdateUser updateUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Claim claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            var result = await userService.UpdateAsync(updateUser, claimId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPatch("update-employee/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmployee(UpdateEmployee updateEmployee, int id)
        {
            var result = await userService.UpdateEmployeeAsync(updateEmployee, id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}