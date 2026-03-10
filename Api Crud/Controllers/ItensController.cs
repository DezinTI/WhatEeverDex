using DzDex.API.Data;
using DzDex.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace DzDex.API.Controllers
{
    [ApiController]
    [Route("api/registros")]
    public class ItensController : ControllerBase
    {
        private readonly DzDexContext _context;
        private readonly IWebHostEnvironment _environment;
        public ItensController(DzDexContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetRegistros([FromQuery] string? busca, [FromQuery] string? tipo)
        {
            var query = _context.Itens.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                query = query.Where(m =>
                    m.Nome.ToLower().Contains(termo) ||
                    m.Descricao.ToLower().Contains(termo) ||
                    m.Tipo.ToLower().Contains(termo));
            }

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                var tipoNormalizado = tipo.Trim().ToLower();
                query = query.Where(m => m.Tipo.ToLower() == tipoNormalizado);
            }

            var registros = await query
                .OrderByDescending(m => m.AtualizadoEm)
                .ToListAsync();

            return Ok(registros.Select(MapToResponse));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRegistro(int id)
        {
            var item = await _context.Itens.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(MapToResponse(item));
        }

        [HttpPost]
        public async Task<IActionResult> CreateRegistro([FromForm] ItemCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return BadRequest("O nome e obrigatorio.");

            if (string.IsNullOrWhiteSpace(dto.Tipo))
                return BadRequest("O tipo e obrigatorio.");

            var tipoNormalizado = dto.Tipo.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(dto.VideoYoutubeUrl))
                return BadRequest("O link do video do YouTube e obrigatorio.");

            var imagemUrl = await SalvarImagemOuUsarUrl(dto.ImagemArquivo, dto.ImagemUrl);
            if (string.IsNullOrWhiteSpace(imagemUrl))
                return BadRequest("Envie um arquivo de imagem ou informe uma URL de imagem.");

            var agora = DateTime.UtcNow;

            var item = new Item
            {
                Nome = dto.Nome.Trim(),
                Tipo = tipoNormalizado,
                ImagemUrl = imagemUrl,
                VideoYoutubeUrl = dto.VideoYoutubeUrl.Trim(),
                Descricao = dto.Descricao?.Trim() ?? string.Empty,
                CriadoEm = agora,
                AtualizadoEm = agora
            };

            _context.Itens.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRegistro), new { id = item.Id }, MapToResponse(item));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegistro(int id, [FromForm] ItemUpdateDto dto)
        {
            var item = await _context.Itens.FindAsync(id);
            if (item == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Nome))
                item.Nome = dto.Nome.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Tipo))
            {
                item.Tipo = dto.Tipo.Trim().ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(dto.VideoYoutubeUrl))
                item.VideoYoutubeUrl = dto.VideoYoutubeUrl.Trim();

            if (dto.Descricao is not null)
                item.Descricao = dto.Descricao.Trim();

            var novaImagem = await SalvarImagemOuUsarUrl(dto.ImagemArquivo, dto.ImagemUrl, permitirVazio: true);
            if (!string.IsNullOrWhiteSpace(novaImagem))
                item.ImagemUrl = novaImagem;

            item.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(MapToResponse(item));
        }

        [HttpPatch("{id}/renomear")]
        public async Task<IActionResult> RenomearRegistro(int id, [FromBody] ItemRenameDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return BadRequest("Informe um nome valido.");

            var item = await _context.Itens.FindAsync(id);
            if (item == null) return NotFound();

            item.Nome = dto.Nome.Trim();
            item.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(MapToResponse(item));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegistro(int id)
        {
            var item = await _context.Itens.FindAsync(id);
            if (item == null) return NotFound();

            _context.Itens.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<string?> SalvarImagemOuUsarUrl(IFormFile? arquivo, string? imagemUrl, bool permitirVazio = false)
        {
            if (arquivo is { Length: > 0 })
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);

                var extension = Path.GetExtension(arquivo.FileName);
                var nomeArquivo = $"{Guid.NewGuid()}{extension}";
                var caminhoArquivo = Path.Combine(uploadsPath, nomeArquivo);

                await using var stream = new FileStream(caminhoArquivo, FileMode.Create);
                await arquivo.CopyToAsync(stream);

                return $"/uploads/{nomeArquivo}";
            }

            if (!string.IsNullOrWhiteSpace(imagemUrl))
                return imagemUrl.Trim();

            return permitirVazio ? string.Empty : null;
        }

        private static object MapToResponse(Item item) => new
        {
            item.Id,
            item.Nome,
            item.Tipo,
            item.ImagemUrl,
            item.VideoYoutubeUrl,
            VideoYoutubeEmbedUrl = ToYoutubeEmbedUrl(item.VideoYoutubeUrl),
            item.Descricao,
            item.CriadoEm,
            item.AtualizadoEm
        };

        private static string ToYoutubeEmbedUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return url;

            var host = uri.Host.ToLowerInvariant();
            if (host.Contains("youtube.com") && uri.AbsolutePath.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
                return url;

            string? videoId = null;

            if (host.Contains("youtu.be"))
            {
                videoId = uri.AbsolutePath.Trim('/');
            }
            else if (host.Contains("youtube.com"))
            {
                if (uri.AbsolutePath.StartsWith("/shorts/", StringComparison.OrdinalIgnoreCase))
                {
                    videoId = uri.AbsolutePath.Replace("/shorts/", string.Empty).Trim('/');
                }
                else
                {
                    var query = QueryHelpers.ParseQuery(uri.Query);
                    if (query.TryGetValue("v", out var value))
                        videoId = value.ToString();
                }
            }

            return string.IsNullOrWhiteSpace(videoId)
                ? url
                : $"https://www.youtube.com/embed/{videoId}";
        }
    }
}





