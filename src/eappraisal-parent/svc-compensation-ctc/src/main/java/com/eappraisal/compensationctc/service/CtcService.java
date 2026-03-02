package com.eappraisal.compensationctc.service;

import com.eappraisal.compensationctc.entity.CtcSnapshot;
import java.util.List;

public interface CtcService {
    List<CtcSnapshot> getCtcByAppraisalId(Long appraisalId);
    List<String> getSupportedCurrencies();
    CtcSnapshot save(CtcSnapshot ctcSnapshot);
}
