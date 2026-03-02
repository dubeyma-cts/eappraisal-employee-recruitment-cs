package com.eappraisal.compensationctc.entity;

import jakarta.persistence.*;
import org.hibernate.annotations.JdbcTypeCode;
import org.hibernate.type.SqlTypes;

import java.time.LocalDateTime;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

@Entity
@Table(name = "ctc_snapshot", schema = "apps")
public class CtcSnapshot {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long ctcId;

    @Column(nullable = false)
    private Long appraisalId;

    @Column(nullable = false)
    private Long approverUserId;

    @JdbcTypeCode(SqlTypes.CHAR)
    @Column(columnDefinition = "CHAR(3)", nullable = false)
    private String currency;

    @Column(columnDefinition = "TEXT", nullable = false)
    private String componentsJson;

    @Column(nullable = false)
    private LocalDateTime approvedAt;

    @Transient
    private Double transientCtcAmount;

    public Long getCtcId() {
        return ctcId;
    }

    public void setCtcId(Long ctcId) {
        this.ctcId = ctcId;
    }

    public Long getAppraisalId() {
        return appraisalId;
    }

    public void setAppraisalId(Long appraisalId) {
        this.appraisalId = appraisalId;
    }

    public Long getApproverUserId() {
        return approverUserId;
    }

    public void setApproverUserId(Long approverUserId) {
        this.approverUserId = approverUserId;
    }

    public String getCurrency() {
        return currency;
    }

    public void setCurrency(String currency) {
        this.currency = currency;
    }

    public String getComponentsJson() {
        return componentsJson;
    }

    public void setComponentsJson(String componentsJson) {
        this.componentsJson = componentsJson;
    }

    public LocalDateTime getApprovedAt() {
        return approvedAt;
    }

    public void setApprovedAt(LocalDateTime approvedAt) {
        this.approvedAt = approvedAt;
    }

    public Long getId() {
        return ctcId;
    }

    public void setId(Long id) {
        this.ctcId = id;
    }

    public Long getEmployeeId() {
        return approverUserId;
    }

    public void setEmployeeId(Long employeeId) {
        this.approverUserId = employeeId;
    }

    public LocalDateTime getCreatedAt() {
        return approvedAt;
    }

    public void setCreatedAt(LocalDateTime createdAt) {
        this.approvedAt = createdAt;
    }

    public Double getCtcAmount() {
        if (transientCtcAmount != null) {
            return transientCtcAmount;
        }
        if (componentsJson == null || componentsJson.isBlank()) {
            return null;
        }

        try {
            return Double.parseDouble(componentsJson.trim());
        } catch (NumberFormatException ignored) {
        }

        Matcher matcher = Pattern.compile("-?\\d+(\\.\\d+)?").matcher(componentsJson);
        if (matcher.find()) {
            try {
                return Double.parseDouble(matcher.group());
            } catch (NumberFormatException ignored) {
                return null;
            }
        }
        return null;
    }

    public void setCtcAmount(Double ctcAmount) {
        this.transientCtcAmount = ctcAmount;
        this.componentsJson = ctcAmount == null ? null : String.valueOf(ctcAmount);
        if (this.currency == null || this.currency.isBlank()) {
            this.currency = "INR";
        }
    }
}
