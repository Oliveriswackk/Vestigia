package com.vestigia.websocket;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;
import org.springframework.web.socket.handler.TextWebSocketHandler;

@Component
public class GameWebSocketHandler extends TextWebSocketHandler {

    private static final Logger log =
            LoggerFactory.getLogger(GameWebSocketHandler.class);

    @Override
    public void afterConnectionEstablished(WebSocketSession session)
            throws Exception {

        log.info("[WS] Cliente conectado: {}", session.getId());

        session.sendMessage(
                new TextMessage("{\"type\":\"connected\"}")
        );
    }

    @Override
    protected void handleTextMessage(
            WebSocketSession session,
            TextMessage message) throws Exception {

        log.info("[WS] Mensaje recibido de {}", session.getId());

        // Prueba inicial: devolver al cliente el mismo mensaje.
        session.sendMessage(message);
    }
}