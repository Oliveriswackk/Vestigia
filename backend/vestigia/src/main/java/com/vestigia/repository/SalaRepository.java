package com.vestigia.repository;

import java.util.List;
import java.util.UUID;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import com.vestigia.model.SalaModel;

@Repository
public interface SalaRepository extends JpaRepository<SalaModel, UUID> {

	List<SalaModel> findAllByOrderByCreadaEnAsc();

}
