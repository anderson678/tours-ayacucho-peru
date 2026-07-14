# Despliegue de la API en MonsterASP.NET

Esta guía publica únicamente `ToursAyacuchoPeruAPI`. No publiques el proyecto de pruebas ni archivos con credenciales.

## 1. Preparar el hosting

1. Crea un sitio ASP.NET Core compatible con .NET 10.
2. Crea una base MSSQL vacía.
3. Ejecuta `database/ToursAyacuchoPeru.sql` conectado a esa base.

El script crea las nueve tablas y los datos iniciales, pero no crea ni selecciona una base de datos.

## 2. Configurar el entorno

Agrega estos valores en Application Settings o variables de entorno del hosting:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=SERVIDOR;Database=BASE;User Id=USUARIO;Password=CONTRASENA;TrustServerCertificate=True;MultipleActiveResultSets=True;
JwtSettings__Secret=CLAVE_ALEATORIA_DE_AL_MENOS_32_CARACTERES
JwtSettings__Issuer=ToursAyacuchoPeruAPI
JwtSettings__Audience=ToursAyacuchoPeruWeb
AllowedOrigins__0=https://URL-DEL-FRONTEND
```

Si utilizas correo, configura también `SmtpSettings__Username` y `SmtpSettings__Password`. No guardes contraseñas ni claves reales en GitHub.

## 3. Publicar

1. Activa WebDeploy y descarga el perfil desde MonsterASP.NET.
2. En Visual Studio, selecciona **Publicar** sobre `ToursAyacuchoPeruAPI`.
3. Importa el perfil y publica en modo **Release**, con destino `net10.0` y dependiente del framework.
4. No agregues el perfil descargado ni la carpeta publicada al repositorio.

## 4. Verificar

- Abre `https://TU-SITIO/swagger`.
- Prueba registro, inicio de sesión y un endpoint autenticado.
- Comprueba paquetes, reservas y persistencia en MSSQL.
- Verifica el acceso desde la URL definitiva del frontend para detectar errores CORS.

Swagger está habilitado temporalmente en producción para la presentación. Después conviene protegerlo o limitarlo al entorno de desarrollo.

## Primer administrador

Registra una cuenta mediante la API y luego cambia su rol desde MSSQL:

```sql
UPDATE Usuarios
SET Rol = 'Administrador'
WHERE Correo = 'CORREO_REGISTRADO';
```

Vuelve a iniciar sesión para generar un JWT con el rol actualizado.
