using System;
using System.Collections.Generic;
using System.Text;
using LocacaoImoveis.Domain.Entities;
using LocacaoImoveis.Domain.Enums;

namespace LocacaoImoveis.Application.DTOs
{
    public class ClienteResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public ClienteStatus Status { get; set; }
      
    }
}
