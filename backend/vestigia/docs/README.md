# Backend de Vestigia — Laboratorio de pruebas (PWA + backend)

Servicio base Spring Boot con CRUD de **salas** sobre PostgreSQL, consumido por la
PWA (`AiltonMV/vestigia-pwa`) desde `http://localhost:5173`.

| Documento | Contenido |
|---|---|
| [01-arquitectura-y-contrato.md](01-arquitectura-y-contrato.md) | Capas, endpoints, JSON, códigos y regla de idempotencia |
| [02-provisioning/](02-provisioning/) | Scripts SQL (crear BD, esquema, semilla, limpieza) |
| [03-evidencias-db/](03-evidencias-db/) | Consultas antes/después del envío de datos |
| [04-logs-backend/](04-logs-backend/) | Logs del servidor y métricas |
| [05-evidencias-pwa/](05-evidencias-pwa/) | Evidencias de la PWA (equipo de PWA) |
| [06-casos-de-prueba.md](06-casos-de-prueba.md) | Casos de prueba |
| [pruebas-api.md](pruebas-api.md) | Comandos curl listos para copiar |

## Requisitos

- **JDK 21 o superior** (`java -version`). El proyecto compila con `release 21`.
- **PostgreSQL** en `localhost:5432` (instalador oficial con pgAdmin, o Docker).
  Ver [02-provisioning/README.md](02-provisioning/README.md).
- Maven no hace falta: se usa el wrapper `./mvnw`.

## Levantar todo desde cero

Todos los comandos desde `backend/vestigia`.

```bash
# 1. Base de datos (una sola vez). Pide la contraseña del usuario postgres.
psql -h localhost -U postgres -d postgres -f docs/02-provisioning/00_create_database.sql
psql -h localhost -U postgres -d vestigia -f docs/02-provisioning/01_schema.sql
psql -h localhost -U postgres -d vestigia -f docs/02-provisioning/02_seed.sql

# 2. Credenciales locales (una sola vez; el archivo está en .gitignore)
cp application-local.properties.example application-local.properties
#    y edita la contraseña en application-local.properties

# 3. Arrancar
./mvnw spring-boot:run
```

### Credenciales de la base de datos

El backend toma las credenciales en este orden de prioridad:

1. Variables de entorno `DB_URL`, `DB_USER`, `DB_PASSWORD`
   (p. ej. `DB_PASSWORD='tu-contraseña' ./mvnw spring-boot:run`).
2. `application-local.properties` en `backend/vestigia/` (no se sube al repo).
3. Valores por defecto de `application.properties` (`postgres`/`postgres` en `localhost:5432/vestigia`).

El archivo local se busca en el directorio desde donde se ejecuta el backend,
así que hay que arrancarlo desde `backend/vestigia`.

## URLs

| URL | Qué es |
|---|---|
| <http://localhost:8080/api/salas> | API de salas |
| <http://localhost:8080/actuator/health> | Salud del servicio y de la BD |
| <http://localhost:8080/actuator/metrics/jvm.memory.used> | Memoria usada de la JVM |
| <http://localhost:8080/actuator/metrics/http.server.requests> | Tiempos y conteo de peticiones |

## Logs

- Consola y archivo `logs/vestigia-backend.log` (ignorado por git).
- Una línea por petición a `/api/**`:
  `[API] POST /api/salas -> 201 | 43 ms | heap 187/512 MB`
- SQL ejecutado por Hibernate (`org.hibernate.SQL`) y parámetros enlazados (`org.hibernate.orm.jdbc.bind`).

## Pruebas

```bash
./mvnw test                     # no requiere PostgreSQL
./mvnw -q -DskipTests package   # genera target/vestigia-0.0.1-SNAPSHOT.jar
```

## CORS

Orígenes permitidos en `app.cors.allowed-origins` (por defecto Vite dev `5173`
y preview `4173`). Se puede cambiar sin tocar código:

```bash
./mvnw spring-boot:run -Dspring-boot.run.arguments=--app.cors.allowed-origins=http://localhost:5173,https://mi-pwa.example
```
