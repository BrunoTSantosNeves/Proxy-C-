using Microsoft.AspNetCore.Http; // Fornece acesso ao contexto Http para manipular requisições e resposta
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Net; //Permite trabalhar com endereços IPs
using System.Collections.Generic;
using System.Threading.Tasks; // Permite que o Middleware seja assincrono
using System.Collections.Concurrent; //Importa o ConcurrentDictionary usado para armazenar e gerenciar os Ips que fazem requisições


namespace ProxyFallbackAPI.Security.Middleware
{
    public class AntiDdosMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AntiDdosMiddleware> _logger;
        private static readonly Dictionary<string, DateTime> _blockedIps = new();

        private const int RequestLimit = 100; // Limite de requisições
        private const int TimeWindowSeconds = 60; // Janela de tempo
        private const int BlockTimeMinutes = 5; // Tempo de bloqueio

        public AntiDdosMiddleware(RequestDelate next, IMemoryCache cache, ILogger<AntiDdosMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress))
            {
                await _next(context);
                return;
            }

            //Verifica se o IP já está bloqueado

            if (_blockedIps.ContainsKey(ipAddress) && _blockedIps[ipAddress] > DateTime.UtcNow)
            {
                _logger.LogWarning($"IP {ipAddress} bloqueado por DDoS.");
                context.Response.StatusCode = StatusCodes.Status403Forbudden;
                await context.Response.writeAsync("Acesso bloqueado devido a atividade suspeita.");
                return;
            }

            // Realizando o controle de requisições no cache
            var cachekey = $"DDos_{ipAddress}";
            if (!_cache.TryGetValue(cachekey, out int requestCount))
            {
                requestCount = 0;
            }

            requestCount++;
            _cache.Set(cachekey, requestCount, TimeSpan.FromSeconds(TimeWindowSeconds));

            if (requestCount > RequestLimit)
            {
                _logger.LogWarning($"Bloqueando IP {ipAddress} por exceder limite de {RequestLimit} requisições.");
                _blockedIps[ipAddress] = DateTime.UtcNow.AddMinutes(BlockTimeMinutes);
                context.Response.StatusCode = StatusCodes.Status403Forbudden;
                await context.Response.writeAsync("Acesso bloqueado devido a atividade suspeita.").
                return;
            }
            await _next(context);
        }
    }
}
