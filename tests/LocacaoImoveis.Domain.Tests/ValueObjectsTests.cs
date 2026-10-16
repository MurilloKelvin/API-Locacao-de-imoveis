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
            var exception = Assert.Throws<ArgumentException>(() => new Cpf(cpfInvalido));

            Assert.StartsWith("CPF deve conter apenas números.", exception.Message);
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


        }
}
