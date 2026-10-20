using LocacaoImoveis.Domain.Policies;

public class PoliticaDeReembolsoTests
{

    // 169 1000 referem-se a 169 horas antes da entrada, resultando em um reembolso de 1000.
    [Theory]
    [InlineData(169, 1000)] // Mais de 7 dias: 100%
    [InlineData(168, 500)]  // Exatamente 7 dias: 50%
    [InlineData(48, 500)]   // Exatamente 48 horas: 50%
    [InlineData(47, 0)]     // Menos de 48 horas: 0%
    public void DeveCalcularReembolsoPelaAntecedencia(
        int horasAntes,
        int valorEsperado)
    {
        // constutor
        var politica = new PoliticaDeReembolso();

        var entrada = new DateTimeOffset(
            2026, 11, 10, 14, 0, 0, TimeSpan.FromHours(-3));

        // Calcula a data de cancelamento com base nas horas antes da entrada
        var cancelamento = entrada.AddHours(-horasAntes);

        // usa o método CalcularReembolso para obter o valor do reembolso
        var reembolso = politica.CalcularReembolso(
            1000m, entrada, cancelamento);

        // verifica se o valor do reembolso calculado é igual ao valor esperado
        Assert.Equal((decimal)valorEsperado, reembolso);
    }

}
