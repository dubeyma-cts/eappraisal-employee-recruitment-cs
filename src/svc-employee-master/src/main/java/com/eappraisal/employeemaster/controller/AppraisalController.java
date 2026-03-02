package com.eappraisal.employeemaster.controller;

import com.eappraisal.employeemaster.dto.AppraisalAssignRequestDTO;
import com.eappraisal.employeemaster.dto.AppraisalCandidateDTO;
import com.eappraisal.employeemaster.entity.AppraisalAssignment;
import com.eappraisal.employeemaster.entity.Employee;
import com.eappraisal.employeemaster.entity.OrgUnit;
import com.eappraisal.employeemaster.service.EmployeeService;
import jakarta.validation.Valid;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.*;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/v1/appraisals")
@CrossOrigin(origins = "*")
public class AppraisalController {
    private final EmployeeService employeeService;

    @Autowired
    public AppraisalController(EmployeeService employeeService) {
        this.employeeService = employeeService;
    }

    @GetMapping("/upcoming")
    public ResponseEntity<List<AppraisalCandidateDTO>> getUpcomingAppraisals(@RequestParam(value = "cycle", required = false) String cycle) {
        String cycleName = normalizeCycle(cycle);
        List<Employee> employees = employeeService.findAll().stream()
                .filter(this::isActive)
                .collect(Collectors.toList());

        Map<Long, OrgUnit> orgUnitById = employeeService.findAllOrgUnits()
                .stream()
                .collect(Collectors.toMap(OrgUnit::getOrgUnitId, unit -> unit, (left, right) -> left));

        List<AppraisalCandidateDTO> candidates = new ArrayList<>();
        for (Employee employee : employees) {
            candidates.add(buildCandidate(employee, cycleName, orgUnitById));
        }

        Comparator<AppraisalCandidateDTO> comparator = Comparator
                .comparing((AppraisalCandidateDTO dto) -> categoryRank(dto.getCategory()))
                .thenComparing(AppraisalCandidateDTO::getEmployeeName, Comparator.nullsLast(String::compareToIgnoreCase));

        candidates.sort(comparator);
        return ResponseEntity.ok(candidates);
    }

    @PostMapping("/assign")
    public ResponseEntity<AppraisalCandidateDTO> assignUpcomingAppraisal(@Valid @RequestBody AppraisalAssignRequestDTO request) {
        String cycleName = normalizeCycle(request.getCycleName());

        Employee employee = employeeService.findById(request.getEmployeeId())
                .orElseThrow(() -> new IllegalArgumentException("Employee not found: " + request.getEmployeeId()));

        if (!isActive(employee)) {
            throw new IllegalArgumentException("Employee is not active: " + request.getEmployeeId());
        }

        Long managerId = request.getManagerId() != null ? request.getManagerId() : employee.getReportsToId();
        if (managerId == null) {
            throw new IllegalArgumentException("Missing manager mapping for employee: " + request.getEmployeeId());
        }

        Employee manager = employeeService.findById(managerId)
                .orElseThrow(() -> new IllegalArgumentException("Manager not found: " + managerId));

        AppraisalAssignment assignment = employeeService
                .findAssignmentByCycleAndEmployee(cycleName, employee.getUserId())
                .orElseGet(AppraisalAssignment::new);

        assignment.setEmployeeId(employee.getUserId());
        assignment.setManagerId(manager.getUserId());
        assignment.setCycleName(cycleName);
        assignment.setStatus("In Progress");
        assignment.setDueDate(calculateDueDate(employee));
        assignment.setAssignedAt(LocalDateTime.now());

        employeeService.saveAssignment(assignment);

        Map<Long, OrgUnit> orgUnitById = employeeService.findAllOrgUnits()
                .stream()
                .collect(Collectors.toMap(OrgUnit::getOrgUnitId, unit -> unit, (left, right) -> left));

        return ResponseEntity.ok(buildCandidate(employee, cycleName, orgUnitById));
    }

    private AppraisalCandidateDTO buildCandidate(Employee employee, String cycleName, Map<Long, OrgUnit> orgUnitById) {
        Optional<AppraisalAssignment> existing = employeeService.findAssignmentByCycleAndEmployee(cycleName, employee.getUserId());

        AppraisalCandidateDTO dto = new AppraisalCandidateDTO();
        dto.setEmployeeId(employee.getUserId());
        dto.setEmployeeName(buildFullName(employee));
        dto.setEmployeeEmail(employee.getWorkEmail());
        dto.setCycleName(cycleName);
        dto.setDueDate(calculateDueDate(employee));

        OrgUnit orgUnit = orgUnitById.get(employee.getOrgUnitId());
        dto.setDepartmentName(orgUnit != null ? orgUnit.getName() : "N/A");

        dto.setReportsToId(employee.getReportsToId());
        if (employee.getReportsToId() != null) {
            employeeService.findById(employee.getReportsToId()).ifPresent(manager -> dto.setReportsToName(buildFullName(manager)));
        }

        if (existing.isPresent()) {
            AppraisalAssignment assignment = existing.get();
            dto.setCategory("ASSIGNED");
            dto.setAssignmentStatus(assignment.getStatus());
            dto.setAssignmentId(assignment.getAssignmentId());
            dto.setAssignedManagerId(assignment.getManagerId());
            employeeService.findById(assignment.getManagerId()).ifPresent(manager -> dto.setAssignedManagerName(buildFullName(manager)));
            return dto;
        }

        if (employee.getReportsToId() == null) {
            dto.setCategory("BLOCKED");
            dto.setAssignmentStatus("Pending");
            dto.setBlockerReason("Missing manager mapping (Reports To)");
            return dto;
        }

        dto.setCategory("ELIGIBLE");
        dto.setAssignmentStatus("Pending");
        dto.setAssignedManagerId(employee.getReportsToId());
        dto.setAssignedManagerName(dto.getReportsToName());
        return dto;
    }

    private String normalizeCycle(String cycle) {
        if (cycle == null || cycle.isBlank()) {
            return "FY-" + LocalDate.now().getYear();
        }
        return cycle.trim();
    }

    private LocalDate calculateDueDate(Employee employee) {
        if (employee.getDoj() != null) {
            return employee.getDoj().plusYears(Math.max(1, employee.getWorkExperience() != null ? employee.getWorkExperience() : 1));
        }
        return LocalDate.now().plusDays(30);
    }

    private boolean isActive(Employee employee) {
        String status = employee.getStatus();
        return status != null && status.equalsIgnoreCase("Active");
    }

    private int categoryRank(String category) {
        if ("ELIGIBLE".equalsIgnoreCase(category)) {
            return 1;
        }
        if ("BLOCKED".equalsIgnoreCase(category)) {
            return 2;
        }
        if ("ASSIGNED".equalsIgnoreCase(category)) {
            return 3;
        }
        return 9;
    }

    private String buildFullName(Employee employee) {
        String given = employee.getGivenName() == null ? "" : employee.getGivenName().trim();
        String family = employee.getFamilyName() == null ? "" : employee.getFamilyName().trim();
        String fullName = (given + " " + family).trim();
        return fullName.isBlank() ? employee.getWorkEmail() : fullName;
    }
}
