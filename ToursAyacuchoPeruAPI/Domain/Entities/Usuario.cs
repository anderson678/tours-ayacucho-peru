// Tarea 1.2 â€” SD-01: Entidad Usuario â€” TOURS AYACUCHO PERÃš
using System;
using System.Collections.Generic;
using ToursAyacuchoPeruAPI.Domain.Enums;

namespace ToursAyacuchoPeruAPI.Domain.Entities
{
    public class Usuario
    {
        public Guid UsuarioId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string HashPassword { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string? FotoUrl { get; set; }
        public RolUsuario Rol { get; set; } = RolUsuario.Cliente;
        public EstadoUsuario Estado { get; set; } = EstadoUsuario.Activo;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Propiedades de navegaciÃ³n
        public virtual BloqueosCuenta? Bloqueo { get; set; }
        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public virtual ICollection<Resena> Resenas { get; set; } = new List<Resena>();
    }
}

