# e-Approval / e-Appraisal — Logical Data Model (LDM)

**Version:** 1.0  
**Generated:** 2026-02-27 13:16

This Logical Data Model translates the previously agreed **conceptual model** into a database-ready structure for an RDBMS (target: **Azure SQL Database**). It specifies **tables, columns, data types, keys, constraints, indexes**, and **reference data**. It is normalized to ~3NF with selective denormalization where performance warrants it.

> Conventions
> - Naming: snake_case table and column names; singular table names for entities.  
> - Keys: `pk_` for primary keys, `fk_` for foreign keys, `ux_` for unique constraints, `ix_` for non-unique indexes.  
> - Data types map to Azure SQL (INT, BIGINT, DECIMAL(18,2), VARCHAR(n), NVARCHAR(n), DATETIME2, BIT, VARBINARY, UNIQUEIDENTIFIER).  
> - All `created_at/updated_at` are `DATETIME2(3)` in UTC; `created_by/updated_by` reference `user.user_id` where applicable.

---

## 1) High-level ER (Mermaid)

```mermaid
erDiagram
  user ||--o{ user_manager : managed_by
  org_unit ||--o{ user : contains
  role ||--o{ user_role : assigned
  permission ||--o{ role_permission : granted

  appraisal_cycle ||--o{ appraisal : contains
  user ||--o{ appraisal : subject
  appraisal ||--o{ objective : has
  objective ||--o{ evidence : has
  appraisal ||--o{ comment : gets
  appraisal ||--o{ rating : gets
  appraisal ||--o{ stage_history : transitions
  appraisal ||--o{ ctc_snapshot : records

  appraisal ||--o{ unlock_request : may_have
  user ||--o{ notification : receives
  appraisal_cycle ||--o{ export_job : schedules

  audit_event }o--|| user : actor
  audit_event }o--|| appraisal : on
````

---


## 2) Table Specifications

### 2.1 user
| Column | Type | Null | Default | Notes |
|---|---|---|---|---|
| user_id | BIGINT IDENTITY(1,1) | NO |  | PK |
| given_name | NVARCHAR(100) | NO |  | PII |
| family_name | NVARCHAR(100) | NO |  | PII |
| work_email | NVARCHAR(256) | NO |  | PII, unique |
| employee_code | NVARCHAR(50) | YES |  | optional external code |
| status | VARCHAR(16) | NO | 'Active' | CHECK in ('Active','Inactive') |
| locale | VARCHAR(16) | YES |  |  |
| time_zone | VARCHAR(64) | YES |  | IANA label or Windows zone |
| org_unit_id | BIGINT | NO |  | FK -> org_unit.org_unit_id |
| created_at | DATETIME2(3) | NO | SYSUTCDATETIME() |  |
| created_by | BIGINT | YES |  | FK -> user.user_id |
| updated_at | DATETIME2(3) | YES |  |  |
| updated_by | BIGINT | YES |  | FK -> user.user_id |

- PK: `pk_user (user_id)`  
- UX: `ux_user_email (work_email)`  
- IX: `ix_user_org (org_unit_id)`

### 2.2 user_manager
| Column | Type | Null | Notes |
|---|---|---|---|
| user_id | BIGINT | NO | FK -> user.user_id |
| manager_id | BIGINT | NO | FK -> user.user_id |
| effective_from | DATETIME2(3) | NO | |
| effective_to | DATETIME2(3) | YES | null = open ended |

- PK: `pk_user_manager (user_id, effective_from)`  
- IX: `ix_user_manager_mgr (manager_id, effective_from DESC)`

### 2.3 org_unit
| Column | Type | Null | Notes |
|---|---|---|---|
| org_unit_id | BIGINT IDENTITY | NO | PK |
| name | NVARCHAR(200) | NO | |
| path | NVARCHAR(400) | NO | materialized lineage (e.g., /Company/BU/Dept) |
| cost_center | NVARCHAR(50) | YES | |

- UX: `ux_org_path (path)`

### 2.4 role, permission, user_role, role_permission
**role**: (role_id PK, name NVARCHAR(100) unique, description NVARCHAR(400))  
**permission**: (permission_id PK, name NVARCHAR(150) unique, scope NVARCHAR(100))  
**user_role**: (user_id FK, role_id FK, effective_from, effective_to; PK (user_id, role_id, effective_from))  
**role_permission**: (role_id FK, permission_id FK; PK (role_id, permission_id))

### 2.5 appraisal_cycle
| Column | Type | Null | Notes |
|---|---|---|---|
| cycle_id | BIGINT IDENTITY | NO | PK |
| name | NVARCHAR(120) | NO | '2026 Mid-Year' |
| start_date | DATE | NO | |
| end_date | DATE | NO | |
| state | VARCHAR(16) | NO | CHECK in ('Open','Frozen','Closed') |
| policy_version | NVARCHAR(50) | YES | |
| rating_scale_id | BIGINT | YES | reserved |
| created_at / created_by / updated_at / updated_by |  |  |  | standard audit |

- UX: `ux_cycle_name (name)`

### 2.6 appraisal
| Column | Type | Null | Notes |
|---|---|---|---|
| appraisal_id | BIGINT IDENTITY | NO | PK |
| cycle_id | BIGINT | NO | FK -> appraisal_cycle |
| subject_user_id | BIGINT | NO | FK -> user |
| manager_user_id | BIGINT | NO | FK -> user (current) |
| status | VARCHAR(16) | NO | CHECK in ('Draft','InReview','Final') |
| overall_rating | DECIMAL(5,2) | YES | nullable until final |
| finalised_at | DATETIME2(3) | YES | |
| locked_until | DATETIME2(3) | YES | set by unlock workflow |
| created_at / created_by / updated_at / updated_by |  |  |  | |

- UX: `ux_appraisal_subject_cycle (subject_user_id, cycle_id)`  
- IX: `ix_appraisal_cycle (cycle_id)`

### 2.7 objective
| Column | Type | Null | Notes |
|---|---|---|---|
| objective_id | BIGINT IDENTITY | NO | PK |
| appraisal_id | BIGINT | NO | FK -> appraisal |
| title | NVARCHAR(200) | NO | |
| description | NVARCHAR(MAX) | YES | |
| weight | DECIMAL(5,2) | YES | 0..100 |

- IX: `ix_objective_appraisal (appraisal_id)`

### 2.8 evidence
| Column | Type | Null | Notes |
|---|---|---|---|
| evidence_id | BIGINT IDENTITY | NO | PK |
| objective_id | BIGINT | NO | FK -> objective |
| file_name | NVARCHAR(260) | NO | |
| storage_uri | NVARCHAR(1024) | NO | SAS/signed URL |
| mime_type | NVARCHAR(100) | NO | |
| size_bytes | BIGINT | NO | |
| uploaded_at | DATETIME2(3) | NO | |

- IX: `ix_evidence_objective (objective_id)`

### 2.9 comment
| Column | Type | Null | Notes |
|---|---|---|---|
| comment_id | BIGINT IDENTITY | NO | PK |
| appraisal_id | BIGINT | NO | FK -> appraisal |
| author_user_id | BIGINT | NO | FK -> user |
| visibility | VARCHAR(10) | NO | CHECK in ('Mgr','HR','All') |
| body | NVARCHAR(MAX) | NO | |
| created_at | DATETIME2(3) | NO | |

- IX: `ix_comment_appraisal (appraisal_id, created_at)`

### 2.10 rating
| Column | Type | Null | Notes |
|---|---|---|---|
| rating_id | BIGINT IDENTITY | NO | PK |
| appraisal_id | BIGINT | NO | FK -> appraisal |
| dimension | NVARCHAR(100) | NO | KRA/Competency/etc |
| score | DECIMAL(6,3) | NO | |
| scale | DECIMAL(6,3) | YES | reference |
| rater_user_id | BIGINT | NO | FK -> user |
| created_at | DATETIME2(3) | NO | |

- IX: `ix_rating_appraisal (appraisal_id)`

### 2.11 stage_history
| Column | Type | Null | Notes |
|---|---|---|---|
| stage_id | BIGINT IDENTITY | NO | PK |
| appraisal_id | BIGINT | NO | FK -> appraisal |
| from_stage | VARCHAR(16) | NO | |
| to_stage | VARCHAR(16) | NO | |
| changed_by | BIGINT | NO | FK -> user |
| changed_at | DATETIME2(3) | NO | |
| reason | NVARCHAR(400) | YES | |

- IX: `ix_stage_appraisal (appraisal_id, changed_at)`

### 2.12 ctc_snapshot
| Column | Type | Null | Notes |
|---|---|---|---|
| ctc_id | BIGINT IDENTITY | NO | PK |
| appraisal_id | BIGINT | NO | FK -> appraisal |
| approver_user_id | BIGINT | NO | FK -> user |
| currency | CHAR(3) | NO | ISO 4217 |
| components_json | NVARCHAR(MAX) | NO | immutable blob |
| approved_at | DATETIME2(3) | NO | |

- IX: `ix_ctc_appraisal (appraisal_id)`

### 2.13 unlock_request
| Column | Type | Null | Notes |
|---|---|---|---|
| unlock_id | BIGINT IDENTITY | NO | PK |
| appraisal_id | BIGINT | NO | FK -> appraisal |
| raised_by | BIGINT | NO | FK -> user |
| reason | NVARCHAR(500) | NO | |
| status | VARCHAR(12) | NO | CHECK in ('Pending','Approved','Rejected') |
| processed_by | BIGINT | YES | FK -> user |
| processed_at | DATETIME2(3) | YES | |
| created_at | DATETIME2(3) | NO | |

- IX: `ix_unlock_appraisal (appraisal_id, created_at)`

### 2.14 notification (Outbox)
| Column | Type | Null | Notes |
|---|---|---|---|
| notification_id | BIGINT IDENTITY | NO | PK |
| recipient_user_id | BIGINT | NO | FK -> user |
| topic | NVARCHAR(120) | NO | |
| payload_json | NVARCHAR(MAX) | NO | |
| status | VARCHAR(10) | NO | CHECK in ('Queued','Sent','Failed') |
| about_appraisal_id | BIGINT | YES | FK -> appraisal |
| retry_count | INT | NO | 0..n |
| created_at | DATETIME2(3) | NO | |
| sent_at | DATETIME2(3) | YES | |

- IX: `ix_notification_status (status, created_at)`  
- IX: `ix_notification_recipient (recipient_user_id, status)`

### 2.15 export_job
| Column | Type | Null | Notes |
|---|---|---|---|
| export_job_id | BIGINT IDENTITY | NO | PK |
| cycle_id | BIGINT | NO | FK -> appraisal_cycle |
| format | VARCHAR(8) | NO | CHECK in ('CSV','XLSX','JSON') |
| filter_json | NVARCHAR(MAX) | YES | |
| created_by | BIGINT | NO | FK -> user |
| created_at | DATETIME2(3) | NO | |
| completed_at | DATETIME2(3) | YES | |
| storage_uri | NVARCHAR(1024) | YES | |

- IX: `ix_export_cycle (cycle_id, created_at DESC)`

### 2.16 audit_event (append-only)
| Column | Type | Null | Notes |
|---|---|---|---|
| audit_id | BIGINT IDENTITY | NO | PK |
| actor_user_id | BIGINT | YES | FK -> user (nullable for system) |
| entity_type | VARCHAR(24) | NO | e.g., 'APPRAISAL','OBJECTIVE' |
| entity_id | BIGINT | NO | |
| action | VARCHAR(24) | NO | e.g., 'CREATE','UPDATE','APPROVE' |
| at | DATETIME2(3) | NO | |
| ip_hash | VARBINARY(32) | YES | SHA-256 truncated |
| user_agent_hash | VARBINARY(32) | YES | |
| details_json | NVARCHAR(MAX) | YES | redacted payload |

- IX: `ix_audit_entity (entity_type, entity_id, at)`  
- IX: `ix_audit_actor (actor_user_id, at)`

---

## 3) Reference Data
- `ref_rating_scale (scale_id PK, name, min_score, max_score, step)`  
- `ref_stage (code PK, name, order_no, is_final BIT)`  
- `ref_visibility (code PK: 'Mgr','HR','All')`  
- `ref_currency (code PK CHAR(3))`

---

## 4) Integrity & Constraints
- **Row ownership & access:** enforce via application layer + database views that filter on subject/manager hierarchy; sensitive CTC rows protected by role-based predicates if Row-Level Security is enabled.  
- **Cascade rules:** deletes are **restricted** for core entities (appraisal, objective); use soft-delete only where explicitly required.  
- **Immutability:** `ctc_snapshot.components_json` is write-once after `approved_at`; enforce with a trigger or CHECK that prevents updates when approved_at IS NOT NULL.  
- **Audit:** only INSERTs into `audit_event`; forbid UPDATE/DELETE via permissions.

---

## 5) Indexing Strategy (examples)
- `appraisal`: `(cycle_id)` and `(subject_user_id, cycle_id)` unique to speed subject lookups in cycle.  
- `comment`: `(appraisal_id, created_at)` to support time-ordered conversations.  
- `notification`: composite indexes for `(recipient_user_id, status)` and `(status, created_at)` for outbox dispatch scans.  
- Covering indexes can be added from query plans during performance testing.

---

## 6) Views & Materializations
- **vw_appraisal_summary**: subject, manager, cycle, overall_rating, status, finalised_at.  
- **vw_ctc_secure**: exposes minimal CTC fields; restricted to Finance/HR.  
- **mv_reporting_appraisals** (optional): pre-aggregated facts for BI (cycle x org x rating bands). Materialize via pipeline if needed.

---

## 7) Migration & Seed Order (DDL execution)
1. Reference tables (ref_*)  
2. org_unit, user, role/permission joins  
3. appraisal_cycle, appraisal, objective, evidence  
4. comment, rating, stage_history  
5. ctc_snapshot, unlock_request  
6. notification, export_job  
7. audit_event  
8. indexes, views

---

## 8) Open Design Items
- Should evidence be **deduplicated** by content hash to reduce storage?  
- Add **surrogate natural keys** for `role` and `permission` (immutable codes) to simplify cross-env promotion.  
- Consider `ROWVERSION` columns on frequently updated tables for optimistic concurrency in API.

