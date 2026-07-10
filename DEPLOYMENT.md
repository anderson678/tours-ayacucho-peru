# Guia de despliegue

Este proyecto tiene tres piezas:

1. Base de datos SQL Server: `ToursAyacuchoPeruDB`.
2. Backend ASP.NET Core: `ToursAyacuchoPeruAPI`.
3. Frontend React/Vite: `tours-ayacucho-frontend`.

## Opcion recomendada para el proyecto academico

Para una entrega local o en una PC/servidor Windows:

1. Crear la base de datos en SQL Server.
2. Publicar la API con `dotnet publish`.
3. Construir el frontend con `npm run build`.
4. Servir la API y el frontend con IIS, o ejecutar la API con `dotnet` y servir el frontend con un hosting estatico.

## 1. Base de datos

Si crearas todo desde cero, ejecuta en SQL Server Management Studio:

```sql
database/ToursAyacuchoPeru.sql
```

Verifica que existan las tablas:

```sql
USE ToursAyacuchoPeruDB;
GO

SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';
```

## 2. Configurar la API

En desarrollo la API usa `appsettings.json`. Para despliegue, configura estos valores con variables de entorno o en `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.\\MSSQLSERVER01;Initial Catalog=ToursAyacuchoPeruDB;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "CAMBIA_ESTE_SECRETO_POR_UNO_LARGO_Y_SEGURO",
    "Issuer": "ToursAyacuchoPeruAPI",
    "Audience": "ToursAyacuchoPeruWeb",
    "ExpiryHours": 8
  },
  "AllowedOrigins": [
    "http://localhost:5173",
    "http://localhost:3000",
    "https://TU-DOMINIO-O-IP-DEL-FRONTEND"
  ]
}
```

No subas secretos reales a GitHub.

## 3. Publicar la API

Desde la raiz del proyecto:

```powershell
dotnet publish ToursAyacuchoPeruAPI\ToursAyacuchoPeruAPI.csproj -c Release -o publish\api
```

Para probar localmente la publicacion:

```powershell
dotnet publish\api\ToursAyacuchoPeruAPI.dll
```

La API debe iniciar sin errores y conectarse a `ToursAyacuchoPeruDB`.

## 4. Configurar y construir el frontend

Crea un archivo `.env.production` dentro de `tours-ayacucho-frontend`:

```env
VITE_API_BASE_URL=https://TU-DOMINIO-O-IP-DE-LA-API
```

Si la API queda local en el mismo equipo:

```env
VITE_API_BASE_URL=http://localhost:5150
```

Luego construye el frontend:

```powershell
cd tours-ayacucho-frontend
npm run build
```

El resultado queda en:

```text
tours-ayacucho-frontend/dist
```

Esa carpeta `dist` es lo que se sube o publica como sitio web estatico.

## 5. Verificacion antes de entregar

Ejecuta pruebas:

```powershell
dotnet test ToursAyacuchoPeruAPI.Tests\ToursAyacuchoPeruAPI.Tests.csproj --no-restore
```

Verifica login:

```text
Correo: admin@toursayacuchoperu.com
Clave: Admin123@
```

Verifica reportes:

```text
GET /api/v1/admin/reports/reservations?from=2026-01-01&to=2026-12-31&format=pdf
GET /api/v1/admin/reports/sales?from=2026-01-01&to=2026-12-31&format=xlsx
```

## Checklist final

- Base `ToursAyacuchoPeruDB` creada.
- Tablas principales existen.
- API publica en modo Release.
- `ConnectionStrings:DefaultConnection` apunta al SQL Server correcto.
- `JwtSettings:Secret` no usa un valor publico o debil.
- `AllowedOrigins` incluye la URL del frontend.
- Frontend construido con `VITE_API_BASE_URL` apuntando a la API.
- Login admin funciona.
- Reportes RF17 descargan PDF/XLSX.
