package com.eappraisal.employeemaster.dao;

import com.eappraisal.employeemaster.entity.Employee;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

@Repository
public interface EmployeeRepository extends JpaRepository<Employee, Long> {
    Optional<Employee> findByWorkEmail(String workEmail);

    List<Employee> findTop10ByGivenNameContainingIgnoreCaseOrFamilyNameContainingIgnoreCaseOrWorkEmailContainingIgnoreCase(
            String givenName,
            String familyName,
            String workEmail
    );
}
