package com.eappraisal.appraisalworkflow.dao;

import com.eappraisal.appraisalworkflow.entity.Appraisal;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

@Repository
public interface AppraisalRepository extends JpaRepository<Appraisal, Long> {
	List<Appraisal> findByCycleId(Long cycleId);
	Optional<Appraisal> findByCycleIdAndSubjectUserId(Long cycleId, Long subjectUserId);
}
