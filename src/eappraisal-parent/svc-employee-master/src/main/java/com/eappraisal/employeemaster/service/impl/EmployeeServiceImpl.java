package com.eappraisal.employeemaster.service.impl;

import com.eappraisal.employeemaster.dao.AppraisalAssignmentRepository;
import com.eappraisal.employeemaster.dao.OrgUnitRepository;
import com.eappraisal.employeemaster.dao.EmployeeRepository;
import com.eappraisal.employeemaster.entity.AppraisalAssignment;
import com.eappraisal.employeemaster.entity.Employee;
import com.eappraisal.employeemaster.entity.OrgUnit;
import org.springframework.jdbc.core.JdbcTemplate;
import com.eappraisal.employeemaster.service.EmployeeService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Optional;

@Service
public class EmployeeServiceImpl implements EmployeeService {
    private final EmployeeRepository employeeRepository;
    private final OrgUnitRepository orgUnitRepository;
    private final AppraisalAssignmentRepository appraisalAssignmentRepository;
    private final JdbcTemplate jdbcTemplate;

    @Autowired
    public EmployeeServiceImpl(
            EmployeeRepository employeeRepository,
            OrgUnitRepository orgUnitRepository,
            AppraisalAssignmentRepository appraisalAssignmentRepository,
            JdbcTemplate jdbcTemplate
    ) {
        this.employeeRepository = employeeRepository;
        this.orgUnitRepository = orgUnitRepository;
        this.appraisalAssignmentRepository = appraisalAssignmentRepository;
        this.jdbcTemplate = jdbcTemplate;
    }

    @Override
    public Optional<Employee> findByEmail(String email) {
        return employeeRepository.findByWorkEmail(email);
    }

    @Override
    public Optional<Employee> findById(Long id) {
        return employeeRepository.findById(id);
    }

    @Override
    public Employee save(Employee employee) {
        // Business logic: set default status and createdAt
        if (employee.getStatus() == null) {
            employee.setStatus("Active");
        }
        if (employee.getCreatedAt() == null) {
            employee.setCreatedAt(java.time.LocalDateTime.now());
        }
        // Add more logic as needed (e.g., manager mapping)
        return employeeRepository.save(employee);
    }

    @Override
    public List<Employee> findAll() {
        return employeeRepository.findAll();
    }

    @Override
    public List<OrgUnit> findAllOrgUnits() {
        return orgUnitRepository.findAllByOrderByNameAsc();
    }

    @Override
    public Optional<OrgUnit> findOrgUnitById(Long id) {
        if (id == null) {
            return Optional.empty();
        }
        return orgUnitRepository.findById(id);
    }

    @Override
    public List<Employee> searchManagers(String query) {
        String safeQuery = query == null ? "" : query.trim();
        if (safeQuery.isEmpty()) {
            return new ArrayList<>();
        }

        LinkedHashMap<Long, Employee> results = new LinkedHashMap<>();
        if (safeQuery.matches("\\d+")) {
            Long id = Long.valueOf(safeQuery);
            employeeRepository.findById(id).ifPresent(employee -> {
                if (isManagerUser(employee.getUserId())) {
                    results.put(employee.getUserId(), employee);
                }
            });
        }

        List<Employee> byNameOrEmail = employeeRepository
                .findTop10ByGivenNameContainingIgnoreCaseOrFamilyNameContainingIgnoreCaseOrWorkEmailContainingIgnoreCase(
                        safeQuery,
                        safeQuery,
                        safeQuery
                );
        for (Employee employee : byNameOrEmail) {
            if (isManagerUser(employee.getUserId())) {
                results.put(employee.getUserId(), employee);
            }
        }
        return new ArrayList<>(results.values());
    }

    private boolean isManagerUser(Long userId) {
        if (userId == null) {
            return false;
        }

        Integer count = jdbcTemplate.queryForObject(
                """
                SELECT COUNT(1)
                FROM apps.user_role ur
                JOIN apps.role r ON r.role_id = ur.role_id
                WHERE ur.user_id = ?
                  AND UPPER(r.name) = 'MANAGER'
                  AND (ur.effective_to IS NULL OR ur.effective_to > CURRENT_TIMESTAMP)
                """,
                Integer.class,
                userId
        );
        return count != null && count > 0;
    }

    @Override
    public List<AppraisalAssignment> findAssignmentsByCycle(String cycleName) {
        return appraisalAssignmentRepository.findAllByCycleName(cycleName);
    }

    @Override
    public Optional<AppraisalAssignment> findAssignmentByCycleAndEmployee(String cycleName, Long employeeId) {
        return appraisalAssignmentRepository.findByCycleNameAndEmployeeId(cycleName, employeeId);
    }

    @Override
    public AppraisalAssignment saveAssignment(AppraisalAssignment assignment) {
        return appraisalAssignmentRepository.save(assignment);
    }
}
