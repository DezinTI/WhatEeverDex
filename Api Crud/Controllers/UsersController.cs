using DzDex.API.Data;
using DzDex.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DzDex.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly DzDexContext _context;

        public UsersController(DzDexContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .OrderBy(item => item.Nome)
                .Select(item => new
                {
                    item.Id,
                    item.Nome,
                    item.Email,
                    item.Role,
                    item.CriadoEm,
                    item.UltimoLoginEm,
                    TotalRegistros = item.RegistrosCriados.Count
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpPatch("{id}/role")]
        public async Task<IActionResult> AlterarRole(int id, [FromBody] AlterarRoleRequest request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var role = request.Role.Trim();
            if (role != "Admin" && role != "User")
                return BadRequest(new { message = "Role invalida. Use Admin ou User." });

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(item => item.Id == id);
            if (usuario == null)
                return NotFound(new { message = "Usuario nao encontrado." });

            usuario.Role = role;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Permissao atualizada com sucesso." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var usuario = await _context.Usuarios
                .Include(item => item.RegistrosCriados)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (usuario == null)
                return NotFound(new { message = "Usuario nao encontrado." });

            foreach (var item in usuario.RegistrosCriados)
            {
                item.CriadoPorId = null;
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario excluido com sucesso." });
        }
    }
}
