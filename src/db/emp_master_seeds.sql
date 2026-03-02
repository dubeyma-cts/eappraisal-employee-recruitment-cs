INSERT INTO apps.org_unit (name, path, cost_center)
VALUES ('Human Resources', '/Corporate/HR', 'CC-HR'),
       ('Engineering', '/Corporate/Engineering', 'CC-ENG'),
       ('Finance', '/Corporate/Finance', 'CC-FIN');

INSERT INTO apps."user" (
    given_name,
    family_name,
    work_email,
    employee_code,
    status,
    locale,
    time_zone,
    org_unit_id,
    created_at,
    address,
    city,
    phone,
    mobile,
    dob,
    gender,
    marital_status,
    doj,
    passport,
    pan,
    work_experience,
    reports_to_id
)
VALUES
    ('Anita', 'Sharma', 'anita.sharma@company.com', 'EMP1001', 'Active', 'en-IN', 'Asia/Kolkata', 1, CURRENT_TIMESTAMP(),
     'Sector 12, Plot 5', 'Bengaluru', '080-2001001', '9000001001', '1989-06-12', 'Female', 'Married', '2015-07-01', 'N1234567', 'ABCDE1234F', 11, NULL),
    ('Rahul', 'Verma', 'rahul.verma@company.com', 'EMP1002', 'Active', 'en-IN', 'Asia/Kolkata', 2, CURRENT_TIMESTAMP(),
     'MG Road 44', 'Bengaluru', '080-2001002', '9000001002', '1990-03-25', 'Male', 'Single', '2018-02-15', 'P7654321', 'PQRSX9876Z', 8, 1);
