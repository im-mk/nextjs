--liquibase formatted sql

--changeset user:80010
--comment: insert lookup data into countries and order statuses tables

-- Insert countries (ISO 3166-1 alpha-2 codes)
INSERT INTO countries (id, name)
VALUES
    ('US', 'United States'),
    ('GB', 'United Kingdom'),
    ('DE', 'Germany'),
    ('FR', 'France'),
    ('ES', 'Spain'),
    ('IT', 'Italy'),
    ('NL', 'Netherlands'),
    ('SE', 'Sweden'),
    ('NO', 'Norway'),
    ('JP', 'Japan');