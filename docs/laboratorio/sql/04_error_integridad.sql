-- Prueba manual directa de PostgreSQL, en la base del laboratorio.
-- Este rechazo NO pasa por Spring Boot y no genera un log HTTP del backend.
-- Primero comprobar que el UUID de esta prueba no existe.
SELECT COUNT(*) AS debe_ser_cero
FROM public.salas
WHERE id = '6d0d06df-8ab4-4988-8ed0-2854592a6681';

-- En pgAdmin, ejecutar BEGIN y el INSERT seleccionados como un bloque.
-- Esperado: CHECK de escenario, SQLSTATE 23514; capturar el mensaje.
BEGIN;
INSERT INTO public.salas (id, nombre, escenario, activa)
VALUES ('6d0d06df-8ab4-4988-8ed0-2854592a6681',
        'Prueba de integridad', 'luna', TRUE);

-- Despues del error, ejecutar ESTA instruccion por separado.
ROLLBACK;

-- Ejecutar al final: se esperan cero filas con el UUID de prueba.
SELECT COUNT(*) AS filas_persistidas
FROM public.salas
WHERE id = '6d0d06df-8ab4-4988-8ed0-2854592a6681';

