using GameAssetStorage.Data;
using GameAssetStorage.Models;
using GameAssetStorage.Services; // Added this crucial using directive
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BCrypt.Net;

namespace GameAssetStorage.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;

        public AuthController(
            AppDbContext context,
            ILogger<AuthController> logger,
            IUserService userService)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(registrationDto.username))
                    return BadRequest("Username is required");
                if (string.IsNullOrWhiteSpace(registrationDto.password))
                    return BadRequest("Password is required");
                if (registrationDto.password.Length < 8)
                    return BadRequest("Password must be at least 8 characters");

                var user = await _userService.Register(
                    registrationDto.username,
                    registrationDto.password);

                return Ok(new { user.username });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                var user = await _userService.Authenticate(
                    loginDto.username,
                    loginDto.password);

                if (user == null)
                    return Unauthorized("Invalid username or password");

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("created_at", user.created_at.ToString("O"))
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddHours(1)
                    });

                return Ok(new
                {
                    Message = "Logged in successfully",
                    username = user.username,
                    isAdmin = user.is_admin
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, "An error occurred during login");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpGet("check-auth")]
        [Authorize]
        public IActionResult CheckAuth()
        {
            var username = User.Identity?.Name;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (username == null || userId == null)
                return Unauthorized();

            return Ok(new
            {
                username = username,
                UserId = userId
            });
        }
    }

    public class UserRegistrationDto
    {
        public required string username { get; set; } = string.Empty;
        public required string password { get; set; } = string.Empty;
    }

    public class UserLoginDto
    {
        public required string username { get; set; } = string.Empty;
        public required string password { get; set; } = string.Empty;
    }
}