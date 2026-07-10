// Tarea 2.8, 3.3 â€” SD-01, SD-02, SD-03: AuthController â€” TOURS AYACUCHO PERÃš
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ToursAyacuchoPeruAPI.Application.DTOs.Auth;
using ToursAyacuchoPeruAPI.Presentation.Extensions;
using ToursAyacuchoPeruAPI.Application.Interfaces;
using ToursAyacuchoPeruAPI.Domain.Exceptions;

namespace ToursAyacuchoPeruAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// SD-01: Registro de nuevo Cliente (RF01)
        /// POST /api/v1/auth/register
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            _logger.LogInformation("Solicitud de registro para correo: {Correo}", dto.Correo);
            var result = await _authService.RegisterAsync(dto);
            return StatusCode(201, result);
        }

        /// <summary>
        /// SD-02: Inicio de SesiÃ³n (RF02)
        /// POST /api/v1/auth/login
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(429)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            _logger.LogInformation("Solicitud de inicio de sesiÃ³n para correo: {Correo}", dto.Correo);
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
    }

    /// <summary>
    /// SD-03: ActualizaciÃ³n de Perfil de Cliente (RF03)
    /// </summary>
    [ApiController]
    [Route("api/v1/clients")]
    [Authorize]
    public class ClientProfileController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ClientProfileController> _logger;

        public ClientProfileController(IAuthService authService, ILogger<ClientProfileController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// SD-03: Obtener perfil del usuario autenticado.
        /// </summary>
        [HttpGet("{clientId:guid}/profile")]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetProfile(Guid clientId)
        {
            var jwtClientId = User.GetClientId();
            if (jwtClientId != clientId)
                throw new ForbiddenException(
                    "No puedes consultar el perfil de otro usuario.",
                    "ACCESO_DENEGADO");

            var result = await _authService.GetProfileAsync(clientId);
            return Ok(result);
        }

        /// <summary>
        /// SD-03: Actualizar perfil del Cliente autenticado.
        /// PUT /api/v1/clients/{clientId}/profile
        /// RN-03-01: NO permite modificar el correo electrÃ³nico.
        /// RN-03-02: TelÃ©fono 9-15 dÃ­gitos numÃ©ricos (validado en Validator).
        /// </summary>
        [HttpPut("{clientId:guid}/profile")]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> UpdateProfile(
            Guid clientId,
            [FromBody] UpdateProfileDto dto)
        {
            // El Cliente solo puede modificar su propio perfil
            var jwtClientId = User.GetClientId();
            if (jwtClientId != clientId)
                throw new ForbiddenException(
                    "No puedes modificar el perfil de otro cliente.",
                    "ACCESO_DENEGADO");

            var result = await _authService.UpdateProfileAsync(clientId, dto);
            return Ok(result);
        }
    }
}

