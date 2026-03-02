package com.eappraisal.identity.service;

import com.eappraisal.identity.entity.User;
import java.util.Optional;

public interface UserService {
    Optional<User> findByEmail(String email);
    User save(User user);
}
