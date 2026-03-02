package com.eappraisal.employeemaster.aop;

import org.aspectj.lang.JoinPoint;
import org.aspectj.lang.annotation.AfterReturning;
import org.aspectj.lang.annotation.Aspect;
import org.springframework.stereotype.Component;

@Aspect
@Component
public class AuditAspect {
    @AfterReturning(pointcut = "execution(* com.eappraisal.employeemaster.service.EmployeeService.save(..))", returning = "result")
    public void triggerAuditEvent(JoinPoint joinPoint, Object result) {
        // Placeholder: Integrate with audit service
        // Example: auditService.recordEvent("Employee saved", result);
        System.out.println("Audit event: Employee saved for " + result);
    }
}
