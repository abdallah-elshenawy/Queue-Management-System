using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QMS.Application.DTOs.Service;
using QMS.Application.Abstracts;

namespace QMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IQueueService queueService;

        public ServiceController(IQueueService queueService)
        {
            this.queueService = queueService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var response = await queueService.GetAllAsync();
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await queueService.GetByIdAsync(id);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateService createService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await queueService.CreateServiceAsync(createService);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(UpdateService updateService, int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await queueService.UpdateServiceAsync(updateService, id);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await queueService.DeleteServiceAsync(id);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
