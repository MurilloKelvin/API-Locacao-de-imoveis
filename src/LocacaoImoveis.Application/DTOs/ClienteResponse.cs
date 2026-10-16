using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Entities;

namespace LocacaoImoveis.Application.DTOs
{
    public class ClienteResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
      
    }
}
