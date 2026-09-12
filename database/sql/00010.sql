--liquibase formatted sql

--changeset user:00010
--comment: Countries and their codes table

CREATE TABLE IF NOT EXISTS public.countries
(
    id VARCHAR(2) NOT NULL,
    name VARCHAR(100) NOT NULL,    
    CONSTRAINT pk_countries_code PRIMARY KEY (id)
);

COMMENT ON COLUMN public.countries.id 
IS 'ISO 3166-1 alpha-2 country code (e.g. US, DE, FR)';

--rollback DROP TABLE IF EXISTS public.countries;