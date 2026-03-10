using Microsoft.AspNetCore.Http;

namespace DzDex.API.Models
{
    public class ItemUpdateDto
    {
        public string? Nome { get; set; }
        public string? Tipo { get; set; }
        public IFormFile? ImagemArquivo { get; set; }
        public string? ImagemUrl { get; set; }
        public string? VideoYoutubeUrl { get; set; }
        public string? Descricao { get; set; }
    }
}

