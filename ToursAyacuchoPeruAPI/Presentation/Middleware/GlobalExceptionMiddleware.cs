// Tarea 1.3 â€” SD-01 a SD-05: Middleware Global de Excepciones â€” TOURS AYACUCHO PERÃš
using System;
using System.Data.Common;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ToursAyacuchoPeruAPI.Domain.Exceptions;

namespace ToursAyacuchoPeruAPI.Presentation.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido una excepciÃ³n no controlada durante la solicitud.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string errorCode = "INTERNAL_ERROR";
            string message = "Ha ocurrido un error inesperado en el servidor.";
            object? responsePayload = null;

            if (exception is ToursAyacuchoPeruException domainEx)
            {
                statusCode = domainEx.StatusCode;
                errorCode = domainEx.ErrorCode;
                message = domainEx.Message;

                if (domainEx is TooManyRequestsException tmEx)
                {
                    responsePayload = new
                    {
                        error = errorCode,
                        mensaje = message,
                        minutosRestantes = tmEx.MinutosRestantes
                    };
                }
                else if (domainEx is UnprocessableEntityException ueEx && ueEx.Errors != null)
                {
                    responsePayload = new
                    {
                        error = errorCode,
                        mensaje = message,
                        detalle = ueEx.Errors
                    };
                }
                else
                {
                    responsePayload = new
                    {
                        error = errorCode,
                        mensaje = message
                    };
                }
            }
            else if (exception is DbException || exception.InnerException is DbException)
            {
                statusCode = HttpStatusCode.ServiceUnavailable;
                errorCode = "DATABASE_UNAVAILABLE";
                message = "No se pudo conectar con la base de datos. Verifica que SQL Server esté iniciado y que la configuración local tenga acceso a ToursAyacuchoPeruDB.";
                responsePayload = new
                {
                    error = errorCode,
                    mensaje = message
                };
            }
            else
            {
                responsePayload = new
                {
                    error = errorCode,
                    mensaje = message
                };
            }

            context.Response.StatusCode = (int)statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var jsonString = JsonSerializer.Serialize(responsePayload, jsonOptions);
            await context.Response.WriteAsync(jsonString);
        }
    }
}

