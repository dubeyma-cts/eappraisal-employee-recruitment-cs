package com.eappraisal.auditcompliance.service;

import com.eappraisal.auditcompliance.dto.ReportRowResponseDTO;

import java.util.List;
import java.util.Set;

public interface ReportService {
    List<ReportRowResponseDTO> getUpcoming(Long userId, Set<String> roles);
    List<ReportRowResponseDTO> getInProcess(Long userId, Set<String> roles);
    List<ReportRowResponseDTO> getCompleted(Long userId, Set<String> roles);
}
