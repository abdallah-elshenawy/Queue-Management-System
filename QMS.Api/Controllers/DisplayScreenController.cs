using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QMS.Application.Services;
using QMS.Application.Abstracts;

namespace QMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisplayScreenController : ControllerBase
    {
        private readonly IDisplayScreenService displayScreenService;

        public DisplayScreenController(IDisplayScreenService displayScreenService)
        {
            this.displayScreenService = displayScreenService;
        }

        [HttpGet("{branchId:int}")]
        [Authorize]
        public async Task<IActionResult> GetScreen(int branchId)
        {
            var result = await displayScreenService.GetDisplayData(branchId);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}
