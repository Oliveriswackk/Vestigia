package com.vestigia.controller;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.delete;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.options;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.put;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.header;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.webmvc.test.autoconfigure.WebMvcTest;
import org.springframework.http.MediaType;
import org.springframework.test.context.bean.override.mockito.MockitoBean;
import org.springframework.test.web.servlet.MockMvc;

import com.vestigia.dto.Sala;
import com.vestigia.service.SalaService;

@WebMvcTest(SalaController.class)
class SalaControllerTest {

	private static final UUID ID = UUID.fromString("c1b7a1de-5f0e-4a58-9a3e-0b5a7f6d1a01");
	private static final Sala SALA = new Sala(ID, "Puesto del pescador", "Donde suena la radio por primera vez.",
			"pueblo", true, LocalDateTime.of(2026, 10, 14, 10, 0));
	private static final String JSON_VALIDO = """
			{"id":"c1b7a1de-5f0e-4a58-9a3e-0b5a7f6d1a01","nombre":"Puesto del pescador",
			 "descripcion":"Donde suena la radio por primera vez.","escenario":"pueblo","activa":true}
			""";

	@Autowired
	private MockMvc mockMvc;

	@MockitoBean
	private SalaService salaService;

	@Test
	void listarDevuelve200EnSnakeCase() throws Exception {
		when(salaService.listar()).thenReturn(List.of(SALA));

		mockMvc.perform(get("/api/salas"))
				.andExpect(status().isOk())
				.andExpect(jsonPath("$[0].id").value(ID.toString()))
				.andExpect(jsonPath("$[0].creada_en").value("2026-10-14T10:00:00"));
	}

	@Test
	void crearNuevaDevuelve201ConLocation() throws Exception {
		when(salaService.crear(any())).thenReturn(new SalaService.ResultadoCreacion(SALA, true));

		mockMvc.perform(post("/api/salas").contentType(MediaType.APPLICATION_JSON).content(JSON_VALIDO))
				.andExpect(status().isCreated())
				.andExpect(header().string("Location", "http://localhost/api/salas/" + ID))
				.andExpect(jsonPath("$.nombre").value("Puesto del pescador"));
	}

	@Test
	void crearConIdExistenteDevuelve200() throws Exception {
		when(salaService.crear(any())).thenReturn(new SalaService.ResultadoCreacion(SALA, false));

		mockMvc.perform(post("/api/salas").contentType(MediaType.APPLICATION_JSON).content(JSON_VALIDO))
				.andExpect(status().isOk())
				.andExpect(header().doesNotExist("Location"))
				.andExpect(jsonPath("$.id").value(ID.toString()));
	}

	@Test
	void crearInvalidoDevuelve400ConDetalles() throws Exception {
		mockMvc.perform(post("/api/salas").contentType(MediaType.APPLICATION_JSON)
				.content("{\"nombre\":\"\",\"escenario\":\"luna\"}"))
				.andExpect(status().isBadRequest())
				.andExpect(jsonPath("$.status").value(400))
				.andExpect(jsonPath("$.error").value("Bad Request"))
				.andExpect(jsonPath("$.message").value("Datos inválidos"))
				.andExpect(jsonPath("$.path").value("/api/salas"))
				.andExpect(jsonPath("$.details.length()").value(2));
		verify(salaService, never()).crear(any());
	}

	@Test
	void jsonMalformadoDevuelve400() throws Exception {
		mockMvc.perform(post("/api/salas").contentType(MediaType.APPLICATION_JSON).content("{nombre:"))
				.andExpect(status().isBadRequest())
				.andExpect(jsonPath("$.status").value(400));
	}

	@Test
	void buscarInexistenteDevuelve404() throws Exception {
		when(salaService.buscarPorId(ID)).thenReturn(Optional.empty());

		mockMvc.perform(get("/api/salas/{id}", ID))
				.andExpect(status().isNotFound())
				.andExpect(jsonPath("$.status").value(404))
				.andExpect(jsonPath("$.path").value("/api/salas/" + ID));
	}

	@Test
	void buscarExistenteDevuelve200() throws Exception {
		when(salaService.buscarPorId(ID)).thenReturn(Optional.of(SALA));

		mockMvc.perform(get("/api/salas/{id}", ID))
				.andExpect(status().isOk())
				.andExpect(jsonPath("$.escenario").value("pueblo"));
	}

	@Test
	void actualizarUsaElIdDeLaRuta() throws Exception {
		when(salaService.actualizar(eq(ID), any())).thenReturn(Optional.of(SALA));

		mockMvc.perform(put("/api/salas/{id}", ID).contentType(MediaType.APPLICATION_JSON).content(JSON_VALIDO))
				.andExpect(status().isOk());
		verify(salaService).actualizar(eq(ID), any());
	}

	@Test
	void eliminarDevuelve204o404() throws Exception {
		when(salaService.eliminar(ID)).thenReturn(true);
		mockMvc.perform(delete("/api/salas/{id}", ID)).andExpect(status().isNoContent());

		UUID otro = UUID.randomUUID();
		when(salaService.eliminar(otro)).thenReturn(false);
		mockMvc.perform(delete("/api/salas/{id}", otro)).andExpect(status().isNotFound());
	}

	@Test
	void corsPermiteLaPwa() throws Exception {
		mockMvc.perform(options("/api/salas")
				.header("Origin", "http://localhost:5173")
				.header("Access-Control-Request-Method", "POST"))
				.andExpect(status().isOk())
				.andExpect(header().string("Access-Control-Allow-Origin", "http://localhost:5173"));
	}

}
