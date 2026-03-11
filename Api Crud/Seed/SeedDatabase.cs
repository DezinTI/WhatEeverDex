using DzDex.API.Models;
using DzDex.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DzDex.API.Seed
{
    public static class SeedDatabase
    {
        public static void Initialize(DzDexContext context)
        {
            context.Database.EnsureCreated();

            GarantirSchema(context);
            GarantirAdminsPadrao(context);

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

        private static void GarantirSchema(DzDexContext context)
        {
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS Usuarios (
                    Id INTEGER NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY AUTOINCREMENT,
                    Nome TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    SenhaHash TEXT NOT NULL,
                    Role TEXT NOT NULL,
                    CriadoEm TEXT NOT NULL,
                    UltimoLoginEm TEXT NULL
                );");

            context.Database.ExecuteSqlRaw("CREATE UNIQUE INDEX IF NOT EXISTS IX_Usuarios_Email ON Usuarios (Email);");

            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ItemEditRequests (
                    Id INTEGER NOT NULL CONSTRAINT PK_ItemEditRequests PRIMARY KEY AUTOINCREMENT,
                    ItemId INTEGER NOT NULL,
                    SolicitadoPorId INTEGER NOT NULL,
                    ResolvidoPorId INTEGER NULL,
                    NomeAtual TEXT NOT NULL,
                    DescricaoAtual TEXT NULL,
                    NomeProposto TEXT NOT NULL,
                    DescricaoProposta TEXT NULL,
                    Status TEXT NOT NULL,
                    Observacao TEXT NULL,
                    CriadoEm TEXT NOT NULL,
                    ResolvidoEm TEXT NULL,
                    CONSTRAINT FK_ItemEditRequests_Itens_ItemId FOREIGN KEY (ItemId) REFERENCES Itens (Id) ON DELETE CASCADE,
                    CONSTRAINT FK_ItemEditRequests_Usuarios_SolicitadoPorId FOREIGN KEY (SolicitadoPorId) REFERENCES Usuarios (Id) ON DELETE RESTRICT,
                    CONSTRAINT FK_ItemEditRequests_Usuarios_ResolvidoPorId FOREIGN KEY (ResolvidoPorId) REFERENCES Usuarios (Id) ON DELETE SET NULL
                );");

            context.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_ItemEditRequests_ItemId_Status ON ItemEditRequests (ItemId, Status);");

            var criadoPorExiste = context.Database.SqlQueryRaw<int>(@"
                SELECT COUNT(*)
                FROM pragma_table_info('Itens')
                WHERE name = 'CriadoPorId';").AsEnumerable().FirstOrDefault() > 0;

            if (!criadoPorExiste)
                context.Database.ExecuteSqlRaw("ALTER TABLE Itens ADD COLUMN CriadoPorId INTEGER NULL;");
        }

        private static void GarantirAdminsPadrao(DzDexContext context)
        {
            GarantirAdmin(context, "Administrador", "adm@adm.com", "adm10");
            GarantirAdmin(context, "Dezin", "dezin@dezin.com", "dezin10");
            context.SaveChanges();
        }

        private static void GarantirAdmin(DzDexContext context, string nome, string email, string senha)
        {
            var emailNormalizado = email.Trim().ToLowerInvariant();
            var usuario = context.Usuarios.FirstOrDefault(item => item.Email == emailNormalizado);

            if (usuario == null)
            {
                context.Usuarios.Add(new Usuario
                {
                    Nome = nome,
                    Email = emailNormalizado,
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
                    Role = "Admin",
                    CriadoEm = DateTime.UtcNow
                });

                return;
            }

            usuario.Nome = nome;
            usuario.Role = "Admin";
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
        }
    }
}



