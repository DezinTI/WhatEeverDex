using DzDex.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DzDex.API.Data
{
    public class DzDexContext : DbContext
    {
        public DzDexContext(DbContextOptions<DzDexContext> options) : base(options) { }

        public DbSet<Item> Itens { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<ItemEditRequest> ItemEditRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>(entity =>
            {
                entity.Property(item => item.Nome).IsRequired().HasMaxLength(200);
                entity.Property(item => item.Tipo).IsRequired().HasMaxLength(100);
                entity.Property(item => item.ImagemUrl).HasMaxLength(500);
                entity.Property(item => item.VideoYoutubeUrl).IsRequired().HasMaxLength(500);
                entity.Property(item => item.Descricao).HasMaxLength(2000);

                entity.HasOne(item => item.CriadoPor)
                    .WithMany(usuario => usuario.RegistrosCriados)
                    .HasForeignKey(item => item.CriadoPorId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(item => item.SolicitacoesEdicao)
                    .WithOne(request => request.Item)
                    .HasForeignKey(request => request.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.Property(usuario => usuario.Nome).IsRequired().HasMaxLength(50);
                entity.Property(usuario => usuario.Email).IsRequired().HasMaxLength(100);
                entity.Property(usuario => usuario.SenhaHash).IsRequired().HasMaxLength(100);
                entity.Property(usuario => usuario.Role).IsRequired().HasMaxLength(20);
                entity.HasIndex(usuario => usuario.Email).IsUnique();
            });

            modelBuilder.Entity<ItemEditRequest>(entity =>
            {
                entity.Property(request => request.NomeAtual).IsRequired().HasMaxLength(200);
                entity.Property(request => request.DescricaoAtual).HasMaxLength(2000);
                entity.Property(request => request.NomeProposto).IsRequired().HasMaxLength(200);
                entity.Property(request => request.DescricaoProposta).HasMaxLength(2000);
                entity.Property(request => request.Status).IsRequired().HasMaxLength(20);
                entity.Property(request => request.Observacao).HasMaxLength(500);

                entity.HasOne(request => request.SolicitadoPor)
                    .WithMany(usuario => usuario.SolicitacoesEdicao)
                    .HasForeignKey(request => request.SolicitadoPorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(request => request.ResolvidoPor)
                    .WithMany(usuario => usuario.SolicitacoesResolvidas)
                    .HasForeignKey(request => request.ResolvidoPorId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(request => new { request.ItemId, request.Status });
            });
        }
    }
}


