# Casos de prueba — Laboratorio PWA + backend

Comandos curl en [pruebas-api.md](pruebas-api.md). Consultas SQL en
[03-evidencias-db/README.md](03-evidencias-db/README.md). Estado inicial: ejecutar
`99_clean.sql` y `02_seed.sql` (6 salas).

| ID | Descripción | Pasos | Resultado esperado | Resultado obtenido | Evidencia |
|---|---|---|---|---|---|
| CP-01 | Consulta de salas (GET) | `GET /api/salas` | 200; arreglo de 6 salas ordenadas por `creada_en`; campos en snake_case | | |
| CP-02 | Consulta de una sala | `GET /api/salas/c1b7a1de-5f0e-4a58-9a3e-0b5a7f6d1a01` | 200; "Puesto del pescador" | | |
| CP-03 | Sala inexistente | `GET /api/salas/{uuid que no existe}` | 404 con cuerpo de error JSON | | |
| CP-04 | Inserción (POST) | `POST /api/salas` con un `id` nuevo y datos válidos | 201; cabecera `Location`; cuerpo con `creada_en` asignado por el servidor | | |
| CP-05 | Idempotencia del POST | Repetir exactamente el POST de CP-04 | 200; mismo cuerpo; no hay nueva fila | | |
| CP-06 | Idempotencia no sobrescribe | POST con el `id` de CP-04 pero otro `nombre` | 200; la sala conserva el nombre original | | |
| CP-07 | POST sin id | `POST /api/salas` sin `id` | 201; el servidor genera el UUID | | |
| CP-08 | Validación de campos | POST con `nombre` vacío y `escenario` = "luna" | 400; `details` con `nombre` y `escenario`; no se inserta nada | | |
| CP-09 | JSON malformado | POST con cuerpo `{nombre:` | 400; `message` = "JSON malformado o ilegible" | | |
| CP-10 | Persistencia en la BD | Tras CP-04/CP-05 ejecutar las consultas "después" | La sala existe una sola vez; conteo = inicial + 1; 0 ids duplicados | | |
| CP-11 | Persistencia tras reinicio | Detener y volver a arrancar el backend; `GET /api/salas` | La sala de CP-04 sigue en el listado | | |
| CP-12 | Actualización (PUT) | `PUT /api/salas/{id}` con `activa: false` | 200; `activa` = false; `creada_en` sin cambio | | |
| CP-13 | Eliminación (DELETE) | `DELETE /api/salas/{id}` dos veces | 204 la primera; 404 la segunda | | |
| CP-14 | Offline: alta sin red | En la PWA, desactivar la red (DevTools → Network → Offline) y crear una sala | La PWA la guarda en su cola (IndexedDB) y lo informa; el backend no recibe nada | | |
| CP-15 | Offline: reenvío sin duplicados | Restaurar la red; la PWA reenvía la cola (forzar 2 reenvíos si es posible) | 1.er envío 201, siguientes 200; en la BD la sala aparece una sola vez | | |
| CP-16 | CORS desde la PWA | Abrir la PWA en `http://localhost:5173` y cargar el listado | Sin errores de CORS en la consola; respuesta 200 | | |
| CP-17 | Logs y métricas | Revisar log tras CP-01, CP-04 y CP-08; `GET /actuator/metrics/jvm.memory.used` | Línea `[API]` con status, ms y heap por petición; métrica de memoria disponible | | |
| CP-18 | Salud | `GET /actuator/health` | 200; `status` = "UP" (incluye la BD) | | |
