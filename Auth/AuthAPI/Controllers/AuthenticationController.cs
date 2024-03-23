using AuthAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthAPI.Controllers
{
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService; // Interface for user management logic

        public AuthenticationController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _userService.GetUserByUsername(request.Username);
                if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                {
                    return Unauthorized("Invalid username or password");
                }

                var tokenString = GenerateToken(user.Id);
                return Ok(new { token = tokenString });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        private bool VerifyPassword(string providedPassword, string storedPasswordHash)
        {
            // Use a secure password hashing library (e.g., BCrypt.Net.NS)
            return BCrypt.Net.BCrypt.Verify(providedPassword, storedPasswordHash);
        }
        private string GenerateToken(int userId)
        {
            var secret = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]); // Retrieve secret from configuration
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("userId", userId.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
