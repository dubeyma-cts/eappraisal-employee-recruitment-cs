package com.eappraisal.commentsfeedback.aop;

import org.aspectj.lang.JoinPoint;
import org.aspectj.lang.annotation.AfterReturning;
import org.aspectj.lang.annotation.Aspect;
import org.springframework.stereotype.Component;

@Aspect
@Component
public class AuditAspect {
    @AfterReturning(pointcut = "execution(* com.eappraisal.commentsfeedback.service.CommentService.save(..))", returning = "result")
    public void triggerAuditEvent(JoinPoint joinPoint, Object result) {
        // Placeholder: Integrate with audit service
        // Example: auditService.recordEvent("Comment saved", result);
        System.out.println("Audit event: Comment saved for " + result);
    }
}
