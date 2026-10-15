# Provisioning de la base de datos

Scripts para dejar el laboratorio en un estado inicial reproducible.

## 1. Levantar PostgreSQL

Elige una opción.

**A. Instalador oficial (incluye pgAdmin).** Descarga PostgreSQL desde
<https://www.postgresql.org/download/> e instálalo con pgAdmin. Anota la
contraseña que asignes al usuario `postgres`.

**B. Docker** (sin Docker Compose):

```bash
docker run --name vestigia-db -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16
```

## 2. Orden de ejecución

| # | Script | Conectado a | Qué hace |
|---|---|---|---|
| 1 | `00_create_database.sql` | `postgres` | Crea la BD `vestigia` |
| 2 | `01_schema.sql` | `vestigia` | Crea la tabla `salas` (idempotente) |
| 3 | `02_seed.sql` | `vestigia` | Inserta 6 salas con UUID fijos (idempotente) |
| — | `99_clean.sql` | `vestigia` | Vacía la tabla (para repetir el laboratorio) |

Para volver al estado inicial: `99_clean.sql` y luego `02_seed.sql`.

### Con psql

Desde esta carpeta (`docs/02-provisioning`):

```bash
psql -h localhost -U postgres -d postgres -f 00_create_database.sql
psql -h localhost -U postgres -d vestigia -f 01_schema.sql
psql -h localhost -U postgres -d vestigia -f 02_seed.sql
```

Con Docker, sin `psql` instalado:

```bash
docker exec -i vestigia-db psql -U postgres -d postgres < 00_create_database.sql
docker exec -i vestigia-db psql -U postgres -d vestigia < 01_schema.sql
docker exec -i vestigia-db psql -U postgres -d vestigia < 02_seed.sql
```

### Con pgAdmin

1. Conéctate al servidor, abre *Query Tool* sobre la BD `postgres` y ejecuta `00_create_database.sql`.
2. Refresca, abre *Query Tool* sobre la BD `vestigia` y ejecuta `01_schema.sql` y `02_seed.sql`.
