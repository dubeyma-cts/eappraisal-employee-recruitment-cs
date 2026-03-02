package com.eappraisal.appraisalworkflow.service;

import com.eappraisal.appraisalworkflow.entity.Appraisal;
import java.util.List;
import java.util.Optional;

public interface AppraisalService {
    Optional<Appraisal> findById(Long id);
    List<Appraisal> findAll();
    List<Appraisal> findByCycleId(Long cycleId);
    Optional<Appraisal> findByCycleIdAndSubjectUserId(Long cycleId, Long subjectUserId);
    Appraisal save(Appraisal appraisal);
}
