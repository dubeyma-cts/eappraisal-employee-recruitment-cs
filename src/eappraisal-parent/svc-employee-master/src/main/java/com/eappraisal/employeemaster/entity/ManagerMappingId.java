package com.eappraisal.employeemaster.entity;

import jakarta.persistence.Embeddable;
import java.io.Serializable;
import java.time.LocalDateTime;

@Embeddable
public class ManagerMappingId implements Serializable {
    private Long userId;
    private LocalDateTime effectiveFrom;

    public ManagerMappingId() {}

    public ManagerMappingId(Long userId, LocalDateTime effectiveFrom) {
        this.userId = userId;
        this.effectiveFrom = effectiveFrom;
    }

    public Long getUserId() {
        return userId;
    }

    public void setUserId(Long userId) {
        this.userId = userId;
    }

    public LocalDateTime getEffectiveFrom() {
        return effectiveFrom;
    }

    public void setEffectiveFrom(LocalDateTime effectiveFrom) {
        this.effectiveFrom = effectiveFrom;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        ManagerMappingId that = (ManagerMappingId) o;
        return userId.equals(that.userId) && effectiveFrom.equals(that.effectiveFrom);
    }

    @Override
    public int hashCode() {
        return java.util.Objects.hash(userId, effectiveFrom);
    }
}
