# Logs del backend

Guarda aquí fragmentos del log del servidor Spring Boot. El archivo completo se
genera en `backend/vestigia/logs/vestigia-backend.log` (no se sube al repo; copia
aquí solo lo necesario, p. ej. `get-salas.log`, `post-salas.log`).

## Qué guardar

| Archivo sugerido | Acción | Qué debe verse |
|---|---|---|
| `arranque.log` | `./mvnw spring-boot:run` | Conexión a PostgreSQL (HikariPool), validación del esquema, `Started VestigiaApplication` |
| `get-salas.log` | `GET /api/salas` | `select ... from salas ... order by creada_en` y `[API] GET /api/salas -> 200 \| N ms \| heap X/Y MB` |
| `post-salas.log` | `POST /api/salas` dos veces con el mismo id | 1.º: `select` + `insert` y `-> 201`; 2.º: solo `select` y `-> 200` |
| `post-400.log` | POST inválido | `[API] POST /api/salas -> 400` sin `insert` |
| `get-404.log` | GET de id inexistente | `-> 404` |
| `actuator.json` | Métricas | Respuestas de los endpoints de abajo |

## Métricas con Actuator

```bash
curl -s http://localhost:8080/actuator/health
curl -s http://localhost:8080/actuator/metrics/jvm.memory.used
curl -s http://localhost:8080/actuator/metrics/jvm.memory.max
curl -s "http://localhost:8080/actuator/metrics/http.server.requests?tag=uri:/api/salas"
```

`http.server.requests` da conteo, tiempo total y máximo por URI, método y status.

## Formato de la línea de petición

```
[API] <método> <ruta> -> <status> | <duración> ms | heap <usada>/<máxima> MB
```

Para extraer solo esas líneas:

```bash
grep "\[API\]" logs/vestigia-backend.log
```
