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

        public DbSet<Reserva> Reservas => Set<Reserva>();


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

            // Reservas
            var reserva = modelBuilder.Entity<Reserva>();

            reserva.HasKey(r => r.Id);

            // um cliente pode ter várias reservas)
            reserva.HasOne<Cliente>()
                .WithMany()
                .HasForeignKey(r => r.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            //  Cada reserva pertence a um cliente e a um imóvel.
            //  Um cliente ou imóvel pode estar associado a várias reservas.
            //  O banco impede excluir fisicamente um cliente ou imóvel que tenha reservas

            // um imóvel pode ter várias reservas, mas uma reserva pertence a apenas um imóvel
            reserva.HasOne<Imovel>()
             .WithMany()
            .HasForeignKey(r => r.ImovelId)
            .OnDelete(DeleteBehavior.Restrict);

        }








        // DbSet<Reserva>

    }
}
