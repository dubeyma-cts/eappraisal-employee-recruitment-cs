CREATE SCHEMA IF NOT EXISTS apps;

INSERT INTO apps.ref_currency (code)
VALUES
    ('INR'),
    ('USD'),
    ('EUR'),
    ('GBP'),
    ('AED'),
    ('SGD'),
    ('JPY'),
    ('AUD'),
    ('CAD'),
    ('CHF')
ON CONFLICT (code) DO NOTHING;
