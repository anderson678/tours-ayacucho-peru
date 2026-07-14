# Plan de Implementación: Plataforma TOURS AYACUCHO PERÚ

## Información del Proyecto

**Paradigma:** Specification-Driven Development (SSD) con OpenSpec
**Stack Tecnológico:** ASP.NET Core (C#) · React.js/Vite · Microsoft SQL Server · JWT · FluentValidation · FsCheck + xUnit
**Módulos MVP:** Autenticación (SD-01, SD-02, SD-03) · Reservas (SD-04) · Pagos (SD-05) · Reprogramación (SD-06)
**Universidad:** Universidad Nacional de San Cristóbal de Huamanga (UNSCH)
**Estudiante:** Anderson Roki Ochoa Medrano · Año: 2026

---

## Resumen del Enfoque de Implementación

La implementación sigue la arquitectura de tres capas definida en el diseño:
**Domain → Application → Infrastructure → Presentation**, sobre ASP.NET Core, más Frontend React.js.
Cada tarea referencia el Spec Delta (SD-XX) y las reglas de negocio (RN-XX-XX) que implementa.
Las pruebas basadas en propiedades (PBT) con FsCheck validan invariantes universales críticos.

---

## Tareas

- [x] 1. Configuración inicial del proyecto y base de datos
  - Crear la solución ASP.NET Core Web API con estructura por capas (`Domain/`, `Application/`, `Infrastructure/`, `Presentation/`, `Tests/`)
  - Crear el proyecto React.js SPA con Vite y estructura de carpetas (`pages/`, `components/`, `context/`, `hooks/`, `api/`, `utils/`)
  - Instalar paquetes NuGet: `BCrypt.Net-Next`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `FsCheck.Xunit`, `FluentValidation.AspNetCore`
  - Instalar paquetes npm: `axios`, `react-router-dom`, `@reduxjs/toolkit`, `react-redux`
  - _Requisitos: RNF02, RNF06_

  - [x] 1.1 Crear esquema DDL de la base de datos en Microsoft SQL Server
    - Ejecutar el script DDL completo recomendado: `database/ToursAyacuchoPeru.sql`
    - Crear base `ToursAyacuchoPeruDB`, `ALTER DATABASE ... SET READ_COMMITTED_SNAPSHOT ON`, y todas las tablas (`Usuarios`, `BloqueosCuenta`, `PaquetesTuristicos`, `Reservas`, `Pagos`, `Comprobantes`, `Resenas`, `NotificacionesCliente`, `ConfiguracionPortada`)
    - Incluir `Usuarios.FotoUrl`, `PaquetesTuristicos.ImagenUrl` y configuración inicial de portada
    - Crear el índice `IX_Reservas_UsuarioId`
    - Verificar las constraints `CHECK`, `UNIQUE` y `FOREIGN KEY` de todas las tablas
    - _Requisitos: Requisito 4 (RN-04-01, RN-04-03), Requisito 5 (RN-05-03)_

  - [x] 1.2 Configurar DbContext y cadena de conexión en ASP.NET Core
    - Crear `ToursAyacuchoPeruDbContext` con `DbSet<>` para todas las entidades
    - Configurar `appsettings.json` con cadena de conexión a `ToursAyacuchoPeruDB`
    - Registrar el contexto en `Program.cs` con `AddDbContext<>()`
    - _Requisitos: Requisito 4 (RN-04-01), Requisito 6 (RN-06-04)_

  - [x] 1.3 Implementar jerarquía de excepciones de dominio y middleware global
    - Crear `ToursAyacuchoPeruException` y subclases: `NotFoundException`, `ConflictException`, `UnprocessableEntityException`, `UnauthorizedException`, `ForbiddenException`, `TooManyRequestsException`
    - Implementar `GlobalExceptionMiddleware` que serializa excepciones de dominio a JSON con `error` y `mensaje`
    - Registrar el middleware en `Program.cs`
    - _Requisitos: SD-01 a SD-05 (todos los códigos HTTP de error)_

- [ ] 2. Módulo de Autenticación — Backend (SD-01: Registro de Cliente)
  - [x] 2.1 Implementar entidad `Usuario` y `BloqueosCuenta` con EF Core
    - Crear clases de modelo `Usuario.cs` y `BloqueosCuenta.cs` con las propiedades del DDL
    - Configurar mapeo en `ToursAyacuchoPeruDbContext` con restricciones (CHECK, UNIQUE) vía Fluent API
    - _Requisitos: Requisito 1 (RN-01-01, RN-01-04)_

  - [x] 2.2 Implementar DTOs y validadores para registro
    - Crear `RegisterRequestDto` con anotaciones `[Required]` y `FluentValidation` para: formato RFC 5322 de correo, complejidad de contraseña (mínimo 8 caracteres, mayúscula, dígito, carácter especial), teléfono obligatorio
    - Crear `RegisterResponseDto` con `ClienteId` y `mensaje`
    - _Requisitos: Requisito 1 (RN-01-01, RN-01-02)_

  - [ ]* 2.3 Escribir prueba de propiedad: Validación de contraseñas inválidas
    - **Propiedad 2: Validación de contraseñas inválidas**
    - Para cualquier cadena que viole al menos un criterio de complejidad, `PasswordValidator.Validate()` DEBE retornar `IsValid = false`
    - **Valida: Requisito 1.4 (RN-01-02)**

  - [x] 2.4 Implementar `JwtService` para generación de tokens
    - Crear `IJwtService` e implementación `JwtService` con `GenerateToken(Guid clientId, string rol)`
    - Firmar con HMAC-SHA256, incluir claims `sub`, `role`, `jti`, `iat`, expiración de 8 horas
    - Leer clave secreta desde `appsettings.json` vía `IOptions<JwtSettings>`
    - _Requisitos: Requisito 2 (RN-02-01, RN-02-02)_

  - [ ]* 2.5 Escribir prueba de propiedad: Integridad del payload del Token JWT
    - **Propiedad 3: Integridad del payload del Token_JWT**
    - Para cualquier par `(clientId, rol)` válido, el token emitido DEBE contener `sub`, `role` y `exp` (emisión + 8 horas)
    - **Valida: Requisito 2.1, 2.2 (RN-02-01, RN-02-02)**

  - [x] 2.6 Implementar `AuthService.RegisterAsync` con hash bcrypt
    - Verificar unicidad de correo (`AnyAsync`), lanzar `ConflictException` si duplicado
    - Aplicar `BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12)`
    - Persistir `Usuario` con `Rol = "Cliente"` y disparar `_notificationService.SendWelcomeEmailAsync`
    - _Requisitos: Requisito 1 (RN-01-01, RN-01-03, RN-01-04, RN-01-05)_

  - [ ]* 2.7 Escribir prueba de propiedad: Almacenamiento seguro de contraseñas (bcrypt)
    - **Propiedad 1: Almacenamiento seguro de contraseñas**
    - Para cualquier contraseña válida, el hash almacenado DEBE ser diferente al original y verificable con `BCrypt.Verify(password, hash) == true`
    - **Valida: Requisito 1.6 (RN-01-03, RN-01-04)**

  - [x] 2.8 Implementar `AuthController.Register` y configurar middleware JWT
    - Crear `AuthController` con ruta `POST /api/v1/auth/register` anotado `[AllowAnonymous]`
    - Configurar `AddAuthentication().AddJwtBearer()` en `Program.cs` con validación de firma, issuer y audience
    - _Requisitos: Requisito 1 (criterios de aceptación 1, 3, 4, 5)_

- [ ] 3. Módulo de Autenticación — Backend (SD-02: Inicio de Sesión)
  - [x] 3.1 Implementar `AuthService.LoginAsync` con bloqueo de cuenta
    - Verificar existencia de cuenta y estado `Activo`; responder con mensaje genérico si no existe (RN-02-04)
    - Consultar `BloqueosCuenta`; lanzar `TooManyRequestsException` si `FechaDesbloqueo > UtcNow`
    - Verificar contraseña con `BCrypt.Verify`; en fallo, incrementar `IntentosFallidos` y bloquear al llegar a 5 (RN-02-03)
    - En login exitoso, resetear `IntentosFallidos` y emitir token con `JwtService`
    - _Requisitos: Requisito 2 (RN-02-01, RN-02-02, RN-02-03, RN-02-04)_

  - [ ]* 3.2 Escribir prueba de propiedad: Bloqueo de cuenta tras intentos fallidos
    - **Propiedad 4: Bloqueo de cuenta tras exactamente 5 intentos fallidos consecutivos**
    - El Auth_Service DEBE responder HTTP 429 con tiempo de bloqueo de 15 minutos, rechazando todos los intentos adicionales hasta expiración
    - **Valida: Requisito 2.4, 2.5 (RN-02-03)**

  - [x] 3.3 Implementar `AuthController.Login` y DTOs de respuesta
    - Crear endpoint `POST /api/v1/auth/login` con `[AllowAnonymous]`
    - Crear `LoginRequestDto`, `LoginResponseDto` (token, expiraEn, clienteId, rol, nombre, correo, telefono y fotoUrl)
    - Crear extensión `ClaimsPrincipalExtensions.GetClientId()` para extraer `sub` del JWT
    - _Requisitos: Requisito 2 (criterios de aceptación 1, 2, 3, 4, 5, 6)_

  - [x] 3.4 Implementar `AuthController.GetProfile` y `AuthController.UpdateProfile`
    - Crear `UpdateProfileRequestDto` con validación de teléfono (9–15 dígitos numéricos) y nombre mediante FluentValidation
    - El DTO DEBE ignorar el campo correo electrónico (RN-03-01); solo procesar `Nombre`, `Telefono` y foto de perfil
    - Agregar método `UpdateProfileAsync(Guid clientId, UpdateProfileRequestDto dto)` en `AuthService`
    - Validar que el `clientId` del path parameter coincide con el `sub` del Token_JWT antes de aplicar cambios (RN-03-02)
    - Crear endpoints `GET` y `PUT /api/v1/clients/{clientId}/profile` con `[Authorize]` en `AuthController`
    - _Requisitos: Requisito 3 (RN-03-01, RN-03-02, criterios 1, 2, 3, 4)_

- [ ] 4. Checkpoint — Autenticación Backend
  - Ejecutar suite de pruebas unitarias y de propiedades del módulo de autenticación
  - Verificar: registro exitoso devuelve HTTP 201, correo duplicado devuelve HTTP 409, contraseña inválida devuelve HTTP 422, login exitoso devuelve token JWT, 5 intentos fallidos generan HTTP 429
  - Asegurar que todos los tests pasen; consultar al usuario si surgen dudas sobre reglas de negocio

- [ ] 5. Módulo de Reservas — Backend (SD-04: Crear Reserva con Control ACID de Overbooking)
  - [x] 5.1 Implementar entidades `Reserva` y `PaqueteTuristico` con EF Core
    - Crear clases `Reserva.cs` y `PaqueteTuristico.cs` con todas las propiedades del DDL
    - Definir enum `EstadoReserva` con valores: `PendientePago`, `Confirmada`, `Reprogramada`, `Completada`, `Cancelada`
    - Configurar relaciones FK y constraints en `ToursAyacuchoPeruDbContext`
    - _Requisitos: Requisito 4 (RN-04-04)_

  - [x] 5.2 Implementar `ReservationService.CreateAsync` con transacción ACID y bloqueo pesimista
    - Iniciar transacción con `IsolationLevel.Serializable`
    - Adquirir `UPDLOCK, ROWLOCK` sobre `PaquetesTuristicos` con `FromSqlRaw`
    - Validar `AsientosDisp >= dto.CantAsientos`; lanzar `ConflictException` si insuficiente (RN-04-02)
    - Verificar reserva duplicada `PENDIENTE_PAGO` para el mismo cliente y paquete (RN-04-05)
    - Crear `Reserva` y decrementar `AsientosDisp` en la misma transacción; hacer `CommitAsync` o `RollbackAsync`
    - _Requisitos: Requisito 4 (RN-04-01, RN-04-02, RN-04-03, RN-04-04, RN-04-05)_

  - [ ]* 5.3 Escribir prueba de propiedad: Prevención de overbooking (invariante ACID)
    - **Propiedad 5: Prevención de overbooking bajo solicitudes concurrentes**
    - Para cualquier paquete con capacidad N y solicitudes concurrentes cuya suma exceda N, el total de reservas confirmadas NUNCA debe superar N; `AsientosDisp >= 0` siempre
    - **Valida: Requisito 4.2, 4.3, 4.4 (RN-04-01, RN-04-02, RN-04-03)**

  - [ ]* 5.4 Escribir prueba de propiedad: Atomicidad de la transacción de reserva
    - **Propiedad 6: Atomicidad ante fallo de transacción**
    - Si la TX es interrumpida tras crear la `Reserva` pero antes de descontar asientos, AMBAS operaciones deben revertirse: ni el registro persiste ni `AsientosDisp` cambia
    - **Valida: Requisito 4.2 (RN-04-03)**

  - [ ] 5.5 Implementar `ReservationController` con endpoints de creación y consulta
    - Crear endpoint `POST /api/v1/reservations` con `[Authorize]`
    - Crear endpoint `GET /api/v1/reservations` con filtro por estado y cliente obtenido del Token_JWT
    - Crear endpoint `GET /api/v1/reservations/{reservationId}` con validación de pertenencia al cliente del JWT
    - _Requisitos: Requisito 4 (criterios 1, 2, 3, 4, 5, 6), Requisito 7 (criterios 1, 2, 3, 4)_

  - [ ]* 5.6 Escribir pruebas unitarias para consulta de reservas
    - Verificar que el endpoint devuelva únicamente reservas del cliente identificado en el JWT (RN-07-02)
    - Verificar que el filtro por estado funcione correctamente para cada valor del enum
    - _Requisitos: Requisito 7 (RN-07-01, RN-07-02)_

- [ ] 6. Módulo de Pagos — Backend (SD-05: Registro de Pago y Comprobante Digital)
  - [x] 6.1 Implementar entidades `Pago` y `Comprobante` con EF Core
    - Crear `Pago.cs` con propiedades del DDL; definir enum `MetodoPago` (TransferenciaBancaria, DepositoCuenta, Yape, Plin) y enum `EstadoPago` (Registrado, Verificado, Rechazado)
    - Crear `Comprobante.cs` con campo `Contenido` de tipo JSON (NVARCHAR MAX)
    - Configurar constraint `UQ_Pagos_Reserva` (una reserva, un pago) en `ToursAyacuchoPeruDbContext`
    - _Requisitos: Requisito 5 (RN-05-01, RN-05-06)_

  - [x] 6.2 Implementar `PaymentService.RegisterPaymentAsync` con transacción ACID
    - Iniciar transacción con `IsolationLevel.ReadCommitted`
    - Verificar existencia de reserva con estado `PendientePago` perteneciente al cliente; lanzar `ConflictException` si ya está `Confirmada` (RN-05-06)
    - Validar monto con tolerancia `Math.Abs(dto.Monto - montoEsperado) <= 0.01m`; lanzar `UnprocessableEntityException` si falla (RN-05-02)
    - Registrar `Pago` y cambiar `Reserva.Estado = Confirmada` en la misma TX; delegar `_notificationService.SendReceiptAsync` fuera de la TX (RN-05-03, RN-05-04)
    - _Requisitos: Requisito 5 (RN-05-01, RN-05-02, RN-05-03, RN-05-04, RN-05-06)_

  - [ ]* 6.3 Escribir prueba de propiedad: Validación de monto con tolerancia
    - **Propiedad 7: Validación de monto de pago con tolerancia S/ 0.01**
    - Para cualquier monto base M, el `PaymentValidator.ValidateMonto` DEBE aceptar montos en `[M − 0.01, M + 0.01]` y rechazar todos los demás
    - **Valida: Requisito 5.4 (RN-05-02)**

  - [ ] 6.4 Implementar generación de comprobante digital en `NotificationService`
    - Crear `Comprobante` con JSON que contenga: `reservaId`, nombre del paquete, nombre del cliente, monto, método de pago, número de referencia, fecha/hora (RN-05-05)
    - Implementar `SendReceiptAsync` con envío SMTP y política de reintentos (3 intentos, intervalo 5 min)
    - Marcar `EnviadoCorreo = true` tras envío exitoso
    - _Requisitos: Requisito 5 (RN-05-04, RN-05-05), Requisito 11 (RN-11-02, RN-11-03)_

  - [ ]* 6.5 Escribir prueba de propiedad: Completitud del comprobante digital
    - **Propiedad 8: Completitud del comprobante digital**
    - Para cualquier pago registrado exitosamente, el comprobante emitido DEBE contener los 7 campos requeridos: `reservaId`, `paquete`, `cliente`, `monto`, `metodoPago`, `numReferencia`, `fechaHora`; ninguno puede ser nulo
    - **Valida: Requisito 5.3 (RN-05-05)**

  - [x] 6.6 Implementar `PaymentController` con endpoints de pago y comprobante
    - Crear endpoint `POST /api/v1/payments` con `[Authorize]`
    - Crear endpoint `GET /api/v1/payments/{paymentId}/receipt` con validación de pertenencia
    - _Requisitos: Requisito 5 (criterios 1, 2, 3, 4, 5, 6, 7)_

- [ ] 7. Módulo de Reprogramación — Backend (SD-06: Reprogramar Reserva con Ventana 12h)
  - [ ] 7.1 Implementar `ReschedulingValidator` con lógica de ventana de tiempo
    - Crear clase estática `ReschedulingValidator` con método `IsWithinRescheduleWindow(DateTime fechaInicio, DateTime ahora)` que retorne `true` si `(fechaInicio - ahora).TotalHours >= 12`
    - Crear método `IsFutureDate(DateOnly nuevaFecha)` que valide que la nueva fecha sea posterior a hoy
    - _Requisitos: Requisito 6 (RN-06-01, RN-06-02)_

  - [ ]* 7.2 Escribir prueba de propiedad: Ventana de reprogramación (12 horas)
    - **Propiedad 9: Ventana de reprogramación de 12 horas**
    - Para cualquier valor de `horasAnticipacion` generado aleatoriamente (rango 0–47h), `ReschedulingValidator.IsWithinRescheduleWindow` DEBE retornar `true` si y sólo si `horasAnticipacion >= 12`
    - **Valida: Requisito 6.4 (RN-06-01)**

  - [ ] 7.3 Implementar `ReschedulingService.RescheduleAsync` con transacción ACID
    - Iniciar transacción con `IsolationLevel.Serializable`
    - Verificar existencia de reserva `Confirmada` del cliente; validar `ContReprogramacion < 1` (RN-06-05)
    - Llamar a `ReschedulingValidator.IsWithinRescheduleWindow`; lanzar `UnprocessableEntityException` si fuera de ventana (RN-06-01)
    - Validar que `dto.NuevaFecha > hoy` (RN-06-02)
    - Adquirir `UPDLOCK` sobre paquete; verificar `AsientosDisp >= reserva.CantAsientos` (RN-06-03)
    - Actualizar `reserva.FechaInicio`, `Estado = Reprogramada`, `ContReprogramacion += 1`; descontar asientos (RN-06-04)
    - Delegar notificación fuera de la TX (RN-06-06)
    - _Requisitos: Requisito 6 (RN-06-01, RN-06-02, RN-06-03, RN-06-04, RN-06-05, RN-06-06)_

  - [ ]* 7.4 Escribir prueba de propiedad: Atomicidad de la transacción de reprogramación
    - **Propiedad 10: Atomicidad de la transacción de reprogramación ante fallo**
    - Si la TX es interrumpida a mitad, los asientos de la fecha original DEBEN mantenerse asignados y los de la nueva fecha no deben descontarse; ningún estado intermedio debe persistir
    - **Valida: Requisito 6.1, 6.2 (RN-06-04)**

  - [ ] 7.5 Implementar `ReservationController.Reschedule` (endpoint PATCH)
    - Agregar endpoint `PATCH /api/v1/reservations/{reservationId}/reschedule` con `[Authorize]`
    - Crear `RescheduleRequestDto` con validación de `NuevaFecha` (campo requerido, tipo `DateOnly`)
    - _Requisitos: Requisito 6 (criterios 1, 2, 3, 4, 5, 6, 7, 8)_

  - [ ]* 7.6 Escribir pruebas unitarias para casos límite de reprogramación
    - Verificar que segunda reprogramación devuelva HTTP 422 (RN-06-05)
    - Verificar que nueva fecha pasada devuelva HTTP 422 (RN-06-02)
    - Verificar que nueva fecha sin disponibilidad devuelva HTTP 409 (RN-06-03)
    - _Requisitos: Requisito 6 (criterios 5, 6, 7)_

- [ ] 8. Checkpoint — Backend Completo (Módulos MVP)
  - Ejecutar suite completa de pruebas: unitarias + propiedades PBT (Propiedades 1–10 con FsCheck, mínimo 100 iteraciones cada una)
  - Verificar cobertura de líneas ≥ 80% en servicios de dominio y cobertura de ramas ≥ 75% en controladores
  - Asegurar que todos los endpoints responden correctamente con Swagger/Postman; consultar al usuario si surgen dudas

- [x] 9. Módulo de Autenticación — Frontend React.js (SD-01, SD-02)
  - [x] 10.1 Implementar cliente HTTP y servicio de autenticación
    - Crear instancia Axios en `src/api/apiClient.js` con `baseURL` e interceptor de request para inyectar JWT desde `localStorage`
    - Consumir `POST /api/v1/auth/register` y `POST /api/v1/auth/login` desde `Register.jsx`, `Login.jsx` y `AuthContext.jsx`
    - _Requisitos: Requisito 1 (criterio 1), Requisito 2 (criterio 1)_

  - [x] 10.2 Implementar estado de autenticación y hook `useAuth`
    - Crear `context/AuthContext.jsx` con estado `{ token, clienteId, rol, nombre, correo, telefono, fotoUrl, expiraEn }`
    - Persistir sesión en `localStorage`; limpiar al hacer logout
    - Crear `hooks/useAuth.js` que exponga `isAuthenticated`, `rol`, `login()` y `logout()`
    - _Requisitos: Requisito 2 (RN-02-01, RN-02-02)_

  - [x] 10.3 Implementar `Register.jsx` con formulario y validación cliente
    - Crear formulario con campos: nombre, correo, contraseña (con indicador de fortaleza), teléfono
    - Validar complejidad de contraseña en el cliente antes de enviar (8 chars, mayúscula, dígito, especial)
    - Mostrar errores HTTP 409 (correo duplicado) y HTTP 422 (lista de criterios incumplidos) con mensajes descriptivos
    - _Requisitos: Requisito 1 (criterios 1, 3, 4, 5)_

  - [x] 10.4 Implementar `Login.jsx` con manejo de inicio de sesión
    - Crear formulario con campos correo y contraseña
    - Manejar HTTP 401 con mensaje genérico; manejar HTTP 429 mostrando cuenta regresiva de minutos de bloqueo
    - Al éxito, despachar `setCredentials` y redirigir según rol (`/client` o `/admin`)
    - _Requisitos: Requisito 2 (criterios 1, 3, 4, 5)_

  - [x] 10.5 Implementar `ProtectedRoute.jsx` y configurar enrutamiento
    - Crear componente `ProtectedRoute` que lea `isAuthenticated` del contexto y redirija a `/login` si no hay sesión
    - Verificar rol requerido para rutas de Cliente y Administrador
    - Configurar `react-router-dom` con rutas públicas (`/register`, `/login`) y protegidas (`/client/*`, `/admin/*`)
    - _Requisitos: Requisito 2 (criterio 2), RNF02_

- [x] 10. Módulo de Reservas — Frontend React.js (SD-04)
  - [x] 11.1 Integrar reservas mediante `api/apiClient.js` y las páginas React
    - Consumir creación, listado y detalle de reservas desde `CreateReservation.jsx` y `MyReservations.jsx`
    - Manejar carga, errores y datos con estado local de React
    - _Requisitos: Requisito 4 (criterio 1), Requisito 7 (criterios 1, 2)_

  - [x] 11.2 Implementar `Packages.jsx` con catálogo completo de paquetes para Cliente
    - Listar todos los paquetes activos desde `GET /api/v1/packages` mostrando imagen, nombre, destino, precio y asientos disponibles
    - Mantener la portada con solo 4 paquetes destacados
    - Agregar botón "Reservar" visible sólo para clientes autenticados; redirigir a login si no autenticado
    - Consumir los endpoints de paquetes mediante `api/apiClient.js`
    - _Requisitos: Requisito 10 (criterio 1), RNF05_

  - [x] 11.3 Implementar `CreateReservation.jsx` con flujo de creación de reserva
    - Mostrar detalle del paquete seleccionado con precio unitario y asientos disponibles
    - Permitir al cliente ingresar `cantAsientos`; calcular y mostrar `montoTotal = precioUnitario * cantAsientos`
    - Manejar HTTP 409 (asientos insuficientes) mostrando disponibilidad actual; mostrar confirmación de éxito con `reservaId`
    - _Requisitos: Requisito 4 (criterios 1, 3), RNF05_

  - [x] 11.4 Implementar `MyReservations.jsx` con lista y filtros de estado
    - Mostrar lista de reservas del cliente con: paquete, fechas, asientos, monto y `ReservationStatusBadge` (componente de badge por estado)
    - Implementar filtro por estado (PENDIENTE_PAGO, CONFIRMADA, REPROGRAMADA, COMPLETADA, CANCELADA)
    - Agregar enlace a detalle de reserva y accesos directos a pago y reprogramación según estado
    - _Requisitos: Requisito 7 (criterios 1, 2, 3)_

- [x] 11. Módulo de Pagos — Frontend React.js (SD-05)
  - [x] 12.1 Implementar registro de pago en `Payment.jsx`
    - Consumir registro de pago y comprobante mediante `api/apiClient.js`
    - Implementar formulario `Payment.jsx` con: monto (pre-llenado de `MontoTotal`), método de pago (select con 4 opciones), número de referencia
    - Manejar HTTP 422 (monto inválido mostrando monto esperado), HTTP 409 (reserva ya confirmada), HTTP 401
    - Al éxito, mostrar confirmación y enlace al comprobante; informar que el comprobante fue enviado al correo
    - _Requisitos: Requisito 5 (criterios 1, 2, 4, 5, 6, 7)_

- [x] 12. Módulo de Reprogramación — Frontend React.js (SD-06)
  - [x] 13.1 Implementar `Reschedule.jsx` con validación de ventana
    - Mostrar datos de la reserva actual (fecha, paquete) y calcular horas restantes para inicio del tour
    - Mostrar advertencia si quedan menos de 12 horas; deshabilitar el formulario si ya no es posible reprogramar
    - Enviar `PATCH /api/v1/reservations/{id}/reschedule` con la nueva fecha seleccionada
    - Manejar HTTP 422 (fuera de ventana, fecha pasada, límite alcanzado) y HTTP 409 (sin disponibilidad) con mensajes claros
    - _Requisitos: Requisito 6 (criterios 1, 4, 5, 6, 7)_

- [x] 13. Módulo de Paquetes y Administración — Backend/Frontend (SD-10, SD-08)
  - [x] 14.1 Implementar `PackageService` con CRUD y `PackageController`
    - Crear endpoints públicos `GET /api/v1/packages` y `GET /api/v1/packages/{packageId}` (sin `[Authorize]`)
    - Crear endpoints de administración `POST`, `PUT`, `DELETE /api/v1/admin/packages` con `[Authorize(Roles = "Administrador")]`
    - Implementar eliminación lógica (`Activo = false`); validar `PrecioUnitario > 0` y `CapacidadTotal > 0`
    - _Requisitos: Requisito 10 (RN-10-01, RN-10-02, RN-10-03, criterios 1–5)_

  - [x] 14.2 Implementar `AdminClientController` para gestión de cuentas de Cliente
    - Crear endpoint `GET /api/v1/admin/clients` con `[Authorize(Roles = "Administrador")]`
    - Crear endpoint `PATCH /api/v1/admin/clients/{clientId}/status` para activar/desactivar con eliminación lógica (`Estado = Inactivo`)
    - Verificar en `LoginAsync` que cuenta `Inactivo` devuelva HTTP 403 con mensaje "cuenta desactivada"
    - _Requisitos: Requisito 8 (RN-08-01, RN-08-02, criterios 1, 2, 3, 4)_

  - [x] 14.3 Implementar administración de paquetes dentro de `AdminDashboard.jsx`
    - Crear formulario de creación con campos: nombre, destino, descripción, imagen, precio, capacidad total y fechas de inicio/fin
    - Implementar listado de paquetes con opciones de editar y desactivar (eliminación lógica, `Activo = false`)
    - Consumir endpoints admin desde `apiClient.js`
    - Mostrar error HTTP 422 cuando el precio sea ≤ S/ 0.00 y HTTP 403 cuando el usuario no tenga rol "Administrador"
    - _Requisitos: Requisito 10 (criterios 2, 3, 4, 5)_

  - [ ]* 14.4 Escribir pruebas unitarias para control de acceso por rol
    - Verificar que endpoints de administración devuelvan HTTP 403 para tokens con rol "Cliente"
    - Verificar que cuenta desactivada genere HTTP 403 al intentar login
    - _Requisitos: Requisito 8 (criterios 3, 4)_

  - [x] 14.5 Implementar configuración editable de portada (SD-13)
    - Crear entidad `ConfiguracionPortada`, DTOs `SiteSettingsDto`, interfaz `ISiteSettingsService` y servicio `SiteSettingsService`
    - Crear `SiteSettingsController` con `GET /api/v1/site-settings`, `GET /api/v1/admin/site-settings` y `PUT /api/v1/admin/site-settings`
    - Agregar pestaña "Portada" en `AdminDashboard.jsx` para editar logo, nombre, textos, estadísticas e imagen principal de fondo
    - Hacer que `Home.jsx` y `Header.jsx` consuman la configuración desde API y usen `localStorage` solo como caché/fallback
    - _Requisitos: Requisito 13 (RN-13-01 a RN-13-05)_

  - [x] 14.6 Implementar perfil dinámico con foto para Cliente y Administrador (SD-03)
    - Agregar `FotoUrl` a `Usuarios`, entidad `Usuario`, DTOs de login/perfil y validadores
    - Actualizar `Profile.jsx`, `Header.jsx` y `AuthContext.jsx` para mostrar y editar la foto de perfil
    - _Requisitos: Requisito 3 (RN-03-01 a RN-03-03)_

- [x] 14. Servicio de Notificaciones — Backend (SD-11)
  - [x] 15.1 Implementar `NotificationService` con envío SMTP y política de reintentos
    - Crear `INotificationService` con métodos: `SendWelcomeEmailAsync`, `SendReceiptAsync`, `SendRescheduleConfirmationAsync`, `SendTourReminderAsync`
    - Configurar cliente SMTP (SendGrid o SMTP externo) desde `appsettings.json`
    - Implementar política de reintentos: hasta 3 intentos con intervalo de 5 minutos; no reenviar si `EnviadoCorreo = true` (RN-11-03)
    - _Requisitos: Requisito 11 (RN-11-01, RN-11-02, RN-11-03, criterios 1, 2, 3, 4, 5)_

  - [x] 15.2 Implementar job programado para recordatorio 24 horas antes del tour
    - Crear `TourReminderBackgroundService` mediante `BackgroundService`
    - Consultar reservas con estado `Confirmada` cuya `FechaInicio` sea igual a `UtcNow + 24h`
    - Invocar `SendTourReminderAsync` para cada reserva encontrada
    - _Requisitos: Requisito 11 (RN-11-02, criterio 4)_

- [x] 15. Módulo de Reportes — Backend (SD-12)
  - [x] 16.1 Implementar `AdminReportController` con generación de PDF y XLSX
    - Implementar `ReportService` para exportar reportes administrativos de reservas y ventas
    - Crear endpoint `GET /api/v1/admin/reports/reservations?from={date}&to={date}&format={pdf|xlsx}` con `[Authorize(Roles = "Administrador")]`
    - Crear endpoint `GET /api/v1/admin/reports/sales?from={date}&to={date}&format={pdf|xlsx}`
    - Validar que `from <= to`; lanzar HTTP 422 si la validación falla (RN-12-02)
    - Generar reporte en ≤ 30 segundos para rangos de hasta 12 meses (RN-12-03)
    - _Requisitos: Requisito 12 (RN-12-01, RN-12-02, RN-12-03, criterios 1–5)_

- [ ] 16. Checkpoint Final — Integración Completa
  - Ejecutar suite completa de tests (unitarios + propiedades PBT + integración)
  - Verificar responsividad en resoluciones 360px–1920px (RNF06)
  - Verificar que todos los endpoints respondan en < 2s para catálogo y < 3s para reservas bajo carga simulada (RNF04)
  - Asegurar que HTTPS está habilitado y JWT middleware rechaza tokens inválidos en todos los endpoints protegidos (RNF02)
  - Consultar al usuario si surgen dudas antes de cerrar la implementación

- [x] 17. Organización final de scripts SQL y documentación de base de datos
  - Consolidar el script completo de instalación en `database/ToursAyacuchoPeru.sql`
  - Mantener un único script SQL oficial, sin variantes duplicadas ni credenciales predeterminadas
  - Mantener las migraciones EF Core en `ToursAyacuchoPeruAPI/Infrastructure/Persistence/Migrations/`
  - Actualizar `database/README.md` con la ejecución correcta sobre una base vacía
  - _Requisitos: Requisito 3, Requisito 10, Requisito 13, RNF03_


---

## Matriz de Descomposición de Tareas

| ID Tarea | Spec Delta de Origen | Componente | Tarea Técnica Específica |
|----------|----------------------|------------|--------------------------|
| 1.1 | SD-01 a SD-05 (Base) | Base de Datos | Ejecutar DDL completo: tablas, constraints, índices en SQL Server |
| 1.2 | SD-01 a SD-05 (Base) | Backend | Configurar `ToursAyacuchoPeruDbContext`, EF Core y cadena de conexión |
| 1.3 | SD-01 a SD-05 (Base) | Backend | Implementar jerarquía de excepciones y `GlobalExceptionMiddleware` |
| 2.1 | SD-01 | Backend | Entidades `Usuario` y `BloqueosCuenta` con EF Core |
| 2.2 | SD-01 | Backend | DTOs `RegisterRequestDto` con validadores FluentValidation |
| 2.3* | SD-01 | Pruebas PBT | **Propiedad 2**: Contraseñas inválidas rechazadas con HTTP 422 |
| 2.4 | SD-02 | Backend | `JwtService.GenerateToken` con HMAC-SHA256 y claims obligatorios |
| 2.5* | SD-02 | Pruebas PBT | **Propiedad 3**: Integridad del payload del Token JWT |
| 2.6 | SD-01 | Backend | `AuthService.RegisterAsync` con hash bcrypt (workFactor: 12) |
| 2.7* | SD-01 | Pruebas PBT | **Propiedad 1**: Hash bcrypt verificable y diferente a la contraseña original |
| 2.8 | SD-01, SD-02 | Backend | `AuthController.Register` + configuración JWT middleware |
| 3.1 | SD-02 | Backend | `AuthService.LoginAsync` con bloqueo tras 5 intentos fallidos |
| 3.2* | SD-02 | Pruebas PBT | **Propiedad 4**: Bloqueo de cuenta 15 min tras 5 intentos fallidos |
| 3.3 | SD-02 | Backend | `AuthController.Login` + `ClaimsPrincipalExtensions.GetClientId()` |
| 3.4 | SD-03 | Backend | `AuthController.UpdateProfile` + `AuthService.UpdateProfileAsync` con validación JWT |
| 5.1 | SD-04 | Backend | Entidades `Reserva` y `PaqueteTuristico` con enum `EstadoReserva` |
| 5.2 | SD-04 | Backend | `ReservationService.CreateAsync` con TX Serializable y UPDLOCK |
| 5.3* | SD-04 | Pruebas PBT | **Propiedad 5**: Overbooking imposible bajo concurrencia |
| 5.4* | SD-04 | Pruebas PBT | **Propiedad 6**: Atomicidad de TX de reserva ante fallo |
| 5.5 | SD-04, SD-07 | Backend | `ReservationController`: POST reservas + GET lista/detalle con filtros |
| 5.6* | SD-07 | Pruebas Unit. | Consulta devuelve solo reservas del cliente del JWT, filtro por estado |
| 6.1 | SD-05 | Backend | Entidades `Pago` y `Comprobante` con enums y constraint UQ |
| 6.2 | SD-05 | Backend | `PaymentService.RegisterPaymentAsync` con TX ReadCommitted |
| 6.3* | SD-05 | Pruebas PBT | **Propiedad 7**: Validación de monto con tolerancia S/ 0.01 |
| 6.4 | SD-05, SD-11 | Backend | `NotificationService.SendReceiptAsync` + generación de comprobante JSON |
| 6.5* | SD-05 | Pruebas PBT | **Propiedad 8**: Comprobante digital con los 7 campos obligatorios |
| 6.6 | SD-05 | Backend | `PaymentController`: POST pago + GET comprobante |
| 7.1 | SD-06 | Backend | `ReschedulingValidator` con lógica de ventana 12h |
| 7.2* | SD-06 | Pruebas PBT | **Propiedad 9**: Ventana de reprogramación correcta para rango 0–47h |
| 7.3 | SD-06 | Backend | `ReschedulingService.RescheduleAsync` con TX Serializable |
| 7.4* | SD-06 | Pruebas PBT | **Propiedad 10**: Atomicidad de TX de reprogramación ante fallo |
| 7.5 | SD-06 | Backend | `ReservationController.Reschedule` (PATCH endpoint) |
| 7.6* | SD-06 | Pruebas Unit. | Casos límite: segunda reprogramación, fecha pasada, sin disponibilidad |
| 10.1 | SD-01, SD-02 | Frontend | Cliente Axios con interceptores JWT en `apiClient.js` |
| 10.2 | SD-02 | Frontend | `AuthContext.jsx` + hook `useAuth.js` |
| 10.3 | SD-01 | Frontend | `Register.jsx` con validación cliente y manejo de errores |
| 10.4 | SD-02 | Frontend | `Login.jsx` con redirección por rol |
| 10.5 | SD-02 | Frontend | `ProtectedRoute` + `AdminRoute` + configuración de rutas |
| 11.1 | SD-04, SD-07 | Frontend | Integración de reservas mediante `apiClient.js` y páginas React |
| 11.2 | SD-10 | Frontend | `Packages.jsx`: catálogo completo para Cliente + portada con 4 destacados |
| 11.3 | SD-04 | Frontend | `CreateReservation.jsx`: flujo de creación con cálculo de monto |
| 11.4 | SD-07 | Frontend | `MyReservations.jsx` con filtros de estado y badges |
| 12.1 | SD-05 | Frontend | `Payment.jsx` con manejo de errores |
| 13.1 | SD-06 | Frontend | `Reschedule.jsx` con validación visual de ventana 12h |
| 14.1 | SD-10 | Backend | `PackageService` + `PackageController` (CRUD + eliminación lógica) |
| 14.2 | SD-08 | Backend | `AdminClientController`: listar clientes + activar/desactivar cuentas |
| 14.3 | SD-10 | Frontend | `AdminDashboard.jsx`: formularios CRUD de paquetes para Administrador |
| 14.4* | SD-08 | Pruebas Unit. | Control de acceso por rol: HTTP 403 para rol incorrecto y cuenta inactiva |
| 14.5 | SD-13 | Backend/Frontend | `SiteSettingsService` + pestaña Portada en `AdminDashboard.jsx` |
| 14.6 | SD-03 | Backend/Frontend/BD | Perfil con `FotoUrl`, avatar en Header y edición en `Profile.jsx` |
| 15.1 | SD-11 | Backend | `NotificationService` SMTP con política de reintentos y deduplicación |
| 15.2 | SD-11 | Backend | `TourReminderBackgroundService` para recordatorio 24h |
| 16.1 | SD-12 | Backend | `AdminReportController` + `ReportService` con generación PDF/XLSX en ≤ 30s |
| 17 | SD-03, SD-10, SD-13 | Base de Datos | `database/ToursAyacuchoPeru.sql`, seeds, migraciones y README |

*Tareas marcadas con `*` son opcionales (sub-tareas de prueba).*


---

## Estado actual de implementación

- Las subtareas marcadas como completadas reflejan funcionalidad existente en `ToursAyacuchoPeruAPI` y verificada con `dotnet build`.
- Verificación del 14 de julio de 2026: `dotnet test` y `npm run build` finalizaron correctamente; npm reportó 0 vulnerabilidades.
- El proyecto `ToursAyacuchoPeruAPI.Tests` contiene pruebas unitarias xUnit y pruebas de integración para autenticación, clientes, reseñas, paquetes, pagos, configuración, validadores, reservas y reportes.
- Última ejecución backend: **46/46 pruebas correctas**, sin errores ni omisiones.
- La consulta de reservas con filtro por estado, administración de cuentas/paquetes, reseñas, reportes y notificaciones SMTP con reintentos ya tienen implementación base en el código actual.
- Permanecen pendientes principalmente las pruebas PBT, pruebas con SQL Server real, pruebas de carga y validación manual completa de responsividad y despliegue.
- La arquitectura vigente del backend es por capas dentro del proyecto API: `Domain`, `Application`, `Infrastructure` y `Presentation`.

---
## Notas de Implementación

- Las tareas marcadas con `*` son **opcionales** y pueden omitirse para una entrega MVP más rápida; sin embargo, se recomienda ejecutarlas para cumplir con los requisitos de la investigación académica (ISO/IEC 25010 — Corrección Funcional).
- Las 10 propiedades PBT definidas en el diseño **deben ejecutarse con al menos 100 iteraciones** cada una (FsCheck default). Se recomienda aumentar a 200 para las propiedades de concurrencia (Propiedades 5 y 6).
- Los **Checkpoints** (tareas 4, 8, 16) son puntos de verificación obligatorios antes de avanzar al siguiente módulo; no son tareas de código sino de validación.
- Cada tarea asume disponibilidad de los documentos de requisitos y diseño como contexto durante la implementación.
- La implementación de **notificaciones SMTP** puede simularse en entorno de desarrollo con un servicio como `MailHog` o `Papercut SMTP`; en producción usar SendGrid.
- Los **reportes PDF/XLSX** (tarea 16.1) son funcionalidad de administración y pueden implementarse como última prioridad del MVP.

---

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2", "1.3"] },
    { "id": 1, "tasks": ["2.1", "2.4"] },
    { "id": 2, "tasks": ["2.2"] },
    { "id": 3, "tasks": ["2.3", "2.5", "2.6"] },
    { "id": 4, "tasks": ["2.7", "2.8", "3.1", "6.1"] },
    { "id": 5, "tasks": ["3.2", "3.3"] },
    { "id": 6, "tasks": ["3.4", "5.1"] },
    { "id": 7, "tasks": ["5.2", "7.1"] },
    { "id": 8, "tasks": ["5.3", "5.4", "5.5", "5.6", "6.2"] },
    { "id": 9, "tasks": ["6.3", "6.4", "6.6", "7.2", "7.3", "10.1"] },
    { "id": 10, "tasks": ["6.5", "7.4", "7.5", "7.6", "10.2", "14.1", "15.1"] },
    { "id": 11, "tasks": ["10.3", "10.4", "10.5", "11.1", "14.2", "15.2"] },
    { "id": 12, "tasks": ["11.2", "11.3", "14.3", "14.4"] },
    { "id": 13, "tasks": ["11.4", "12.1", "13.1"] },
    { "id": 14, "tasks": ["16.1"] }
  ]
}
```
