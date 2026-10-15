package com.vestigia.service;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.time.LocalDateTime;
import java.util.Optional;
import java.util.UUID;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import com.vestigia.dto.Sala;
import com.vestigia.model.SalaModel;
import com.vestigia.repository.SalaRepository;

@ExtendWith(MockitoExtension.class)
class SalaServiceTest {

	private static final UUID ID = UUID.fromString("c1b7a1de-5f0e-4a58-9a3e-0b5a7f6d1a01");

	@Mock
	private SalaRepository salaRepository;

	@InjectMocks
	private SalaService salaService;

	@Test
	void crearConIdNuevoInsertaLaSala() {
		when(salaRepository.findById(ID)).thenReturn(Optional.empty());
		when(salaRepository.save(any(SalaModel.class))).thenAnswer(inv -> inv.getArgument(0));

		Sala entrada = new Sala(ID, "Puesto del pescador", null, "pueblo", null, LocalDateTime.of(2000, 1, 1, 0, 0));
		SalaService.ResultadoCreacion resultado = salaService.crear(entrada);

		assertThat(resultado.creada()).isTrue();
		assertThat(resultado.sala().id()).isEqualTo(ID);
		assertThat(resultado.sala().activa()).isTrue();
		// creada_en lo asigna el servidor, no el cliente
		assertThat(resultado.sala().creadaEn()).isAfter(LocalDateTime.of(2000, 1, 1, 0, 0));
	}

	@Test
	void crearSinIdGeneraUno() {
		when(salaRepository.save(any(SalaModel.class))).thenAnswer(inv -> inv.getArgument(0));

		SalaService.ResultadoCreacion resultado = salaService.crear(new Sala(null, "Plaza", null, "pueblo", true, null));

		assertThat(resultado.creada()).isTrue();
		assertThat(resultado.sala().id()).isNotNull();
		verify(salaRepository, never()).findById(any());
	}

	@Test
	void crearConIdExistenteDevuelveLaExistenteSinModificarla() {
		SalaModel existente = new SalaModel();
		existente.setId(ID);
		existente.setNombre("Original");
		existente.setEscenario("faro");
		existente.setActiva(true);
		existente.setCreadaEn(LocalDateTime.of(2026, 10, 14, 9, 0));
		when(salaRepository.findById(ID)).thenReturn(Optional.of(existente));

		SalaService.ResultadoCreacion resultado = salaService.crear(new Sala(ID, "Otro nombre", null, "pueblo", false, null));

		assertThat(resultado.creada()).isFalse();
		assertThat(resultado.sala().nombre()).isEqualTo("Original");
		assertThat(resultado.sala().escenario()).isEqualTo("faro");
		verify(salaRepository, never()).save(any());
	}

	@Test
	void buscarIdInexistenteDevuelveVacio() {
		when(salaRepository.findById(ID)).thenReturn(Optional.empty());

		assertThat(salaService.buscarPorId(ID)).isEmpty();
		assertThat(salaService.actualizar(ID, new Sala(null, "X", null, "faro", true, null))).isEmpty();
	}

	@Test
	void eliminarIdInexistenteDevuelveFalse() {
		when(salaRepository.existsById(ID)).thenReturn(false);

		assertThat(salaService.eliminar(ID)).isFalse();
		verify(salaRepository, never()).deleteById(any());
	}

}
