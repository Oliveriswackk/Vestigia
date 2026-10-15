-- Ejecutar en la base vestigia, antes y despues del POST.
SELECT current_database() AS base, version() AS motor;

SELECT column_name, data_type, character_maximum_length, is_nullable, column_default
FROM information_schema.columns
WHERE table_schema = 'public' AND table_name = 'salas'
ORDER BY ordinal_position;

SELECT conname, pg_get_constraintdef(oid) AS definicion
FROM pg_constraint
WHERE conrelid = 'public.salas'::regclass
ORDER BY conname;

SELECT COUNT(*) AS total FROM public.salas;

SELECT id, nombre, descripcion, escenario, activa, creada_en
FROM public.salas
ORDER BY creada_en, id;

-- Antes del alta: 0; despues de alta y reenvio: 1.
SELECT COUNT(*) AS filas_uuid_laboratorio
FROM public.salas
WHERE id = '7d4f2c10-3b6a-4e8e-9b1a-2f5c6d7e8f90';

SELECT *
FROM public.salas
WHERE id = '7d4f2c10-3b6a-4e8e-9b1a-2f5c6d7e8f90';

-- Debe devolver cero filas. El UUID estable comprueba el reenvio,
-- mientras que la PK por si sola impide IDs fisicamente repetidos.
SELECT id, COUNT(*) AS repeticiones
FROM public.salas
GROUP BY id
HAVING COUNT(*) > 1;

SELECT COUNT(*) AS filas_invalidas
FROM public.salas
WHERE nombre IS NULL
   OR btrim(nombre) = ''
   OR escenario IS NULL
   OR escenario NOT IN ('pueblo', 'faro', 'observatorio')
   OR activa IS NULL
   OR creada_en IS NULL;

