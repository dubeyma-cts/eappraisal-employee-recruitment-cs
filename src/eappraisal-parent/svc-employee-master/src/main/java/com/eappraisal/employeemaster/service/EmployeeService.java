package com.eappraisal.employeemaster.service;

import com.eappraisal.employeemaster.entity.OrgUnit;
import com.eappraisal.employeemaster.entity.AppraisalAssignment;
import com.eappraisal.employeemaster.entity.Employee;
import java.util.List;
import java.util.Optional;

public interface EmployeeService {
    Optional<Employee> findByEmail(String email);
    Optional<Employee> findById(Long id);
    Employee save(Employee employee);
    List<Employee> findAll();
    List<OrgUnit> findAllOrgUnits();
    Optional<OrgUnit> findOrgUnitById(Long id);
    List<Employee> searchManagers(String query);
    List<AppraisalAssignment> findAssignmentsByCycle(String cycleName);
    Optional<AppraisalAssignment> findAssignmentByCycleAndEmployee(String cycleName, Long employeeId);
    AppraisalAssignment saveAssignment(AppraisalAssignment assignment);
}
