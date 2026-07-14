# TOURS AYACUCHO PERU

Plataforma web para gestionar paquetes turísticos, reservas, pagos, reseñas y clientes.

## Tecnologías

- Backend: ASP.NET Core y C# (.NET 10)
- Frontend: React y Vite
- Base de datos: Microsoft SQL Server
- ORM: Entity Framework Core
- Autenticación: JWT
- Pruebas: xUnit

## Estructura

```text
ToursAyacuchoPeruAPI/        API backend
ToursAyacuchoPeruAPI.Tests/  Pruebas automatizadas
tours-ayacucho-frontend/     Aplicación frontend
database/                    Script de base de datos
```

## Base de datos local

El script crea las tablas y los datos iniciales, pero no crea la base de datos. Primero crea una base vacía llamada `ToursAyacuchoPeruDB` y después ejecuta:

```powershell
sqlcmd -S ".\MSSQLSERVER01" -E -d "ToursAyacuchoPeruDB" -i "database/ToursAyacuchoPeru.sql"
```

También puedes abrir `database/ToursAyacuchoPeru.sql` con SQL Server Management Studio y ejecutarlo sobre la base vacía.

## Ejecutar el backend

```powershell
cd ToursAyacuchoPeruAPI
dotnet restore
dotnet run
```

La API local utiliza `http://localhost:5150`. Configura las credenciales mediante variables de entorno o `dotnet user-secrets`; no guardes secretos reales en archivos del repositorio.

## Ejecutar el frontend

```powershell
cd tours-ayacucho-frontend
npm install
npm run dev
```

Puedes copiar `.env.example` como `.env` para configurar la URL de la API. El archivo `.env` está excluido de Git.

## Pruebas y compilación

```powershell
dotnet test ToursAyacuchoPeruAPI.Tests
cd tours-ayacucho-frontend
npm run build
```

La guía opcional para publicar la API está en `DEPLOY_MONSTERASP.md`.
