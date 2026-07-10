// Tarea 1.3 â€” SD-01 a SD-05: ExcepciÃ³n Base del Dominio â€” TOURS AYACUCHO PERÃš
using System;
using System.Net;

namespace ToursAyacuchoPeruAPI.Domain.Exceptions
{
    public class ToursAyacuchoPeruException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ErrorCode { get; }

        public ToursAyacuchoPeruException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string errorCode = "INTERNAL_ERROR")
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public ToursAyacuchoPeruException(string message, Exception innerException, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string errorCode = "INTERNAL_ERROR")
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}

