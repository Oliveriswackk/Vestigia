# CLAUDE.md — Backend de Vestigia

Instrucciones para trabajar en el backend Spring Boot de **Vestigia**, proyecto integrador de la Universidad Tecnológica de Chihuahua (grupo IDGS101N, Ingeniería en Desarrollo y Gestión de Software).

> Regla general: lo marcado como **[PENDIENTE]** o **[PROVISIONAL]** no está decidido por el equipo. No lo inventes ni lo des por hecho; si una tarea depende de ello, pregunta.

## 1. Contexto del proyecto

Vestigia es un videojuego de exploración y acertijos con tres clientes que **no se conectan entre sí**; comparten información mediante una plataforma común:

| Cliente | Tecnología | Papel |
|---|---|---|
| Videojuego | Unity + C# | Experiencia principal (~15 min) |
| App móvil | Flutter + Dart | Libreta complementaria; algunos acertijos se juegan en el teléfono y se reflejan en Unity |
| PWA | React + Vite + TypeScript | Interfaz web responsiva con información complementaria |

- Escenarios: **pueblo, faro y observatorio**. Mecánica central: cambiar la perspectiva de cámara para descubrir objetos, accesos y pistas.
- Primera versión en español; mercado México.
- El juego debe poder completarse sin la app móvil. Las apps complementarias son apoyo y su alcance se mantiene subordinado al juego.
- Equipo de 6 personas, **Scrum** gestionado con GitHub Projects. Product Owner: Aldo Barrera. Scrum Master: María José Durán.
- El rol de **backend e integraciones** diseña el modelo de datos, configura la plataforma de datos y verifica la comunicación con los clientes.

## 2. Alcance actual: Laboratorio de pruebas (PWA + backend)

Entrega por equipo para el **14 de octubre de 2026**. Este backend lo desarrolla Raúl; la PWA la desarrolla otro compañero.

**Objetivo:** validar la integración de una PWA con un backend y una base de datos: consultas, renderizado de datos, persistencia, envío de nuevas transacciones y comportamiento offline.

**Lo que pide el profesor de PWA:**
- Backend Spring Boot simple con endpoints **CRUD** que la PWA consuma.
- Estructura como el ejemplo `customer` de clase: `controller`, `dto`, `model`, `repository`, `service`.
- **Solo el servicio base.** "Comp" se refiere al servicio compuesto de clase (`comp_customer`, que llama al servicio base vía WebClient). **No** es Docker Compose y **no** se requiere.

**Alcance decidido:** solo la entidad **salas**. El esquema completo de la base de datos aún no está diseñado.

**Fuera de alcance (no implementar ni asumir):** modelos de pistas, progreso, jugadores o acertijos; servicio compuesto; Docker Compose; Firebase.

## 3. Stack del backend

- **Java 21** (`release 21`; compila también con JDKs más nuevos), **Spring Boot 4.1.1**, **Maven** con wrapper (`./mvnw`), empaquetado JAR. Group/paquete `com.vestigia`.
- Dependencias: Spring Web MVC, Spring Data JPA, Validation, PostgreSQL Driver y Actuator. **No agregar** Lombok, MapStruct, Flyway, Swagger ni otras sin acuerdo del equipo.
- Spring Boot 4 usa Jackson 3 y `@WebMvcTest` vive en `org.springframework.boot.webmvc.test.autoconfigure`; `@MockitoBean` en lugar de `@MockBean`.
- **PostgreSQL** como base de datos. Decisión vigente: Firebase queda fuera del alcance actual.
- El esquema lo crean los scripts de `docs/02-provisioning/`; Hibernate solo lo valida (`spring.jpa.hibernate.ddl-auto=validate`). JSON en snake_case (`spring.jackson.property-naming-strategy=SNAKE_CASE`).
- Spring Boot Actuator expone `health` y `metrics`.
- CORS para la PWA: `http://localhost:5173` (Vite dev) y `http://localhost:4173` (preview), configurable con `app.cors.allowed-origins`.
- Credenciales de BD por variables de entorno `DB_URL`, `DB_USER`, `DB_PASSWORD`. Los valores por defecto de `application.properties` son solo de desarrollo local. Las contraseñas reales **nunca** van en el repo ni en este archivo.
- Logs: consola y `logs/vestigia-backend.log` (ignorado por git), con SQL de Hibernate y una línea `[API]` por petición (ver `RequestLoggingFilter`).

## 4. Repositorios

- **Monorepo del equipo:** `github.com/Oliveriswackk/Vestigia` (público, cuenta personal de un compañero). El backend vive en `backend/` (proyecto `backend/vestigia`).
- **Repo de la PWA (separado):** `github.com/AiltonMV/vestigia-pwa`. Lo revisa el profesor de PWA; no se modifica desde aquí.
- Credenciales y secretos **nunca** se guardan en el repositorio (el repo es público).
- Entornos lógicos previstos: desarrollo, prueba y producción, con credenciales y configuración separadas en la medida que permitan los recursos.

## 5. Estructura del código

Paquetes por capa, siguiendo el ejemplo `customer` de clase (el profesor califica esta estructura):

```
com.vestigia
├── controller/   SalaController, ApiExceptionHandler (@RestControllerAdvice)
├── dto/          Sala            (record, sin sufijo; es lo que viaja en JSON)
├── model/        SalaModel       (entidad JPA, sufijo Model)
├── repository/   SalaRepository
├── service/      SalaService     (convierte SalaModel <-> Sala)
├── config/       CorsConfig, RequestLoggingFilter   (únicos extras permitidos)
└── VestigiaApplication.java
```

- Clases en singular y PascalCase. El DTO se llama como la entidad; el modelo lleva sufijo `Model`.
- El controller **no** toca el repository; todo pasa por el service.
- Sin Lombok: getters y setters a mano.
- No crear servicio compuesto, `WebClient`, Docker Compose ni capas o patrones adicionales. Tipos auxiliares pequeños van como records anidados (p. ej. `SalaService.ResultadoCreacion`, `ApiExceptionHandler.ErrorBody`), no como archivos nuevos.

## 6. Entidad `salas`

"Sala" se interpreta como un área o sala de los escenarios del juego. **Esquema provisional, sin confirmar por el equipo:**

```sql
CREATE TABLE salas (
  id          UUID PRIMARY KEY,
  nombre      VARCHAR(100) NOT NULL,
  descripcion TEXT,
  escenario   VARCHAR(20) NOT NULL CHECK (escenario IN ('pueblo','faro','observatorio')),
  activa      BOOLEAN NOT NULL DEFAULT TRUE,
  creada_en   TIMESTAMP NOT NULL DEFAULT NOW()
);
```

Si cambian los campos, actualizar en el mismo cambio: este archivo, `docs/02-provisioning/01_schema.sql` (y `02_seed.sql` si aplica), `SalaModel`, `Sala` y el contrato en `docs/01-arquitectura-y-contrato.md`.

Reglas del DTO: `nombre` obligatorio (máx. 100), `escenario` obligatorio y uno de `pueblo|faro|observatorio`, `activa` null se guarda como `true`, `creada_en` lo asigna el servidor y se ignora si el cliente lo manda.

## 7. Endpoints

Contrato completo en `docs/01-arquitectura-y-contrato.md`.

| Método | Ruta | Respuesta |
|---|---|---|
| GET | `/api/salas` | 200, ordenadas por `creada_en` ascendente |
| GET | `/api/salas/{id}` | 200 / 404 |
| POST | `/api/salas` | 201 con `Location` si la crea / 200 si el `id` ya existía / 400 |
| PUT | `/api/salas/{id}` | 200 / 404 / 400. El `id` de la ruta manda; `creada_en` no cambia |
| DELETE | `/api/salas/{id}` | 204 / 404 |

Errores con cuerpo JSON uniforme (`timestamp`, `status`, `error`, `message`, `path`, `details`): 400 validación, JSON malformado o id no UUID; 404 no encontrado; 500 genérico sin stacktrace.

### Comportamiento offline

- El `id` (UUID) **lo genera la PWA**; si no viene, el servidor genera uno.
- `POST /api/salas` es **idempotente por `id`, no upsert**: si el `id` ya existe, **no modifica nada ni duplica** y responde 200 con la sala existente. Así el reenvío de la cola del Service Worker al recuperar la red no genera duplicados ni sobrescribe. Para modificar se usa `PUT`.
- Para la PWA, 201 y 200 son ambos éxito: en los dos casos saca la petición de su cola.
- Reenvíos simultáneos con el mismo `id`: la llave primaria impide el duplicado y el servicio devuelve la existente (200).
- El backend no debe romper el estado guardado ante reenvíos repetidos (RF-13: las interfaces informan fallos de sincronización sin corromper el estado).
- La PWA consulta información complementaria mediante interfaz responsiva (RF-10).

## 8. Control de versiones

### Commits

Prefijos obligatorios (definidos por el equipo para el repo de la PWA; se usan también aquí):

| Prefijo | Uso |
|---|---|
| `New:` | Algo nuevo (funcionalidad, archivo, endpoint) |
| `Refactor:` | Reestructurar código existente sin cambiar su comportamiento |
| `Fix:` | Corrección de errores |
| `Merge:` | Integración de ramas |

Formato: `Prefijo: descripción breve`. Ejemplo: `New: endpoint GET /api/salas`.

### Ramas

- **Convención de nombres: [PENDIENTE]**, el equipo aún no la decide. No impongas un esquema; usa nombres descriptivos y cortos y avisa si hace falta uno.
- Modelo de trabajo definido en la planeación:
  - Rama principal **estable**, siempre en condiciones de producir una versión liberable.
  - Ramas **de corta duración**, una por tarea.
  - Cambios incorporados mediante **pull request** con revisión.
  - Cada PR describe su propósito, cómo se comprobó y a qué tarea o requisito se relaciona.
  - Una falla en las comprobaciones **bloquea** la integración.
  - Los cambios que afecten a varios componentes se revisan en conjunto antes de integrarse.
  - Las versiones aceptadas se identifican con etiquetas y notas de versión.

### CI

Se planea GitHub Actions con comprobaciones por componente. Para el backend se prevé validar compilación y pruebas, aumentando la complejidad gradualmente. Los workflows aún no están definidos (**[PENDIENTE]**).

## 9. Entregables y evidencias del laboratorio

Los entregables van en `backend/vestigia/docs/`:

```
docs/
  README.md                       cómo levantar todo
  01-arquitectura-y-contrato.md
  02-provisioning/                00_create_database.sql, 01_schema.sql, 02_seed.sql, 99_clean.sql, README.md
  03-evidencias-db/               consultas antes/después (capturas las agrega Raúl)
  04-logs-backend/                qué logs guardar (los agrega Raúl)
  05-evidencias-pwa/              a cargo del compañero de PWA
  06-casos-de-prueba.md
  pruebas-api.md                  comandos curl
```

Los scripts de `02-provisioning/` son idempotentes salvo `00_create_database.sql`. Para volver al estado inicial: `99_clean.sql` y luego `02_seed.sql`.

(Si el equipo decide mover `docs/` a la raíz del monorepo, actualizar las rutas de este archivo y de los README.)

**Backend / base de datos:**
- **Scripts de provisioning** (`.sql`): esquema, datos semilla y limpieza para dejar el laboratorio en un estado inicial reproducible.
- **Capturas o logs de queries** desde pgAdmin o consola, **antes y después** del envío de datos, mostrando estructura e integridad.
- **Logs del servidor Spring Boot** durante el `POST` y los `GET`, con consumo de memoria, tiempos de procesamiento y códigos de respuesta.

El backend debe permitir capturar la evidencia de la PWA (listado tras el `GET`, y en DevTools: Network, Application con Service Worker y Cache/IndexedDB, y Console).

La documentación técnica del repo va en **Markdown**. Los entregables académicos usan el formato Word estándar del equipo (portada UTCH, índice, encabezados numerados) y se hacen aparte.

## 10. Pruebas

Plan de pruebas del proyecto (documento aparte): integración, conectividad y tiempo de respuesta, además de unitarias, funcionales, usabilidad, compatibilidad, regresión y humo.

- `./mvnw test` debe pasar **sin PostgreSQL**: pruebas unitarias de `SalaService` con Mockito y del controller con `@WebMvcTest` + `MockMvc`. No agregar pruebas `@SpringBootTest` que requieran la BD.
- Verificar siempre que el POST repetido con el mismo `id` no duplica ni modifica registros.
- Pruebas manuales contra la BD real: `docs/pruebas-api.md` y `docs/06-casos-de-prueba.md`.

## 11. Pendientes por definir

- Esquema completo de la base de datos.
- Significado exacto de "sala" y sus campos finales.
- Convención de nombres de ramas.
- Si Firebase tendrá algún papel más adelante (hoy fuera de alcance).
- Workflows de GitHub Actions para el backend.