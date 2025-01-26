using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace ProxyFallbackAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ProxyController(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        // Modelo para o payload
        public class ProxyRequest
        {
            public string Cep { get; set; } = string.Empty;
        }

        // Endpoint principal do proxy
        [HttpPost]
        public async Task<IActionResult> Proxy([FromBody] ProxyRequest payload)
        {
            // Validar o CEP do payload
            if (string.IsNullOrWhiteSpace(payload.Cep))
            {
                return BadRequest("CEP é obrigatório.");
            }

            string cep = payload.Cep;

            // URLs das APIs com o CEP
            var primaryApiUrl = $"https://viacep.com.br/ws/{cep}/json/";
            var secondaryApiUrl = $"https://brasilapi.com.br/api/cep/v1/{cep}";

            // Lógica de fallback
            try
            {
                // Tentar a API primária
                var primaryResponse = await _httpClient.GetAsync(primaryApiUrl);
                if (primaryResponse.IsSuccessStatusCode)
                {
                    var primaryContent = await primaryResponse.Content.ReadAsStringAsync();
                    return Ok(primaryContent);
                }

                // Tentar a API secundária
                var secondaryResponse = await _httpClient.GetAsync(secondaryApiUrl);
                if (secondaryResponse.IsSuccessStatusCode)
                {
                    var secondaryContent = await secondaryResponse.Content.ReadAsStringAsync();
                    return Ok(secondaryContent);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao processar requisição: {ex.Message}");
            }

            // Retornar erro se ambas as APIs falharem
            return StatusCode(502, "Ambas as APIs falharam.");
        }

        // Endpoint de debug para verificar URLs das APIs
        [HttpGet("debug")]
        public IActionResult Debug()
        {
            var debugInfo = new
            {
                PrimaryApiUrl = "https://viacep.com.br/ws/01001000/json",
                SecondaryApiUrl = "https://brasilapi.com.br/api/cep/v1/01001000",
            };

            return Ok(debugInfo);
        }

        // Endpoint de health check para verificar as APIs externas
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            var primaryApiUrl = "https://viacep.com.br/ws/01001000/json";
            var secondaryApiUrl = "https://brasilapi.com.br/api/cep/v1/01001000";

            var primaryApiStatus = await CheckApiHealth(primaryApiUrl);
            var secondaryApiStatus = await CheckApiHealth(secondaryApiUrl);

            return Ok(new
            {
                PrimaryApi = new { Url = primaryApiUrl, Status = primaryApiStatus },
                SecondaryApi = new { Url = secondaryApiUrl, Status = secondaryApiStatus }
            });
        }

        // Método auxiliar para verificar a saúde de uma API
        private async Task<string> CheckApiHealth(string apiUrl)
        {
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                return response.IsSuccessStatusCode ? "Healthy" : "Unhealthy";
            }
            catch
            {
                return "Unreachable";
            }
        }
    }
}
