// Tarea 1.3 â€” SD-01 a SD-05: ExcepciÃ³n Unauthorized â€” TOURS AYACUCHO PERÃš
using System.Net;

namespace ToursAyacuchoPeruAPI.Domain.Exceptions
{
    public class UnauthorizedException : ToursAyacuchoPeruException
    {
        public UnauthorizedException(string message, string errorCode = "UNAUTHORIZED")
            : base(message, HttpStatusCode.Unauthorized, errorCode)
        {
        }
    }
}

