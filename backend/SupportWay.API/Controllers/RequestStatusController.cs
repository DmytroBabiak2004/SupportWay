using Microsoft.AspNetCore.Mvc;
using SupportWay.Data.DTOs;
using SupportWay.Data.Models;
using SupportWay.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportWay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestStatusController : ControllerBase
    {
        private readonly IRequestStatusService _service;

        public RequestStatusController(IRequestStatusService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestStatusDto>>> GetAll()
        {
            var statuses = await _service.GetAllAsync();
            // Мапінг у DTO
            var dtos = statuses.Select(s => new RequestStatusDto
            {
                Id = s.Id,
                NameOfStatus = s.NameOfStatus
            });

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RequestStatusDto>> GetById(Guid id)
        {
            var status = await _service.GetByIdAsync(id);
            if (status == null) return NotFound();

            return Ok(new RequestStatusDto
            {
                Id = status.Id,
                NameOfStatus = status.NameOfStatus
            });
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateRequestStatusDto dto)
        {
            var status = new RequestStatus
            {
                Id = Guid.NewGuid(),
                NameOfStatus = dto.NameOfStatus
            };

            await _service.AddAsync(status);
            return CreatedAtAction(nameof(GetById), new { id = status.Id }, status);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] CreateRequestStatusDto dto)
        {
            var existingStatus = await _service.GetByIdAsync(id);
            if (existingStatus == null) return NotFound();

            existingStatus.NameOfStatus = dto.NameOfStatus;

            await _service.UpdateAsync(existingStatus);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}