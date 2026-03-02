package com.eappraisal.employeemaster.dao;

import com.eappraisal.employeemaster.entity.OrgUnit;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface OrgUnitRepository extends JpaRepository<OrgUnit, Long> {
	java.util.List<OrgUnit> findAllByOrderByNameAsc();
}
