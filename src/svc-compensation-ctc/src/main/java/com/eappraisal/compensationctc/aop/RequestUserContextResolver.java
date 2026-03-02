package com.eappraisal.compensationctc.aop;

import jakarta.servlet.http.HttpServletRequest;
import org.springframework.stereotype.Component;
import org.springframework.web.server.ResponseStatusException;

import java.util.Arrays;
import java.util.Locale;
import java.util.Set;
import java.util.stream.Collectors;

import static org.springframework.http.HttpStatus.UNAUTHORIZED;

@Component
public class RequestUserContextResolver {

    private static final String USER_ID_HEADER = "X-User-Id";
    private static final String USER_ROLES_HEADER = "X-User-Roles";

    public RequestUserContext resolve(HttpServletRequest request) {
        String rawUserId = request.getHeader(USER_ID_HEADER);
        if (rawUserId == null || rawUserId.isBlank()) {
            throw new ResponseStatusException(UNAUTHORIZED, "Missing X-User-Id header");
        }

        Long userId;
        try {
            userId = Long.parseLong(rawUserId.trim());
        } catch (NumberFormatException ex) {
            throw new ResponseStatusException(UNAUTHORIZED, "Invalid X-User-Id header");
        }

        String rawRoles = request.getHeader(USER_ROLES_HEADER);
        if (rawRoles == null || rawRoles.isBlank()) {
            throw new ResponseStatusException(UNAUTHORIZED, "Missing X-User-Roles header");
        }

        Set<String> roles = Arrays.stream(rawRoles.split(","))
                .map(String::trim)
                .filter(role -> !role.isEmpty())
                .map(role -> role.toUpperCase(Locale.ROOT))
                .collect(Collectors.toSet());

        if (roles.isEmpty()) {
            throw new ResponseStatusException(UNAUTHORIZED, "Invalid X-User-Roles header");
        }

        return new RequestUserContext(userId, roles);
    }
}
