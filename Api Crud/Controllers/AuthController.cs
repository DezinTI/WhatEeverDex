using Microsoft.AspNetCore.Mvc;

namespace DzDex.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly Dictionary<string, string> Usuarios = new Dictionary<string, string>
        {
            { "admin", "admin" },
            { "user", "user" }
        };

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (Usuarios.TryGetValue(request.Username.ToLower(), out var senha) && senha == request.Password)
            {
                return Ok(new { role = request.Username == "admin" ? "admin" : "user" });
            }

            return Unauthorized(new { message = "Credenciais invÃ¡lidas" });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

