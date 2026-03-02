package com.eappraisal.compensationctc.dto;

public class CtcSnapshotResponseDTO {
    private Long id;
    private Long employeeId;
    private Long appraisalId;
    private Double ctcAmount;
    private String createdAt;
    // Getters and setters
    public Long getId() { return id; }
    public void setId(Long id) { this.id = id; }
    public Long getEmployeeId() { return employeeId; }
    public void setEmployeeId(Long employeeId) { this.employeeId = employeeId; }
    public Long getAppraisalId() { return appraisalId; }
    public void setAppraisalId(Long appraisalId) { this.appraisalId = appraisalId; }
    public Double getCtcAmount() { return ctcAmount; }
    public void setCtcAmount(Double ctcAmount) { this.ctcAmount = ctcAmount; }
    public String getCreatedAt() { return createdAt; }
    public void setCreatedAt(String createdAt) { this.createdAt = createdAt; }
}
