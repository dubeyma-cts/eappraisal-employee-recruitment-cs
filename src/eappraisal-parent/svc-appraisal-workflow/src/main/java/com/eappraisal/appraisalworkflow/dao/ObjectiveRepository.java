package com.eappraisal.appraisalworkflow.dao;

import com.eappraisal.appraisalworkflow.entity.Objective;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface ObjectiveRepository extends JpaRepository<Objective, Long> {
}
