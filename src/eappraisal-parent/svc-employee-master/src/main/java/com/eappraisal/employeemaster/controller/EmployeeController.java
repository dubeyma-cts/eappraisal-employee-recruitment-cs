package com.eappraisal.employeemaster.controller;

import com.eappraisal.employeemaster.entity.Employee;
import com.eappraisal.employeemaster.service.EmployeeService;
import com.eappraisal.employeemaster.dto.EmployeeRequestDTO;
import com.eappraisal.employeemaster.dto.EmployeeResponseDTO;
import com.eappraisal.employeemaster.dto.ManagerOptionDTO;
import com.eappraisal.employeemaster.dto.OrgUnitDTO;
import com.eappraisal.employeemaster.entity.OrgUnit;
import jakarta.validation.Valid;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.Arrays;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/v1/employees")
@CrossOrigin(origins = "*")
public class EmployeeController {
    private final EmployeeService employeeService;

    @Autowired
    public EmployeeController(EmployeeService employeeService) {
        this.employeeService = employeeService;
    }


    @GetMapping
    public ResponseEntity<List<EmployeeResponseDTO>> getEmployees() {
        List<EmployeeResponseDTO> employees = employeeService.findAll()
                .stream()
                .map(this::mapToDto)
                .collect(Collectors.toList());
        return ResponseEntity.ok(employees);
    }

    @GetMapping("/departments")
    public ResponseEntity<List<OrgUnitDTO>> getDepartments() {
        List<OrgUnitDTO> departments = employeeService.findAllOrgUnits()
                .stream()
                .map(this::mapOrgUnitToDto)
                .collect(Collectors.toList());
        return ResponseEntity.ok(departments);
    }

    @GetMapping("/managers/search")
    public ResponseEntity<List<ManagerOptionDTO>> searchManagers(@RequestParam("q") String query) {
        List<ManagerOptionDTO> options = employeeService.searchManagers(query)
                .stream()
                .map(this::mapManagerOption)
                .collect(Collectors.toList());
        return ResponseEntity.ok(options);
    }

    @GetMapping("/{email}")
    public ResponseEntity<EmployeeResponseDTO> getEmployeeByEmail(@PathVariable("email") String email) {
        Optional<Employee> empOpt = employeeService.findByEmail(email);
        return empOpt.map(employee -> ResponseEntity.ok(mapToDto(employee)))
                .orElseGet(() -> ResponseEntity.notFound().build());
    }

    @PostMapping
    public ResponseEntity<EmployeeResponseDTO> createEmployee(@Valid @RequestBody EmployeeRequestDTO employeeRequest) {
        Employee emp = new Employee();
        String[] nameParts = splitName(employeeRequest.getName());
        emp.setGivenName(nameParts[0]);
        emp.setFamilyName(nameParts[1]);
        emp.setWorkEmail(employeeRequest.getEmail());
        emp.setAddress(employeeRequest.getAddress());
        emp.setCity(employeeRequest.getCity());
        emp.setPhone(employeeRequest.getPhone());
        emp.setMobile(employeeRequest.getMobile());
        emp.setDob(employeeRequest.getDob());
        emp.setGender(employeeRequest.getGender());
        emp.setMaritalStatus(employeeRequest.getMaritalStatus());
        emp.setDoj(employeeRequest.getDoj());
        emp.setPassport(employeeRequest.getPassport());
        emp.setPan(employeeRequest.getPan());
        emp.setWorkExperience(employeeRequest.getWorkExperience());
        Long reportsTo = employeeRequest.getReportsToId() != null ? employeeRequest.getReportsToId() : employeeRequest.getManagerId();
        if (reportsTo != null && employeeService.findById(reportsTo).isEmpty()) {
            throw new IllegalArgumentException("Reports To employee does not exist: " + reportsTo);
        }
        emp.setReportsToId(reportsTo);
        emp.setOrgUnitId(resolveDepartmentId(employeeRequest));
        emp.setStatus("Active");
        emp.setCreatedAt(java.time.LocalDateTime.now());

        Employee saved = employeeService.save(emp);
        return ResponseEntity.ok(mapToDto(saved));
    }

    private EmployeeResponseDTO mapToDto(Employee emp) {
        EmployeeResponseDTO dto = new EmployeeResponseDTO();
        dto.setId(emp.getUserId());
        String given = emp.getGivenName() != null ? emp.getGivenName() : "";
        String family = emp.getFamilyName() != null ? emp.getFamilyName() : "";
        String fullName = (given + " " + family).trim();
        dto.setName(fullName.isEmpty() ? emp.getWorkEmail() : fullName);
        dto.setEmail(emp.getWorkEmail());
        dto.setAddress(emp.getAddress());
        dto.setCity(emp.getCity());
        dto.setPhone(emp.getPhone());
        dto.setMobile(emp.getMobile());
        dto.setDob(emp.getDob());
        dto.setGender(emp.getGender());
        dto.setMaritalStatus(emp.getMaritalStatus());
        dto.setDoj(emp.getDoj());
        dto.setPassport(emp.getPassport());
        dto.setPan(emp.getPan());
        dto.setWorkExperience(emp.getWorkExperience());
        dto.setReportsToId(emp.getReportsToId());
        dto.setManagerId(emp.getReportsToId());

        if (emp.getReportsToId() != null) {
            employeeService.findById(emp.getReportsToId()).ifPresent(manager -> {
                String managerName = buildFullName(manager.getGivenName(), manager.getFamilyName());
                dto.setReportsToName(managerName.isBlank() ? manager.getWorkEmail() : managerName);
            });
        }

        dto.setDepartmentId(emp.getOrgUnitId());
        if (emp.getOrgUnitId() != null) {
            employeeService.findOrgUnitById(emp.getOrgUnitId()).ifPresent(orgUnit -> {
                dto.setDepartmentName(orgUnit.getName());
                dto.setOrgUnitCode(orgUnit.getName());
            });
        }
        if (dto.getOrgUnitCode() == null) {
            dto.setOrgUnitCode("N/A");
        }
        return dto;
    }

    private OrgUnitDTO mapOrgUnitToDto(OrgUnit orgUnit) {
        OrgUnitDTO dto = new OrgUnitDTO();
        dto.setId(orgUnit.getOrgUnitId());
        dto.setName(orgUnit.getName());
        return dto;
    }

    private ManagerOptionDTO mapManagerOption(Employee employee) {
        ManagerOptionDTO dto = new ManagerOptionDTO();
        dto.setId(employee.getUserId());
        String name = buildFullName(employee.getGivenName(), employee.getFamilyName());
        String resolvedName = name.isBlank() ? employee.getWorkEmail() : name;
        dto.setName(resolvedName);
        dto.setDisplayLabel(resolvedName);
        return dto;
    }

    private String[] splitName(String fullName) {
        if (fullName == null || fullName.isBlank()) {
            return new String[]{"", ""};
        }
        List<String> parts = Arrays.stream(fullName.trim().split("\\s+"))
                .filter(part -> !part.isBlank())
                .collect(Collectors.toList());
        if (parts.isEmpty()) {
            return new String[]{"", ""};
        }
        if (parts.size() == 1) {
            return new String[]{parts.get(0), ""};
        }
        String given = parts.get(0);
        String family = String.join(" ", parts.subList(1, parts.size()));
        return new String[]{given, family};
    }

    private String buildFullName(String givenName, String familyName) {
        String given = givenName == null ? "" : givenName.trim();
        String family = familyName == null ? "" : familyName.trim();
        return (given + " " + family).trim();
    }

    private Long resolveDepartmentId(EmployeeRequestDTO employeeRequest) {
        if (employeeRequest.getDepartmentId() != null) {
            Long departmentId = employeeRequest.getDepartmentId();
            if (employeeService.findOrgUnitById(departmentId).isEmpty()) {
                throw new IllegalArgumentException("Department does not exist: " + departmentId);
            }
            return departmentId;
        }

        String orgUnitCode = employeeRequest.getOrgUnitCode();
        if (orgUnitCode != null && !orgUnitCode.isBlank()) {
            Optional<OrgUnit> match = employeeService.findAllOrgUnits().stream()
                    .filter(unit -> unit.getName() != null && unit.getName().equalsIgnoreCase(orgUnitCode.trim()))
                    .findFirst();
            if (match.isPresent()) {
                return match.get().getOrgUnitId();
            }
        }

        throw new IllegalArgumentException("Department is required");
    }
}
