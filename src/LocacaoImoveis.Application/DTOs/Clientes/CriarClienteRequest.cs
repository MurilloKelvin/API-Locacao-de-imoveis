using LocacaoImoveis.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Application.DTOs.Clientes
{
    public class CriarClienteRequestDTO
    {
        public string Nome { get; set; } = null!;
        public string Telefone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Documento { get; set; } = null!;
    }
}
