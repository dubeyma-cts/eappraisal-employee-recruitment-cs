package com.eappraisal.employeemaster.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotNull;

import java.time.LocalDate;

public class EmployeeRequestDTO {
    @NotBlank(message = "Name is required")
    private String name;

    @Email(message = "Email should be valid")
    @NotBlank(message = "Email is required")
    private String email;

    private String orgUnitCode;

    private Long managerId;
    private String address;
    private String city;
    private String phone;
    private String mobile;
    private LocalDate dob;
    private String gender;
    private String maritalStatus;
    private LocalDate doj;
    private String passport;
    private String pan;
    private Integer workExperience;
    private Long reportsToId;
    @NotNull(message = "Department is required")
    private Long departmentId;

    // Getters and setters
    public String getName() { return name; }
    public void setName(String name) { this.name = name; }
    public String getEmail() { return email; }
    public void setEmail(String email) { this.email = email; }
    public String getOrgUnitCode() { return orgUnitCode; }
    public void setOrgUnitCode(String orgUnitCode) { this.orgUnitCode = orgUnitCode; }
    public Long getManagerId() { return managerId; }
    public void setManagerId(Long managerId) { this.managerId = managerId; }
    public String getAddress() { return address; }
    public void setAddress(String address) { this.address = address; }
    public String getCity() { return city; }
    public void setCity(String city) { this.city = city; }
    public String getPhone() { return phone; }
    public void setPhone(String phone) { this.phone = phone; }
    public String getMobile() { return mobile; }
    public void setMobile(String mobile) { this.mobile = mobile; }
    public LocalDate getDob() { return dob; }
    public void setDob(LocalDate dob) { this.dob = dob; }
    public String getGender() { return gender; }
    public void setGender(String gender) { this.gender = gender; }
    public String getMaritalStatus() { return maritalStatus; }
    public void setMaritalStatus(String maritalStatus) { this.maritalStatus = maritalStatus; }
    public LocalDate getDoj() { return doj; }
    public void setDoj(LocalDate doj) { this.doj = doj; }
    public String getPassport() { return passport; }
    public void setPassport(String passport) { this.passport = passport; }
    public String getPan() { return pan; }
    public void setPan(String pan) { this.pan = pan; }
    public Integer getWorkExperience() { return workExperience; }
    public void setWorkExperience(Integer workExperience) { this.workExperience = workExperience; }
    public Long getReportsToId() { return reportsToId; }
    public void setReportsToId(Long reportsToId) { this.reportsToId = reportsToId; }
    public Long getDepartmentId() { return departmentId; }
    public void setDepartmentId(Long departmentId) { this.departmentId = departmentId; }
}
