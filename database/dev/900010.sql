--liquibase formatted sql

--changeset user:90010
--comment: insert sample data data

-- Insert sample contacts
INSERT INTO contacts
    (id, first_name, last_name, email, phone, company)
VALUES
    (1, 'John', 'Doe', 'john@doe@example.com', '123-456-7890', 'Doe Inc.'),
    (2, 'Jane', 'Smith', 'jane@example.com', '987-654-3210', 'Smith LLC'),
    (3, 'Alice', 'Johnson', 'Alice@johnson@example.com', '555-555-5555', 'Johnson & Co.'),
    (4, 'Bob', 'Brown', 'bob@brown@example.com', '333-333-333', 'Bob & Sons'),
    (5, 'Charlie', 'Davis', 'charlie@davis@example.com', '444-444-44', 'Charlie & Co.');

SELECT setval(pg_get_serial_sequence('public.contacts','id'), COALESCE((SELECT MAX(id) FROM public.contacts), 1), true);

-- Insert sample addresses
INSERT INTO addresses
    (id, address_line1, address_line2, address_line3, address_line4, postcode, country)
VALUES
    (1, '123 Main St', NULL, NULL, NULL, '10001', 'US'),
    (2, '456 Oak Ave', NULL, NULL, NULL, '20002', 'US'),
    (3, '789 Pine Rd', NULL, NULL, NULL, '30003', 'US'),
    (4, '321 Elm St', NULL, NULL, NULL, '40004', 'US'),
    (5, '654 Maple Ln', NULL, NULL, NULL, '50005', 'US');

SELECT setval(pg_get_serial_sequence('public.addresses','id'), COALESCE((SELECT MAX(id) FROM public.addresses), 1), true);

-- Map contacts to addresses (contact_addresses)
INSERT INTO contact_addresses
    (id, contact_id, address_id, address_type)
VALUES
    (1, 1, 1, 'Billing'),
    (2, 1, 1, 'Shipping'),
    (3, 2, 2, 'Billing'),
    (4, 2, 2, 'Shipping'),
    (5, 3, 2, 'Billing'),
    (6, 3, 3, 'Shipping'),
    (7, 4, 4, 'Billing'),
    (8, 5, 5, 'Shipping');

SELECT setval(pg_get_serial_sequence('public.contact_addresses','id'), COALESCE((SELECT MAX(id) FROM public.contact_addresses), 1), true);