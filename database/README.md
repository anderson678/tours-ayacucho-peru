# Scripts de base de datos

Ejecuta `ToursAyacuchoPeru.sql` cuando quieras crear la base desde cero. Ese archivo contiene:

1. Creacion de la base `ToursAyacuchoPeruDB`.
2. Creacion de tablas, constraints e indices.
3. Usuario administrador inicial.
4. Configuracion inicial de portada.
5. Paquetes turisticos iniciales.

Los archivos numerados sirven para mantenimiento por partes:

- `01_schema.sql`: estructura principal de la base.
- `02_seed_admin.sql`: crea o actualiza el administrador inicial.
- `03_seed_packages.sql`: carga paquetes turisticos iniciales.
- `04_seed_site_settings.sql`: crea o actualiza la configuracion de portada.
- `migrations/`: cambios incrementales para bases ya existentes.

Para una instalacion nueva usa solo:

```sql
database/ToursAyacuchoPeru.sql
```

Para actualizar una base existente, ejecuta los scripts incrementales en este orden:

```sql
database/migrations/2026-07-09_add_user_profile_photo.sql
database/migrations/2026-07-09_add_package_image_url.sql
database/migrations/2026-07-09_add_notification_log.sql
database/migrations/2026-07-09_add_site_settings.sql
database/migrations/2026-07-09_align_review_rating_type.sql
```

Despues puedes ejecutar los seeds que necesites:

```sql
database/02_seed_admin.sql
database/03_seed_packages.sql
database/04_seed_site_settings.sql
```

Nota: el proyecto mantiene el esquema con scripts SQL. Las migraciones EF Core que existen en la API no deben usarse como fuente principal del esquema.
