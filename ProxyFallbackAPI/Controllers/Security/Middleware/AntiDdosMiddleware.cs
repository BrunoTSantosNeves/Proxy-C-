using Microsoft.AspNetCore.Http; // Permite manipular requisições e respostas HTTP
using Microsoft.Extensions.Caching.Memory; // Fornece cache em memória
using Microsoft.Extensions.Logging; // Permite registrar logs
using System;
using System.Collections.Generic;
using System.Threading.Tasks; // Permite que o middleware seja assíncrono

namespace ProxyFallbackAPI.Security.Middleware
{
    public class AntiDdosMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AntiDdosMiddleware> _logger;
        private static readonly Dictionary<string, DateTime> _blockedIps = new();

        private const int RequestLimit = 100; // Número máximo de requisições permitidas por IP
        private const int TimeWindowSeconds = 60; // Tempo de contagem das requisições
        private const int BlockTimeMinutes = 5; // Tempo de bloqueio do IP se ultrapassar o limite

        public AntiDdosMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<AntiDdosMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress))
            {
                await _next(context);
                return;
            }

            // Verifica se o IP já está bloqueado
            if (_blockedIps.ContainsKey(ipAddress) && _blockedIps[ipAddress] > DateTime.UtcNow)
            {
                _logger.LogWarning($"IP {ipAddress} bloqueado por DDoS.");
                context.Response.StatusCode = StatusCodes.Status302Found;
                context.Response.Headers["Location"] = "/bloqueado.html";
                await context.Response.CompleteAsync();
                return;
            }

            // Controle de requisições no cache
            var cacheKey = $"DDos_{ipAddress}";
            if (!_cache.TryGetValue(cacheKey, out int requestCount))
            {
                requestCount = 0;
            }

            requestCount++;
            _cache.Set(cacheKey, requestCount, TimeSpan.FromSeconds(TimeWindowSeconds));

            if (requestCount > RequestLimit)
            {
                _logger.LogWarning($"Bloqueando IP {ipAddress} por exceder limite de {RequestLimit} requisições.");
                _blockedIps[ipAddress] = DateTime.UtcNow.AddMinutes(BlockTimeMinutes);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Acesso bloqueado devido a atividade suspeita.");
                return;
            }

            await _next(context);
        }
    }
}