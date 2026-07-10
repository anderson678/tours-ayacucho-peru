// Tarea 1.3 â€” SD-01 a SD-05: ExcepciÃ³n Forbidden â€” TOURS AYACUCHO PERÃš
using System.Net;

namespace ToursAyacuchoPeruAPI.Domain.Exceptions
{
    public class ForbiddenException : ToursAyacuchoPeruException
    {
        public ForbiddenException(string message, string errorCode = "FORBIDDEN")
            : base(message, HttpStatusCode.Forbidden, errorCode)
        {
        }
    }
}

