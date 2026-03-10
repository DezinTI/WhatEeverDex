using System.ComponentModel.DataAnnotations;

namespace DzDex.API.Models
{
    public class ItemRenameDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;
    }
}

