using Microsoft.AspNetCore.Mvc;
using SupportWay.API.DTOs;
using SupportWay.Services; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _usersService;

        public UsersController(IUserService usersService)
        {
            _usersService = usersService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<UserSearchDto>>> SearchUsers([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            var users = await _usersService.SearchUsersByNameAsync(name);
            return Ok(users);
        }
    }
}