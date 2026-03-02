package com.eappraisal.appraisalworkflow.service.impl;

import com.eappraisal.appraisalworkflow.dao.AppraisalRepository;
import com.eappraisal.appraisalworkflow.entity.Appraisal;
import com.eappraisal.appraisalworkflow.service.AppraisalService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.util.List;
import java.util.Optional;

@Service
public class AppraisalServiceImpl implements AppraisalService {
    private final AppraisalRepository appraisalRepository;

    @Autowired
    public AppraisalServiceImpl(AppraisalRepository appraisalRepository) {
        this.appraisalRepository = appraisalRepository;
    }

    @Override
    public Optional<Appraisal> findById(Long id) {
        return appraisalRepository.findById(id);
    }

    @Override
    public List<Appraisal> findAll() {
        return appraisalRepository.findAll();
    }

    @Override
    public List<Appraisal> findByCycleId(Long cycleId) {
        return appraisalRepository.findByCycleId(cycleId);
    }

    @Override
    public Optional<Appraisal> findByCycleIdAndSubjectUserId(Long cycleId, Long subjectUserId) {
        return appraisalRepository.findByCycleIdAndSubjectUserId(cycleId, subjectUserId);
    }

    @Override
    public Appraisal save(Appraisal appraisal) {
        appraisal.setStatus(normalizeAppraisalStatus(appraisal.getStatus()));
        // Business logic: calculate overall rating if not set
        if (appraisal.getOverallRating() == null) {
            appraisal.setOverallRating(calculateDefaultRating(appraisal));
        }
        // Set createdAt if not set
        if (appraisal.getCreatedAt() == null) {
            appraisal.setCreatedAt(java.time.LocalDateTime.now());
        }
        return appraisalRepository.save(appraisal);
    }

    private String normalizeAppraisalStatus(String status) {
        if (status == null || status.isBlank()) {
            return "Draft";
        }

        String normalized = status.trim().toLowerCase().replace("_", "").replace(" ", "");
        if (normalized.equals("draft")) {
            return "Draft";
        }
        if (normalized.equals("inreview") || normalized.equals("inprogress") || normalized.equals("assigned")) {
            return "InReview";
        }
        if (normalized.equals("final") || normalized.equals("finalized") || normalized.equals("finalised")
                || normalized.equals("completed") || normalized.equals("closed")) {
            return "Final";
        }

        throw new IllegalArgumentException("Invalid appraisal status: " + status + ". Allowed values are Draft, InReview, Final.");
    }

    private BigDecimal calculateDefaultRating(Appraisal appraisal) {
        // Placeholder: implement actual rating logic
        return BigDecimal.valueOf(3.0d);
    }
}
