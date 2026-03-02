package com.eappraisal.compensationctc.service;

import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.stereotype.Service;
import org.springframework.web.server.ResponseStatusException;

import java.util.Locale;
import java.util.Map;
import java.util.Set;

import static org.springframework.http.HttpStatus.FORBIDDEN;
import static org.springframework.http.HttpStatus.NOT_FOUND;

@Service
public class CtcAuthorizationService {

    private final JdbcTemplate jdbcTemplate;

    public CtcAuthorizationService(JdbcTemplate jdbcTemplate) {
        this.jdbcTemplate = jdbcTemplate;
    }

    public void assertCanListCurrencies(Set<String> roles) {
        if (hasAnyRole(roles, "MANAGER", "HR", "FINANCE")) {
            return;
        }
        throw new ResponseStatusException(FORBIDDEN, "You are not allowed to access currencies");
    }

    public void assertCanCreateCtc(Long appraisalId, Long userId, Set<String> roles) {
        AppraisalAccess access = getAppraisalAccess(appraisalId);

        boolean isManager = roles.contains("MANAGER");
        boolean isAssignedManager = userId.equals(access.managerUserId());
        boolean isFinal = "FINAL".equalsIgnoreCase(access.status());

        if (!isManager || !isAssignedManager) {
            throw new ResponseStatusException(FORBIDDEN, "Only the assigned manager can submit CTC");
        }

        if (isFinal) {
            throw new ResponseStatusException(FORBIDDEN, "CTC cannot be updated after appraisal is finalized");
        }
    }

    public void assertCanViewCtc(Long appraisalId, Long userId, Set<String> roles) {
        AppraisalAccess access = getAppraisalAccess(appraisalId);

        boolean isAssignedManager = roles.contains("MANAGER") && userId.equals(access.managerUserId());
        if (isAssignedManager) {
            return;
        }

        boolean isHrOrFinance = hasAnyRole(roles, "HR", "FINANCE");
        boolean isFinal = "FINAL".equalsIgnoreCase(access.status());
        if (isHrOrFinance && isFinal) {
            return;
        }

        throw new ResponseStatusException(FORBIDDEN, "You are not allowed to view detailed CTC");
    }

    private AppraisalAccess getAppraisalAccess(Long appraisalId) {
        if (appraisalId == null || appraisalId <= 0) {
            throw new ResponseStatusException(FORBIDDEN, "Invalid appraisal id");
        }

        var rows = jdbcTemplate.queryForList(
                "SELECT manager_user_id, status FROM apps.appraisal WHERE appraisal_id = ?",
                appraisalId
        );

        if (rows.isEmpty()) {
            throw new ResponseStatusException(NOT_FOUND, "Appraisal not found");
        }

        Map<String, Object> row = rows.get(0);
        Number managerUserIdValue = (Number) row.get("manager_user_id");
        String status = row.get("status") == null ? "" : String.valueOf(row.get("status")).trim();

        if (managerUserIdValue == null) {
            throw new ResponseStatusException(FORBIDDEN, "Appraisal manager is not configured");
        }

        return new AppraisalAccess(managerUserIdValue.longValue(), status.toUpperCase(Locale.ROOT));
    }

    private boolean hasAnyRole(Set<String> roles, String... allowed) {
        for (String role : allowed) {
            if (roles.contains(role)) {
                return true;
            }
        }
        return false;
    }

    private record AppraisalAccess(Long managerUserId, String status) {
    }
}
