using System;
using System.Collections.Generic;
using System.Text;

namespace LocacaoImoveis.Application.Exceptions
{
    public class EmailJaCadastradoException : Exception
    {
        public EmailJaCadastradoException()
            : base("Já existe um cliente cadastrado com este email.")
        {
        }
    }
}
