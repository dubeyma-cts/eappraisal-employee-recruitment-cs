package com.eappraisal.auditcompliance.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

public class AuditEventRequestDTO {
    @NotBlank(message = "Event type is required")
    private String eventType;

    @NotNull(message = "User ID is required")
    private Long userId;

    private String details;

    // Getters and setters
    public String getEventType() { return eventType; }
    public void setEventType(String eventType) { this.eventType = eventType; }
    public Long getUserId() { return userId; }
    public void setUserId(Long userId) { this.userId = userId; }
    public String getDetails() { return details; }
    public void setDetails(String details) { this.details = details; }
}
