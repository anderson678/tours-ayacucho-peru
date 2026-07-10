// Tarea 1.3 â€” SD-01 a SD-05: ExcepciÃ³n Conflict â€” TOURS AYACUCHO PERÃš
using System.Net;

namespace ToursAyacuchoPeruAPI.Domain.Exceptions
{
    public class ConflictException : ToursAyacuchoPeruException
    {
        public ConflictException(string message, string errorCode = "CONFLICT")
            : base(message, HttpStatusCode.Conflict, errorCode)
        {
        }
    }
}

