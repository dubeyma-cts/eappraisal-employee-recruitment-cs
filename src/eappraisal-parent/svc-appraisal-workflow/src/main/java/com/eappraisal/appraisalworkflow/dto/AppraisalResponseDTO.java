package com.eappraisal.appraisalworkflow.dto;

public class AppraisalResponseDTO {
    private Long id;
    private Long employeeId;
    private Long cycleId;
    private String status;
    // Getters and setters
    public Long getId() { return id; }
    public void setId(Long id) { this.id = id; }
    public Long getEmployeeId() { return employeeId; }
    public void setEmployeeId(Long employeeId) { this.employeeId = employeeId; }
    public Long getCycleId() { return cycleId; }
    public void setCycleId(Long cycleId) { this.cycleId = cycleId; }
    public String getStatus() { return status; }
    public void setStatus(String status) { this.status = status; }
}
