package com.eappraisal.compensationctc.aop;

import java.util.Set;

public record RequestUserContext(Long userId, Set<String> roles) {
}
