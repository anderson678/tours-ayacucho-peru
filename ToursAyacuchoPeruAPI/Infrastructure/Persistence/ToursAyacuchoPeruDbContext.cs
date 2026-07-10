// Tarea 1.2 â€” SD-01 a SD-05: Contexto EF Core â€” TOURS AYACUCHO PERÃš
using Microsoft.EntityFrameworkCore;
using ToursAyacuchoPeruAPI.Domain.Entities;
using ToursAyacuchoPeruAPI.Domain.Enums;

namespace ToursAyacuchoPeruAPI.Infrastructure.Persistence
{
    public class ToursAyacuchoPeruDbContext : DbContext
    {
        public ToursAyacuchoPeruDbContext(DbContextOptions<ToursAyacuchoPeruDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<BloqueosCuenta> BloqueosCuenta { get; set; } = null!;
        public DbSet<PaqueteTuristico> PaquetesTuristicos { get; set; } = null!;
        public DbSet<Reserva> Reservas { get; set; } = null!;
        public DbSet<Pago> Pagos { get; set; } = null!;
        public DbSet<Comprobante> Comprobantes { get; set; } = null!;
        public DbSet<Resena> Resenas { get; set; } = null!;
        public DbSet<NotificacionCliente> NotificacionesCliente { get; set; } = null!;
        public DbSet<ConfiguracionPortada> ConfiguracionPortada { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ------------------------------------------------------------
            // ConfiguraciÃ³n: Usuario
            // ------------------------------------------------------------
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.UsuarioId);

                entity.Property(e => e.UsuarioId)
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Correo)
                    .IsRequired()
                    .HasMaxLength(254);

                entity.HasIndex(e => e.Correo)
                    .IsUnique()
                    .HasDatabaseName("UQ_Usuarios_Correo");

                entity.Property(e => e.HashPassword)
                    .IsRequired()
                    .HasMaxLength(72);

                entity.Property(e => e.Telefono)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.Property(e => e.FotoUrl)
                    .HasMaxLength(600);

                // Los nombres de los miembros de RolUsuario y EstadoUsuario coinciden
                // exactamente con los valores del CHECK constraint en el DDL, por lo que
                // ToString()/Parse() es suficiente y no requiere un mapeo adicional.
                entity.Property(e => e.Rol)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasConversion(
                        v => v.ToString(),
                        v => (RolUsuario)System.Enum.Parse(typeof(RolUsuario), v));

                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasConversion(
                        v => v.ToString(),
                        v => (EstadoUsuario)System.Enum.Parse(typeof(EstadoUsuario), v));

                entity.Property(e => e.FechaRegistro)
                    .HasColumnType("DATETIME2(0)")
                    .HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // ------------------------------------------------------------
            // ConfiguraciÃ³n: BloqueosCuenta
            // ------------------------------------------------------------
            modelBuilder.Entity<BloqueosCuenta>(entity =>
            {
                entity.ToTable("BloqueosCuenta");
                entity.HasKey(e => e.BloqueoId);

                entity.Property(e => e.IntentosFallidos)
                    .HasDefaultValue((byte)0);

                entity.Property(e => e.FechaBloqueo)
                    .HasColumnType("DATETIME2(0)");

                entity.Property(e => e.FechaDesbloqueo)
                    .HasColumnType("DATETIME2(0)");

                // RelaciÃ³n 1-a-1 con Usuario (UsuarioId es UNIQUE en el DDL)
                entity.HasIndex(e => e.UsuarioId)
                    .IsUnique()
                    .HasDatabaseName("UQ_BloqueosCuenta_UsuarioId");

                entity.HasOne(d => d.Usuario)
                    .WithOne(p => p.Bloqueo)
                    .HasForeignKey<BloqueosCuenta>(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_BloqueosCuenta_Usuarios");
            });

            // ------------------------------------------------------------
            // ConfiguraciÃ³n: PaqueteTuristico
            // ------------------------------------------------------------
            modelBuilder.Entity<PaqueteTuristico>(entity =>
            {
                entity.ToTable("PaquetesTuristicos");
                entity.HasKey(e => e.PaqueteId);

                entity.Property(e => e.PaqueteId)
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Destino)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(2000);

                entity.Property(e => e.ImagenUrl)
                    .HasMaxLength(600);

                entity.Property(e => e.PrecioUnitario)
                    .HasColumnType("DECIMAL(10,2)");

                entity.Property(e => e.FechaInicio)
                    .HasColumnType("DATE");

                entity.Property(e => e.FechaFin)
                    .HasColumnType("DATE");

                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("DATETIME2(0)")
                    .HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // ------------------------------------------------------------
            // ConfiguraciÃ³n: Reserva
            // ------------------------------------------------------------
            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.ToTable("Reservas");
                entity.HasKey(e => e.ReservaId);

                entity.Property(e => e.ReservaId)
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

                entity.Property(e => e.FechaInicio)
                    .HasColumnType("DATE");

                entity.Property(e => e.MontoTotal)
                    .HasColumnType("DECIMAL(10,2)");

                // Los nombres de EstadoReserva son idÃ©nticos a los valores del CHECK
                // constraint (PENDIENTE_PAGO, CONFIRMADA, REPROGRAMADA, COMPLETADA, CANCELADA).
                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasConversion(
                        v => v.ToString(),
                        v => (EstadoReserva)System.Enum.Parse(typeof(EstadoReserva), v));

                entity.Property(e => e.ContReprogramacion)
                    .HasDefaultValue((byte)0);

                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("DATETIME2(0)")
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.HasIndex(e => e.UsuarioId)
                    .HasDatabaseName("IX_Reservas_UsuarioId");

                // RN-04-05: un cliente no debe tener mÃ¡s de una reserva PENDIENTE_PAGO
                // para el mismo paquete. Se refuerza con un Ã­ndice Ãºnico filtrado a nivel
                // de base de datos (ver database/ToursAyacuchoPeru.sql) ademÃ¡s de la validaciÃ³n en
                // ReservationService, para eliminar la condiciÃ³n de carrera bajo concurrencia.
                entity.HasIndex(e => new { e.UsuarioId, e.PaqueteId })
                    .IsUnique()
                    .HasFilter("[Estado] = 'PENDIENTE_PAGO'")
                    .HasDatabaseName("UQ_Reservas_PendienteUnicaPorPaquete");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Reservas)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Reservas_Usuarios");

                entity.HasOne(d => d.Paquete)
                    .WithMany(p => p.Reservas)
                    .HasForeignKey(d => d.PaqueteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Reservas_Paquetes");
            });

            // ------------------------------------------------------------
            // ConfiguraciÃ³n: Pago
            // ------------------------------------------------------------
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.ToTable("Pagos");
                entity.HasKey(e => e.PagoId);

                entity.Property(e => e.PagoId)
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

                entity.Property(e => e.Monto)
                    .HasColumnType("DECIMAL(10,2)");

                entity.Property(e => e.MetodoPago)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasConversion(
                        v => v.ToString(),
                        v => (MetodoPago)System.Enum.Parse(typeof(MetodoPago), v));

                entity.Property(e => e.NumReferencia)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.FechaPago)
                    .HasColumnType("DATETIME2(0)")
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasConversion(
                        v => v.ToString(),
                        v => (EstadoPago)System.Enum.Parse(typeof(EstadoPago), v));

                entity.HasIndex(e => e.ReservaId)
                    .IsUnique()
                    .HasDatabaseName("UQ_Pagos_Reserva"); // SD-05: una reserva, un pago

                entity.HasOne(d => d.Reserva)
                    .WithOne(p => p.Pago)
                    .HasForeignKey<Pago>(d => d.ReservaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Pagos_Reservas");
            });

            // ------------------------------------------------------------
            // ConfiguraciÃ³n: Comprobante
            // ------------------------------------------------------------
            modelBuilder.Entity<Comprobante>(entity =>
            {
                entity.ToTable("Comprobantes");
                entity.HasKey(e => e.ComprobanteId);

                entity.Property(e => e.ComprobanteId)
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

                entity.Property(e => e.Contenido)
                    .IsRequired();

                entity.Property(e => e.FechaEmision)
                    .HasColumnType("DATETIME2(0)")
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(e => e.EnviadoCorreo)
                    .HasDefaultValue(false);

                entity.HasIndex(e => e.PagoId)
                    .IsUnique()
                    .HasDatabaseName("UQ_Comprobantes_Pago");

                entity.HasOne(d => d.Pago)
                    .WithOne(p => p.Comprobante)
                    .HasForeignKey<Comprobante>(d => d.PagoId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Comprobantes_Pagos");
            });

            // ------------------------------------------------------------
            // ConfiguraciÃ³n: Resena
            // ------------------------------------------------------------
            modelBuilder.Entity<Resena>(entity =>
            {
                entity.ToTable("Resenas");
                entity.HasKey(e => e.ResenaId);

                entity.Property(e => e.ResenaId)
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

                entity.Property(e => e.Comentario)
                    .HasMaxLength(1000);

                entity.Property(e => e.FechaPublicacion)
                    .HasColumnType("DATETIME2(0)")
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                // Unique constraint (UsuarioId, PaqueteId) â€” RN-09-02
                entity.HasIndex(e => new { e.UsuarioId, e.PaqueteId })
                    .IsUnique()
                    .HasDatabaseName("UQ_Resenas_UsuarioPaquete");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Resenas)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Resenas_Usuarios");

                entity.HasOne(d => d.Paquete)
                    .WithMany(p => p.Resenas)
                    .HasForeignKey(d => d.PaqueteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Resenas_Paquetes");
            });

            // ------------------------------------------------------------
            // Configuracion: NotificacionCliente
            // ------------------------------------------------------------
            modelBuilder.Entity<NotificacionCliente>(entity =>
            {
                entity.ToTable("NotificacionesCliente");
                entity.HasKey(e => e.NotificacionId);

                entity.Property(e => e.NotificacionId)
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

                entity.Property(e => e.EventKey)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.HasIndex(e => e.EventKey)
                    .IsUnique()
                    .HasDatabaseName("UQ_NotificacionesCliente_EventKey");

                entity.Property(e => e.DestinatarioEmail)
                    .IsRequired()
                    .HasMaxLength(254);

                entity.Property(e => e.Asunto)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.UltimoError)
                    .HasMaxLength(1000);

                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("DATETIME2(0)")
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(e => e.FechaEntrega)
                    .HasColumnType("DATETIME2(0)");
            });

            // ------------------------------------------------------------
            // Configuracion: ConfiguracionPortada
            // ------------------------------------------------------------
            modelBuilder.Entity<ConfiguracionPortada>(entity =>
            {
                entity.ToTable("ConfiguracionPortada");
                entity.HasKey(e => e.ConfiguracionPortadaId);

                entity.Property(e => e.ConfiguracionPortadaId)
                    .ValueGeneratedNever();

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(80);

                entity.Property(e => e.CompanySubtitle)
                    .IsRequired()
                    .HasMaxLength(120);

                entity.Property(e => e.LogoUrl)
                    .HasMaxLength(600);

                entity.Property(e => e.HeroBadge)
                    .IsRequired()
                    .HasMaxLength(160);

                entity.Property(e => e.HeroTitle)
                    .IsRequired()
                    .HasMaxLength(220);

                entity.Property(e => e.HeroSubtitle)
                    .IsRequired()
                    .HasMaxLength(600);

                entity.Property(e => e.HeroStatsTours)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.HeroStatsTravelers)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.HeroStatsRating)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.HeroImagesJson)
                    .IsRequired();

                entity.Property(e => e.FechaActualizacion)
                    .HasColumnType("DATETIME2(0)")
                    .HasDefaultValueSql("SYSUTCDATETIME()");
            });
        }
    }
}


