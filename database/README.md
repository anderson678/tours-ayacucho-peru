# Base de datos

`ToursAyacuchoPeru.sql` contiene el esquema y los datos iniciales del sistema para Microsoft SQL Server.

## Ejecución

1. Crea o selecciona una base de datos vacía.
2. Ejecuta `ToursAyacuchoPeru.sql` conectado directamente a esa base.

Ejemplo local:

```powershell
sqlcmd -S ".\MSSQLSERVER01" -E -d "ToursAyacuchoPeruDB" -i "database/ToursAyacuchoPeru.sql"
```

El script crea las nueve tablas, relaciones, restricciones, índices, configuración de portada y paquetes turísticos iniciales. No contiene `CREATE DATABASE`, no cambia la base seleccionada y no crea usuarios con contraseñas predeterminadas.

Para crear el primer administrador, registra una cuenta mediante la API y actualiza su rol de forma controlada:

```sql
UPDATE Usuarios
SET Rol = 'Administrador'
WHERE Correo = 'CORREO_REGISTRADO';
```

No subas respaldos, archivos físicos de SQL Server ni datos reales de clientes al repositorio.
