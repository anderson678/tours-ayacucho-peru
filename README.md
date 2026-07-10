# TOURS AYACUCHO PERU

Plataforma web para la gestion de ventas, reservas, pagos, resenas y administracion de paquetes turisticos de **TOURS AYACUCHO PERU**.

El repositorio contiene backend, frontend, scripts de base de datos, pruebas y documentacion de requisitos del proyecto.

## Tecnologias

- Backend: ASP.NET Core / C# (.NET 10)
- Frontend: React + Vite
- Base de datos: Microsoft SQL Server
- ORM: Entity Framework Core
- Autenticacion: JWT
- Pruebas: xUnit

## Estructura

```text
.
|-- ToursAyacuchoPeruAPI/        # API backend
|-- ToursAyacuchoPeruAPI.Tests/  # Pruebas automatizadas
|-- tours-ayacucho-frontend/     # Aplicacion frontend
|-- database/                    # Script SQL principal
|-- .kiro/                       # Requisitos, diseno y tareas del proyecto
|-- .gitignore
`-- README.md
```

## Base de datos

Ejecuta el script principal:

```powershell
sqlcmd -S ".\MSSQLSERVER01" -E -d master -v DatabaseName="ToursAyacuchoPeruDB" -i "database/ToursAyacuchoPeru.sql"
```

Este script crea la base `ToursAyacuchoPeruDB`, sus tablas, relaciones, datos iniciales, configuracion de portada y paquetes turisticos.

## Backend

```powershell
cd ToursAyacuchoPeruAPI
dotnet restore
dotnet run
```

La API se ejecuta localmente en:

```text
http://localhost:5150
```

## Frontend

```powershell
cd tours-ayacucho-frontend
npm install
npm run dev
```

## Configuracion local

Antes de ejecutar el backend, configura la cadena de conexion y la clave JWT para tu entorno local. Para evitar subir secretos a GitHub, usa variables de entorno o `dotnet user-secrets`.

Ejemplo:

```powershell
cd ToursAyacuchoPeruAPI
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\MSSQLSERVER01;Database=ToursAyacuchoPeruDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;"
dotnet user-secrets set "JwtSettings:Secret" "CAMBIA_ESTA_CLAVE_LOCAL_DE_AL_MENOS_32_CARACTERES"
```

## Documentacion del proyecto

La carpeta `.kiro/` contiene la documentacion usada durante el desarrollo, incluyendo requisitos, diseno y tareas implementadas.

## Limpieza del repositorio

No se incluyen carpetas generadas como `bin/`, `obj/`, `publish/`, `node_modules/` o `dist/`. Estas se regeneran automaticamente al compilar o instalar dependencias.
