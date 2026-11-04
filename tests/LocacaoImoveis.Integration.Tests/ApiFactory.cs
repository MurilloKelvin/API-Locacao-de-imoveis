using LocacaoImoveis.API.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace LocacaoImoveis.Integration.Tests;

//Inicializa a aplicação ASP.NET Core usando o mesmo Program da API real
//permitindo que os testes de integração façam chamadas HTTP para a API como se fosse um cliente real.
public sealed class ApiFactory : WebApplicationFactory<Program> // esse WebApplicationFactory<Program> indica que o projeto de teste encontre a entrada da API no program q tem  "public partial class Program"

{
    // configuração JWT própria dos testes, sem depender dos user secrets
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", "chave-de-testes-com-mais-de-32-caracteres!");
        builder.UseSetting("Jwt:ClientId", "cliente-teste");
        builder.UseSetting("Jwt:ClientSecret", "segredo-teste");
    }

    // todo HttpClient criado pela factory já envia um token JWT válido
    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);

        var token = Services.GetRequiredService<TokenService>().GerarToken("cliente-teste").Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
