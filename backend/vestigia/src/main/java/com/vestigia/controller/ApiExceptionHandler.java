package com.vestigia.controller;

import java.time.LocalDateTime;
import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.HttpStatus;
import org.springframework.http.HttpStatusCode;
import org.springframework.http.ResponseEntity;
import org.springframework.http.converter.HttpMessageNotReadableException;
import org.springframework.web.ErrorResponse;
import org.springframework.web.bind.MethodArgumentNotValidException;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;
import org.springframework.web.method.annotation.MethodArgumentTypeMismatchException;
import org.springframework.web.server.ResponseStatusException;

import jakarta.servlet.http.HttpServletRequest;

/** Convierte las excepciones en un cuerpo JSON de error uniforme. */
@RestControllerAdvice
public class ApiExceptionHandler {

	private static final Logger log = LoggerFactory.getLogger(ApiExceptionHandler.class);

	public record ErrorBody(LocalDateTime timestamp, int status, String error, String message, String path,
			List<FieldDetail> details) {
	}

	public record FieldDetail(String field, String message) {
	}

	@ExceptionHandler(MethodArgumentNotValidException.class)
	public ResponseEntity<ErrorBody> validacion(MethodArgumentNotValidException ex, HttpServletRequest request) {
		List<FieldDetail> details = ex.getBindingResult().getFieldErrors().stream()
				.map(e -> new FieldDetail(e.getField(), e.getDefaultMessage()))
				.toList();
		return body(HttpStatus.BAD_REQUEST, "Datos inválidos", request, details);
	}

	@ExceptionHandler(HttpMessageNotReadableException.class)
	public ResponseEntity<ErrorBody> jsonMalformado(HttpMessageNotReadableException ex, HttpServletRequest request) {
		return body(HttpStatus.BAD_REQUEST, "JSON malformado o ilegible", request, List.of());
	}

	@ExceptionHandler(MethodArgumentTypeMismatchException.class)
	public ResponseEntity<ErrorBody> parametroInvalido(MethodArgumentTypeMismatchException ex,
			HttpServletRequest request) {
		return body(HttpStatus.BAD_REQUEST, "Valor inválido para '" + ex.getName() + "'", request, List.of());
	}

	@ExceptionHandler(ResponseStatusException.class)
	public ResponseEntity<ErrorBody> conEstado(ResponseStatusException ex, HttpServletRequest request) {
		return body(ex.getStatusCode(), ex.getReason(), request, List.of());
	}

	@ExceptionHandler(Exception.class)
	public ResponseEntity<ErrorBody> inesperado(Exception ex, HttpServletRequest request) {
		// Errores propios de Spring MVC (405, 415, ruta inexistente...) conservan su código.
		if (ex instanceof ErrorResponse errorResponse) {
			return body(errorResponse.getStatusCode(), errorResponse.getBody().getDetail(), request, List.of());
		}
		log.error("Error no controlado en {} {}", request.getMethod(), request.getRequestURI(), ex);
		return body(HttpStatus.INTERNAL_SERVER_ERROR, "Error interno del servidor", request, List.of());
	}

	private ResponseEntity<ErrorBody> body(HttpStatusCode status, String message, HttpServletRequest request,
			List<FieldDetail> details) {
		HttpStatus httpStatus = HttpStatus.resolve(status.value());
		String error = httpStatus != null ? httpStatus.getReasonPhrase() : String.valueOf(status.value());
		ErrorBody errorBody = new ErrorBody(LocalDateTime.now(), status.value(), error, message,
				request.getRequestURI(), details);
		return ResponseEntity.status(status).body(errorBody);
	}

}
