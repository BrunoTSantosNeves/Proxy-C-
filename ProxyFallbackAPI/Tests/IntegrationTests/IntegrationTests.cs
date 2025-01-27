using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

public class IntegrationTests
{
    [Fact]
    public async Task ProxyEndpoint_ShouldReturnSuccess_WhenPrimaryApiResponds()
    {
        // Arrange
        using var application = new WebApplicationFactory<Program>();
        var client = application.CreateClient();
        var payload = new
        {
            cep = "01001-000"
        };

        // Act
        var response = await client.PostAsJsonAsync("/proxy", payload);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Praça da Sé", content);
    }
}
