using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs;
using SupportWay.Data.Models;
using SupportWay.Services;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestItemsController : ControllerBase
    {
        private readonly IRequestItemService _service;

        public RequestItemsController(IRequestItemService service)
        {
            _service = service;
        }

        [HttpGet("by-helprequest/{helpRequestId}")]
        public async Task<IActionResult> GetByHelpRequestId(Guid helpRequestId)
        {
            var items = await _service.GetByHelpRequestIdAsync(helpRequestId);
            // Бажано також мапити результат у List<RequestItemDto>
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRequestItemDto dto)
        {
            var item = new RequestItem
            {
                Id = Guid.NewGuid(),
                HelpRequestId = dto.HelpRequestId,
                Name = dto.Name,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                SupportTypeId = dto.SupportTypeId
            };

            await _service.AddAsync(item);
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateRequestItemDto dto)
        {
            var existingItem = await _service.GetByIdAsync(id);
            if (existingItem == null) return NotFound();

            existingItem.Name = dto.Name;
            existingItem.Quantity = dto.Quantity;
            existingItem.UnitPrice = dto.UnitPrice;
            existingItem.SupportTypeId = dto.SupportTypeId;

            await _service.UpdateAsync(existingItem);
            return NoContent();
        }
 

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}