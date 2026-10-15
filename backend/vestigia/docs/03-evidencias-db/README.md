# Evidencias de la base de datos

Capturas de pgAdmin (o salida de `psql`) **antes y después** del envío de datos
desde la PWA o con curl. Nombra los archivos con el número de paso, p. ej.
`01-estructura.png`, `02-antes.png`, `03-despues.png`.

## 1. Estructura e integridad

```sql
-- Columnas y tipos
SELECT column_name, data_type, character_maximum_length, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'salas'
ORDER BY ordinal_position;

-- Restricciones (PK y CHECK de escenario)
SELECT conname, pg_get_constraintdef(oid)
FROM pg_constraint
WHERE conrelid = 'salas'::regclass;
```

En `psql` también sirve `\d salas`.

## 2. Antes del envío

```sql
SELECT COUNT(*) AS total FROM salas;
SELECT id, nombre, escenario, activa, creada_en FROM salas ORDER BY creada_en;
```

## 3. Después del envío (POST)

Repite las consultas del paso 2 y además:

```sql
-- La sala enviada existe (sustituye el id)
SELECT * FROM salas WHERE id = '00000000-0000-0000-0000-000000000000';

-- Idempotencia: no hay ids duplicados (debe devolver 0 filas)
SELECT id, COUNT(*) FROM salas GROUP BY id HAVING COUNT(*) > 1;

-- Conteo por escenario
SELECT escenario, COUNT(*) FROM salas GROUP BY escenario ORDER BY escenario;
```

## 4. Integridad (opcional)

Debe fallar por el `CHECK`, demostrando que la BD protege los datos:

```sql
INSERT INTO salas (id, nombre, escenario) VALUES (gen_random_uuid(), 'Prueba', 'luna');
```
