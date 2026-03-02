package com.eappraisal.compensationctc.aop;

import org.aspectj.lang.JoinPoint;
import org.aspectj.lang.annotation.AfterReturning;
import org.aspectj.lang.annotation.Aspect;
import org.springframework.stereotype.Component;

@Aspect
@Component
public class AuditAspect {
    @AfterReturning(pointcut = "execution(* com.eappraisal.compensationctc.service.CtcService.save(..))", returning = "result")
    public void triggerAuditEvent(JoinPoint joinPoint, Object result) {
        // Placeholder: Integrate with audit service
        // Example: auditService.recordEvent("CTC decision saved", result);
        System.out.println("Audit event: CTC decision saved for " + result);
    }
}
