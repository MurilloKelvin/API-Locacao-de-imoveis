using Microsoft.EntityFrameworkCore;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.ValueObjects;

namespace LocacaoImoveis.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Imovel> Imoveis => Set<Imovel>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);

            // Clientes
            var cliente = modelBuilder.Entity<Cliente>();

            cliente.HasKey(c => c.Id); // primary key

            cliente.Property(c => c.Nome)
                .IsRequired();

            cliente.Property(c => c.Telefone)
                .IsRequired();

            // cpf e email convertidos para string no banco de dados, mas mantidos como Value Objects na aplicação
            cliente.Property(c => c.Cpf)
                .HasConversion(
                cpf => cpf.Valor,
                valor => new Cpf(valor))
                .HasMaxLength(11)
                .IsRequired();

            // garante q ue o cpf seja único no banco de dados
            cliente.HasIndex(c => c.Cpf)
                .IsUnique(); // 

            cliente.Property(c => c.Email)
                .HasConversion(
                email => email.Valor,
                valor => new Email(valor))
                .IsRequired();

            // Imoveis
            var imovel = modelBuilder.Entity<Imovel>();

            imovel.HasKey(i => i.Id);

            imovel.Property(i => i.Titulo)
                .IsRequired();

            imovel.Property(i => i.Endereco)
                .IsRequired();

            imovel.Property(i => i.CapacidadeDeHospedes)
                .IsRequired();

            imovel.Property(i => i.ValorDiaria)
                .IsRequired();

            imovel.Property(i => i.Status)
                .IsRequired();

        }








        // DbSet<Reserva>

    }
}
