using System.Text;
using System.Text.Json;
using DzDex.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DzDex.API.Controllers
{
    [ApiController]
    [Route("api/categorias")]
    public class CategoriasController : ControllerBase
    {
        private static readonly string[] CategoriasPadrao =
        [
            "luta-anime",
            "alien-ben10"
        ];

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

            var todas = CategoriasPadrao
                .Concat(categoriasSalvas)
                .Concat(categoriasItens)
                .Select(NormalizarCategoria)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .Select(c => new
                {
                    valor = c,
                    nome = NomeAmigavel(c)
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
            if (CategoriasPadrao.Contains(categoriaNormalizada) || existentes.Contains(categoriaNormalizada))
                return Conflict("Essa categoria ja existe.");

            existentes.Add(categoriaNormalizada);
            await SalvarCategoriasAsync(existentes);

            return Created("/api/categorias", new
            {
                valor = categoriaNormalizada,
                nome = NomeAmigavel(categoriaNormalizada)
            });
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
