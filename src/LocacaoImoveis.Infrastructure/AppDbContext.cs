using Microsoft.EntityFrameworkCore;

namespace LocacaoImoveis.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options)
        {
        }

        // DbSet<Cliente>
        // DbSet<Imovel>
        // DbSet<Reserva>

    }
}
