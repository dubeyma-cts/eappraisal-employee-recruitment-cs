package com.eappraisal.appraisalworkflow.controller;

import com.eappraisal.appraisalworkflow.dao.AppraisalCycleRepository;
import com.eappraisal.appraisalworkflow.entity.Appraisal;
import com.eappraisal.appraisalworkflow.entity.AppraisalCycle;
import com.eappraisal.appraisalworkflow.service.AppraisalService;
import com.eappraisal.appraisalworkflow.dto.AppraisalAssignRequestDTO;
import com.eappraisal.appraisalworkflow.dto.AppraisalCandidateDTO;
import com.eappraisal.appraisalworkflow.dto.FinalizeAppraisalRequestDTO;
import com.eappraisal.appraisalworkflow.dto.AppraisalRequestDTO;
import com.eappraisal.appraisalworkflow.dto.AppraisalResponseDTO;
import jakarta.validation.Valid;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.ResponseEntity;
import org.springframework.web.client.RestClientException;
import org.springframework.web.client.RestTemplate;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.format.DateTimeParseException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/v1/workflow/appraisals")
@CrossOrigin(origins = "*")
public class AppraisalController {
    private final AppraisalService appraisalService;
    private final AppraisalCycleRepository appraisalCycleRepository;
    private final RestTemplate restTemplate = new RestTemplate();

    @Value("${integration.employee-master.base-url:http://localhost:8081}")
    private String employeeMasterBaseUrl;

    @Autowired
    public AppraisalController(AppraisalService appraisalService,
                               AppraisalCycleRepository appraisalCycleRepository) {
        this.appraisalService = appraisalService;
        this.appraisalCycleRepository = appraisalCycleRepository;
    }


    @GetMapping("/upcoming")
        public ResponseEntity<List<AppraisalCandidateDTO>> getUpcomingAppraisals(
            @RequestParam(value = "cycle", required = false) String cycle,
            @RequestParam(value = "managerUserId", required = false) Long managerUserId,
            @RequestParam(value = "employeeUserId", required = false) Long employeeUserId) {
        String cycleName = normalizeCycle(cycle);
        Long cycleId = resolveCycleIdForRead(cycleName);

        List<EmployeeDirectoryItem> employees = fetchEmployees();
        Map<Long, EmployeeDirectoryItem> employeesById = employees.stream()
                .collect(Collectors.toMap(EmployeeDirectoryItem::getId, item -> item, (left, right) -> left));
        Map<Long, String> namesById = employees.stream()
                .collect(Collectors.toMap(EmployeeDirectoryItem::getId,
                        item -> item.getName() == null || item.getName().isBlank() ? "Employee " + item.getId() : item.getName(),
                        (left, right) -> left));

        Map<Long, Appraisal> assignmentsByEmployee = new HashMap<>();
        if (cycleId != null) {
            assignmentsByEmployee = appraisalService.findByCycleId(cycleId).stream()
                .collect(Collectors.toMap(Appraisal::getSubjectUserId, assignment -> assignment, (oldValue, newValue) -> newValue));
        }

        List<AppraisalCandidateDTO> candidates = new ArrayList<>();
        for (EmployeeDirectoryItem employee : employees) {
            Appraisal appraisal = assignmentsByEmployee.get(employee.getId());
            candidates.add(mapToCandidate(employee, appraisal, cycleName, namesById));
        }

        if (managerUserId != null && managerUserId > 0) {
            candidates = candidates.stream()
                    .filter(candidate -> "ASSIGNED".equalsIgnoreCase(candidate.getCategory()))
                    .filter(candidate -> managerUserId.equals(candidate.getAssignedManagerId()))
                    .collect(Collectors.toList());
        }

        if (employeeUserId != null && employeeUserId > 0) {
            candidates = candidates.stream()
                .filter(candidate -> "ASSIGNED".equalsIgnoreCase(candidate.getCategory()))
                .filter(candidate -> employeeUserId.equals(candidate.getEmployeeId()))
                .collect(Collectors.toList());
        }

        candidates.sort(Comparator.comparing(AppraisalCandidateDTO::getEmployeeName, String.CASE_INSENSITIVE_ORDER));

        return ResponseEntity.ok(candidates);
    }

    @PostMapping("/assign")
    public ResponseEntity<AppraisalCandidateDTO> assignUpcomingAppraisal(@Valid @RequestBody AppraisalAssignRequestDTO request) {
        String cycleName = normalizeCycle(request.getCycleName());
        Long cycleId = resolveOrCreateCycleId(cycleName);
        List<EmployeeDirectoryItem> employees = fetchEmployees();
        Map<Long, EmployeeDirectoryItem> employeesById = employees.stream()
                .collect(Collectors.toMap(EmployeeDirectoryItem::getId, item -> item, (left, right) -> left));
        Map<Long, String> namesById = employees.stream()
                .collect(Collectors.toMap(EmployeeDirectoryItem::getId,
                        item -> item.getName() == null || item.getName().isBlank() ? "Employee " + item.getId() : item.getName(),
                        (left, right) -> left));

        EmployeeDirectoryItem employee = employeesById.get(request.getEmployeeId());
        if (employee == null) {
            throw new IllegalArgumentException("Employee not found in employee-master: " + request.getEmployeeId());
        }

        Long mappedManagerId = employee.getReportsToId() != null ? employee.getReportsToId() : employee.getManagerId();
        Long finalManagerId = request.getManagerId() != null ? request.getManagerId() : mappedManagerId;
        if (finalManagerId == null) {
            throw new IllegalArgumentException("Cannot assign appraisal because reports-to manager is missing for employee: " + request.getEmployeeId());
        }

        Optional<Appraisal> existing = appraisalService.findByCycleIdAndSubjectUserId(cycleId, request.getEmployeeId());
        Appraisal appraisal = existing.orElseGet(Appraisal::new);
        appraisal.setSubjectUserId(request.getEmployeeId());
        appraisal.setManagerUserId(finalManagerId);
        appraisal.setCycleId(cycleId);
        appraisal.setStatus("InReview");
        appraisal.setUpdatedAt(LocalDateTime.now());
        appraisal.setUpdatedBy(0L);
        if (existing.isEmpty()) {
            appraisal.setCreatedAt(LocalDateTime.now());
            appraisal.setCreatedBy(0L);
        }

        Appraisal saved = appraisalService.save(appraisal);
        return ResponseEntity.ok(mapToCandidate(employee, saved, cycleName, namesById));
    }

    @PutMapping("/{id:\\d+}/finalize")
    public ResponseEntity<Map<String, Object>> finalizeAppraisal(@PathVariable("id") Long id,
                                                                  @RequestBody(required = false) FinalizeAppraisalRequestDTO request) {
        Optional<Appraisal> appraisalOpt = appraisalService.findById(id);
        if (appraisalOpt.isEmpty()) {
            return ResponseEntity.notFound().build();
        }

        Appraisal appraisal = appraisalOpt.get();
        appraisal.setStatus("Final");
        appraisal.setFinalisedAt(LocalDateTime.now());
        appraisal.setUpdatedAt(LocalDateTime.now());
        appraisal.setUpdatedBy(0L);

        if (request != null && request.getNextAppraisalDate() != null && !request.getNextAppraisalDate().isBlank()) {
            try {
                LocalDate nextDate = LocalDate.parse(request.getNextAppraisalDate().trim());
                appraisal.setLockedUntil(nextDate.atStartOfDay());
            } catch (DateTimeParseException ignored) {
            }
        }

        Appraisal saved = appraisalService.save(appraisal);
        Map<String, Object> response = new HashMap<>();
        response.put("appraisalId", saved.getAppraisalId());
        response.put("status", saved.getStatus());
        response.put("finalisedAt", saved.getFinalisedAt());
        response.put("message", "Appraisal finalized successfully");
        return ResponseEntity.ok(response);
    }

    @GetMapping("/{id:\\d+}")
    public ResponseEntity<AppraisalResponseDTO> getAppraisalById(@PathVariable("id") Long id) {
        Optional<Appraisal> appraisalOpt = appraisalService.findById(id);
        if (appraisalOpt.isPresent()) {
            Appraisal appraisal = appraisalOpt.get();
            AppraisalResponseDTO dto = new AppraisalResponseDTO();
            dto.setId(appraisal.getAppraisalId());
            dto.setEmployeeId(appraisal.getSubjectUserId());
            dto.setCycleId(appraisal.getCycleId());
            dto.setStatus(appraisal.getStatus());
            return ResponseEntity.ok(dto);
        } else {
            return ResponseEntity.notFound().build();
        }
    }

    @PostMapping
    public ResponseEntity<AppraisalResponseDTO> createAppraisal(@RequestBody AppraisalRequestDTO request) {
        // Map DTO to entity
        Appraisal appraisal = new Appraisal();
        appraisal.setSubjectUserId(request.getEmployeeId());
        appraisal.setManagerUserId(request.getEmployeeId());
        Long requestedCycleId = request.getCycleId();
        if (requestedCycleId != null && appraisalCycleRepository.existsById(requestedCycleId)) {
            appraisal.setCycleId(requestedCycleId);
        } else {
            String fallbackCycleName = requestedCycleId == null
                    ? normalizeCycle(null)
                    : "FY-" + requestedCycleId;
            appraisal.setCycleId(resolveOrCreateCycleId(fallbackCycleName));
        }
        appraisal.setStatus(request.getStatus());
        Appraisal saved = appraisalService.save(appraisal);
        // Map entity to response DTO
        AppraisalResponseDTO dto = new AppraisalResponseDTO();
        dto.setId(saved.getAppraisalId());
        dto.setEmployeeId(saved.getSubjectUserId());
        dto.setCycleId(saved.getCycleId());
        dto.setStatus(saved.getStatus());
        return ResponseEntity.ok(dto);
    }

    private AppraisalCandidateDTO mapToCandidate(EmployeeDirectoryItem employee,
                                                 Appraisal appraisal,
                                                 String cycleName,
                                                 Map<Long, String> namesById) {
        AppraisalCandidateDTO dto = new AppraisalCandidateDTO();
        Long mappedManagerId = employee.getReportsToId() != null ? employee.getReportsToId() : employee.getManagerId();

        dto.setEmployeeId(employee.getId());
        dto.setEmployeeName((employee.getName() == null || employee.getName().isBlank()) ? "Employee " + employee.getId() : employee.getName());
        dto.setEmployeeEmail(employee.getEmail() == null ? "unknown@company.com" : employee.getEmail());
        dto.setDepartmentName(employee.getDepartmentName() == null || employee.getDepartmentName().isBlank() ? "N/A" : employee.getDepartmentName());
        dto.setReportsToId(mappedManagerId);
        dto.setReportsToName(resolvePersonName(mappedManagerId, namesById));
        dto.setDueDate(LocalDate.now().plusDays(30));
        dto.setCycleName(cycleName);

        if (appraisal != null) {
            dto.setAssignmentId(appraisal.getAppraisalId());
            dto.setAssignedManagerId(appraisal.getManagerUserId());
            dto.setAssignedManagerName(resolvePersonName(appraisal.getManagerUserId(), namesById));
        } else {
            dto.setAssignmentId(null);
            dto.setAssignedManagerId(mappedManagerId);
            dto.setAssignedManagerName(resolvePersonName(mappedManagerId, namesById));
        }

        String status = appraisal == null || appraisal.getStatus() == null ? "Pending" : appraisal.getStatus();
        dto.setAssignmentStatus(status);

        if (mappedManagerId == null || mappedManagerId <= 0) {
            dto.setCategory("BLOCKED");
            dto.setBlockerReason("Missing manager mapping (Reports To)");
            dto.setAssignedManagerId(null);
            dto.setAssignedManagerName(null);
            dto.setReportsToId(null);
            dto.setReportsToName(null);
            return dto;
        }

        dto.setBlockerReason(null);
        String normalizedStatus = status.trim().toLowerCase().replace(" ", "");
        if (normalizedStatus.equals("inprogress")
            || normalizedStatus.equals("inreview")
            || normalizedStatus.equals("assigned")
            || normalizedStatus.equals("final")
            || normalizedStatus.equals("closed")
            || normalizedStatus.equals("completed")) {
            dto.setCategory("ASSIGNED");
            return dto;
        }

        dto.setCategory("ELIGIBLE");
        dto.setAssignmentStatus("Pending");
        return dto;
    }

    private String normalizeCycle(String cycleName) {
        if (cycleName == null || cycleName.isBlank()) {
            return "FY-" + LocalDate.now().getYear();
        }
        return cycleName.trim();
    }

    private Long resolveCycleIdForRead(String cycleName) {
        return appraisalCycleRepository.findAll().stream()
                .filter(cycle -> cycle.getName() != null && cycle.getName().equalsIgnoreCase(cycleName))
                .map(AppraisalCycle::getCycleId)
                .findFirst()
                .orElse(null);
    }

    private Long resolveOrCreateCycleId(String cycleName) {
        Long existingCycleId = resolveCycleIdForRead(cycleName);
        if (existingCycleId != null) {
            return existingCycleId;
        }

        LocalDate today = LocalDate.now();
        AppraisalCycle cycle = new AppraisalCycle();
        cycle.setName(cycleName);
        cycle.setStartDate(today.withMonth(1).withDayOfMonth(1));
        cycle.setEndDate(today.withMonth(12).withDayOfMonth(31));
        cycle.setState("Open");
        cycle.setCreatedAt(java.time.LocalDateTime.now());
        cycle.setCreatedBy(0L);
        cycle.setUpdatedAt(java.time.LocalDateTime.now());
        cycle.setUpdatedBy(0L);

        AppraisalCycle saved = appraisalCycleRepository.save(cycle);
        return saved.getCycleId();
    }

    private String resolvePersonName(Long personId, Map<Long, String> namesById) {
        if (personId == null) {
            return null;
        }
        return namesById.getOrDefault(personId, "Manager " + personId);
    }

    private List<EmployeeDirectoryItem> fetchEmployees() {
        String endpoint = employeeMasterBaseUrl + "/api/v1/employees";
        try {
            EmployeeDirectoryItem[] response = restTemplate.getForObject(endpoint, EmployeeDirectoryItem[].class);
            if (response == null) {
                return List.of();
            }
            return Arrays.stream(response)
                    .filter(item -> item.getId() != null)
                    .collect(Collectors.toList());
        } catch (RestClientException ex) {
            throw new IllegalStateException("Unable to fetch employees from employee-master at " + endpoint, ex);
        }
    }

    public static class EmployeeDirectoryItem {
        private Long id;
        private String name;
        private String email;
        private String departmentName;
        private Long reportsToId;
        private Long managerId;

        public Long getId() {
            return id;
        }

        public void setId(Long id) {
            this.id = id;
        }

        public String getName() {
            return name;
        }

        public void setName(String name) {
            this.name = name;
        }

        public String getEmail() {
            return email;
        }

        public void setEmail(String email) {
            this.email = email;
        }

        public String getDepartmentName() {
            return departmentName;
        }

        public void setDepartmentName(String departmentName) {
            this.departmentName = departmentName;
        }

        public Long getReportsToId() {
            return reportsToId;
        }

        public void setReportsToId(Long reportsToId) {
            this.reportsToId = reportsToId;
        }

        public Long getManagerId() {
            return managerId;
        }

        public void setManagerId(Long managerId) {
            this.managerId = managerId;
        }
    }
}
