// Tarea 1.3 â€” SD-01 a SD-05: ExcepciÃ³n NotFound â€” TOURS AYACUCHO PERÃš
using System.Net;

namespace ToursAyacuchoPeruAPI.Domain.Exceptions
{
    public class NotFoundException : ToursAyacuchoPeruException
    {
        public NotFoundException(string message, string errorCode = "NOT_FOUND")
            : base(message, HttpStatusCode.NotFound, errorCode)
        {
        }
    }
}

