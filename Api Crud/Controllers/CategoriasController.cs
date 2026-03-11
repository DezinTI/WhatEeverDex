using System.Text;
using System.Text.Json;
using DzDex.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DzDex.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/categorias")]
    public class CategoriasController : ControllerBase
    {
        private readonly DzDexContext _context;
        private readonly string _arquivoCategorias;

        public CategoriasController(DzDexContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _arquivoCategorias = Path.Combine(environment.ContentRootPath, "App_Data", "categorias.json");
        }

        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            var categoriasSalvas = await LerCategoriasSalvasAsync();
            var categoriasItens = await _context.Itens
                .AsNoTracking()
                .Select(i => i.Tipo)
                .Distinct()
                .ToListAsync();

            var totais = await _context.Itens
                .AsNoTracking()
                .GroupBy(item => item.Tipo)
                .Select(grupo => new { Tipo = grupo.Key, Total = grupo.Count() })
                .ToDictionaryAsync(item => item.Tipo, item => item.Total);

            var todas = categoriasSalvas
                .Concat(categoriasItens)
                .Select(NormalizarCategoria)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .Select(c => new
                {
                    valor = c,
                    nome = NomeAmigavel(c),
                    totalItens = totais.TryGetValue(c, out var total) ? total : 0
                });

            return Ok(todas);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategoria([FromBody] CategoriaRequest? request)
        {
            var categoriaNormalizada = NormalizarCategoria(request?.Nome ?? string.Empty);
            if (string.IsNullOrWhiteSpace(categoriaNormalizada))
                return BadRequest("Informe um nome valido para a categoria.");

            var existentes = await LerCategoriasSalvasAsync();
            if (existentes.Contains(categoriaNormalizada))
                return Conflict("Essa categoria ja existe.");

            existentes.Add(categoriaNormalizada);
            await SalvarCategoriasAsync(existentes);

            return Created("/api/categorias", new
            {
                valor = categoriaNormalizada,
                nome = NomeAmigavel(categoriaNormalizada)
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategoria(string id, [FromBody] CategoriaRequest? request)
        {
            var categoriaAtual = NormalizarCategoria(id);
            var categoriaNova = NormalizarCategoria(request?.Nome ?? string.Empty);

            if (string.IsNullOrWhiteSpace(categoriaAtual))
                return BadRequest("Categoria invalida.");

            if (string.IsNullOrWhiteSpace(categoriaNova))
                return BadRequest("Informe um nome valido para a categoria.");

            var existentes = await LerCategoriasSalvasAsync();
            var categoriaExiste = existentes.Contains(categoriaAtual)
                || await _context.Itens.AnyAsync(item => item.Tipo == categoriaAtual);

            if (!categoriaExiste)
                return NotFound("Categoria nao encontrada.");

            if (categoriaAtual != categoriaNova)
            {
                var duplicada = existentes.Contains(categoriaNova)
                    || await _context.Itens.AnyAsync(item => item.Tipo == categoriaNova);

                if (duplicada)
                    return Conflict("Ja existe uma categoria com esse nome.");
            }

            var itens = await _context.Itens.Where(item => item.Tipo == categoriaAtual).ToListAsync();
            foreach (var item in itens)
            {
                item.Tipo = categoriaNova;
                item.AtualizadoEm = DateTime.UtcNow;
            }

            var atualizadas = existentes.Where(item => item != categoriaAtual).ToList();
            if (!atualizadas.Contains(categoriaNova))
                atualizadas.Add(categoriaNova);

            await SalvarCategoriasAsync(atualizadas);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                valor = categoriaNova,
                nome = NomeAmigavel(categoriaNova)
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategoria(string id)
        {
            var categoria = NormalizarCategoria(id);
            if (string.IsNullOrWhiteSpace(categoria))
                return BadRequest("Categoria invalida.");

            var existentes = await LerCategoriasSalvasAsync();
            var categoriaExiste = existentes.Contains(categoria)
                || await _context.Itens.AnyAsync(item => item.Tipo == categoria);

            if (!categoriaExiste)
                return NotFound("Categoria nao encontrada.");

            var possuiItens = await _context.Itens.AnyAsync(item => item.Tipo == categoria);
            if (possuiItens)
                return BadRequest("Nao e possivel excluir categoria com registros vinculados.");

            var atualizadas = existentes.Where(item => item != categoria).ToList();
            await SalvarCategoriasAsync(atualizadas);

            return Ok(new { mensagem = "Categoria excluida com sucesso!" });
        }

        private async Task<List<string>> LerCategoriasSalvasAsync()
        {
            if (!System.IO.File.Exists(_arquivoCategorias))
                return new List<string>();

            var json = await System.IO.File.ReadAllTextAsync(_arquivoCategorias, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();

            var categorias = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

            return categorias
                .Select(NormalizarCategoria)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        private async Task SalvarCategoriasAsync(List<string> categorias)
        {
            var pasta = Path.GetDirectoryName(_arquivoCategorias);
            if (!string.IsNullOrWhiteSpace(pasta))
                Directory.CreateDirectory(pasta);

            var json = JsonSerializer.Serialize(categorias.OrderBy(c => c), new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await System.IO.File.WriteAllTextAsync(_arquivoCategorias, json, Encoding.UTF8);
        }

        private static string NormalizarCategoria(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            var texto = valor.Trim().ToLowerInvariant();
            var builder = new StringBuilder();

            foreach (var ch in texto)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    builder.Append(ch);
                    continue;
                }

                if (ch == ' ' || ch == '_' || ch == '-')
                    builder.Append('-');
            }

            var normalizado = builder.ToString();
            while (normalizado.Contains("--"))
                normalizado = normalizado.Replace("--", "-");

            return normalizado.Trim('-');
        }

        private static string NomeAmigavel(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            var partes = valor
                .Split('-', StringSplitOptions.RemoveEmptyEntries)
                .Select(parte => char.ToUpper(parte[0]) + parte[1..]);

            return string.Join(' ', partes);
        }

        public class CategoriaRequest
        {
            public string Nome { get; set; } = string.Empty;
        }
    }
}
