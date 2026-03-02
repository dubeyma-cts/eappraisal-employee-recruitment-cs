CREATE SCHEMA IF NOT EXISTS apps;

INSERT INTO apps.appraisal_cycle (
    cycle_id,
    name,
    start_date,
    end_date,
    state,
    policy_version,
    rating_scale_id,
    created_at,
    created_by,
    updated_at,
    updated_by
)
SELECT
    1,
    'FY-2026',
    DATE '2026-01-01',
    DATE '2026-12-31',
    'Open',
    NULL,
    NULL,
    CURRENT_TIMESTAMP,
    NULL,
    NULL,
    NULL
WHERE NOT EXISTS (
    SELECT 1 FROM apps.appraisal_cycle WHERE cycle_id = 1
);

INSERT INTO apps.appraisal (
    cycle_id,
    subject_user_id,
    manager_user_id,
    status,
    created_at,
    overall_rating,
    finalised_at,
    locked_until,
    created_by,
    updated_at,
    updated_by
)
SELECT
    1, 3, 2, 'Draft', CURRENT_TIMESTAMP, 3.00, NULL, NULL, NULL, NULL, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM apps.appraisal WHERE cycle_id = 1 AND subject_user_id = 3
)
AND EXISTS (
    SELECT 1 FROM apps.appraisal_cycle WHERE cycle_id = 1
)
AND EXISTS (
    SELECT 1 FROM apps."user" WHERE user_id = 3
)
AND EXISTS (
    SELECT 1 FROM apps."user" WHERE user_id = 2
);

INSERT INTO apps.appraisal (
    cycle_id,
    subject_user_id,
    manager_user_id,
    status,
    created_at,
    overall_rating,
    finalised_at,
    locked_until,
    created_by,
    updated_at,
    updated_by
)
SELECT
    1, 4, 2, 'InReview', CURRENT_TIMESTAMP, 3.20, NULL, NULL, NULL, NULL, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM apps.appraisal WHERE cycle_id = 1 AND subject_user_id = 4
)
AND EXISTS (
    SELECT 1 FROM apps.appraisal_cycle WHERE cycle_id = 1
)
AND EXISTS (
    SELECT 1 FROM apps."user" WHERE user_id = 4
)
AND EXISTS (
    SELECT 1 FROM apps."user" WHERE user_id = 2
);
