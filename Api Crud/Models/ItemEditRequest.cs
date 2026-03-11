using System.ComponentModel.DataAnnotations;

namespace DzDex.API.Models
{
    public class ItemEditRequest
    {
        public int Id { get; set; }

        public int ItemId { get; set; }
        public Item? Item { get; set; }

        public int SolicitadoPorId { get; set; }
        public Usuario? SolicitadoPor { get; set; }

        public int? ResolvidoPorId { get; set; }
        public Usuario? ResolvidoPor { get; set; }

        [Required]
        [StringLength(200)]
        public string NomeAtual { get; set; } = string.Empty;

        [StringLength(2000)]
        public string DescricaoAtual { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string NomeProposto { get; set; } = string.Empty;

        [StringLength(2000)]
        public string DescricaoProposta { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public string? Observacao { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvidoEm { get; set; }
    }
}