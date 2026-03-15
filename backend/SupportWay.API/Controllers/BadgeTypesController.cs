using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs.BadgeTypes;
using SupportWay.API.Services.Interfaces;


namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BadgeTypesController : ControllerBase
    {
        private readonly IBadgeTypeService _badgeTypeService;

        public BadgeTypesController(IBadgeTypeService badgeTypeService)
        {
            _badgeTypeService = badgeTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _badgeTypeService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _badgeTypeService.GetByIdAsync(id);

            if (result == null)
                return NotFound("Тип нагороди не знайдено.");

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBadgeTypeRequest request)
        {
            var id = await _badgeTypeService.CreateAsync(request);
            return Ok(new { Id = id, Message = "Тип нагороди успішно створено." });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateBadgeTypeRequest request)
        {
            await _badgeTypeService.UpdateAsync(request);
            return Ok(new { Message = "Тип нагороди успішно оновлено." });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _badgeTypeService.DeleteAsync(id);
            return Ok(new { Message = "Тип нагороди успішно видалено." });
        }
    }
}