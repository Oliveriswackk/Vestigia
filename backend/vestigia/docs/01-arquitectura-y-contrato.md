# Arquitectura y contrato de la API

## Arquitectura

Servicio base (equivalente al ejemplo `customer` de clase). No hay servicio compuesto.

```
PWA (React + Vite, :5173) ──HTTP/JSON──> Spring Boot (:8080) ──JPA──> PostgreSQL (salas)
```

```
com.vestigia
├── controller/   SalaController, ApiExceptionHandler
├── dto/          Sala            (record expuesto en JSON)
├── model/        SalaModel       (entidad JPA, tabla salas)
├── repository/   SalaRepository  (Spring Data JPA)
├── service/      SalaService     (lógica; convierte SalaModel <-> Sala)
├── config/       CorsConfig, RequestLoggingFilter
└── VestigiaApplication.java
```

- El controller solo habla con el service; el service es el único que usa el repository.
- El esquema lo crean los scripts de `02-provisioning/`; Hibernate solo lo valida (`ddl-auto=validate`).

## Modelo

```sql
CREATE TABLE salas (
  id          UUID PRIMARY KEY,
  nombre      VARCHAR(100) NOT NULL,
  descripcion TEXT,
  escenario   VARCHAR(20)  NOT NULL CHECK (escenario IN ('pueblo','faro','observatorio')),
  activa      BOOLEAN      NOT NULL DEFAULT TRUE,
  creada_en   TIMESTAMP    NOT NULL DEFAULT NOW()
);
```

> Esquema **provisional**, pendiente de confirmar por el equipo.

## JSON (snake_case)

```json
{
  "id": "c1b7a1de-5f0e-4a58-9a3e-0b5a7f6d1a01",
  "nombre": "Puesto del pescador",
  "descripcion": "Donde suena la radio por primera vez.",
  "escenario": "pueblo",
  "activa": true,
  "creada_en": "2026-10-14T10:00:00"
}
```

| Campo | Tipo | Reglas |
|---|---|---|
| `id` | UUID | Lo genera la PWA. Si falta en el POST, lo genera el servidor |
| `nombre` | string | Obligatorio, no vacío, máx. 100 |
| `descripcion` | string | Opcional |
| `escenario` | string | Obligatorio: `pueblo`, `faro` u `observatorio` |
| `activa` | boolean | Opcional; si es `null` se guarda `true` |
| `creada_en` | fecha-hora ISO | Solo lectura: lo asigna el servidor; si el cliente lo manda, se ignora |

## Endpoints

| Método | Ruta | Éxito | Errores |
|---|---|---|---|
| GET | `/api/salas` | 200, arreglo ordenado por `creada_en` ascendente | — |
| GET | `/api/salas/{id}` | 200 | 404, 400 (id no es UUID) |
| POST | `/api/salas` | 201 (creada, con `Location`) / 200 (ya existía) | 400 |
| PUT | `/api/salas/{id}` | 200 | 400, 404 |
| DELETE | `/api/salas/{id}` | 204 sin cuerpo | 404 |

- **PUT** reemplaza `nombre`, `descripcion`, `escenario` y `activa`. El `id` de la ruta
  manda sobre el del cuerpo; `creada_en` no cambia.

## Regla de idempotencia del POST (modo offline)

La PWA genera el `id` y puede reenviar la misma petición varias veces desde su cola
offline (Service Worker / IndexedDB) cuando recupera la red.

| Situación | Resultado |
|---|---|
| `id` no existe | Inserta. **201 Created**, cabecera `Location: /api/salas/{id}`, cuerpo = sala creada |
| `id` ya existe | **No modifica nada ni duplica.** **200 OK** con la sala tal como está en la BD |
| `id` ausente | El servidor genera un UUID. 201 |

Si dos reenvíos con el mismo `id` llegan al mismo tiempo, la llave primaria impide el
duplicado y el segundo recibe 200 con la sala existente.

Para la PWA: **201 y 200 son ambos éxito**; en los dos casos puede sacar la petición de su cola.
Para modificar una sala existente se usa `PUT`, no `POST`.

## Formato de errores

```json
{
  "timestamp": "2026-10-14T10:00:00.123",
  "status": 400,
  "error": "Bad Request",
  "message": "Datos inválidos",
  "path": "/api/salas",
  "details": [ { "field": "nombre", "message": "no debe estar vacío" } ]
}
```

| Código | Cuándo | `message` |
|---|---|---|
| 400 | Falla de validación (`details` lista los campos) | `Datos inválidos` |
| 400 | JSON malformado | `JSON malformado o ilegible` |
| 400 | `{id}` no es un UUID | `Valor inválido para 'id'` |
| 404 | La sala no existe | `No existe la sala {id}` |
| 405 / 415 | Método o tipo de contenido no soportado | Mensaje de Spring |
| 500 | Error inesperado (no expone stacktrace; se registra en el log) | `Error interno del servidor` |

`details` siempre está presente (vacío salvo en errores de validación).

## CORS

Orígenes `http://localhost:5173` y `http://localhost:4173` (propiedad
`app.cors.allowed-origins`), métodos GET, POST, PUT, PATCH, DELETE, OPTIONS, todas
las cabeceras. Se expone `Location` para que la PWA pueda leerla.
