package com.eappraisal.identity.controller;

// import removed: org.springframework.security.access.prepost.PreAuthorize
import com.eappraisal.identity.entity.User;
import com.eappraisal.identity.service.UserService;
import com.eappraisal.identityaccess.dto.UserRequestDTO;
import com.eappraisal.identityaccess.dto.UserResponseDTO;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import java.util.Optional;

@RequestMapping("/api/v1/identity/users")
@CrossOrigin(origins = "*")
public class UserController {
    private final UserService userService;

    @Autowired
    public UserController(UserService userService) {
        this.userService = userService;
    }


    @GetMapping("/{email}")
    public ResponseEntity<UserResponseDTO> getUserByEmail(@PathVariable("email") String email) {
        Optional<User> userOpt = userService.findByEmail(email);
        if (userOpt.isPresent()) {
            User user = userOpt.get();
            UserResponseDTO dto = new UserResponseDTO();
            dto.setId(user.getUserId());
            dto.setUsername(user.getGivenName() + " " + user.getFamilyName());
            dto.setEmail(user.getWorkEmail());
            dto.setRole("USER"); // Placeholder, update as needed
            return ResponseEntity.ok(dto);
        } else {
            return ResponseEntity.notFound().build();
        }
    }

    @PostMapping
    public ResponseEntity<UserResponseDTO> createUser(@RequestBody UserRequestDTO userRequest) {
        // Map DTO to entity
        User user = new User();
        user.setGivenName(userRequest.getUsername()); // Simplified mapping
        user.setWorkEmail(userRequest.getEmail());
        user.setStatus("ACTIVE");
        user.setCreatedAt(java.time.LocalDateTime.now());
        // Set other fields as needed
        User saved = userService.save(user);
        // Map entity to response DTO
        UserResponseDTO dto = new UserResponseDTO();
        dto.setId(saved.getUserId());
        dto.setUsername(saved.getGivenName());
        dto.setEmail(saved.getWorkEmail());
        dto.setRole("USER"); // Placeholder
        return ResponseEntity.ok(dto);
    }
}
