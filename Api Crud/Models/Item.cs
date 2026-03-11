using System.ComponentModel.DataAnnotations;

namespace DzDex.API.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        public string ImagemUrl { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        [Required]
        public string VideoYoutubeUrl { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

        public int? CriadoPorId { get; set; }
        public Usuario? CriadoPor { get; set; }

        public ICollection<ItemEditRequest> SolicitacoesEdicao { get; set; } = new List<ItemEditRequest>();
    }
}


