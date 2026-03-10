using DzDex.API.Models;
using DzDex.API.Data;
using System.Linq;

namespace DzDex.API.Seed
{
    public static class SeedDatabase
    {
        public static void Initialize(DzDexContext context)
        {
            context.Database.EnsureCreated();

            if (context.Itens.Any())
                return;

            var agora = DateTime.UtcNow;

            context.Itens.AddRange(
                new Item
                {
                    Nome = "Naruto vs Sasuke",
                    Tipo = "luta-anime",
                    ImagemUrl = "https://i.ytimg.com/vi/MYBbo4bY7f4/maxresdefault.jpg",
                    VideoYoutubeUrl = "https://www.youtube.com/watch?v=MYBbo4bY7f4",
                    Descricao = "Luta clÃ¡ssica no Vale do Fim.",
                    CriadoEm = agora,
                    AtualizadoEm = agora
                },
                new Item
                {
                    Nome = "Goku vs Jiren",
                    Tipo = "luta-anime",
                    ImagemUrl = "https://i.ytimg.com/vi/HxA8rSZx9xA/maxresdefault.jpg",
                    VideoYoutubeUrl = "https://www.youtube.com/watch?v=HxA8rSZx9xA",
                    Descricao = "Confronto do Torneio do Poder.",
                    CriadoEm = agora,
                    AtualizadoEm = agora
                },
                new Item
                {
                    Nome = "Chama",
                    Tipo = "alien-ben10",
                    ImagemUrl = "https://ben10.fandom.com/wiki/Heatblast?file=HeatblastOS.png",
                    VideoYoutubeUrl = "https://www.youtube.com/watch?v=RW4A5hoAq0Q",
                    Descricao = "Pyronite com poderes de fogo.",
                    CriadoEm = agora,
                    AtualizadoEm = agora
                },
                new Item
                {
                    Nome = "Diamante",
                    Tipo = "alien-ben10",
                    ImagemUrl = "https://ben10.fandom.com/wiki/Diamondhead?file=DiamondheadOS.png",
                    VideoYoutubeUrl = "https://www.youtube.com/watch?v=B8QF7I2M7sQ",
                    Descricao = "Petrosapien com criaÃ§Ã£o de cristais.",
                    CriadoEm = agora,
                    AtualizadoEm = agora
                }
            );

            context.SaveChanges();
        }
    }
}



