package com.eappraisal.compensationctc.service.impl;

import com.eappraisal.compensationctc.dao.CtcSnapshotRepository;
import com.eappraisal.compensationctc.entity.CtcSnapshot;
import com.eappraisal.compensationctc.service.CtcService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Locale;

@Service
public class CtcServiceImpl implements CtcService {
    private final CtcSnapshotRepository ctcSnapshotRepository;
    private final JdbcTemplate jdbcTemplate;

    @Autowired
    public CtcServiceImpl(CtcSnapshotRepository ctcSnapshotRepository,
                          JdbcTemplate jdbcTemplate) {
        this.ctcSnapshotRepository = ctcSnapshotRepository;
        this.jdbcTemplate = jdbcTemplate;
    }

    @Override
    public List<CtcSnapshot> getCtcByAppraisalId(Long appraisalId) {
        return ctcSnapshotRepository.findByAppraisalId(appraisalId);
    }

    @Override
    public List<String> getSupportedCurrencies() {
        return jdbcTemplate.queryForList(
                "SELECT code FROM apps.ref_currency ORDER BY code",
                String.class
        );
    }

    @Override
    public CtcSnapshot save(CtcSnapshot ctcSnapshot) {
        // Business logic: set createdAt and validate CTC amount
        if (ctcSnapshot.getCreatedAt() == null) {
            ctcSnapshot.setCreatedAt(java.time.LocalDateTime.now());
        }
        if (ctcSnapshot.getCtcAmount() == null || ctcSnapshot.getCtcAmount() < 0) {
            throw new IllegalArgumentException("CTC amount must be non-negative");
        }

        String currency = ctcSnapshot.getCurrency();
        if (currency == null || currency.isBlank()) {
            currency = "INR";
        }
        currency = currency.trim().toUpperCase(Locale.ROOT);
        ctcSnapshot.setCurrency(currency);
        ensureCurrencyExists(currency);

        return ctcSnapshotRepository.save(ctcSnapshot);
    }

    private void ensureCurrencyExists(String currencyCode) {
        jdbcTemplate.update(
                "INSERT INTO apps.ref_currency(code) VALUES (?) ON CONFLICT (code) DO NOTHING",
                currencyCode
        );
    }
}
