package com.vestigia.service;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;
import java.util.UUID;

import org.springframework.dao.DataIntegrityViolationException;
import org.springframework.stereotype.Service;

import com.vestigia.dto.Sala;
import com.vestigia.model.SalaModel;
import com.vestigia.repository.SalaRepository;

@Service
public class SalaService {

	/** Resultado de crear: la sala y si se insertó (true) o ya existía (false). */
	public record ResultadoCreacion(Sala sala, boolean creada) {
	}

	private final SalaRepository salaRepository;

	public SalaService(SalaRepository salaRepository) {
		this.salaRepository = salaRepository;
	}

	public List<Sala> listar() {
		return salaRepository.findAllByOrderByCreadaEnAsc().stream()
				.map(this::toDto)
				.toList();
	}

	public Optional<Sala> buscarPorId(UUID id) {
		return salaRepository.findById(id).map(this::toDto);
	}

	/**
	 * Crea la sala de forma idempotente por id: si el id ya existe no modifica
	 * nada y devuelve la sala existente. Así los reenvíos de la cola offline de
	 * la PWA no duplican registros.
	 */
	public ResultadoCreacion crear(Sala sala) {
		if (sala.id() != null) {
			Optional<SalaModel> existente = salaRepository.findById(sala.id());
			if (existente.isPresent()) {
				return new ResultadoCreacion(toDto(existente.get()), false);
			}
		}

		SalaModel model = new SalaModel();
		model.setId(sala.id() != null ? sala.id() : UUID.randomUUID());
		copiarCampos(sala, model);
		model.setCreadaEn(LocalDateTime.now());

		try {
			return new ResultadoCreacion(toDto(salaRepository.save(model)), true);
		} catch (DataIntegrityViolationException e) {
			// Dos reenvíos simultáneos con el mismo id: el segundo choca con la PK.
			return salaRepository.findById(model.getId())
					.map(m -> new ResultadoCreacion(toDto(m), false))
					.orElseThrow(() -> e);
		}
	}

	public Optional<Sala> actualizar(UUID id, Sala sala) {
		return salaRepository.findById(id).map(model -> {
			copiarCampos(sala, model);
			return toDto(salaRepository.save(model));
		});
	}

	public boolean eliminar(UUID id) {
		if (!salaRepository.existsById(id)) {
			return false;
		}
		salaRepository.deleteById(id);
		return true;
	}

	private void copiarCampos(Sala sala, SalaModel model) {
		model.setNombre(sala.nombre());
		model.setDescripcion(sala.descripcion());
		model.setEscenario(sala.escenario());
		model.setActiva(sala.activa() != null ? sala.activa() : Boolean.TRUE);
	}

	private Sala toDto(SalaModel model) {
		return new Sala(
				model.getId(),
				model.getNombre(),
				model.getDescripcion(),
				model.getEscenario(),
				model.getActiva(),
				model.getCreadaEn());
	}

}
