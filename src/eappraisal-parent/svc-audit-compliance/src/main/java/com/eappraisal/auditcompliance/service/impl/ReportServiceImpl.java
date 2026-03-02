package com.eappraisal.auditcompliance.service.impl;

import com.eappraisal.auditcompliance.dto.ReportRowResponseDTO;
import com.eappraisal.auditcompliance.service.ReportService;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.stereotype.Service;
import org.springframework.web.server.ResponseStatusException;

import java.sql.Timestamp;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Set;

import static org.springframework.http.HttpStatus.FORBIDDEN;

@Service
public class ReportServiceImpl implements ReportService {

    private final JdbcTemplate jdbcTemplate;

    public ReportServiceImpl(JdbcTemplate jdbcTemplate) {
        this.jdbcTemplate = jdbcTemplate;
    }

    @Override
    public List<ReportRowResponseDTO> getUpcoming(Long userId, Set<String> roles) {
        String baseSql = """
                SELECT a.appraisal_id,
                       ac.name AS cycle_name,
                       a.subject_user_id AS employee_id,
                       CONCAT(su.given_name, ' ', su.family_name) AS employee_name,
                       a.manager_user_id AS manager_id,
                       CONCAT(mu.given_name, ' ', mu.family_name) AS manager_name,
                       a.status,
                       ac.end_date,
                       a.finalised_at,
                       NULL::numeric AS total_ctc,
                       NULL::text AS currency
                FROM apps.appraisal a
                JOIN apps.appraisal_cycle ac ON ac.cycle_id = a.cycle_id
                JOIN apps."user" su ON su.user_id = a.subject_user_id
                JOIN apps."user" mu ON mu.user_id = a.manager_user_id
                WHERE ac.state = 'Open' AND a.status IN ('Draft', 'InReview')
                """;
        return queryWithRoleFilter(baseSql, userId, roles, false);
    }

    @Override
    public List<ReportRowResponseDTO> getInProcess(Long userId, Set<String> roles) {
        String baseSql = """
                SELECT a.appraisal_id,
                       ac.name AS cycle_name,
                       a.subject_user_id AS employee_id,
                       CONCAT(su.given_name, ' ', su.family_name) AS employee_name,
                       a.manager_user_id AS manager_id,
                       CONCAT(mu.given_name, ' ', mu.family_name) AS manager_name,
                       a.status,
                       ac.end_date,
                       a.finalised_at,
                       NULL::numeric AS total_ctc,
                       NULL::text AS currency
                FROM apps.appraisal a
                JOIN apps.appraisal_cycle ac ON ac.cycle_id = a.cycle_id
                JOIN apps."user" su ON su.user_id = a.subject_user_id
                JOIN apps."user" mu ON mu.user_id = a.manager_user_id
                WHERE a.status = 'InReview'
                """;
        return queryWithRoleFilter(baseSql, userId, roles, false);
    }

    @Override
    public List<ReportRowResponseDTO> getCompleted(Long userId, Set<String> roles) {
        boolean includeCtc = hasAnyRole(roles, "HR", "FINANCE", "MANAGER");
        String ctcSelect = includeCtc
            ? "CAST(cs.components_json AS numeric) AS total_ctc, TRIM(cs.currency) AS currency\n"
            : "NULL::numeric AS total_ctc, NULL::text AS currency\n";

        String baseSql = """
                SELECT a.appraisal_id,
                       ac.name AS cycle_name,
                       a.subject_user_id AS employee_id,
                       CONCAT(su.given_name, ' ', su.family_name) AS employee_name,
                       a.manager_user_id AS manager_id,
                       CONCAT(mu.given_name, ' ', mu.family_name) AS manager_name,
                       a.status,
                       ac.end_date,
                       a.finalised_at,
            """ + ctcSelect + """
                FROM apps.appraisal a
                JOIN apps.appraisal_cycle ac ON ac.cycle_id = a.cycle_id
                JOIN apps."user" su ON su.user_id = a.subject_user_id
                JOIN apps."user" mu ON mu.user_id = a.manager_user_id
                LEFT JOIN apps.ctc_snapshot cs ON cs.appraisal_id = a.appraisal_id
                WHERE a.status = 'Final'
                """;
        return queryWithRoleFilter(baseSql, userId, roles, includeCtc);
    }

    private List<ReportRowResponseDTO> queryWithRoleFilter(String baseSql,
                                                           Long userId,
                                                           Set<String> roles,
                                                           boolean includeCtc) {
        if (hasAnyRole(roles, "HR", "FINANCE")) {
            return mapRows(jdbcTemplate.queryForList(baseSql + " ORDER BY a.appraisal_id DESC"), includeCtc);
        }

        if (roles.contains("MANAGER")) {
            return mapRows(
                    jdbcTemplate.queryForList(baseSql + " AND a.manager_user_id = ? ORDER BY a.appraisal_id DESC", userId),
                    includeCtc
            );
        }

        if (roles.contains("EMPLOYEE")) {
            return mapRows(
                    jdbcTemplate.queryForList(baseSql + " AND a.subject_user_id = ? ORDER BY a.appraisal_id DESC", userId),
                    false
            );
        }

        throw new ResponseStatusException(FORBIDDEN, "Role is not allowed to access reports");
    }

    private List<ReportRowResponseDTO> mapRows(List<Map<String, Object>> rows, boolean includeCtc) {
        List<ReportRowResponseDTO> result = new ArrayList<>();
        for (Map<String, Object> row : rows) {
            ReportRowResponseDTO dto = new ReportRowResponseDTO();
            dto.setAppraisalId(asLong(row.get("appraisal_id")));
            dto.setCycleName(asString(row.get("cycle_name")));
            dto.setEmployeeId(asLong(row.get("employee_id")));
            dto.setEmployeeName(asString(row.get("employee_name")));
            dto.setManagerId(asLong(row.get("manager_id")));
            dto.setManagerName(asString(row.get("manager_name")));
            dto.setStatus(asString(row.get("status")));
            dto.setCycleEndDate(asString(row.get("end_date")));
            dto.setFinalizedAt(asTimestampString(row.get("finalised_at")));
            if (includeCtc) {
                dto.setTotalCtc(asDouble(row.get("total_ctc")));
                dto.setCurrency(asString(row.get("currency")));
            }
            result.add(dto);
        }
        return result;
    }

    private boolean hasAnyRole(Set<String> roles, String... expected) {
        for (String role : expected) {
            if (roles.contains(role)) {
                return true;
            }
        }
        return false;
    }

    private Long asLong(Object value) {
        return value instanceof Number number ? number.longValue() : null;
    }

    private Double asDouble(Object value) {
        return value instanceof Number number ? number.doubleValue() : null;
    }

    private String asString(Object value) {
        return value == null ? null : String.valueOf(value);
    }

    private String asTimestampString(Object value) {
        if (value instanceof Timestamp ts) {
            return ts.toLocalDateTime().toString();
        }
        return asString(value);
    }
}
