using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Application.Exceptions
{
    public class CpfJaCadastradoException : Exception
    {
        public CpfJaCadastradoException()
            : base("Já existe um cliente cadastrado com este CPF.")
        {
        }
    }
}
