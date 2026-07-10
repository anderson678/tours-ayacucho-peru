// Tarea 2.6, 3.1, 3.3 â€” SD-01, SD-02, SD-03: Servicio AuthService â€” TOURS AYACUCHO PERÃš
using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ToursAyacuchoPeruAPI.Infrastructure.Persistence;
using ToursAyacuchoPeruAPI.Application.DTOs.Auth;
using ToursAyacuchoPeruAPI.Domain.Exceptions;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Enums;
using ToursAyacuchoPeruAPI.Application.Interfaces;

namespace ToursAyacuchoPeruAPI.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ToursAyacuchoPeruDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly INotificationService _notificationService;

        // RN-02-04: Mensaje genÃ©rico que NUNCA distingue correo vs. contraseÃ±a
        private const string MensajeCredencialesInvalidas = "Credenciales incorrectas.";

        public AuthService(
            ToursAyacuchoPeruDbContext db,
            IJwtService jwtService,
            INotificationService notificationService)
        {
            _db = db;
            _jwtService = jwtService;
            _notificationService = notificationService;
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // SD-01: Registro de Cliente
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            // RN-01-01: Verificar unicidad del correo electrÃ³nico
            var correoNormalizado = dto.Correo.Trim().ToLowerInvariant();
            bool correoExiste = await _db.Usuarios
                .AnyAsync(u => u.Correo == correoNormalizado);

            if (correoExiste)
                throw new ConflictException(
                    "El correo electrÃ³nico ya se encuentra registrado en el sistema.",
                    "CORREO_DUPLICADO");

            // RN-01-03 y RN-01-04: Hash bcrypt con workFactor = 12 (>= 10 requerido)
            // NUNCA se almacena la contraseÃ±a en texto plano
            string hashPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);

            var usuario = new Usuario
            {
                UsuarioId = Guid.NewGuid(),
                Nombre = dto.Nombre.Trim(),
                Correo = correoNormalizado,
                HashPassword = hashPassword,
                Telefono = dto.Telefono.Trim(),
                Rol = RolUsuario.Cliente,
                Estado = EstadoUsuario.Activo,
                FechaRegistro = DateTime.UtcNow
            };

            _db.Usuarios.Add(usuario);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                throw new ConflictException(
                    "El correo electrónico ya se encuentra registrado en el sistema.",
                    "CORREO_DUPLICADO");
            }

            // RN-01-05: Enviar correo de bienvenida asÃ­ncrono fuera del flujo principal
            _ = Task.Run(() =>
                _notificationService.SendWelcomeEmailAsync(usuario.Correo, usuario.Nombre));

            return new RegisterResponseDto
            {
                ClienteId = usuario.UsuarioId,
                Mensaje = "Cuenta creada exitosamente. Revise su correo electrÃ³nico."
            };
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // SD-02: Inicio de SesiÃ³n con bloqueo de cuenta
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var correoNormalizado = dto.Correo.Trim().ToLowerInvariant();

            // Buscar usuario; respuesta SIEMPRE con mensaje genÃ©rico (RN-02-04)
            var usuario = await _db.Usuarios
                .Include(u => u.Bloqueo)
                .FirstOrDefaultAsync(u => u.Correo == correoNormalizado);

            // SD-08: Cuenta Inactiva â†’ HTTP 403
            if (usuario is not null && usuario.Estado == EstadoUsuario.Inactivo)
                throw new ForbiddenException(
                    "Cuenta desactivada. Contacte al administrador.",
                    "CUENTA_DESACTIVADA");

            // Si no existe el usuario â†’ respuesta genÃ©rica (RN-02-04)
            if (usuario is null)
                throw new UnauthorizedException(MensajeCredencialesInvalidas, "CREDENCIALES_INVALIDAS");

            // RN-02-03: Verificar si la cuenta estÃ¡ bloqueada temporalmente
            var bloqueo = usuario.Bloqueo;
            if (bloqueo?.FechaDesbloqueo.HasValue == true
                && bloqueo.FechaDesbloqueo.Value > DateTime.UtcNow)
            {
                int minutosRestantes = (int)Math.Ceiling(
                    (bloqueo.FechaDesbloqueo.Value - DateTime.UtcNow).TotalMinutes);
                throw new TooManyRequestsException(
                    $"Cuenta bloqueada temporalmente. Intente nuevamente en {minutosRestantes} minuto(s).",
                    minutosRestantes,
                    "CUENTA_BLOQUEADA");
            }

            if (bloqueo?.FechaDesbloqueo.HasValue == true
                && bloqueo.FechaDesbloqueo.Value <= DateTime.UtcNow)
            {
                bloqueo.IntentosFallidos = 0;
                bloqueo.FechaBloqueo = null;
                bloqueo.FechaDesbloqueo = null;
                _db.BloqueosCuenta.Update(bloqueo);
                await _db.SaveChangesAsync();
            }

            // Verificar contraseÃ±a con bcrypt
            bool credencialesValidas = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.HashPassword);

            if (!credencialesValidas)
            {
                // Incrementar contador e imponer bloqueo si se alcanza el lÃ­mite
                var minutosBloqueo = await IncrementarIntentosFallidosAsync(usuario.UsuarioId, bloqueo);
                if (minutosBloqueo.HasValue)
                {
                    throw new TooManyRequestsException(
                        $"Cuenta bloqueada temporalmente. Intente nuevamente en {minutosBloqueo.Value} minuto(s).",
                        minutosBloqueo.Value,
                        "CUENTA_BLOQUEADA");
                }

                throw new UnauthorizedException(MensajeCredencialesInvalidas, "CREDENCIALES_INVALIDAS");
            }

            // Login exitoso: reiniciar contador de intentos fallidos
            if (bloqueo is not null)
            {
                bloqueo.IntentosFallidos = 0;
                bloqueo.FechaBloqueo = null;
                bloqueo.FechaDesbloqueo = null;
                _db.BloqueosCuenta.Update(bloqueo);
                await _db.SaveChangesAsync();
            }

            // RN-02-01 y RN-02-02: Emitir Token JWT con sub=clientId, role, exp=8h
            var jwt = _jwtService.GenerateToken(usuario.UsuarioId, usuario.Rol.ToString());

            return new LoginResponseDto
            {
                Token = jwt.Token,
                ExpiraEn = jwt.ExpiresAt,
                ClienteId = usuario.UsuarioId,
                Rol = usuario.Rol.ToString(),
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Telefono = usuario.Telefono,
                FotoUrl = usuario.FotoUrl
            };
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // SD-03: ActualizaciÃ³n de Perfil
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public async Task<UpdateProfileResponseDto> GetProfileAsync(Guid clientId)
        {
            var usuario = await _db.Usuarios.FindAsync(clientId)
                ?? throw new NotFoundException("Usuario no encontrado.");

            return ToProfileResponse(usuario);
        }

        public async Task<UpdateProfileResponseDto> UpdateProfileAsync(Guid clientId, UpdateProfileDto dto)
        {
            var usuario = await _db.Usuarios.FindAsync(clientId)
                ?? throw new NotFoundException("Usuario no encontrado.");

            // RN-03-01: Solo se permite modificar Nombre, Telefono y FotoUrl; Correo se ignora.
            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                usuario.Nombre = dto.Nombre.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Telefono))
                usuario.Telefono = dto.Telefono.Trim();

            if (dto.FotoUrl is not null)
                usuario.FotoUrl = string.IsNullOrWhiteSpace(dto.FotoUrl) ? null : dto.FotoUrl.Trim();

            _db.Usuarios.Update(usuario);
            await _db.SaveChangesAsync();

            return ToProfileResponse(usuario);
        }

        private static UpdateProfileResponseDto ToProfileResponse(Usuario usuario) => new()
        {
            ClienteId = usuario.UsuarioId,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Telefono = usuario.Telefono,
            FotoUrl = usuario.FotoUrl,
            Rol = usuario.Rol.ToString()
        };

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // MÃ©todo privado: gestiÃ³n de intentos fallidos (RN-02-03)
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private async Task<int?> IncrementarIntentosFallidosAsync(Guid usuarioId, BloqueosCuenta? bloqueo)
        {
            if (bloqueo is null)
            {
                bloqueo = new BloqueosCuenta
                {
                    UsuarioId = usuarioId,
                    IntentosFallidos = 1
                };
                _db.BloqueosCuenta.Add(bloqueo);
            }
            else
            {
                bloqueo.IntentosFallidos++;
                _db.BloqueosCuenta.Update(bloqueo);
            }

            // RN-02-03: Bloquear 15 minutos tras 5 intentos consecutivos fallidos
            if (bloqueo.IntentosFallidos >= 5)
            {
                bloqueo.FechaBloqueo = DateTime.UtcNow;
                bloqueo.FechaDesbloqueo = DateTime.UtcNow.AddMinutes(15);
            }

            await _db.SaveChangesAsync();

            if (bloqueo.FechaDesbloqueo.HasValue && bloqueo.FechaDesbloqueo.Value > DateTime.UtcNow)
                return (int)Math.Ceiling((bloqueo.FechaDesbloqueo.Value - DateTime.UtcNow).TotalMinutes);

            return null;
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException
                && (sqlException.Number == 2601 || sqlException.Number == 2627);
        }
    }
}

