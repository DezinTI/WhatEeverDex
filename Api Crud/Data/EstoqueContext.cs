using DzDex.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DzDex.API.Data
{
    public class DzDexContext : DbContext
    {
        public DzDexContext(DbContextOptions<DzDexContext> options) : base(options) { }

        public DbSet<Item> Itens { get; set; }
    }
}


