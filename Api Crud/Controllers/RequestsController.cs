using System.Security.Claims;
using DzDex.API.Data;
using DzDex.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DzDex.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/requests")]
    public class RequestsController : ControllerBase
    {
        private readonly DzDexContext _context;

        public RequestsController(DzDexContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingRequests()
        {
            var requests = await _context.ItemEditRequests
                .AsNoTracking()
                .Include(request => request.Item)
                .Include(request => request.SolicitadoPor)
                .Where(request => request.Status == "Pending")
                .OrderByDescending(request => request.CriadoEm)
                .Select(request => new
                {
                    request.Id,
                    request.ItemId,
                    ItemNome = request.Item != null ? request.Item.Nome : request.NomeAtual,
                    request.NomeAtual,
                    request.DescricaoAtual,
                    request.NomeProposto,
                    request.DescricaoProposta,
                    request.CriadoEm,
                    SolicitadoPorNome = request.SolicitadoPor != null ? request.SolicitadoPor.Nome : "Usuario removido",
                    SolicitadoPorEmail = request.SolicitadoPor != null ? request.SolicitadoPor.Email : string.Empty
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost("{id}/aprovar")]
        public async Task<IActionResult> Aprovar(int id, [FromBody] ItemEditDecisionDto? dto)
        {
            var request = await _context.ItemEditRequests
                .Include(item => item.Item)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (request == null)
                return NotFound(new { message = "Solicitacao nao encontrada." });

            if (request.Status != "Pending")
                return BadRequest(new { message = "Essa solicitacao ja foi processada." });

            if (request.Item == null)
                return NotFound(new { message = "Item da solicitacao nao encontrado." });

            request.Item.Nome = request.NomeProposto;
            request.Item.Descricao = request.DescricaoProposta;
            request.Item.AtualizadoEm = DateTime.UtcNow;
            request.Status = "Approved";
            request.Observacao = dto?.Observacao?.Trim();
            request.ResolvidoEm = DateTime.UtcNow;
            request.ResolvidoPorId = GetCurrentUserId();

            await _context.SaveChangesAsync();

            return Ok(new { message = "Solicitacao aprovada com sucesso." });
        }

        [HttpPost("{id}/recusar")]
        public async Task<IActionResult> Recusar(int id, [FromBody] ItemEditDecisionDto? dto)
        {
            var request = await _context.ItemEditRequests.FirstOrDefaultAsync(item => item.Id == id);

            if (request == null)
                return NotFound(new { message = "Solicitacao nao encontrada." });

            if (request.Status != "Pending")
                return BadRequest(new { message = "Essa solicitacao ja foi processada." });

            request.Status = "Rejected";
            request.Observacao = dto?.Observacao?.Trim();
            request.ResolvidoEm = DateTime.UtcNow;
            request.ResolvidoPorId = GetCurrentUserId();

            await _context.SaveChangesAsync();

            return Ok(new { message = "Solicitacao recusada." });
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var userId) ? userId : null;
        }
    }
}