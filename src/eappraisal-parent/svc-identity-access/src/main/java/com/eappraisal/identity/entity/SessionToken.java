package com.eappraisal.identity.entity;

import jakarta.persistence.*;
import java.time.LocalDateTime;

@Entity
@Table(name = "session_token", schema = "apps")
public class SessionToken {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long sessionId;

    @Column(nullable = false)
    private Long userId;

    @Column(nullable = false, length = 512)
    private String token;

    @Column(nullable = false)
    private LocalDateTime issuedAt;

    @Column(nullable = false)
    private LocalDateTime expiresAt;

    @Column(length = 64)
    private String ipAddress;

    @Column(length = 256)
    private String userAgent;

    @Column(nullable = false)
    private Boolean revoked = false;

    private LocalDateTime revokedAt;

    // Getters and setters
}
