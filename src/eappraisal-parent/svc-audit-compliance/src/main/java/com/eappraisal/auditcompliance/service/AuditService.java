package com.eappraisal.auditcompliance.service;

import com.eappraisal.auditcompliance.entity.AuditEvent;
import java.util.List;

public interface AuditService {
    List<AuditEvent> getAuditEvents(String entityType, Long entityId);
    AuditEvent save(AuditEvent event);
}
