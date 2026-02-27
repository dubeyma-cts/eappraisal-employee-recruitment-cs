
/*
 e-Approval / e-Appraisal — DDL for Azure SQL Database
 Generated: 2026-02-27 13:25 UTC
 Notes:
   - Create a dedicated schema 'apps' for all objects
   - Use DATETIME2(3) (UTC) for timestamps
   - Enforce key constraints, basic CHECKs, and critical indexes
*/

-- =============================================
-- 0) SCHEMA
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'apps')
    EXEC('CREATE SCHEMA apps AUTHORIZATION dbo');
GO

-- =============================================
-- 1) REFERENCE TABLES
-- =============================================
CREATE TABLE apps.ref_rating_scale (
  scale_id       BIGINT IDENTITY(1,1) PRIMARY KEY,
  name           NVARCHAR(120) NOT NULL,
  min_score      DECIMAL(6,3) NOT NULL,
  max_score      DECIMAL(6,3) NOT NULL,
  step           DECIMAL(6,3) NOT NULL,
  CONSTRAINT ck_ref_rating_scale_range CHECK (max_score > min_score AND step > 0)
);
GO

CREATE TABLE apps.ref_stage (
  code           VARCHAR(16)  NOT NULL PRIMARY KEY,
  name           NVARCHAR(120) NOT NULL,
  order_no       INT NOT NULL,
  is_final       BIT NOT NULL DEFAULT(0)
);
GO

CREATE TABLE apps.ref_visibility (
  code           VARCHAR(10) NOT NULL PRIMARY KEY -- 'Mgr','HR','All'
);
GO

CREATE TABLE apps.ref_currency (
  code           CHAR(3) NOT NULL PRIMARY KEY -- ISO 4217
);
GO

-- =============================================
-- 2) ORG & SECURITY
-- =============================================
CREATE TABLE apps.org_unit (
  org_unit_id    BIGINT IDENTITY(1,1) CONSTRAINT pk_org_unit PRIMARY KEY,
  name           NVARCHAR(200) NOT NULL,
  path           NVARCHAR(400) NOT NULL UNIQUE,
  cost_center    NVARCHAR(50) NULL
);
GO

CREATE TABLE apps.[user] (
  user_id        BIGINT IDENTITY(1,1) CONSTRAINT pk_user PRIMARY KEY,
  given_name     NVARCHAR(100) NOT NULL,
  family_name    NVARCHAR(100) NOT NULL,
  work_email     NVARCHAR(256) NOT NULL CONSTRAINT ux_user_email UNIQUE,
  employee_code  NVARCHAR(50) NULL,
  status         VARCHAR(16) NOT NULL CONSTRAINT ck_user_status CHECK (status IN ('Active','Inactive')),
  locale         VARCHAR(16) NULL,
  time_zone      VARCHAR(64) NULL,
  org_unit_id    BIGINT NOT NULL,
  created_at     DATETIME2(3) NOT NULL CONSTRAINT df_user_created_at DEFAULT (SYSUTCDATETIME()),
  created_by     BIGINT NULL,
  updated_at     DATETIME2(3) NULL,
  updated_by     BIGINT NULL,
  CONSTRAINT fk_user_org_unit FOREIGN KEY (org_unit_id) REFERENCES apps.org_unit(org_unit_id)
);
GO

CREATE INDEX ix_user_org ON apps.[user](org_unit_id);
GO

CREATE TABLE apps.user_manager (
  user_id        BIGINT NOT NULL,
  manager_id     BIGINT NOT NULL,
  effective_from DATETIME2(3) NOT NULL,
  effective_to   DATETIME2(3) NULL,
  CONSTRAINT pk_user_manager PRIMARY KEY (user_id, effective_from),
  CONSTRAINT fk_um_user     FOREIGN KEY (user_id)    REFERENCES apps.[user](user_id),
  CONSTRAINT fk_um_manager  FOREIGN KEY (manager_id) REFERENCES apps.[user](user_id)
);
GO

CREATE TABLE apps.role (
  role_id        BIGINT IDENTITY(1,1) CONSTRAINT pk_role PRIMARY KEY,
  name           NVARCHAR(100) NOT NULL CONSTRAINT ux_role_name UNIQUE,
  description    NVARCHAR(400) NULL
);
GO

CREATE TABLE apps.permission (
  permission_id  BIGINT IDENTITY(1,1) CONSTRAINT pk_permission PRIMARY KEY,
  name           NVARCHAR(150) NOT NULL CONSTRAINT ux_permission_name UNIQUE,
  scope          NVARCHAR(100) NULL
);
GO

CREATE TABLE apps.user_role (
  user_id        BIGINT NOT NULL,
  role_id        BIGINT NOT NULL,
  effective_from DATETIME2(3) NOT NULL,
  effective_to   DATETIME2(3) NULL,
  CONSTRAINT pk_user_role PRIMARY KEY (user_id, role_id, effective_from),
  CONSTRAINT fk_ur_user FOREIGN KEY (user_id) REFERENCES apps.[user](user_id),
  CONSTRAINT fk_ur_role FOREIGN KEY (role_id) REFERENCES apps.role(role_id)
);
GO

CREATE TABLE apps.role_permission (
  role_id        BIGINT NOT NULL,
  permission_id  BIGINT NOT NULL,
  CONSTRAINT pk_role_permission PRIMARY KEY (role_id, permission_id),
  CONSTRAINT fk_rp_role       FOREIGN KEY (role_id)       REFERENCES apps.role(role_id),
  CONSTRAINT fk_rp_permission FOREIGN KEY (permission_id) REFERENCES apps.permission(permission_id)
);
GO

-- =============================================
-- 3) APPRAISAL CORE
-- =============================================
CREATE TABLE apps.appraisal_cycle (
  cycle_id       BIGINT IDENTITY(1,1) CONSTRAINT pk_appraisal_cycle PRIMARY KEY,
  name           NVARCHAR(120) NOT NULL,
  start_date     DATE NOT NULL,
  end_date       DATE NOT NULL,
  state          VARCHAR(16) NOT NULL CONSTRAINT ck_cycle_state CHECK (state IN ('Open','Frozen','Closed')),
  policy_version NVARCHAR(50) NULL,
  rating_scale_id BIGINT NULL,
  created_at     DATETIME2(3) NOT NULL CONSTRAINT df_cycle_created_at DEFAULT (SYSUTCDATETIME()),
  created_by     BIGINT NULL,
  updated_at     DATETIME2(3) NULL,
  updated_by     BIGINT NULL
);
GO

CREATE UNIQUE INDEX ux_cycle_name ON apps.appraisal_cycle(name);
GO

CREATE TABLE apps.appraisal (
  appraisal_id      BIGINT IDENTITY(1,1) CONSTRAINT pk_appraisal PRIMARY KEY,
  cycle_id          BIGINT NOT NULL,
  subject_user_id   BIGINT NOT NULL,
  manager_user_id   BIGINT NOT NULL,
  status            VARCHAR(16) NOT NULL CONSTRAINT ck_appraisal_status CHECK (status IN ('Draft','InReview','Final')),
  overall_rating    DECIMAL(5,2) NULL,
  finalised_at      DATETIME2(3) NULL,
  locked_until      DATETIME2(3) NULL,
  created_at        DATETIME2(3) NOT NULL CONSTRAINT df_appraisal_created_at DEFAULT (SYSUTCDATETIME()),
  created_by        BIGINT NULL,
  updated_at        DATETIME2(3) NULL,
  updated_by        BIGINT NULL,
  CONSTRAINT fk_appraisal_cycle  FOREIGN KEY (cycle_id)        REFERENCES apps.appraisal_cycle(cycle_id),
  CONSTRAINT fk_appraisal_subject FOREIGN KEY (subject_user_id) REFERENCES apps.[user](user_id),
  CONSTRAINT fk_appraisal_manager FOREIGN KEY (manager_user_id) REFERENCES apps.[user](user_id)
);
GO

CREATE UNIQUE INDEX ux_appraisal_subject_cycle ON apps.appraisal(subject_user_id, cycle_id);
CREATE INDEX ix_appraisal_cycle ON apps.appraisal(cycle_id);
GO

CREATE TABLE apps.objective (
  objective_id   BIGINT IDENTITY(1,1) CONSTRAINT pk_objective PRIMARY KEY,
  appraisal_id   BIGINT NOT NULL,
  title          NVARCHAR(200) NOT NULL,
  description    NVARCHAR(MAX) NULL,
  weight         DECIMAL(5,2) NULL,
  CONSTRAINT fk_objective_appraisal FOREIGN KEY (appraisal_id) REFERENCES apps.appraisal(appraisal_id)
);
GO

CREATE INDEX ix_objective_appraisal ON apps.objective(appraisal_id);
GO

CREATE TABLE apps.evidence (
  evidence_id    BIGINT IDENTITY(1,1) CONSTRAINT pk_evidence PRIMARY KEY,
  objective_id   BIGINT NOT NULL,
  file_name      NVARCHAR(260) NOT NULL,
  storage_uri    NVARCHAR(1024) NOT NULL,
  mime_type      NVARCHAR(100) NOT NULL,
  size_bytes     BIGINT NOT NULL,
  uploaded_at    DATETIME2(3) NOT NULL,
  CONSTRAINT fk_evidence_objective FOREIGN KEY (objective_id) REFERENCES apps.objective(objective_id)
);
GO

CREATE INDEX ix_evidence_objective ON apps.evidence(objective_id);
GO

CREATE TABLE apps.comment (
  comment_id     BIGINT IDENTITY(1,1) CONSTRAINT pk_comment PRIMARY KEY,
  appraisal_id   BIGINT NOT NULL,
  author_user_id BIGINT NOT NULL,
  visibility     VARCHAR(10) NOT NULL CONSTRAINT ck_comment_visibility CHECK (visibility IN ('Mgr','HR','All')),
  body           NVARCHAR(MAX) NOT NULL,
  created_at     DATETIME2(3) NOT NULL,
  CONSTRAINT fk_comment_appraisal FOREIGN KEY (appraisal_id) REFERENCES apps.appraisal(appraisal_id),
  CONSTRAINT fk_comment_author   FOREIGN KEY (author_user_id) REFERENCES apps.[user](user_id)
);
GO

CREATE INDEX ix_comment_appraisal ON apps.comment(appraisal_id, created_at);
GO

CREATE TABLE apps.rating (
  rating_id      BIGINT IDENTITY(1,1) CONSTRAINT pk_rating PRIMARY KEY,
  appraisal_id   BIGINT NOT NULL,
  dimension      NVARCHAR(100) NOT NULL,
  score          DECIMAL(6,3) NOT NULL,
  scale          DECIMAL(6,3) NULL,
  rater_user_id  BIGINT NOT NULL,
  created_at     DATETIME2(3) NOT NULL,
  CONSTRAINT fk_rating_appraisal FOREIGN KEY (appraisal_id) REFERENCES apps.appraisal(appraisal_id),
  CONSTRAINT fk_rating_rater     FOREIGN KEY (rater_user_id) REFERENCES apps.[user](user_id)
);
GO

CREATE INDEX ix_rating_appraisal ON apps.rating(appraisal_id);
GO

CREATE TABLE apps.stage_history (
  stage_id       BIGINT IDENTITY(1,1) CONSTRAINT pk_stage_history PRIMARY KEY,
  appraisal_id   BIGINT NOT NULL,
  from_stage     VARCHAR(16) NOT NULL,
  to_stage       VARCHAR(16) NOT NULL,
  changed_by     BIGINT NOT NULL,
  changed_at     DATETIME2(3) NOT NULL,
  reason         NVARCHAR(400) NULL,
  CONSTRAINT fk_stage_appraisal FOREIGN KEY (appraisal_id) REFERENCES apps.appraisal(appraisal_id),
  CONSTRAINT fk_stage_changed_by FOREIGN KEY (changed_by)   REFERENCES apps.[user](user_id)
);
GO

CREATE INDEX ix_stage_appraisal ON apps.stage_history(appraisal_id, changed_at);
GO

CREATE TABLE apps.ctc_snapshot (
  ctc_id            BIGINT IDENTITY(1,1) CONSTRAINT pk_ctc_snapshot PRIMARY KEY,
  appraisal_id      BIGINT NOT NULL,
  approver_user_id  BIGINT NOT NULL,
  currency          CHAR(3) NOT NULL,
  components_json   NVARCHAR(MAX) NOT NULL,
  approved_at       DATETIME2(3) NOT NULL,
  CONSTRAINT fk_ctc_appraisal FOREIGN KEY (appraisal_id) REFERENCES apps.appraisal(appraisal_id),
  CONSTRAINT fk_ctc_approver  FOREIGN KEY (approver_user_id) REFERENCES apps.[user](user_id),
  CONSTRAINT fk_ctc_currency  FOREIGN KEY (currency) REFERENCES apps.ref_currency(code)
);
GO

CREATE INDEX ix_ctc_appraisal ON apps.ctc_snapshot(appraisal_id);
GO

CREATE TABLE apps.unlock_request (
  unlock_id      BIGINT IDENTITY(1,1) CONSTRAINT pk_unlock_request PRIMARY KEY,
  appraisal_id   BIGINT NOT NULL,
  raised_by      BIGINT NOT NULL,
  reason         NVARCHAR(500) NOT NULL,
  status         VARCHAR(12) NOT NULL CONSTRAINT ck_unlock_status CHECK (status IN ('Pending','Approved','Rejected')),
  processed_by   BIGINT NULL,
  processed_at   DATETIME2(3) NULL,
  created_at     DATETIME2(3) NOT NULL,
  CONSTRAINT fk_unlock_appraisal FOREIGN KEY (appraisal_id) REFERENCES apps.appraisal(appraisal_id),
  CONSTRAINT fk_unlock_raiser   FOREIGN KEY (raised_by)    REFERENCES apps.[user](user_id),
  CONSTRAINT fk_unlock_processor FOREIGN KEY (processed_by) REFERENCES apps.[user](user_id)
);
GO

CREATE INDEX ix_unlock_appraisal ON apps.unlock_request(appraisal_id, created_at);
GO

-- =============================================
-- 4) OUTBOX / EXPORTS / AUDIT
-- =============================================
CREATE TABLE apps.notification (
  notification_id     BIGINT IDENTITY(1,1) CONSTRAINT pk_notification PRIMARY KEY,
  recipient_user_id   BIGINT NOT NULL,
  topic               NVARCHAR(120) NOT NULL,
  payload_json        NVARCHAR(MAX) NOT NULL,
  status              VARCHAR(10) NOT NULL CONSTRAINT ck_notification_status CHECK (status IN ('Queued','Sent','Failed')),
  about_appraisal_id  BIGINT NULL,
  retry_count         INT NOT NULL CONSTRAINT df_notification_retry DEFAULT(0),
  created_at          DATETIME2(3) NOT NULL,
  sent_at             DATETIME2(3) NULL,
  CONSTRAINT fk_notification_recipient FOREIGN KEY (recipient_user_id)  REFERENCES apps.[user](user_id),
  CONSTRAINT fk_notification_appraisal FOREIGN KEY (about_appraisal_id) REFERENCES apps.appraisal(appraisal_id)
);
GO

CREATE INDEX ix_notification_status ON apps.notification(status, created_at);
CREATE INDEX ix_notification_recipient ON apps.notification(recipient_user_id, status);
GO

CREATE TABLE apps.export_job (
  export_job_id   BIGINT IDENTITY(1,1) CONSTRAINT pk_export_job PRIMARY KEY,
  cycle_id        BIGINT NOT NULL,
  format          VARCHAR(8) NOT NULL CONSTRAINT ck_export_format CHECK (format IN ('CSV','XLSX','JSON')),
  filter_json     NVARCHAR(MAX) NULL,
  created_by      BIGINT NOT NULL,
  created_at      DATETIME2(3) NOT NULL,
  completed_at    DATETIME2(3) NULL,
  storage_uri     NVARCHAR(1024) NULL,
  CONSTRAINT fk_export_cycle FOREIGN KEY (cycle_id) REFERENCES apps.appraisal_cycle(cycle_id),
  CONSTRAINT fk_export_creator FOREIGN KEY (created_by) REFERENCES apps.[user](user_id)
);
GO

CREATE INDEX ix_export_cycle ON apps.export_job(cycle_id, created_at DESC);
GO

CREATE TABLE apps.audit_event (
  audit_id        BIGINT IDENTITY(1,1) CONSTRAINT pk_audit_event PRIMARY KEY,
  actor_user_id   BIGINT NULL,
  entity_type     VARCHAR(24) NOT NULL,
  entity_id       BIGINT NOT NULL,
  action          VARCHAR(24) NOT NULL,
  [at]            DATETIME2(3) NOT NULL,
  ip_hash         VARBINARY(32) NULL,
  user_agent_hash VARBINARY(32) NULL,
  details_json    NVARCHAR(MAX) NULL,
  CONSTRAINT fk_audit_actor FOREIGN KEY (actor_user_id) REFERENCES apps.[user](user_id)
);
GO

CREATE INDEX ix_audit_entity ON apps.audit_event(entity_type, entity_id, [at]);
CREATE INDEX ix_audit_actor  ON apps.audit_event(actor_user_id, [at]);
GO

-- =============================================
-- 5) OPTIONAL VIEWS (examples)
-- =============================================
CREATE OR ALTER VIEW apps.vw_appraisal_summary AS
SELECT a.appraisal_id, a.cycle_id, a.subject_user_id, a.manager_user_id,
       a.status, a.overall_rating, a.finalised_at
FROM apps.appraisal a;
GO

CREATE OR ALTER VIEW apps.vw_ctc_secure AS
SELECT c.ctc_id, c.appraisal_id, c.currency, c.approved_at
FROM apps.ctc_snapshot c;
GO

