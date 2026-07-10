// Tarea 3.3 â€” SD-02: ExtensiÃ³n ClaimsPrincipalExtensions â€” TOURS AYACUCHO PERÃš
using System;
using System.Security.Claims;
using ToursAyacuchoPeruAPI.Domain.Exceptions;

namespace ToursAyacuchoPeruAPI.Presentation.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Extrae el identificador Ãºnico del cliente (sub) del payload del Token JWT.
        /// RN-02-02: el payload del token contiene sub = clientId.
        /// </summary>
        public static Guid GetClientId(this ClaimsPrincipal principal)
        {
            var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? principal.FindFirstValue("sub");

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var clientId))
                throw new UnauthorizedException(
                    "El token JWT no contiene un identificador de cliente válido.",
                    "TOKEN_INVALIDO");

            return clientId;
        }

        /// <summary>
        /// Extrae el rol del cliente del payload del Token JWT.
        /// </summary>
        public static string GetRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Role)
                ?? principal.FindFirstValue("role")
                ?? string.Empty;
        }
    }
}

