package com.eappraisal.employeemaster.dao;

import com.eappraisal.employeemaster.entity.AppraisalAssignment;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

@Repository
public interface AppraisalAssignmentRepository extends JpaRepository<AppraisalAssignment, Long> {
    Optional<AppraisalAssignment> findByCycleNameAndEmployeeId(String cycleName, Long employeeId);

    List<AppraisalAssignment> findAllByCycleName(String cycleName);
}
