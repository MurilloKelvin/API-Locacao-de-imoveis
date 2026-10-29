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


            // documento e email convertidos para string no banco de dados, mas mantidos como Value Objects na aplicação
            cliente.Property(c => c.DocumentoCliente)
                .HasConversion(
                documento => documento.Valor,
                valor => new DocumentoCliente(valor))
                .HasMaxLength(14)
                .IsRequired();


            // garante q ue o documento seja único no banco de dados
            cliente.HasIndex(c => c.DocumentoCliente)
                .IsUnique(); // 

            cliente.Property(c => c.Email)
                .HasConversion(
                email => email.Valor,
                valor => new Email(valor))
                .IsRequired();

            cliente.HasIndex(c => c.Email)
                .IsUnique(); // garante que o email seja único no banco de dados



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
