package com.eappraisal.identityaccess.dto;

import java.util.List;

public class RoleAssignmentRequestDTO {
    private List<Long> roleIds;

    public List<Long> getRoleIds() {
        return roleIds;
    }

    public void setRoleIds(List<Long> roleIds) {
        this.roleIds = roleIds;
    }
}
