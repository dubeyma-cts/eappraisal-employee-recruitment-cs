package com.eappraisal.appraisalworkflow.dao;

import com.eappraisal.appraisalworkflow.entity.AppraisalCycle;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface AppraisalCycleRepository extends JpaRepository<AppraisalCycle, Long> {
}
