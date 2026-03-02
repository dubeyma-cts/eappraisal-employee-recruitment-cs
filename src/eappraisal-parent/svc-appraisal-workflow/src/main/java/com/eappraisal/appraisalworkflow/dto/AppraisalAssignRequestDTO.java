package com.eappraisal.appraisalworkflow.dto;

import jakarta.validation.constraints.NotNull;

public class AppraisalAssignRequestDTO {
    @NotNull(message = "Employee ID is required")
    private Long employeeId;
    private Long managerId;
    private String cycleName;

    public Long getEmployeeId() {
        return employeeId;
    }

    public void setEmployeeId(Long employeeId) {
        this.employeeId = employeeId;
    }

    public Long getManagerId() {
        return managerId;
    }

    public void setManagerId(Long managerId) {
        this.managerId = managerId;
    }

    public String getCycleName() {
        return cycleName;
    }

    public void setCycleName(String cycleName) {
        this.cycleName = cycleName;
    }
}
