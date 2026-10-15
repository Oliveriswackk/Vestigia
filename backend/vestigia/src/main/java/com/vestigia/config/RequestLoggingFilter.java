package com.vestigia.config;

import java.io.IOException;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;

import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

/**
 * Registra una línea por petición a /api/** con método, ruta, código, duración
 * y memoria de la JVM. Sirve como evidencia de los logs del laboratorio.
 */
@Component
public class RequestLoggingFilter extends OncePerRequestFilter {

	private static final Logger log = LoggerFactory.getLogger(RequestLoggingFilter.class);
	private static final long MB = 1024 * 1024;

	@Override
	protected boolean shouldNotFilter(HttpServletRequest request) {
		return !request.getRequestURI().startsWith("/api/");
	}

	@Override
	protected void doFilterInternal(HttpServletRequest request, HttpServletResponse response, FilterChain filterChain)
			throws ServletException, IOException {
		long inicio = System.nanoTime();
		try {
			filterChain.doFilter(request, response);
		} finally {
			long ms = (System.nanoTime() - inicio) / 1_000_000;
			Runtime runtime = Runtime.getRuntime();
			long usadaMb = (runtime.totalMemory() - runtime.freeMemory()) / MB;
			long maximaMb = runtime.maxMemory() / MB;
			log.info("[API] {} {} -> {} | {} ms | heap {}/{} MB", request.getMethod(), request.getRequestURI(),
					response.getStatus(), ms, usadaMb, maximaMb);
		}
	}

}
