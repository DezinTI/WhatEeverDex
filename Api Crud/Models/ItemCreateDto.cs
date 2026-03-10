using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DzDex.API.Models
{
    public class ItemCreateDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Tipo { get; set; } = string.Empty;

        public IFormFile? ImagemArquivo { get; set; }

        public string? ImagemUrl { get; set; }

        [Required]
        public string VideoYoutubeUrl { get; set; } = string.Empty;

        public string? Descricao { get; set; }
    }
}

