using LocacaoImoveis.Domain.ValueObjects;

namespace LocacaoImoveis.Domain.Tests
{
    public class ValueObjectsTests
    {
        [Fact]
        public void DeveRejeitarCpfComTodosNumerosIguais()
        {
            // Arrange
            var cpfInvalido = "11111111111";
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new Cpf(cpfInvalido)); // tenta criar o cpf

            Assert.Equal("valor", exception.ParamName); // precisa verificar o parametro se n falha
        }
        [Fact]
        public void DeveNormalizarCpfValido()
        {
            // Arrange
            var cpfValido = "529.982.247-25";
            // Act Assert
            var cpf = new Cpf(cpfValido);
            
            Assert.Equal("52998224725", cpf.Valor);
        }
        [Fact]
        public void DeveRejeitarCpfComLetras()
        {
            // Arrange
            var cpfInvalido = "529.982.247-@A";
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => new Cpf(cpfInvalido)); // tenta criar o cpf

            Assert.StartsWith(
                "CPF deve conter apenas números.", exception.Message);

            Assert.Equal("valor", exception.ParamName); //precisa verificar o parametro se n falha
        }

        [Theory]
        [InlineData("11.222.333/0001-81", "11222333000181")]
        [InlineData("00.000.000/E08G-12", "00000000E08G12")]
        public void DeveNormalizarCnpjValido(string cnpj, string cnpjEsperado)
        {
            var cnpjObj = new Cnpj(cnpj);

            Assert.Equal(cnpjEsperado, cnpjObj.Valor);
        }


        [Theory]
        [InlineData("123")]
        [InlineData("00000000E08@12")] // caractere não permitido
        public void DeveRejeitarCnpjInvalido(string valor)
        {
            var erro = Assert.Throws<ArgumentException>(() => new Cnpj(valor));

            Assert.Equal("valor", erro.ParamName);
        }


        [Theory]
        [InlineData("529.982.247-25", "52998224725")]
        [InlineData("11.222.333/0001-81", "11222333000181")]
        [InlineData("00.000.000/E08G-12", "00000000E08G12")]
        public void DeveAceitarDocumentoDeCliente(string entrada, string esperado)
        {
            var documento = new DocumentoCliente(entrada);

            Assert.Equal(esperado, documento.Valor);
        }

        [Fact]
        public void DeveRejeitarDocumentoDeClienteComTamanhoInvalido()
        {
            var erro = Assert.Throws<ArgumentException>(() =>
                new DocumentoCliente("123"));

            Assert.Equal("valor", erro.ParamName);
        }
    }
}
