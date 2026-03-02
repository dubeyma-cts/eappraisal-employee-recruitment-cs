package com.eappraisal.auditcompliance.controller;

import com.eappraisal.auditcompliance.entity.AuditEvent;
import com.eappraisal.auditcompliance.service.AuditService;
import com.eappraisal.auditcompliance.dto.AuditEventRequestDTO;
import com.eappraisal.auditcompliance.dto.AuditEventResponseDTO;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@CrossOrigin(origins = "*")
@RequestMapping("/api/v1/audit/events")
public class AuditController {
    private final AuditService auditService;

    @Autowired
    public AuditController(AuditService auditService) {
        this.auditService = auditService;
    }


    @GetMapping("/{entityType}/{entityId}")
    public ResponseEntity<List<AuditEventResponseDTO>> getAuditEvents(@PathVariable String entityType, @PathVariable Long entityId) {
        List<AuditEvent> events = auditService.getAuditEvents(entityType, entityId);
        List<AuditEventResponseDTO> dtos = events.stream().map(event -> {
            AuditEventResponseDTO dto = new AuditEventResponseDTO();
            dto.setId(event.getId());
            dto.setEventType(event.getEventType());
            dto.setUserId(event.getUserId());
            dto.setDetails(event.getDetails());
            dto.setCreatedAt(event.getCreatedAt() != null ? event.getCreatedAt().toString() : null);
            return dto;
        }).toList();
        return ResponseEntity.ok(dtos);
    }

    @PostMapping
    public ResponseEntity<AuditEventResponseDTO> createAuditEvent(@RequestBody AuditEventRequestDTO request) {
        // Map DTO to entity
        AuditEvent event = new AuditEvent();
        event.setEventType(request.getEventType());
        event.setUserId(request.getUserId());
        event.setDetails(request.getDetails());
        event.setCreatedAt(java.time.LocalDateTime.now());
        AuditEvent saved = auditService.save(event);
        // Map entity to response DTO
        AuditEventResponseDTO dto = new AuditEventResponseDTO();
        dto.setId(saved.getId());
        dto.setEventType(saved.getEventType());
        dto.setUserId(saved.getUserId());
        dto.setDetails(saved.getDetails());
        dto.setCreatedAt(saved.getCreatedAt() != null ? saved.getCreatedAt().toString() : null);
        return ResponseEntity.ok(dto);
    }
}
