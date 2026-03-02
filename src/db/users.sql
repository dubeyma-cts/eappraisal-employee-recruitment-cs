CREATE SCHEMA IF NOT EXISTS apps;

CREATE TABLE IF NOT EXISTS apps.org_unit (
  org_unit_id    BIGINT PRIMARY KEY,
  name           VARCHAR(200) NOT NULL,
  path           VARCHAR(400) NOT NULL UNIQUE,
  cost_center    VARCHAR(50)
);

INSERT INTO apps.org_unit (org_unit_id, name, path, cost_center)
VALUES (1, 'Corporate', '/Corporate', NULL)
ON CONFLICT (org_unit_id) DO NOTHING;

INSERT INTO apps.role (role_id, name) VALUES
  (1, 'HR'),
  (2, 'MANAGER'),
  (3, 'EMPLOYEE'),
  (4, 'ADMIN')
ON CONFLICT (role_id) DO NOTHING;

-- Example permissions (expand as needed based on requirements)
INSERT INTO apps.permission (permission_id, name) VALUES
  (1, 'VIEW_EMPLOYEE'),
  (2, 'EDIT_EMPLOYEE'),
  (3, 'APPRAISE_EMPLOYEE'),
  (4, 'VIEW_AUDIT'),
  (5, 'MANAGE_COMMENTS'),
  (6, 'VIEW_COMPENSATION'),
  (7, 'EDIT_COMPENSATION'),
  (8, 'MANAGE_IDENTITY')
ON CONFLICT (permission_id) DO NOTHING;

-- Role-Permission mapping
INSERT INTO apps.role_permission (role_id, permission_id) VALUES
  (1, 1), (1, 2), (1, 3), (1, 4), (1, 5), (1, 6), (1, 7), (1, 8), -- HR: all permissions
  (2, 1), (2, 3), (2, 4), (2, 5), (2, 6), -- MANAGER: view/appraise/audit/comments/comp
  (3, 1), (3, 5), (3, 6), -- EMPLOYEE: view/comments/comp
  (4, 1), (4, 2), (4, 3), (4, 4), (4, 5), (4, 6), (4, 7), (4, 8) -- ADMIN: all permissions
ON CONFLICT (role_id, permission_id) DO NOTHING;

-- Initial users
INSERT INTO apps."user" (given_name, family_name, work_email, password, status, org_unit_id, created_at) VALUES
  ('Alice', 'HR', 'alice.hr@eappraisal.com', '$2a$10$RHOJ5SWErJ/51yILT1kgKOqkyLdveJMul0loeAIxu6BV6BXX.tg1a', 'Active', 1, CURRENT_TIMESTAMP),
  ('Bob', 'Manager', 'bob.manager@eappraisal.com', '$2a$10$RHOJ5SWErJ/51yILT1kgKOqkyLdveJMul0loeAIxu6BV6BXX.tg1a', 'Active', 1, CURRENT_TIMESTAMP),
  ('Carol', 'Employee', 'carol.employee@eappraisal.com', '$2a$10$RHOJ5SWErJ/51yILT1kgKOqkyLdveJMul0loeAIxu6BV6BXX.tg1a', 'Active', 1, CURRENT_TIMESTAMP),
  ('Dave', 'Admin', 'dave.admin@eappraisal.com', '$2a$10$RHOJ5SWErJ/51yILT1kgKOqkyLdveJMul0loeAIxu6BV6BXX.tg1a', 'Active', 1, CURRENT_TIMESTAMP)
ON CONFLICT (work_email) DO NOTHING;

-- User-Role mapping
INSERT INTO apps.user_role (user_id, role_id, effective_from, effective_to)
SELECT v.user_id, v.role_id, CURRENT_TIMESTAMP, NULL
FROM (VALUES
  (1, 1), -- Alice: HR
  (2, 2), -- Bob: MANAGER
  (3, 3), -- Carol: EMPLOYEE
  (4, 4)  -- Dave: ADMIN
) AS v(user_id, role_id)
WHERE NOT EXISTS (
  SELECT 1
  FROM apps.user_role ur
  WHERE ur.user_id = v.user_id
    AND ur.role_id = v.role_id
    AND ur.effective_to IS NULL
);

  