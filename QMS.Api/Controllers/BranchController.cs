using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using QMS.Application.DTOs.Branch;
using QMS.Application.Abstracts;

namespace QMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService branchService;

        public BranchController(IBranchService branchService)
        {
            this.branchService = branchService;
        }

        [HttpGet("get-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(bool includeInfo = false) 
        {
            var result = await branchService.GetAllAsync(includeInfo);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id, bool includeInfo = false)
        {
            var result = await branchService.GetByIdAsync(id, includeInfo);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateBranch createBranch)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var result = await branchService.CreateAsync(createBranch);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(UpdateBranch updateBranch, int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await branchService.UpdateAsync(updateBranch, id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // this endpoint is used for activation and deactivation a branch
        [HttpPatch("toggle-branch/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateDeactivate(int id)
        {
            var result = await branchService.ToggleStatusAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> HardDelete(int id)
        {
            var result = await branchService.HardDeleteAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}

