[Uploading README.md…]()
# Vestigia

Repositorio principal del proyecto integrador **Vestigia**.

Vestigia es un MVP de videojuego de aventura y acertijos con enfoque en perspectiva, acompañado de una PWA y una aplicación móvil complementaria. Los componentes del proyecto se desarrollan de manera independiente y se integran mediante servicios compartidos y sincronización de información.

## Estructura del proyecto

La estructura raíz establece una organización común para los diferentes componentes del proyecto:

```text
vestigia/
│
├── assets/
├── game/
├── web/
├── mobile/
├── backend/
├── docs/
├── .gitignore
└── README.md
```

### `assets/`

Recursos generales y compartidos del proyecto que pueden ser utilizados por más de un componente, como personajes, referencias visuales, elementos gráficos, audio u otros recursos comunes.

### `game/`

Desarrollo del videojuego principal realizado en Unity y C#.

### `web/`

Desarrollo de la PWA que complementa la experiencia del videojuego.

### `mobile/`

Desarrollo de la aplicación móvil complementaria al videojuego.

### `backend/`

Servicios, configuraciones y componentes relacionados con la comunicación, almacenamiento y sincronización de información entre los diferentes componentes del proyecto.

### `docs/`

Documentación relacionada con la planeación, diseño, arquitectura, integración, pruebas y desarrollo del proyecto.

---

## Organización del trabajo

La estructura de las carpetas principales se mantiene como la **columna vertebral del repositorio**.

Cada área puede organizar internamente sus archivos, código y recursos de acuerdo con las necesidades de su componente, procurando mantener su trabajo dentro de la carpeta correspondiente.

La estructura interna de cada componente puede modificarse conforme avance el desarrollo y se definan las tecnologías, herramientas y responsabilidades del equipo.

La estructura raíz no se considera inmutable; cualquier cambio necesario deberá responder a las necesidades reales del proyecto y mantenerse consistente con la organización general.

## Desarrollo

El proyecto se desarrolla mediante iteraciones cortas, permitiendo ajustar funcionalidades, herramientas y responsabilidades conforme avance la implementación.

Los diferentes componentes pueden desarrollarse de forma paralela y posteriormente integrarse mediante los servicios compartidos definidos para el proyecto.

## Tecnologías

Las tecnologías contempladas inicialmente son:

* **Videojuego:** Unity + C#
* **PWA:** React + Vite
* **Aplicación móvil:** Flutter o React Native
* **Servicios compartidos:** Firebase
* **Control de versiones:** Git + GitHub
* **Diseño:** Figma

Las tecnologías marcadas como preliminares podrán modificarse durante el desarrollo de acuerdo con las necesidades del proyecto.

## Estado

**En desarrollo — MVP**

La estructura y documentación del repositorio se encuentran en construcción y podrán evolucionar junto con el proyecto.

---

## Laboratorio de pruebas PWA

Este laboratorio verifica la consulta y renderización de salas, el envío de nuevas salas hacia Spring Boot, su persistencia en PostgreSQL y el comportamiento offline de la aplicación.

La documentación y evidencias se guardan en este repositorio, dentro de `docs/laboratorio/`. La PWA utilizada para las pruebas se descarga de su repositorio separado; el backend se ejecuta desde `backend/vestigia/` de este monorepo.

| Carpeta | Contenido |
|---|---|
| [docs/laboratorio/evidencias](docs/laboratorio/evidencias/) | Capturas de interfaz, DevTools, consultas y logs |
| [docs/laboratorio/sql](docs/laboratorio/sql/) | Scripts de carga, limpieza y verificación |
| [backend/vestigia/docs](backend/vestigia/docs/) | Contrato y documentación del backend |

### Repositorios y versión de prueba

- Frontend: [AiltonMV/vestigia-pwa, rama develop](https://github.com/AiltonMV/vestigia-pwa/tree/develop).
- Backend: [Oliveriswackk/Vestigia, backend/vestigia](https://github.com/Oliveriswackk/Vestigia/tree/main/backend/vestigia).
- Frontend revisado: `614895281f573ad35b64b58bffb78849c6def922`.
- Backend revisado: `dc8b26ef3460bb70a4ff66ffc1eae4caa0e532d1`.

Registrar al ejecutar los commits reales de ambos repositorios. El laboratorio de salas está en `develop`; no usar el mock como evidencia de persistencia en PostgreSQL.

### Requisitos

- Git.
- Node.js compatible con el lockfile. El Vite revisado requiere `^20.19.0 || >=22.12.0`.
- JDK 21 para el backend.
- PostgreSQL local con pgAdmin o Docker Desktop con un contenedor PostgreSQL.
- Puertos 5432, 8080 y 4173 disponibles.

Verificar las herramientas:

~~~powershell
git --version
node --version
npm --version
java -version
~~~

### 1. Descargar los proyectos

Ejecutar desde una carpeta de trabajo:

~~~powershell
git clone https://github.com/Oliveriswackk/Vestigia.git
git clone --branch develop https://github.com/AiltonMV/vestigia-pwa.git
git -C .\Vestigia rev-parse HEAD
git -C .\vestigia-pwa rev-parse HEAD
~~~

Si ya están clonados, usar esas carpetas. En el frontend, guardar primero cualquier trabajo local y seleccionar la rama existente con `git switch develop`; si solo está en remoto, usar `git switch --track origin/develop` después de `git fetch origin`.

### 2. Preparar la base de datos

Los scripts originales están en:

[backend/vestigia/docs/02-provisioning](https://github.com/Oliveriswackk/Vestigia/tree/main/backend/vestigia/docs/02-provisioning).

También se incluye una copia en [docs/laboratorio/sql](docs/laboratorio/sql/README.md).

**Con PostgreSQL y pgAdmin:**

1. Abrir Query Tool sobre la base `postgres` y ejecutar `00_create_database.sql` una sola vez, si `vestigia` no existe.
2. Abrir Query Tool sobre `vestigia` y ejecutar `01_schema.sql` y `02_seed.sql`.
3. Ejecutar:

~~~sql
SELECT COUNT(*) AS total FROM public.salas;
SELECT id, nombre, escenario, activa, creada_en
FROM public.salas ORDER BY creada_en, id;
~~~

En una base nueva se esperan seis salas semilla. Guardar la captura del estado inicial.

**Alternativa con Docker Desktop:**

~~~powershell
docker run --name vestigia-db -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=vestigia -p 5432:5432 -v vestigia-lab-data:/var/lib/postgresql/data -d postgres:16
docker exec vestigia-db pg_isready -U postgres -d vestigia
~~~

Esperar `accepting connections`. Si el contenedor ya existe, iniciar con `docker start vestigia-db`. No crear otro servidor en el mismo puerto.

Desde la carpeta que contiene ambos repositorios:

~~~powershell
Set-Location .\Vestigia\backend\vestigia
docker cp .\docs\02-provisioning\01_schema.sql vestigia-db:/tmp/01_schema.sql
docker cp .\docs\02-provisioning\02_seed.sql vestigia-db:/tmp/02_seed.sql
docker exec vestigia-db psql -v ON_ERROR_STOP=1 -U postgres -d vestigia -f /tmp/01_schema.sql
docker exec vestigia-db psql -v ON_ERROR_STOP=1 -U postgres -d vestigia -f /tmp/02_seed.sql
docker exec vestigia-db psql -U postgres -d vestigia -c "SELECT COUNT(*) AS total FROM salas;"
~~~

`POSTGRES_DB` crea la base al inicializar un volumen nuevo; por eso esta opción no requiere `00_create_database.sql`. El volumen conserva los datos entre reinicios.

### 3. Ejecutar Spring Boot

Abrir una terminal en `Vestigia/backend/vestigia`:

~~~powershell
Copy-Item .\application-local.properties.example .\application-local.properties
notepad .\application-local.properties
~~~

Sustituir `tu-contraseña` por la contraseña del usuario PostgreSQL. Para el contenedor anterior es `postgres`. Guardar y ejecutar:

~~~powershell
.\mvnw.cmd spring-boot:run
~~~

La primera ejecución requiere descargar Maven y sus dependencias. Mantener esta terminal abierta y guardar la evidencia de `Started VestigiaApplication`.

El backend valida el esquema con Hibernate; las tablas deben crearse antes del arranque. El archivo local de configuración se busca desde el directorio de trabajo, por lo que debe arrancarse desde `backend/vestigia`. Si están definidas, revisar las variables `DB_URL`, `DB_USER` y `DB_PASSWORD`.

En otra terminal:

~~~powershell
Invoke-RestMethod http://localhost:8080/actuator/health
Invoke-RestMethod http://localhost:8080/api/salas | Format-Table id,nombre,escenario,activa
~~~

Se espera `UP` y el listado inicial.

### 4. Configurar y ejecutar la PWA con el backend real

Abrir otra terminal en `vestigia-pwa`, rama `develop`:

~~~powershell
Copy-Item .\.env.example .\.env
notepad .\.env
~~~

Confirmar estos valores, conservando las otras variables que el equipo necesite:

~~~dotenv
VITE_API_URL=http://localhost:8080
VITE_API_MOCK=false
~~~

Instalar y compilar:

~~~powershell
npm ci
npm run build
~~~

Solo cuando el build termine correctamente, iniciar:

~~~powershell
npm run preview -- --port 4173 --strictPort
~~~

Abrir [http://localhost:4173/salas](http://localhost:4173/salas).

Usar preview para las evidencias de Service Worker y funcionamiento offline. Para desarrollo se puede usar `npm run dev -- --strictPort` y abrir `http://localhost:5173/salas`, pero la configuración revisada no activa el Service Worker en desarrollo.

Si cambia `.env`, volver a compilar y reiniciar preview. Las variables VITE se incorporan al build.

### 5. Probar consulta y persistencia

1. Abrir DevTools con F12 y activar `Preserve log` en Network y Console.
2. Cargar `/salas` y comprobar el GET y las seis salas de PostgreSQL.
3. Registrar el conteo inicial en la base.
4. Crear desde el formulario una sala llamada `Sala laboratorio online`, con escenario `faro` y descripción `Prueba de persistencia`.
5. En Network guardar Payload, respuesta y cabeceras del POST. Para un UUID nuevo se espera HTTP 201 y `Location`.
6. Copiar el UUID que devuelve la API y comprobarlo en PostgreSQL:

~~~sql
SELECT id, nombre, descripcion, escenario, activa, creada_en
FROM public.salas
WHERE nombre = 'Sala laboratorio online'
ORDER BY creada_en DESC;
SELECT COUNT(*) AS total FROM public.salas;
~~~

Si se partió de seis salas y nadie agregó otras, el total esperado es siete. Comparar además el UUID, nombre, escenario y estado del POST con la fila almacenada.

7. Revisar el log `Vestigia/backend/vestigia/logs/vestigia-backend.log`:

~~~powershell
# Ejecutar desde Vestigia/backend/vestigia
Get-Content .\logs\vestigia-backend.log -Tail 120
~~~

Guardar el bloque real que muestre consultas SQL, parámetros y líneas `[API]` del GET y POST. La línea de API contiene método, ruta, estado HTTP, duración y memoria heap de la JVM. No mide memoria de PostgreSQL ni tiempo exclusivo de la consulta SQL.

8. Recargar la interfaz y reiniciar el backend para confirmar que la sala sigue almacenada. Guardar capturas y registrar el resultado.

### 6. Probar offline y reconexión

1. En `http://localhost:4173/salas`, con conexión, abrir Application → Service Workers. Esperar a que el worker esté activo y controle la página; recargar online si es necesario.
2. Comprobar Application → Cache Storage: recursos precacheados y caché `api-salas` después de consultar el listado.
3. En Network seleccionar `Offline`. Recargar `/salas` y comprobar la apertura y el listado previamente almacenado.
4. Crear `Sala laboratorio offline` con escenario `observatorio`. Debe mostrarse como pendiente.
5. Abrir Application → IndexedDB → `vestigia` → `salasPendientes` y actualizar la vista. Guardar la entrada con su UUID.
6. En pgAdmin o consola PostgreSQL, comprobar que ese UUID aún no existe en la base. La simulación Offline del navegador no impide consultar PostgreSQL por separado.
7. Volver Network a `No throttling`, con la página abierta. Conservar en Network el POST de sincronización.
8. Confirmar que la cola queda vacía y la interfaz informa la sincronización. Si el primer envío llega por primera vez al backend se espera 201; si el mismo UUID ya existía, 200.
9. Consultar PostgreSQL con el UUID capturado:

~~~sql
-- Sustituir el UUID por el de IndexedDB/Payload.
SELECT * FROM public.salas WHERE id = 'UUID-DE-LA-SALA';
SELECT COUNT(*) AS filas
FROM public.salas WHERE id = 'UUID-DE-LA-SALA';
~~~

Se espera una sola fila. Si se prueba el reenvío manual, repetir el mismo UUID y payload: el backend debe responder 200 sin crear otra sala. Un nuevo alta desde el formulario genera otro UUID y representa otra sala.

En esta versión la cola se sincroniza desde la aplicación al volver la red o al abrir `/salas` con conexión. No se documenta como Background Sync con la app cerrada. Solo las altas se encolan: editar y borrar salas confirmadas requieren conexión.

Como los GET usan `stale-while-revalidate`, una respuesta puede venir de la caché. Para demostrar la consulta real, capturar también la petición que llega al backend y su log. Si hace falta renovar el estado visible, recargar online después de actualizar la caché.

### 7. Capturas y archivos de evidencia

Guardar los archivos en `docs/laboratorio/evidencias`. El [checklist de capturas](docs/laboratorio/evidencias/README.md) indica cómo obtener cada uno.

| Archivo | Qué demuestra |
|---|---|
| `01-db-inicial.png` | Scripts aplicados y seis salas iniciales |
| `02-backend-iniciado.png` | Spring Boot iniciado y conexión disponible |
| `03-listado.png` | Datos de GET renderizados en la interfaz |
| `04-network-get.png` | URL real, HTTP, headers y tamaño de respuesta |
| `05-network-timing.png` | Tiempo de la petición GET |
| `06-post-payload.png` | Datos y UUID enviados desde el formulario |
| `07-post-respuesta.png` | HTTP 201, Location y JSON de la sala |
| `08-db-despues.png` | Fila persistida con el mismo UUID |
| `09-logs-backend.png` y `backend-get-post.log` | SQL, parámetros, HTTP, duración y heap |
| `10-service-worker.png` | Worker activo que controla la aplicación |
| `11-cache-storage.png` | Cachés de recursos y de salas |
| `12-offline-pendiente.png` | Interfaz sin conexión con alta pendiente |
| `13-indexeddb.png` | UUID y payload conservados en la cola local |
| `14-reconexion.png` | POST de sincronización y cola vacía |
| `15-db-sincronizada.png` | Una fila en PostgreSQL con el UUID offline |
| `16-console.png` | Mensajes de consulta, creación y sincronización |

Agregar imágenes al README con rutas relativas; ejemplo:

~~~markdown
### Listado obtenido desde la API
![Listado de salas](docs/laboratorio/evidencias/03-listado.png)

La interfaz presenta las salas devueltas por GET /api/salas.

### Persistencia después del POST
![Registro almacenado](docs/laboratorio/evidencias/08-db-despues.png)

La fila conserva el mismo UUID y los mismos datos de la solicitud.
~~~

Agregar las demás imágenes con un título y una explicación breve de lo que se observa realmente.

### 8. Integridad y resultados

Los [scripts de verificación](docs/laboratorio/sql/README.md) permiten consultar las columnas, las restricciones y los datos antes/después. `id` es UUID y clave primaria; `escenario` tiene un CHECK. La API valida nombre no vacío y escenario permitido.

El estado HTTP del backend y el SQLSTATE de PostgreSQL son datos distintos. El filtro actual muestra HTTP; para acreditar el código del motor, conservar el mensaje real del cliente PostgreSQL ante la prueba CHECK de `04_error_integridad.sql`. Se espera SQLSTATE `23514`. Ese rechazo directo de SQL no pasa por Spring Boot.

El backend revisado no registra SQLSTATE para cada petición ni el payload JSON completo. El payload se acredita en Network y se relaciona con los parámetros SQL por UUID. Si la revisión exige SQLSTATE expresamente dentro de los logs de Spring Boot, ese dato requerirá instrumentación adicional; las líneas HTTP no lo sustituyen.

Las métricas adicionales están disponibles en:

- `http://localhost:8080/actuator/metrics/jvm.memory.used`
- `http://localhost:8080/actuator/metrics/http.server.requests`

Completar esta tabla después de ejecutar, con valores reales:

| Prueba | Resultado esperado | Obtenido | Evidencia |
|---|---|---|---|
| GET y renderización | Mismos datos en PostgreSQL, JSON e interfaz | Pendiente | |
| POST y persistencia | 201; nueva fila con el UUID enviado | Pendiente | |
| Reenvío del mismo POST | 200; una fila y datos conservados | Pendiente | |
| Integridad | Rechazo de escenario inválido; base sin fila inválida | Pendiente | |
| Reinicio | La sala permanece almacenada | Pendiente | |
| Offline | App abre y alta permanece en IndexedDB | Pendiente | |
| Reconexión | Alta enviada, cola vacía y una fila en PostgreSQL | Pendiente | |

Fecha, responsable, versiones y commits de la ejecución: **por completar**.

**Estado de las evidencias:** los resultados siguen pendientes hasta ejecutar las pruebas y añadir capturas y logs reales. La tabla anterior indica resultados esperados, no pruebas aprobadas.

### 9. Repetir el laboratorio

Guardar primero las evidencias. En la base dedicada al laboratorio, `99_clean.sql` vacía toda la tabla `salas`; después, `02_seed.sql` restaura las seis salas iniciales.

Para reiniciar también el estado del navegador, eliminar la cola y cachés de este sitio únicamente después de verificar que ya no quedan altas que se necesite conservar.

