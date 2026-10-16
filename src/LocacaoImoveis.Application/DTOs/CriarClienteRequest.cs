using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Application.DTOs
{
    public class CriarClienteRequestDTO
    {
        public string Nome { get; set; } = null!;
        public string Telefone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Cpf { get; set; } = null!;
    }
}
