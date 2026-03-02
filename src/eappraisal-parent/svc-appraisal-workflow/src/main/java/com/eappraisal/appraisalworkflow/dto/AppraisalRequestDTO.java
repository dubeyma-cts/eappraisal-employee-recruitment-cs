package com.eappraisal.appraisalworkflow.dto;

import jakarta.validation.constraints.NotNull;

public class AppraisalRequestDTO {
    @NotNull(message = "Employee ID is required")
    private Long employeeId;

    @NotNull(message = "Cycle ID is required")
    private Long cycleId;

    @jakarta.validation.constraints.NotBlank(message = "Status is required")
    private String status;

    // Getters and setters
    public Long getEmployeeId() { return employeeId; }
    public void setEmployeeId(Long employeeId) { this.employeeId = employeeId; }
    public Long getCycleId() { return cycleId; }
    public void setCycleId(Long cycleId) { this.cycleId = cycleId; }
    public String getStatus() { return status; }
    public void setStatus(String status) { this.status = status; }
}
