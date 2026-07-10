// Tarea 2.4 â€” SD-02: Interfaz IJwtService â€” TOURS AYACUCHO PERÃš
using System;

namespace ToursAyacuchoPeruAPI.Application.Interfaces
{
    public interface IJwtService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(Guid clientId, string rol);
    }
}

