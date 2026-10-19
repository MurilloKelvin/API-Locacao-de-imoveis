using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Enums;

namespace LocacaoImoveis.Domain.Entities
{
    public class Imovel
    {
        public Guid Id { get; private set; }
        public string Endereco { get; private set; }
        public string Titulo { get; private set; }
        public int CapacidadeDeHospedes { get; private set; }
        public decimal ValorDiaria { get; private set; }
        public ImovelStatus Status { get; private set; }


        // Construtor privado para EF reconstruir o obj
        private Imovel()
        {
            Endereco = null!;
            Titulo = null!;
        }

        public Imovel(string endereco, string titulo, int capacidadeDeHospedes, decimal valorDiaria)
        {
            if(string.IsNullOrWhiteSpace(endereco))
                throw new ArgumentException("Endereço não pode ser vazio.", nameof(endereco));

            if(string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("Título não pode ser vazio.", nameof(titulo));

            if(capacidadeDeHospedes <= 0)
                throw new ArgumentException("Capacidade de hóspedes deve ser maior que zero.", nameof(capacidadeDeHospedes));

            if(valorDiaria <= 0)
                throw new ArgumentException("Valor da diária deve ser maior que zero.", nameof(valorDiaria));

            Id = Guid.NewGuid();
            Endereco = endereco.Trim();
            Titulo = titulo.Trim();
            CapacidadeDeHospedes = capacidadeDeHospedes;
            ValorDiaria = valorDiaria;
            Status = ImovelStatus.Disponivel;
        }

    }


}
