package com.eappraisal.identity.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;
import org.mindrot.jbcrypt.BCrypt;
import java.util.HashMap;
import java.util.Map;
import java.time.LocalDateTime;
import com.eappraisal.identity.dao.UserRepository;

@RestController
@RequestMapping("/api/v1/identity/auth")
@CrossOrigin(origins = "*")
public class AuthController {
    @Autowired
    private UserRepository userRepository;
    // Use BCrypt directly for password verification

    @PostMapping("/login")
    public Map<String, String> login(@RequestParam(name = "username") String username, @RequestParam(name = "password") String password) {
        Map<String, String> response = new HashMap<>();
        userRepository.findByWorkEmail(username).ifPresentOrElse(user -> {
            String status = user.getStatus();
            boolean isActive = status != null && status.equalsIgnoreCase("ACTIVE");
            if (!isActive) {
                response.put("status", "failure");
                response.put("message", "User is inactive. Contact HR admin.");
                return;
            }
            // Use jBCrypt for password verification
            if (BCrypt.checkpw(password, user.getPassword())) {
                response.put("status", "success");
                response.put("userId", String.valueOf(user.getUserId()));
                String roles = user.getRoles() != null ? user.getRoles().stream().map(r -> r.getName()).reduce((a, b) -> a + "," + b).orElse("") : "";
                response.put("roles", roles);
            } else {
                response.put("status", "failure");
                response.put("message", "Invalid username or password");
            }
        }, () -> {
            response.put("status", "failure");
            response.put("message", "Invalid username or password");
        });
        return response;
    }

    @PostMapping("/reset-password")
    public Map<String, String> resetPassword(@RequestParam(name = "username") String username,
                                             @RequestParam(name = "newPassword") String newPassword) {
        Map<String, String> response = new HashMap<>();

        String safeUsername = username == null ? "" : username.trim();
        String safePassword = newPassword == null ? "" : newPassword.trim();

        if (safeUsername.isEmpty() || safePassword.isEmpty()) {
            response.put("status", "failure");
            response.put("message", "Username and new password are required");
            return response;
        }

        if (safePassword.length() < 6) {
            response.put("status", "failure");
            response.put("message", "Password must be at least 6 characters");
            return response;
        }

        userRepository.findByWorkEmail(safeUsername).ifPresentOrElse(user -> {
            user.setPassword(BCrypt.hashpw(safePassword, BCrypt.gensalt()));
            user.setUpdatedAt(LocalDateTime.now());
            userRepository.save(user);
            response.put("status", "success");
            response.put("message", "Password reset successful");
        }, () -> {
            response.put("status", "failure");
            response.put("message", "User not found");
        });

        return response;
    }
}
