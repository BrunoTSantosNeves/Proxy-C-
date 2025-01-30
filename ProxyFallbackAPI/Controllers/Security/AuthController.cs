using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProxyFallbackAPI.Security.Services;
using System;
using System.Threading.Tasks

namespace ProxyFallbackAPI.Controllers.Security
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public AuthController(ITokenService tokenService, IUserService userService)
        {
            _tokenService = _tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _userService = _userService ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        /// Endpoint para autenticação e geração de token JWT.
        /// </summary>
        /// userName = Nome do usuario
        /// passWord = Senha do usuario
        /// <returns>Retorna um token JWT se a autenticação for bem-sucedida.</returns>
        
        [AllowAnonymous]
        [HttpPost("login")]
    }
}