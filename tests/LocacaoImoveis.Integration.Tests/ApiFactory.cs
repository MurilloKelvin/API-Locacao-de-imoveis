using Microsoft.AspNetCore.Mvc.Testing;

namespace LocacaoImoveis.Integration.Tests;

//Inicializa a aplicação ASP.NET Core usando o mesmo Program da API real
//permitindo que os testes de integração façam chamadas HTTP para a API como se fosse um cliente real.
public sealed class ApiFactory : WebApplicationFactory<Program> // esse WebApplicationFactory<Program> indica que o projeto de teste encontre a entrada da API no program q tem  "public partial class Program"

{
}
