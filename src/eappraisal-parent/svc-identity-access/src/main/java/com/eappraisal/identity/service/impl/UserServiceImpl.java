package com.eappraisal.identity.service.impl;

import com.eappraisal.identity.dao.UserRepository;
import com.eappraisal.identity.entity.User;
import com.eappraisal.identity.service.UserService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.Optional;

@Service
public class UserServiceImpl implements UserService {
    private final UserRepository userRepository;

    @Autowired
    public UserServiceImpl(UserRepository userRepository) {
        this.userRepository = userRepository;
    }

    @Override
    public Optional<User> findByEmail(String email) {
        return userRepository.findByWorkEmail(email);
    }

    @Override
    public User save(User user) {
        return userRepository.save(user);
    }
}
