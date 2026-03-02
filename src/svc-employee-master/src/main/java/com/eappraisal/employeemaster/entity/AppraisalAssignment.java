package com.eappraisal.employeemaster.entity;

import jakarta.persistence.*;

import java.time.LocalDate;
import java.time.LocalDateTime;

@Entity
@Table(name = "appraisal_assignment", schema = "apps", uniqueConstraints = {
        @UniqueConstraint(name = "ux_appraisal_assignment_cycle_emp", columnNames = {"cycle_name", "employee_id"})
})
public class AppraisalAssignment {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long assignmentId;

    @Column(nullable = false)
    private Long employeeId;

    @Column(nullable = false)
    private Long managerId;

    @Column(nullable = false, length = 32)
    private String cycleName;

    @Column(nullable = false, length = 24)
    private String status;

    private LocalDate dueDate;

    @Column(nullable = false)
    private LocalDateTime assignedAt;

    public Long getAssignmentId() {
        return assignmentId;
    }

    public void setAssignmentId(Long assignmentId) {
        this.assignmentId = assignmentId;
    }

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

    public String getStatus() {
        return status;
    }

    public void setStatus(String status) {
        this.status = status;
    }

    public LocalDate getDueDate() {
        return dueDate;
    }

    public void setDueDate(LocalDate dueDate) {
        this.dueDate = dueDate;
    }

    public LocalDateTime getAssignedAt() {
        return assignedAt;
    }

    public void setAssignedAt(LocalDateTime assignedAt) {
        this.assignedAt = assignedAt;
    }
}
