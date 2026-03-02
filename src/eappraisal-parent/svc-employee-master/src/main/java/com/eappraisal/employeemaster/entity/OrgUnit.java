package com.eappraisal.employeemaster.entity;

import jakarta.persistence.*;

@Entity
@Table(name = "org_unit", schema = "apps")
public class OrgUnit {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long orgUnitId;

    @Column(nullable = false, length = 200)
    private String name;

    @Column(nullable = false, length = 400, unique = true)
    private String path;

    @Column(length = 50)
    private String costCenter;

    public Long getOrgUnitId() {
        return orgUnitId;
    }

    public void setOrgUnitId(Long orgUnitId) {
        this.orgUnitId = orgUnitId;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getPath() {
        return path;
    }

    public void setPath(String path) {
        this.path = path;
    }

    public String getCostCenter() {
        return costCenter;
    }

    public void setCostCenter(String costCenter) {
        this.costCenter = costCenter;
    }
}
