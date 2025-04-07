using GameAssetStorage.Data;
using GameAssetStorage.Models;
using GameAssetStorage.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
                    return BadRequest(new { message = "Username is required" });

                if (string.IsNullOrWhiteSpace(registrationDto.password))
                    return BadRequest(new { message = "Password is required" });

                if (registrationDto.password.Length < 8)
                    return BadRequest(new { message = "Password must be at least 8 characters" });

                var user = await _userService.Register(
                    registrationDto.username,
                    registrationDto.password);

                return Ok(new { username = user.username });
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { message = argEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration");
                return StatusCode(500, new { message = "An unexpected error occurred during registration" });
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
                    return Unauthorized(new { message = "Invalid username or password" });

                if (user.is_banned)
                    return Forbid();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("IsAdmin", user.is_admin ? "true" : "false"),
                    new Claim("is_banned", user.is_banned.ToString())
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
                    message = "Logged in successfully",
                    username = user.username,
                    isAdmin = user.is_admin,
                    userId = user.Id.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return StatusCode(500, new { message = "An unexpected error occurred during login" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("check-auth")]
        [Authorize]
        public IActionResult CheckAuth()
        {
            var username = User.Identity?.Name;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.FindFirstValue("IsAdmin") == "true";

            if (username == null || userId == null)
                return Unauthorized(new { message = "Unauthorized access" });

            return Ok(new
            {
                username = username,
                userId = userId,
                isAdmin = isAdmin
            });
        }

        [HttpPost("edit-profile")]
        [Authorize]
        public async Task<IActionResult> EditProfile([FromBody] EditProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(new { message = "Unauthorized" });

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound(new { message = "User not found" });

            if (!string.IsNullOrWhiteSpace(dto.username))
                user.username = dto.username;

            if (!string.IsNullOrWhiteSpace(dto.password))
                user.password = BCrypt.Net.BCrypt.HashPassword(dto.password);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully." });
        }

        [HttpGet("debug-users")]
        public async Task<IActionResult> DebugUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(new { count = users.Count, users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "DB access error", error = ex.Message });
            }
        }
    }

    public class UserRegistrationDto
    {
        public required string username { get; set; }
        public required string password { get; set; }
    }

    public class UserLoginDto
    {
        public required string username { get; set; }
        public required string password { get; set; }
    }

    public class EditProfileDto
    {
        public string? username { get; set; }
        public string? password { get; set; }
    }
}
