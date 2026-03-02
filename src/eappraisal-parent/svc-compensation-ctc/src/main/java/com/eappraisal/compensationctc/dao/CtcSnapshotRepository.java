package com.eappraisal.compensationctc.dao;

import com.eappraisal.compensationctc.entity.CtcSnapshot;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface CtcSnapshotRepository extends JpaRepository<CtcSnapshot, Long> {
    List<CtcSnapshot> findByAppraisalId(Long appraisalId);
}
