package com.vestigia.dto;

import java.time.LocalDateTime;
import java.util.UUID;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Pattern;
import jakarta.validation.constraints.Size;

/**
 * Representación JSON de una sala. Se serializa en snake_case
 * (creadaEn -> creada_en). creada_en solo se devuelve; el servidor ignora el
 * valor que mande el cliente.
 */
public record Sala(
		UUID id,

		@NotBlank(message = "no debe estar vacío")
		@Size(max = 100, message = "no debe exceder 100 caracteres")
		String nombre,

		String descripcion,

		@NotNull(message = "no debe ser nulo")
		@Pattern(regexp = "pueblo|faro|observatorio", message = "debe ser pueblo, faro u observatorio")
		String escenario,

		Boolean activa,

		LocalDateTime creadaEn) {
}
