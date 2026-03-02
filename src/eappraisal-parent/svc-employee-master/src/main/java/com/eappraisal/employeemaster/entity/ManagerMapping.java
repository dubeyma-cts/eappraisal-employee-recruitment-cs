package com.eappraisal.employeemaster.entity;

import jakarta.persistence.*;
import java.time.LocalDateTime;

@Entity
@Table(name = "user_manager", schema = "apps")
public class ManagerMapping {
    @EmbeddedId
    private ManagerMappingId id;

    @Column(nullable = false)
    private Long managerId;

    @Column(nullable = false, insertable = false, updatable = false)
    private LocalDateTime effectiveFrom;

    private LocalDateTime effectiveTo;

    // Getters and setters
}

