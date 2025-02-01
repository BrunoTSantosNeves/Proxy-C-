using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProxyFallbackAPI.Security.Services;
using System;
using System.Threading.Tasks;



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
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        /// Endpoint para autenticação e geração de token JWT.
        /// </summary>
        /// param name = Nome do usuario
        /// param name = Senha do usuario
        /// <returns>Retorna um token JWT se a autenticação for bem-sucedida.</returns>
        
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest("Nome de usuário ou senha não podem estar vazios.");
            }

            var user = await _userService.ValidateUserAsync(username, password);
            if (!user)
            {
                return Unauthorized("Usuário ou senha inválidos.");
            }

            // Gerar token JWT
            var token = _tokenService.GenerateToken(user.Username);

            return Ok(new { Token = token });
        }
    }
}