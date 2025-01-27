using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using RichardSzalay.MockHttp;
using ProxyFallbackAPI.Controllers;

public class HealthCheckTests
{
    [Fact]
    public async Task HealthCheck_ShouldReturnHealthStatus_WhenApisAreAvailable()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();

        // Configura respostas simuladas para as APIs
        mockHandler.When("https://viacep.com.br/ws/01001000/json")
                   .Respond("application/json", "{ \"status\": \"Healthy\" }");
        mockHandler.When("https://brasilapi.com.br/api/cep/v1/01001000")
                   .Respond("application/json", "{ \"status\": \"Healthy\" }");

        var httpClient = new HttpClient(mockHandler);
        var controller = new ProxyController(httpClient);

        // Act
        var result = await controller.HealthCheck();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value); // Adicione esta linha para garantir que o valor não seja nulo
        Assert.Contains("Healthy", okResult.Value.ToString());
    }
}
