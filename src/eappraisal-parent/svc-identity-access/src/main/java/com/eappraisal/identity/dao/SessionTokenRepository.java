package com.eappraisal.identity.dao;

import com.eappraisal.identity.entity.SessionToken;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface SessionTokenRepository extends JpaRepository<SessionToken, Long> {
    List<SessionToken> findByUserIdAndRevokedFalse(Long userId);
}
