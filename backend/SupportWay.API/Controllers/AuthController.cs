using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SupportWay.API.Models;
using SupportWay.API.Services.Interface;
using SupportWay.Data.Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IProfileService _profileService;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            IProfileService profileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _profileService = profileService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid || request == null)
            {
                return BadRequest("Invalid registration data");
            }

            var user = new User { UserName = request.Username };
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }
            //await _profileService.AddProfileAsync(user.Id);

            if (!string.IsNullOrEmpty(request.Role))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
                if (!roleResult.Succeeded)
                {
                    return BadRequest(new { Errors = roleResult.Errors.Select(e => e.Description) });
                }
            }
            var roles = await _userManager.GetRolesAsync(user);
            var token = await GenerateJwtToken(user);
            return Ok(new
            {
                id = user.Id,
                username = user.UserName,
                Roles = roles,
                Token = token
            });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid || request == null)
            {
                return BadRequest("Invalid login data");
            }

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid username or password");
            }

            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                id = user.Id,
                username = user.UserName,
                Roles = roles,
                Token = token
            });

        }

        [HttpGet("check-session")]
        [Authorize]
        public async Task<IActionResult> CheckSession()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User not identified");
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return Unauthorized("User not found");
            }
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                Message = "Session is valid",
                Username = username,
                Roles = roles,
                id = user.Id
            });
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var roles = await _userManager.GetRolesAsync(user);
            var username = user.UserName ?? throw new InvalidOperationException("UserName cannot be null");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            claims.AddRange(roles.Where(role => role != null).Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }
    }
}