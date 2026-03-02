package com.eappraisal.identity.entity;

import jakarta.persistence.*;
import java.util.Set;

@Entity
@Table(name = "role", schema = "apps")
public class Role {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long roleId;

    @Column(nullable = false, unique = true)
    private String name; // e.g., MANAGER, EMPLOYEE, HR, ADMIN

    @ManyToMany(mappedBy = "roles")
    private Set<User> users;

    // Getters and setters
    public Long getRoleId() { return roleId; }
    public void setRoleId(Long roleId) { this.roleId = roleId; }
    public String getName() { return name; }
    public void setName(String name) { this.name = name; }
    public Set<User> getUsers() { return users; }
    public void setUsers(Set<User> users) { this.users = users; }
}
