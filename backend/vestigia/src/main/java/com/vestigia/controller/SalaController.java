package com.vestigia.controller;

import java.net.URI;
import java.util.List;
import java.util.UUID;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.server.ResponseStatusException;
import org.springframework.web.servlet.support.ServletUriComponentsBuilder;

import com.vestigia.dto.Sala;
import com.vestigia.service.SalaService;

import jakarta.validation.Valid;

@RestController
@RequestMapping("/api/salas")
public class SalaController {

	private final SalaService salaService;

	public SalaController(SalaService salaService) {
		this.salaService = salaService;
	}

	@GetMapping
	public List<Sala> listar() {
		return salaService.listar();
	}

	@GetMapping("/{id}")
	public Sala buscarPorId(@PathVariable UUID id) {
		return salaService.buscarPorId(id).orElseThrow(() -> noEncontrada(id));
	}

	/** Idempotente por id: 201 si la crea, 200 con la existente si el id ya estaba. */
	@PostMapping
	public ResponseEntity<Sala> crear(@Valid @RequestBody Sala sala) {
		SalaService.ResultadoCreacion resultado = salaService.crear(sala);
		if (!resultado.creada()) {
			return ResponseEntity.ok(resultado.sala());
		}
		URI location = ServletUriComponentsBuilder.fromCurrentRequest()
				.path("/{id}")
				.buildAndExpand(resultado.sala().id())
				.toUri();
		return ResponseEntity.created(location).body(resultado.sala());
	}

	@PutMapping("/{id}")
	public Sala actualizar(@PathVariable UUID id, @Valid @RequestBody Sala sala) {
		return salaService.actualizar(id, sala).orElseThrow(() -> noEncontrada(id));
	}

	@DeleteMapping("/{id}")
	public ResponseEntity<Void> eliminar(@PathVariable UUID id) {
		if (!salaService.eliminar(id)) {
			throw noEncontrada(id);
		}
		return ResponseEntity.noContent().build();
	}

	private ResponseStatusException noEncontrada(UUID id) {
		return new ResponseStatusException(HttpStatus.NOT_FOUND, "No existe la sala " + id);
	}

}
