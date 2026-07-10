# Base de datos

Script SQL para crear la base de datos completa de **TOURS AYACUCHO PERU**.

Motor recomendado: **Microsoft SQL Server**.

## Archivo principal

```text
database/
|-- ToursAyacuchoPeru.sql
`-- README.md
```

## Como crear la base de datos

Ejecuta este script en SQL Server Management Studio, Azure Data Studio o `sqlcmd`:

```sql
database/ToursAyacuchoPeru.sql
```

El script crea la base completa `ToursAyacuchoPeruDB`, incluyendo:

- Tablas principales del sistema.
- Relaciones, constraints e indices.
- Usuario administrador inicial.
- Configuracion inicial de portada.
- Paquetes turisticos iniciales.
- Soporte para imagenes de paquetes y foto de perfil de usuarios.
- Registro de notificaciones y comprobantes.

## Administrador inicial

El script crea un usuario administrador para pruebas locales:

```text
Correo: admin@toursayacuchoperu.com
Clave:  Admin123@
```

Cambia esta clave despues de la primera ejecucion si usas la base fuera de un entorno local.

## Notas para GitHub

- No subas respaldos `.bak`.
- No subas archivos fisicos de SQL Server como `.mdf` o `.ldf`.
- No subas datos reales de clientes.
- Si necesitas modificar la estructura de la base, actualiza `ToursAyacuchoPeru.sql` para mantener un unico script oficial.
