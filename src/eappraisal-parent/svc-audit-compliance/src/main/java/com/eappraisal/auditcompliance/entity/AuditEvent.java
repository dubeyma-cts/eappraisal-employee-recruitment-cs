package com.eappraisal.auditcompliance.entity;

import jakarta.persistence.*;
import java.time.LocalDateTime;

@Entity
@Table(name = "audit_event", schema = "apps")
public class AuditEvent {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long auditId;

    private Long actorUserId;

    @Column(nullable = false, length = 24)
    private String entityType;

    @Column(nullable = false)
    private Long entityId;

    @Column(nullable = false, length = 24)
    private String action;

    @Column(name = "at", nullable = false)
    private LocalDateTime at;

    @Column(columnDefinition = "VARBINARY(32)")
    private byte[] ipHash;

    @Column(columnDefinition = "VARBINARY(32)")
    private byte[] userAgentHash;

    @Column(columnDefinition = "NVARCHAR(MAX)")
    private String detailsJson;

    public Long getAuditId() {
        return auditId;
    }

    public void setAuditId(Long auditId) {
        this.auditId = auditId;
    }

    public Long getActorUserId() {
        return actorUserId;
    }

    public void setActorUserId(Long actorUserId) {
        this.actorUserId = actorUserId;
    }

    public String getEntityType() {
        return entityType;
    }

    public void setEntityType(String entityType) {
        this.entityType = entityType;
    }

    public Long getEntityId() {
        return entityId;
    }

    public void setEntityId(Long entityId) {
        this.entityId = entityId;
    }

    public String getAction() {
        return action;
    }

    public void setAction(String action) {
        this.action = action;
    }

    public LocalDateTime getAt() {
        return at;
    }

    public void setAt(LocalDateTime at) {
        this.at = at;
    }

    public byte[] getIpHash() {
        return ipHash;
    }

    public void setIpHash(byte[] ipHash) {
        this.ipHash = ipHash;
    }

    public byte[] getUserAgentHash() {
        return userAgentHash;
    }

    public void setUserAgentHash(byte[] userAgentHash) {
        this.userAgentHash = userAgentHash;
    }

    public String getDetailsJson() {
        return detailsJson;
    }

    public void setDetailsJson(String detailsJson) {
        this.detailsJson = detailsJson;
    }

    public Long getId() {
        return auditId;
    }

    public void setId(Long id) {
        this.auditId = id;
    }

    public String getEventType() {
        return action;
    }

    public void setEventType(String eventType) {
        this.action = eventType;
    }

    public Long getUserId() {
        return actorUserId;
    }

    public void setUserId(Long userId) {
        this.actorUserId = userId;
    }

    public String getDetails() {
        return detailsJson;
    }

    public void setDetails(String details) {
        this.detailsJson = details;
    }

    public LocalDateTime getCreatedAt() {
        return at;
    }

    public void setCreatedAt(LocalDateTime createdAt) {
        this.at = createdAt;
    }
}
