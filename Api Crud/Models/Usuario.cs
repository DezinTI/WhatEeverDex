using System.ComponentModel.DataAnnotations;

namespace DzDex.API.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SenhaHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User";

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? UltimoLoginEm { get; set; }

        public ICollection<Item> RegistrosCriados { get; set; } = new List<Item>();
        public ICollection<ItemEditRequest> SolicitacoesEdicao { get; set; } = new List<ItemEditRequest>();
        public ICollection<ItemEditRequest> SolicitacoesResolvidas { get; set; } = new List<ItemEditRequest>();
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Senha { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string Senha { get; set; } = string.Empty;
    }

    public class AlterarRoleRequest
    {
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User";
    }

    public class ItemEditRequestCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Descricao { get; set; } = string.Empty;
    }

    public class ItemEditDecisionDto
    {
        [StringLength(500)]
        public string? Observacao { get; set; }
    }
}
