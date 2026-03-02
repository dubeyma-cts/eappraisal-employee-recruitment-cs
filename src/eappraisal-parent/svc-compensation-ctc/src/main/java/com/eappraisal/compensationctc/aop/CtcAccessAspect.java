package com.eappraisal.compensationctc.aop;

import com.eappraisal.compensationctc.dto.CtcSnapshotRequestDTO;
import com.eappraisal.compensationctc.service.CtcAuthorizationService;
import jakarta.servlet.http.HttpServletRequest;
import org.aspectj.lang.ProceedingJoinPoint;
import org.aspectj.lang.annotation.Around;
import org.aspectj.lang.annotation.Aspect;
import org.springframework.stereotype.Component;
import org.springframework.web.server.ResponseStatusException;

import static org.springframework.http.HttpStatus.FORBIDDEN;

@Aspect
@Component
public class CtcAccessAspect {

    private final HttpServletRequest request;
    private final RequestUserContextResolver userContextResolver;
    private final CtcAuthorizationService authorizationService;

    public CtcAccessAspect(HttpServletRequest request,
                           RequestUserContextResolver userContextResolver,
                           CtcAuthorizationService authorizationService) {
        this.request = request;
        this.userContextResolver = userContextResolver;
        this.authorizationService = authorizationService;
    }

    @Around("@annotation(enforceCtcAccess)")
    public Object enforce(ProceedingJoinPoint joinPoint, EnforceCtcAccess enforceCtcAccess) throws Throwable {
        RequestUserContext user = userContextResolver.resolve(request);

        switch (enforceCtcAccess.action()) {
            case LIST_CURRENCIES -> authorizationService.assertCanListCurrencies(user.roles());
            case VIEW_CTC -> authorizationService.assertCanViewCtc(resolveAppraisalIdForView(joinPoint), user.userId(), user.roles());
            case CREATE_CTC -> authorizationService.assertCanCreateCtc(resolveAppraisalIdForCreate(joinPoint), user.userId(), user.roles());
            default -> throw new ResponseStatusException(FORBIDDEN, "Unsupported CTC access action");
        }

        return joinPoint.proceed();
    }

    private Long resolveAppraisalIdForView(ProceedingJoinPoint joinPoint) {
        for (Object arg : joinPoint.getArgs()) {
            if (arg instanceof Long value && value > 0) {
                return value;
            }
        }
        throw new ResponseStatusException(FORBIDDEN, "Unable to resolve appraisal id for CTC view");
    }

    private Long resolveAppraisalIdForCreate(ProceedingJoinPoint joinPoint) {
        for (Object arg : joinPoint.getArgs()) {
            if (arg instanceof CtcSnapshotRequestDTO requestDTO) {
                return requestDTO.getAppraisalId();
            }
        }
        throw new ResponseStatusException(FORBIDDEN, "Unable to resolve appraisal id for CTC create");
    }
}
