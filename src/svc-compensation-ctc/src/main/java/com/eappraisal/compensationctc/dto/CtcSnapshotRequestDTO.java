package com.eappraisal.compensationctc.dto;

import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Pattern;
import jakarta.validation.constraints.Size;

public class CtcSnapshotRequestDTO {
    @NotNull(message = "Employee ID is required")
    private Long employeeId;

    @NotNull(message = "Appraisal ID is required")
    private Long appraisalId;

    @jakarta.validation.constraints.NotNull(message = "CTC amount is required")
    @jakarta.validation.constraints.Min(value = 0, message = "CTC amount must be non-negative")
    private Double ctcAmount;

    @NotBlank(message = "Currency is required")
    @Size(min = 3, max = 3, message = "Currency must be a 3-letter code")
    @Pattern(regexp = "^[A-Za-z]{3}$", message = "Currency must be alphabetic")
    private String currency;

    // Getters and setters
    public Long getEmployeeId() { return employeeId; }
    public void setEmployeeId(Long employeeId) { this.employeeId = employeeId; }
    public Long getAppraisalId() { return appraisalId; }
    public void setAppraisalId(Long appraisalId) { this.appraisalId = appraisalId; }
    public Double getCtcAmount() { return ctcAmount; }
    public void setCtcAmount(Double ctcAmount) { this.ctcAmount = ctcAmount; }
    public String getCurrency() { return currency; }
    public void setCurrency(String currency) { this.currency = currency; }
}
