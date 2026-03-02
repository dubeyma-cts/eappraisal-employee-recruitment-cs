package com.eappraisal.auditcompliance.service.impl;

import com.eappraisal.auditcompliance.dao.AuditEventRepository;
import com.eappraisal.auditcompliance.entity.AuditEvent;
import com.eappraisal.auditcompliance.service.AuditService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class AuditServiceImpl implements AuditService {
    private final AuditEventRepository auditEventRepository;

    @Autowired
    public AuditServiceImpl(AuditEventRepository auditEventRepository) {
        this.auditEventRepository = auditEventRepository;
    }

    @Override
    public List<AuditEvent> getAuditEvents(String entityType, Long entityId) {
        return auditEventRepository.findByEntityTypeAndEntityId(entityType, entityId);
    }

    @Override
    public AuditEvent save(AuditEvent event) {
        // Business logic: set createdAt
        if (event.getCreatedAt() == null) {
            event.setCreatedAt(java.time.LocalDateTime.now());
        }
        return auditEventRepository.save(event);
    }
}
