package com.eappraisal.identity.dao;

import com.eappraisal.identity.entity.RolePermission;
import com.eappraisal.identity.entity.RolePermissionId;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface UserRoleRepository extends JpaRepository<RolePermission, RolePermissionId> {

    @Query(value = "SELECT role_id FROM apps.user_role WHERE user_id = :userId AND effective_to IS NULL", nativeQuery = true)
    List<Long> findActiveRoleIdsByUserId(@Param("userId") Long userId);

    @Modifying
    @Query(value = "UPDATE apps.user_role SET effective_to = CURRENT_TIMESTAMP WHERE user_id = :userId AND effective_to IS NULL", nativeQuery = true)
    int deactivateAllActiveRoles(@Param("userId") Long userId);

    @Modifying
    @Query(value = """
            INSERT INTO apps.user_role (user_id, role_id, effective_from, effective_to)
            SELECT :userId, :roleId, CURRENT_TIMESTAMP, NULL
            WHERE NOT EXISTS (
                SELECT 1 FROM apps.user_role
                WHERE user_id = :userId AND role_id = :roleId AND effective_to IS NULL
            )
            """, nativeQuery = true)
    int assignRoleIfMissing(@Param("userId") Long userId, @Param("roleId") Long roleId);
}
