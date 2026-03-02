package com.eappraisal.employeemaster.dao;

import com.eappraisal.employeemaster.entity.ManagerMapping;
import com.eappraisal.employeemaster.entity.ManagerMappingId;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface ManagerMappingRepository extends JpaRepository<ManagerMapping, ManagerMappingId> {
}
