package com.eappraisal.appraisalworkflow.dto;

public class FinalizeAppraisalRequestDTO {
    private Boolean promotionRecommended;
    private String nextAppraisalDate;

    public Boolean getPromotionRecommended() {
        return promotionRecommended;
    }

    public void setPromotionRecommended(Boolean promotionRecommended) {
        this.promotionRecommended = promotionRecommended;
    }

    public String getNextAppraisalDate() {
        return nextAppraisalDate;
    }

    public void setNextAppraisalDate(String nextAppraisalDate) {
        this.nextAppraisalDate = nextAppraisalDate;
    }
}
