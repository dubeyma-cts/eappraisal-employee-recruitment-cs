package com.eappraisal.employeemaster.entity;

import jakarta.persistence.*;
import java.time.LocalDate;
import java.time.LocalDateTime;

@Entity
@Table(name = "\"user\"", schema = "apps")
public class Employee {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long userId;

    @Column(nullable = false, length = 100)
    private String givenName;

    @Column(nullable = false, length = 100)
    private String familyName;

    @Column(nullable = false, length = 256, unique = true)
    private String workEmail;

    @Column(length = 50)
    private String employeeCode;

    @Column(nullable = false, length = 16)
    private String status;

    @Column(length = 16)
    private String locale;

    @Column(length = 64)
    private String timeZone;

    @Column(nullable = false)
    private Long orgUnitId;

    @Column(length = 255)
    private String address;

    @Column(length = 100)
    private String city;

    @Column(length = 20)
    private String phone;

    @Column(length = 20)
    private String mobile;

    private LocalDate dob;

    @Column(length = 16)
    private String gender;

    @Column(length = 20)
    private String maritalStatus;

    private LocalDate doj;

    @Column(length = 32)
    private String passport;

    @Column(length = 32)
    private String pan;

    private Integer workExperience;

    private Long reportsToId;

    @Column(nullable = false)
    private LocalDateTime createdAt;

    private Long createdBy;
    private LocalDateTime updatedAt;
    private Long updatedBy;

    public Long getUserId() {
        return userId;
    }

    public void setUserId(Long userId) {
        this.userId = userId;
    }

    public String getGivenName() {
        return givenName;
    }

    public void setGivenName(String givenName) {
        this.givenName = givenName;
    }

    public String getFamilyName() {
        return familyName;
    }

    public void setFamilyName(String familyName) {
        this.familyName = familyName;
    }

    public String getWorkEmail() {
        return workEmail;
    }

    public void setWorkEmail(String workEmail) {
        this.workEmail = workEmail;
    }

    public String getEmployeeCode() {
        return employeeCode;
    }

    public void setEmployeeCode(String employeeCode) {
        this.employeeCode = employeeCode;
    }

    public String getStatus() {
        return status;
    }

    public void setStatus(String status) {
        this.status = status;
    }

    public String getLocale() {
        return locale;
    }

    public void setLocale(String locale) {
        this.locale = locale;
    }

    public String getTimeZone() {
        return timeZone;
    }

    public void setTimeZone(String timeZone) {
        this.timeZone = timeZone;
    }

    public Long getOrgUnitId() {
        return orgUnitId;
    }

    public void setOrgUnitId(Long orgUnitId) {
        this.orgUnitId = orgUnitId;
    }

    public String getAddress() {
        return address;
    }

    public void setAddress(String address) {
        this.address = address;
    }

    public String getCity() {
        return city;
    }

    public void setCity(String city) {
        this.city = city;
    }

    public String getPhone() {
        return phone;
    }

    public void setPhone(String phone) {
        this.phone = phone;
    }

    public String getMobile() {
        return mobile;
    }

    public void setMobile(String mobile) {
        this.mobile = mobile;
    }

    public LocalDate getDob() {
        return dob;
    }

    public void setDob(LocalDate dob) {
        this.dob = dob;
    }

    public String getGender() {
        return gender;
    }

    public void setGender(String gender) {
        this.gender = gender;
    }

    public String getMaritalStatus() {
        return maritalStatus;
    }

    public void setMaritalStatus(String maritalStatus) {
        this.maritalStatus = maritalStatus;
    }

    public LocalDate getDoj() {
        return doj;
    }

    public void setDoj(LocalDate doj) {
        this.doj = doj;
    }

    public String getPassport() {
        return passport;
    }

    public void setPassport(String passport) {
        this.passport = passport;
    }

    public String getPan() {
        return pan;
    }

    public void setPan(String pan) {
        this.pan = pan;
    }

    public Integer getWorkExperience() {
        return workExperience;
    }

    public void setWorkExperience(Integer workExperience) {
        this.workExperience = workExperience;
    }

    public Long getReportsToId() {
        return reportsToId;
    }

    public void setReportsToId(Long reportsToId) {
        this.reportsToId = reportsToId;
    }

    public LocalDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(LocalDateTime createdAt) {
        this.createdAt = createdAt;
    }

    public Long getCreatedBy() {
        return createdBy;
    }

    public void setCreatedBy(Long createdBy) {
        this.createdBy = createdBy;
    }

    public LocalDateTime getUpdatedAt() {
        return updatedAt;
    }

    public void setUpdatedAt(LocalDateTime updatedAt) {
        this.updatedAt = updatedAt;
    }

    public Long getUpdatedBy() {
        return updatedBy;
    }

    public void setUpdatedBy(Long updatedBy) {
        this.updatedBy = updatedBy;
    }
}
