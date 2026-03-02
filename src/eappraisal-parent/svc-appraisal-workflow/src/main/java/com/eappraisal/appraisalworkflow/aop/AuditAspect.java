package com.eappraisal.appraisalworkflow.aop;

import org.aspectj.lang.JoinPoint;
import org.aspectj.lang.annotation.AfterReturning;
import org.aspectj.lang.annotation.Aspect;
import org.springframework.stereotype.Component;

@Aspect
@Component
public class AuditAspect {
    @AfterReturning(pointcut = "execution(* com.eappraisal.appraisalworkflow.service.AppraisalService.save(..))", returning = "result")
    public void triggerAuditEvent(JoinPoint joinPoint, Object result) {
        // Placeholder: Integrate with audit service
        // Example: auditService.recordEvent("Appraisal saved", result);
        System.out.println("Audit event: Appraisal saved for " + result);
    }
}
