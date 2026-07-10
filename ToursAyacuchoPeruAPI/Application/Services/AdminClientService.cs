using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToursAyacuchoPeruAPI.Application.DTOs.Admin;
using ToursAyacuchoPeruAPI.Application.Interfaces;
using ToursAyacuchoPeruAPI.Domain.Enums;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;

namespace ToursAyacuchoPeruAPI.Application.Services
{
    public class AdminClientService : IAdminClientService
    {
        private readonly ToursAyacuchoPeruDbContext _db;

        public AdminClientService(ToursAyacuchoPeruDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<AdminClientResponseDto>> GetClientsAsync()
        {
            return await _db.Usuarios
                .Where(u => u.Rol == RolUsuario.Cliente)
                .OrderByDescending(u => u.FechaRegistro)
                .AsNoTracking()
                .Select(u => new AdminClientResponseDto
                {
                    ClienteId = u.UsuarioId,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Estado = u.Estado.ToString(),
                    FechaRegistro = u.FechaRegistro
                })
                .ToListAsync();
        }

        public async Task<AdminClientResponseDto> UpdateClientStatusAsync(Guid clientId, UpdateClientStatusDto dto)
        {
            if (dto.Estado is not EstadoUsuario.Activo and not EstadoUsuario.Inactivo)
            {
                throw new UnprocessableEntityException(
                    "El estado solicitado no es válido para la gestión de cuentas.",
                    new[] { "Estados permitidos: Activo, Inactivo" },
                    "ESTADO_USUARIO_INVALIDO");
            }

            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == clientId && u.Rol == RolUsuario.Cliente)
                ?? throw new NotFoundException("Cliente no encontrado.");

            usuario.Estado = dto.Estado;
            _db.Usuarios.Update(usuario);
            await _db.SaveChangesAsync();

            return new AdminClientResponseDto
            {
                ClienteId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Estado = usuario.Estado.ToString(),
                FechaRegistro = usuario.FechaRegistro
            };
        }
    }
}
