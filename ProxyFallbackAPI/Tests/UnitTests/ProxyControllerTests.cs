using Microsoft.AspNetCore.Mvc;
using ProxyFallbackAPI.Controllers; //  usar o namespace correto
using ProxyFallbackAPI.Models;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using RichardSzalay.MockHttp;

namespace ProxyFallbackAPI.Tests.UnitTests
{
    public class ProxyControllerTests
    {
        [Fact]
        public async Task Proxy_ReturnsBadRequest_WhenCepIsMissing()
        {
            // Arrange
            var mockHttpHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(mockHttpHandler);
            var controller = new ProxyController(httpClient);
            
            // Payload inválido (cep nulo)
            var payload = new ProxyRequest { Cep = null };

            // Act
            var result = await controller.Proxy(payload);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Proxy_CallsPrimaryApi_AndReturnsData_WhenPayloadIsValid()
        {
            // Arrange
            var mockHttpHandler = new MockHttpMessageHandler();
            mockHttpHandler.When("https://viacep.com.br/ws/01001-000/json/")
                           .Respond(HttpStatusCode.OK, "application/json", "{ \"logradouro\": \"Praça da Sé\" }");

            var httpClient = new HttpClient(mockHttpHandler);
            var controller = new ProxyController(httpClient);

            // Payload válido
            var payload = new ProxyRequest { Cep = "01001-000" };

            // Act
            var result = await controller.Proxy(payload);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Contains("\"logradouro\":\"Praça da Sé\"", okResult.Value.ToString());
        }
    }
}
