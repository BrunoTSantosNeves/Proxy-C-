using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProxyFallbackAPI.Security.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyFallbackAPI.Security.Middleware
{
    public class AutenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AutenticationMiddleware> _logger;
        private readonly ITokenService _tokenService;

        public AutenticationMiddleware(RequestDelegate next, ILogger<AutenticationMiddleware> logger, ITokenService tokenService)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authorizationHeader.Substring("Bearer ".Length).Trim();

                try
                {
                    if (_tokenService.ValidateToken(token, out var claimsPrinciapal))
                    {
                        context.User = claimsPrinciapal; // Associa o usuário autenticdo ao contexto 
                    }
                    else
                    {
                        _logger.LogWarning("Token inválido fornecido.");
                        context.Response.StatusCode = StatusCode.Status401Unauthorized;
                        await context.Response.WriteAsync("Token invalido.");
                        return;
                    }
                }
                catch (SecurityTokenException ex)
                {
                    _logger.LogError(ex, "Erro ao validar o token.");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Erro ao validar o token.");
                    return;
                }
            }
            else
            {
                _logger.LogInformation("Cabeçalho de autorização ausente ou mal formatado.");
            }

            await _next(context); // Continua o pipeline se tudo estiver correto.
        }
    }
}