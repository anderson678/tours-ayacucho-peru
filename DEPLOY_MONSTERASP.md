# Despliegue de ToursAyacuchoPeruAPI en MonsterASP.NET

Esta guia publica solamente `ToursAyacuchoPeruAPI`. No publique `ToursAyacuchoPeruAPI.Tests` ni la solucion completa.

## 1. Crear los recursos

1. Cree una cuenta e ingrese al panel de MonsterASP.NET.
2. Cree un **Website ASP.NET Core** y seleccione **.NET 10**.
3. Cree una base de datos **MSSQL** desde el panel.
4. Guarde el servidor, nombre de base, usuario y contrasena generados por el hosting.

## 2. Crear el esquema de la base de datos

1. Abra la base creada usando SQL Server Management Studio o el administrador SQL del panel.
2. Asegurese de estar conectado a la base de datos asignada, que debe estar vacia.
3. Ejecute completo `database/ToursAyacuchoPeru.sql`.
4. Confirme que se hayan creado las nueve tablas y que el resultado final las enumere.

El script no contiene `CREATE DATABASE`, `USE` ni `ALTER DATABASE`, porque la base ya es creada y seleccionada por MonsterASP.NET.

## 3. Configurar variables de entorno

Configure estos valores en las variables de entorno o Application Settings del Website:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=SERVIDOR;Database=BASE;User Id=USUARIO;Password=CONTRASENA;TrustServerCertificate=True;MultipleActiveResultSets=True;
JwtSettings__Secret=CLAVE_ALEATORIA_LARGA_DE_AL_MENOS_32_CARACTERES
JwtSettings__Issuer=ToursAyacuchoPeruAPI
JwtSettings__Audience=ToursAyacuchoPeruWeb
AllowedOrigins__0=https://URL-DEFINITIVA-DEL-FRONTEND
```

No guarde la contrasena SQL ni el secreto JWT en `appsettings.json` o en archivos de publicacion.

Si se enviaran correos, configure tambien:

```text
SmtpSettings__Username=USUARIO_SMTP
SmtpSettings__Password=CONTRASENA_O_APP_PASSWORD_SMTP
```

Los valores no secretos de servidor, puerto, remitente y SSL pueden permanecer en `appsettings.json`. Para permitir mas de un frontend, agregue `AllowedOrigins__1`, `AllowedOrigins__2`, etc. Las URLs no deben terminar en `/`.

## 4. Publicar con Visual Studio y Web Deploy

1. En el panel del Website, active **WebDeploy**.
2. Descargue el archivo de perfil **PublishSettings**.
3. En Visual Studio, haga clic derecho exclusivamente sobre `ToursAyacuchoPeruAPI` y seleccione **Publicar**.
4. Importe el archivo `.PublishSettings` descargado.
5. Use configuracion **Release**, destino **net10.0** y despliegue dependiente del framework.
6. Publique y espere la confirmacion de Web Deploy.

El proyecto de pruebas es una referencia separada y no es dependencia de la API, por lo que no forma parte de la publicacion del `.csproj` de la API.

## 5. Verificar el despliegue

1. Abra `https://SU-SITIO/swagger` y confirme que carga Swagger UI en Production.
2. Pruebe el registro y login desde Swagger y verifique que login devuelve un JWT.
3. Autorice Swagger con `Bearer TOKEN` y pruebe un endpoint autenticado.
4. Pruebe la consulta de paquetes y un flujo de reserva.
5. Compruebe en MSSQL que los datos se hayan guardado.
6. Desde la URL definitiva del frontend, compruebe que no existan errores CORS.
7. Si habilito SMTP, pruebe el envio y revise los registros de la aplicacion.

## 6. Crear el primer administrador sin guardar contrasenas en SQL

El script no incluye credenciales predeterminadas. Para crear el primer administrador de forma segura:

1. Registre una cuenta mediante el endpoint de registro de la API usando una contrasena privada.
2. Conectese a MSSQL y promueva exclusivamente esa cuenta:

```sql
UPDATE Usuarios
SET Rol = 'Administrador'
WHERE Correo = 'CORREO_REGISTRADO';
```

3. Inicie sesion nuevamente para obtener un JWT que incluya el rol actualizado.

Para una operacion publica posterior a la presentacion, proteja Swagger o vuelva a limitarlo al entorno Development.
