package com.eappraisal.identity.entity;

import jakarta.persistence.*;
import java.io.Serializable;

@Entity
@Table(name = "role_permission", schema = "apps")
@IdClass(RolePermissionId.class)
public class RolePermission implements Serializable {
    @Id
    @Column(name = "role_id")
    private Long roleId;

    @Id
    @Column(name = "permission_id")
    private Long permissionId;

    // Getters and setters
    public Long getRoleId() {
        return roleId;
    }
    public void setRoleId(Long roleId) {
        this.roleId = roleId;
    }
    public Long getPermissionId() {
        return permissionId;
    }
    public void setPermissionId(Long permissionId) {
        this.permissionId = permissionId;
    }
}
