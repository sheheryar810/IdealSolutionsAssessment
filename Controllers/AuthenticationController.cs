using Assessment.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Assessment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(AppDbContext context, IConfiguration configuration, ILogger<AuthenticationController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
            {
                _logger.LogWarning("Login request with empty username or password.");
                return BadRequest(new { message = "Username and password are required" });
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == login.Username);
            if (user == null || user.Password != login.Password)
            {
                _logger.LogWarning("Login failed for username: {Username}", login.Username);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var token = GenerateJwtToken(user);

            _logger.LogInformation("Login successful for username: {Username}. Token generated.", login.Username);

            return Ok(new { token });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
            );

            _logger.LogDebug("JWT generated for user: {Username}, expires at: {Expiration}", user.Username, token.ValidTo);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
