// Tarea 1.3 â€” SD-01 a SD-05: ExcepciÃ³n UnprocessableEntity â€” TOURS AYACUCHO PERÃš
using System.Collections.Generic;
using System.Net;

namespace ToursAyacuchoPeruAPI.Domain.Exceptions
{
    public class UnprocessableEntityException : ToursAyacuchoPeruException
    {
        public IEnumerable<string> Errors { get; }

        public UnprocessableEntityException(string message, string errorCode = "UNPROCESSABLE_ENTITY")
            : base(message, HttpStatusCode.UnprocessableEntity, errorCode)
        {
            Errors = new List<string>();
        }

        public UnprocessableEntityException(string message, IEnumerable<string> errors, string errorCode = "UNPROCESSABLE_ENTITY")
            : base(message, HttpStatusCode.UnprocessableEntity, errorCode)
        {
            Errors = errors;
        }
    }
}

