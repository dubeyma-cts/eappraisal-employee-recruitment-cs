package com.eappraisal.compensationctc.controller;

import com.eappraisal.compensationctc.aop.CtcAccessAction;
import com.eappraisal.compensationctc.aop.EnforceCtcAccess;
import com.eappraisal.compensationctc.entity.CtcSnapshot;
import com.eappraisal.compensationctc.service.CtcService;
import com.eappraisal.compensationctc.dto.CtcSnapshotRequestDTO;
import com.eappraisal.compensationctc.dto.CtcSnapshotResponseDTO;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/v1/ctc/decisions")
@CrossOrigin(origins = "*")
public class CtcController {
    private final CtcService ctcService;

    @Autowired
    public CtcController(CtcService ctcService) {
        this.ctcService = ctcService;
    }


    @GetMapping("/appraisal/{appraisalId}")
    @EnforceCtcAccess(action = CtcAccessAction.VIEW_CTC)
    public ResponseEntity<List<CtcSnapshotResponseDTO>> getCtcByAppraisal(@PathVariable Long appraisalId) {
        List<CtcSnapshot> ctcList = ctcService.getCtcByAppraisalId(appraisalId);
        List<CtcSnapshotResponseDTO> dtos = ctcList.stream().map(ctc -> {
            CtcSnapshotResponseDTO dto = new CtcSnapshotResponseDTO();
            dto.setId(ctc.getId());
            dto.setEmployeeId(ctc.getEmployeeId());
            dto.setAppraisalId(ctc.getAppraisalId());
            dto.setCtcAmount(ctc.getCtcAmount());
            dto.setCreatedAt(ctc.getCreatedAt() != null ? ctc.getCreatedAt().toString() : null);
            return dto;
        }).toList();
        return ResponseEntity.ok(dtos);
    }

    @GetMapping("/currencies")
    @EnforceCtcAccess(action = CtcAccessAction.LIST_CURRENCIES)
    public ResponseEntity<List<String>> getSupportedCurrencies() {
        return ResponseEntity.ok(ctcService.getSupportedCurrencies());
    }

    @PostMapping
    @EnforceCtcAccess(action = CtcAccessAction.CREATE_CTC)
    public ResponseEntity<CtcSnapshotResponseDTO> createCtcDecision(@RequestBody CtcSnapshotRequestDTO request) {
        // Map DTO to entity
        CtcSnapshot ctc = new CtcSnapshot();
        ctc.setEmployeeId(request.getEmployeeId());
        ctc.setAppraisalId(request.getAppraisalId());
        ctc.setCtcAmount(request.getCtcAmount());
        ctc.setCurrency(request.getCurrency());
        ctc.setCreatedAt(java.time.LocalDateTime.now());
        CtcSnapshot saved = ctcService.save(ctc);
        // Map entity to response DTO
        CtcSnapshotResponseDTO dto = new CtcSnapshotResponseDTO();
        dto.setId(saved.getId());
        dto.setEmployeeId(saved.getEmployeeId());
        dto.setAppraisalId(saved.getAppraisalId());
        dto.setCtcAmount(saved.getCtcAmount());
        dto.setCreatedAt(saved.getCreatedAt() != null ? saved.getCreatedAt().toString() : null);
        return ResponseEntity.ok(dto);
    }
}
