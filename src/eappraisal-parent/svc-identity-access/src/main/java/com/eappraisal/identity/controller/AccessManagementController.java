package com.eappraisal.identity.controller;

import com.eappraisal.identity.dao.RoleRepository;
import com.eappraisal.identity.dao.UserRepository;
import com.eappraisal.identity.dao.UserRoleRepository;
import com.eappraisal.identity.entity.Role;
import com.eappraisal.identity.entity.User;
import com.eappraisal.identityaccess.dto.AccessRoleDTO;
import com.eappraisal.identityaccess.dto.RoleAssignmentRequestDTO;
import com.eappraisal.identityaccess.dto.StatusUpdateRequestDTO;
import jakarta.transaction.Transactional;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDateTime;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/v1/identity/access")
@CrossOrigin(origins = "*")
public class AccessManagementController {
    private final UserRepository userRepository;
    private final RoleRepository roleRepository;
    private final UserRoleRepository userRoleRepository;

    @Autowired
    public AccessManagementController(UserRepository userRepository,
                                      RoleRepository roleRepository,
                                      UserRoleRepository userRoleRepository) {
        this.userRepository = userRepository;
        this.roleRepository = roleRepository;
        this.userRoleRepository = userRoleRepository;
    }

    @GetMapping("/roles")
    public ResponseEntity<List<AccessRoleDTO>> getAvailableRoles() {
        List<AccessRoleDTO> roles = roleRepository.findAll()
                .stream()
                .sorted((a, b) -> a.getName().compareToIgnoreCase(b.getName()))
                .map(this::mapRole)
                .toList();
        return ResponseEntity.ok(roles);
    }

    @GetMapping("/users/{userId}/roles")
    public ResponseEntity<Map<String, Object>> getAssignedRoles(@PathVariable("userId") Long userId) {
        if (userRepository.findById(userId).isEmpty()) {
            return ResponseEntity.notFound().build();
        }
        List<Long> roleIds = userRoleRepository.findActiveRoleIdsByUserId(userId);
        Map<String, Object> response = new LinkedHashMap<>();
        response.put("userId", userId);
        response.put("roleIds", roleIds);
        return ResponseEntity.ok(response);
    }

    @GetMapping("/users/statuses")
    public ResponseEntity<Map<Long, Boolean>> getUserStatuses(@RequestParam("userIds") List<Long> userIds) {
        if (userIds == null || userIds.isEmpty()) {
            return ResponseEntity.ok(Map.of());
        }

        Set<Long> requestedIds = userIds.stream()
                .filter(id -> id != null && id > 0)
                .collect(Collectors.toSet());

        Map<Long, Boolean> statusByUserId = userRepository.findAllById(requestedIds)
                .stream()
                .collect(Collectors.toMap(
                        User::getUserId,
                        user -> user.getStatus() != null && user.getStatus().equalsIgnoreCase("ACTIVE"),
                        (left, right) -> right,
                        LinkedHashMap::new
                ));

        requestedIds.forEach(id -> statusByUserId.putIfAbsent(id, false));
        return ResponseEntity.ok(statusByUserId);
    }

    @PutMapping("/users/{userId}/status")
    @Transactional
    public ResponseEntity<Map<String, Object>> updateUserStatus(@PathVariable("userId") Long userId,
                                                                 @RequestBody StatusUpdateRequestDTO request) {
        User user = userRepository.findById(userId).orElse(null);
        if (user == null) {
            return ResponseEntity.notFound().build();
        }
        boolean active = request.getActive() != null && request.getActive();
        user.setStatus(active ? "Active" : "Inactive");
        user.setUpdatedAt(LocalDateTime.now());
        userRepository.save(user);

        Map<String, Object> response = new LinkedHashMap<>();
        response.put("userId", user.getUserId());
        response.put("status", user.getStatus());
        return ResponseEntity.ok(response);
    }

    @PutMapping("/users/{userId}/roles")
    @Transactional
    public ResponseEntity<Map<String, Object>> assignRoles(@PathVariable("userId") Long userId,
                                                            @RequestBody RoleAssignmentRequestDTO request) {
        if (userRepository.findById(userId).isEmpty()) {
            return ResponseEntity.notFound().build();
        }

        List<Long> requestedRoleIds = request.getRoleIds() == null
                ? List.of()
                : request.getRoleIds().stream().filter(id -> id != null && id > 0).distinct().toList();

        if (!requestedRoleIds.isEmpty()) {
            Set<Long> validRoleIds = roleRepository.findAllById(requestedRoleIds)
                    .stream()
                    .map(Role::getRoleId)
                    .collect(Collectors.toSet());
            if (validRoleIds.size() != requestedRoleIds.size()) {
                return ResponseEntity.badRequest().body(Map.of("error", "One or more role IDs are invalid"));
            }
        }

        userRoleRepository.deactivateAllActiveRoles(userId);
        requestedRoleIds.forEach(roleId -> userRoleRepository.assignRoleIfMissing(userId, roleId));

        List<Long> roleIds = userRoleRepository.findActiveRoleIdsByUserId(userId);
        Map<String, Object> response = new LinkedHashMap<>();
        response.put("userId", userId);
        response.put("roleIds", roleIds);
        response.put("message", roleIds.isEmpty() ? "Roles revoked" : "Roles updated");
        return ResponseEntity.ok(response);
    }

    private AccessRoleDTO mapRole(Role role) {
        AccessRoleDTO dto = new AccessRoleDTO();
        dto.setId(role.getRoleId());
        dto.setName(role.getName());
        return dto;
    }
}
