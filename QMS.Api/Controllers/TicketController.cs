using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QMS.Application.DTOs.Ticket;
using QMS.Application.Abstracts;
using System.Security.Claims;

namespace QMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService ticketService;

        public TicketController(ITicketService ticketService)
        {
            this.ticketService = ticketService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var result = await ticketService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await ticketService.GetByIdAsync(id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTicket(CreateTicket createTicket)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var claimId = User.Claims.FirstOrDefault(c => ClaimTypes.NameIdentifier == c.Type);
            var result = await ticketService.CreateAsync(createTicket, claimId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPatch("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateTicket(UpdateTicket updateTicket, int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var claimId = User.Claims.FirstOrDefault(c => ClaimTypes.NameIdentifier == c.Type);
            var result = await ticketService.UpdateAsync(updateTicket, id, claimId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var claimId = User.Claims.FirstOrDefault(c => ClaimTypes.NameIdentifier == c.Type);
            var result = await ticketService.DeleteAsync(id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);

        }
    }
}
