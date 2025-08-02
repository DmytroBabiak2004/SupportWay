using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SupportWay.API.Models;

namespace SupportWay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid || request == null)
            {
                return BadRequest("Invalid registration data");
            }

            var user = new IdentityUser { UserName = request.Username };
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            if (!string.IsNullOrEmpty(request.Role))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
                if (!roleResult.Succeeded)
                {
                    return BadRequest(new { Errors = roleResult.Errors.Select(e => e.Description) });
                }
            }

            var token = await GenerateJwtToken(user);
            return Ok(new { Token = token });
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
            return Ok(new { Token = token });
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
                Roles = roles
            });
        }

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var roles = await _userManager.GetRolesAsync(user);
            var username = user.UserName ?? throw new InvalidOperationException("UserName cannot be null");

            var claims = new List<Claim>
{
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