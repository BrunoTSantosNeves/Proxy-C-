using Microsoft.AspNetCore.Mvc;
using ProxyFallbackAPI.Controllers;
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
        public async Task Proxy_ShouldReturnBadRequest_WhenCepIsMissing()
        {
            // Arrange
            var mockHttpHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(mockHttpHandler);
            var controller = new ProxyController(httpClient);
            dynamic payload = new { }; // Payload sem o CEP

            // Act
            var result = await controller.Proxy(payload);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Proxy_ShouldCallPrimaryApi_WhenPayloadIsValid()
        {
            // Arrange
            var mockHttpHandler = new MockHttpMessageHandler();
            mockHttpHandler.When("https://viacep.com.br/ws/01001-000/json/")
                           .Respond(HttpStatusCode.OK, "application/json", "{ \"logradouro\": \"Praça da Sé\" }");

            var httpClient = new HttpClient(mockHttpHandler);
            var controller = new ProxyController(httpClient);
            dynamic payload = new { cep = "01001-000" };

            // Act
            var result = await controller.Proxy(payload);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Praça da Sé", okResult.Value.ToString());
        }
    }
}
