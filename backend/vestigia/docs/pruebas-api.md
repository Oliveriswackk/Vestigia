# Pruebas de la API con curl

Con el backend corriendo en `http://localhost:8080`. `-i` muestra el código y las
cabeceras. En Windows usa Git Bash o WSL (las comillas simples no funcionan igual en `cmd`).

```bash
API=http://localhost:8080/api/salas
```

## 1. Listar salas (GET → 200)

```bash
curl -i $API
```

## 2. Consultar una sala (GET → 200)

```bash
curl -i $API/c1b7a1de-5f0e-4a58-9a3e-0b5a7f6d1a01
```

## 3. Crear una sala (POST → 201)

```bash
curl -i -X POST $API \
  -H "Content-Type: application/json" \
  -d '{"id":"7d4f2c10-3b6a-4e8e-9b1a-2f5c6d7e8f90","nombre":"Mirador de la cima","descripcion":"Vista del pueblo y el faro.","escenario":"observatorio","activa":true}'
```

## 4. Crear dos veces el mismo id (idempotencia)

Ejecuta **exactamente el mismo comando** del paso 3 otra vez.

- 1.ª vez: `201 Created` con `Location`.
- 2.ª vez: `200 OK` con la misma sala; no se crea otra fila.

Comprobación en la BD (debe dar `1`):

```bash
psql -h localhost -U postgres -d vestigia -c "SELECT COUNT(*) FROM salas WHERE id = '7d4f2c10-3b6a-4e8e-9b1a-2f5c6d7e8f90';"
```

Versión que muestra solo los códigos:

```bash
for i in 1 2 3; do
  curl -s -o /dev/null -w "%{http_code}\n" -X POST $API \
    -H "Content-Type: application/json" \
    -d '{"id":"7d4f2c10-3b6a-4e8e-9b1a-2f5c6d7e8f90","nombre":"Mirador de la cima","escenario":"observatorio"}'
done
# Esperado tras limpiar: 201, 200, 200
```

## 5. POST inválido (400)

```bash
curl -i -X POST $API \
  -H "Content-Type: application/json" \
  -d '{"nombre":"","escenario":"luna"}'
```

JSON malformado:

```bash
curl -i -X POST $API -H "Content-Type: application/json" -d '{nombre:'
```

## 6. Id inexistente (404)

```bash
curl -i $API/00000000-0000-0000-0000-000000000000
```

## 7. Actualizar (PUT → 200)

```bash
curl -i -X PUT $API/7d4f2c10-3b6a-4e8e-9b1a-2f5c6d7e8f90 \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Mirador de la cima","descripcion":"Cerrado por niebla.","escenario":"observatorio","activa":false}'
```

## 8. Eliminar (DELETE → 204, luego 404)

```bash
curl -i -X DELETE $API/7d4f2c10-3b6a-4e8e-9b1a-2f5c6d7e8f90
curl -i -X DELETE $API/7d4f2c10-3b6a-4e8e-9b1a-2f5c6d7e8f90
```

## 9. CORS (preflight desde la PWA)

```bash
curl -i -X OPTIONS $API \
  -H "Origin: http://localhost:5173" \
  -H "Access-Control-Request-Method: POST"
# Esperado: 200 y Access-Control-Allow-Origin: http://localhost:5173
```

## 10. Actuator

```bash
curl -s http://localhost:8080/actuator/health
curl -s http://localhost:8080/actuator/metrics/jvm.memory.used
curl -s "http://localhost:8080/actuator/metrics/http.server.requests?tag=uri:/api/salas"
```
