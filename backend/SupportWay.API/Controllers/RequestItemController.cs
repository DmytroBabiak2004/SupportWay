using Microsoft.AspNetCore.Mvc;
using SupportWay.Data.Models;
using SupportWay.Services;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestItemsController : ControllerBase
    {
        private readonly RequestItemService _service;

        public RequestItemsController(RequestItemService service)
        {
            _service = service;
        }

        [HttpGet("by-helprequest/{helpRequestId}")]
        public async Task<IActionResult> GetByHelpRequestId(int helpRequestId)
        {
            var items = await _service.GetByHelpRequestIdAsync(helpRequestId);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RequestItem item)
        {
            await _service.AddAsync(item);
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RequestItem item)
        {
            if (id != item.Id)
                return BadRequest("ID in URL does not match ID in body");

            await _service.UpdateAsync(item);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
