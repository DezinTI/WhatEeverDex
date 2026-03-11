using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DzDex.API.Data;
using DzDex.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DzDex.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly DzDexContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(DzDexContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var email = request.Email.Trim().ToLowerInvariant();
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(item => item.Email == email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
                return Unauthorized(new { message = "Credenciais invalidas." });

            usuario.UltimoLoginEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = GerarToken(usuario),
                user = MapUsuario(usuario)
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var email = request.Email.Trim().ToLowerInvariant();
            var existente = await _context.Usuarios.AnyAsync(item => item.Email == email);
            if (existente)
                return Conflict(new { message = "Ja existe um usuario com esse email." });

            var usuario = new Usuario
            {
                Nome = request.Nome.Trim(),
                Email = email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                Role = "User",
                CriadoEm = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Created("/api/auth/register", new
            {
                message = "Usuario criado com sucesso.",
                user = MapUsuario(usuario)
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out var id))
                return Unauthorized();

            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
            if (usuario == null)
                return Unauthorized();

            return Ok(MapUsuario(usuario));
        }

        private string GerarToken(Usuario usuario)
        {
            var jwt = _configuration.GetSection("Jwt");
            var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key nao configurado.");
            var issuer = jwt["Issuer"] ?? "DzDexAPI";
            var audience = jwt["Audience"] ?? "DzDexClient";
            var expiresInHours = int.TryParse(jwt["ExpiresInHours"], out var horas) ? horas : 24;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role)
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiresInHours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static object MapUsuario(Usuario usuario) => new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role,
            usuario.CriadoEm,
            usuario.UltimoLoginEm
        };
    }
}

