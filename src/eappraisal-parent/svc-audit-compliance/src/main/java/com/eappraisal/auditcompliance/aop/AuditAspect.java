package com.eappraisal.auditcompliance.aop;

import org.aspectj.lang.JoinPoint;
import org.aspectj.lang.annotation.AfterReturning;
import org.aspectj.lang.annotation.Aspect;
import org.springframework.stereotype.Component;

@Aspect
@Component
public class AuditAspect {
    @AfterReturning(pointcut = "execution(* com.eappraisal.auditcompliance.service.AuditService.save(..))", returning = "result")
    public void triggerAuditEvent(JoinPoint joinPoint, Object result) {
        // Placeholder: Integrate with audit service
        // Example: auditService.recordEvent("Audit event saved", result);
        System.out.println("Audit event: Audit event saved for " + result);
    }
}
