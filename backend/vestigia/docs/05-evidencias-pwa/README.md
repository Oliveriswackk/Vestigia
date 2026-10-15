# Evidencias de la PWA

Esta carpeta la llena el **equipo de la PWA** (`AiltonMV/vestigia-pwa`).

Evidencias esperadas:

- Listado de salas renderizado tras el `GET /api/salas`.
- DevTools → **Network**: peticiones a `http://localhost:8080/api/salas` con su código (200/201).
- DevTools → **Application**: Service Worker registrado, Cache Storage e IndexedDB (cola offline).
- DevTools → **Console**: sin errores de CORS.
- Modo offline: alta de una sala sin red y reenvío al recuperarla (201 la primera vez, 200 en reenvíos).
