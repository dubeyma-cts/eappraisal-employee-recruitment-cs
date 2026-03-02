package com.eappraisal.appraisalworkflow.dto;

import java.time.LocalDate;

public class AppraisalCandidateDTO {
    private Long employeeId;
    private String employeeName;
    private String employeeEmail;
    private String departmentName;
    private Long reportsToId;
    private String reportsToName;
    private LocalDate dueDate;
    private String cycleName;
    private String category;
    private String assignmentStatus;
    private Long assignmentId;
    private Long assignedManagerId;
    private String assignedManagerName;
    private String blockerReason;

    public Long getEmployeeId() {
        return employeeId;
    }

    public void setEmployeeId(Long employeeId) {
        this.employeeId = employeeId;
    }

    public String getEmployeeName() {
        return employeeName;
    }

    public void setEmployeeName(String employeeName) {
        this.employeeName = employeeName;
    }

    public String getEmployeeEmail() {
        return employeeEmail;
    }

    public void setEmployeeEmail(String employeeEmail) {
        this.employeeEmail = employeeEmail;
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

    public String getReportsToName() {
        return reportsToName;
    }

    public void setReportsToName(String reportsToName) {
        this.reportsToName = reportsToName;
    }

    public LocalDate getDueDate() {
        return dueDate;
    }

    public void setDueDate(LocalDate dueDate) {
        this.dueDate = dueDate;
    }

    public String getCycleName() {
        return cycleName;
    }

    public void setCycleName(String cycleName) {
        this.cycleName = cycleName;
    }

    public String getCategory() {
        return category;
    }

    public void setCategory(String category) {
        this.category = category;
    }

    public String getAssignmentStatus() {
        return assignmentStatus;
    }

    public void setAssignmentStatus(String assignmentStatus) {
        this.assignmentStatus = assignmentStatus;
    }

    public Long getAssignmentId() {
        return assignmentId;
    }

    public void setAssignmentId(Long assignmentId) {
        this.assignmentId = assignmentId;
    }

    public Long getAssignedManagerId() {
        return assignedManagerId;
    }

    public void setAssignedManagerId(Long assignedManagerId) {
        this.assignedManagerId = assignedManagerId;
    }

    public String getAssignedManagerName() {
        return assignedManagerName;
    }

    public void setAssignedManagerName(String assignedManagerName) {
        this.assignedManagerName = assignedManagerName;
    }

    public String getBlockerReason() {
        return blockerReason;
    }

    public void setBlockerReason(String blockerReason) {
        this.blockerReason = blockerReason;
    }
}
