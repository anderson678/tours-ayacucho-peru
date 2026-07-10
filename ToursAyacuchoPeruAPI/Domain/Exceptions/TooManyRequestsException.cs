// Tarea 1.3 â€” SD-02: ExcepciÃ³n TooManyRequests â€” TOURS AYACUCHO PERÃš
using System.Net;

namespace ToursAyacuchoPeruAPI.Domain.Exceptions
{
    public class TooManyRequestsException : ToursAyacuchoPeruException
    {
        public int MinutosRestantes { get; }

        public TooManyRequestsException(string message, int minutosRestantes, string errorCode = "TOO_MANY_REQUESTS")
            : base(message, HttpStatusCode.TooManyRequests, errorCode)
        {
            MinutosRestantes = minutosRestantes;
        }
    }
}

