# Capturas y logs por agregar

Esta carpeta todavía no contiene resultados de ejecución. Tomar las capturas siguiendo el README principal y usar los nombres que se indican a continuación. No cambiar los estados «Pendiente» antes de ejecutar.

| Archivo | Cómo obtener la captura |
|---|---|
| `01-db-inicial.png` | pgAdmin o psql con COUNT y listado de las seis salas semilla. Conservar también la salida de aplicación de los scripts. |
| `02-backend-iniciado.png` | Terminal después de `Started VestigiaApplication`; incluir conexión Hikari si aparece. |
| `03-listado.png` | Abrir `http://localhost:4173/salas` online y mostrar las tarjetas cargadas desde la API. |
| `04-network-get.png` | F12 → Network, filtro `salas`, seleccionar GET real a 8080; mostrar URL, status, Headers y columna Size. Comprobar el log backend si hay respuesta de SW/caché. |
| `05-network-timing.png` | En esa petición, abrir Timing. Que se vean duración y espera del servidor. |
| `06-post-payload.png` | Crear «Sala laboratorio online» y seleccionar POST → Payload. Mostrar id, nombre, descripción, escenario y activa. |
| `07-post-respuesta.png` | POST → Headers con HTTP 201 y Location. Agregar otra captura de Response si no cabe el JSON. |
| `08-db-despues.png` | Consulta en PostgreSQL que muestre la fila del mismo UUID y el nuevo conteo. |
| `09-logs-backend.png` | Log con SQL y líneas `[API]` de GET/POST, estados, ms y heap. Guardar el bloque también como `backend-get-post.log`. |
| `10-service-worker.png` | Application → Service Workers. Mostrar el worker activado y la página controlada, después del build y preview. |
| `11-cache-storage.png` | Application → Cache Storage. Mostrar precache y `api-salas` con su GET. |
| `12-offline-pendiente.png` | Network → Offline; recargar y crear «Sala laboratorio offline». Mostrar aviso y tarjeta pendiente. |
| `13-indexeddb.png` | Application → IndexedDB → `vestigia` → `salasPendientes`. Actualizar la vista y mostrar UUID y payload. |
| `14-reconexion.png` | Restaurar red con la app abierta; capturar POST de sincronización. Añadir captura de la cola vacía si no cabe en la misma imagen. |
| `15-db-sincronizada.png` | SELECT por el UUID offline y COUNT=1. Comparar con la consulta anterior a reconectar, que debe ser 0. |
| `16-console.png` | Console con nivel Info habilitado; mensajes `[Vestigia]` de GET, POST, pendiente y sincronización. |

La interfaz del navegador puede variar según idioma y versión. Usar varias imágenes cuando un solo panel no muestre todos los datos requeridos.

## Evidencias de integridad y reinicio

Guardar además:

- `17-integridad.png`: columnas/restricciones y mensaje SQLSTATE de la prueba CHECK.
- `18-reinicio.png`: consulta de la sala después de reiniciar Spring Boot.

## Cómo insertar una captura en el README

Después de guardar la imagen, agregar bajo el apartado que corresponda:

~~~markdown
![Service Worker activo](docs/laboratorio/evidencias/10-service-worker.png)
~~~

Para enlazar los logs:

~~~markdown
[Log de GET y POST](docs/laboratorio/evidencias/backend-get-post.log)
~~~

Conservar fecha y UUID para relacionar solicitud, respuesta, SQL y captura. Antes de subir los logs, revisar el fragmento y retirar credenciales si aparecieran.

