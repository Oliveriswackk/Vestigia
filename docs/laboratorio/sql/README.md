# Scripts del laboratorio

Ejecutar los scripts en la base indicada, siguiendo el README principal.

| Script | Base | Función |
|---|---|---|
| `00_create_database.sql` | `postgres` | Crear `vestigia` si no existe; omitir si Docker ya la creó |
| `01_schema.sql` | `vestigia` | Crear la tabla `salas` con sus restricciones |
| `02_seed.sql` | `vestigia` | Cargar seis salas con UUID fijos |
| `03_verificacion.sql` | `vestigia` | Consultar estructura, restricciones y datos antes/después |
| `04_error_integridad.sql` | `vestigia` | Prueba manual del CHECK; ejecutar ROLLBACK por separado después del error |
| `99_clean.sql` | `vestigia` | Eliminar todas las filas para repetir el laboratorio |

Los cuatro scripts originales de creación, esquema, semilla y limpieza se copiaron sin cambios desde [el backend, commit dc8b26e](https://github.com/Oliveriswackk/Vestigia/tree/dc8b26ef3460bb70a4ff66ffc1eae4caa0e532d1/backend/vestigia/docs/02-provisioning).

`99_clean.sql` vacía todas las salas. Ejecutarlo únicamente en la base dedicada a las pruebas y después de guardar las evidencias. Para restablecer el estado inicial, limpiar y volver a cargar `02_seed.sql`.

`03_verificacion.sql` trae el UUID fijo del ejemplo de la API. Para verificar una sala creada desde la interfaz, sustituirlo por el UUID real capturado en el POST o en IndexedDB.

`CREATE TABLE IF NOT EXISTS` no migra una tabla que ya tenga una estructura diferente. Comparar columnas y restricciones si Hibernate informa errores de validación.

El código `23514` corresponde a una violación CHECK en [PostgreSQL 16](https://www.postgresql.org/docs/16/errcodes-appendix.html). La prueba SQL directa no pasa por Spring Boot; el filtro actual registra estados HTTP, no SQLSTATE por petición.

## Referencias para las capturas

- [Chrome DevTools: depurar PWA](https://developer.chrome.com/docs/devtools/progressive-web-apps/).
- [Chrome DevTools: panel Network](https://developer.chrome.com/docs/devtools/network/reference/).

