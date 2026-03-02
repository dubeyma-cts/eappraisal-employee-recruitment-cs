package com.eappraisal.auditcompliance.controller;

import com.eappraisal.auditcompliance.aop.RequestUserContext;
import com.eappraisal.auditcompliance.aop.RequestUserContextResolver;
import com.eappraisal.auditcompliance.dto.ReportRowResponseDTO;
import com.eappraisal.auditcompliance.service.ReportService;
import jakarta.servlet.http.HttpServletRequest;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.CrossOrigin;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;

@RestController
@RequestMapping("/api/v1/reports")
@CrossOrigin(origins = "*")
public class ReportController {

    private final ReportService reportService;
    private final RequestUserContextResolver contextResolver;

    public ReportController(ReportService reportService,
                            RequestUserContextResolver contextResolver) {
        this.reportService = reportService;
        this.contextResolver = contextResolver;
    }

    @GetMapping("/upcoming")
    public ResponseEntity<List<ReportRowResponseDTO>> upcoming(HttpServletRequest request) {
        RequestUserContext user = contextResolver.resolve(request);
        return ResponseEntity.ok(reportService.getUpcoming(user.userId(), user.roles()));
    }

    @GetMapping("/in-process")
    public ResponseEntity<List<ReportRowResponseDTO>> inProcess(HttpServletRequest request) {
        RequestUserContext user = contextResolver.resolve(request);
        return ResponseEntity.ok(reportService.getInProcess(user.userId(), user.roles()));
    }

    @GetMapping("/completed")
    public ResponseEntity<List<ReportRowResponseDTO>> completed(HttpServletRequest request) {
        RequestUserContext user = contextResolver.resolve(request);
        return ResponseEntity.ok(reportService.getCompleted(user.userId(), user.roles()));
    }
}
