--liquibase formatted sql

--changeset user:00020
--comment: addresses table contains address information.

CREATE TABLE IF NOT EXISTS public.addresses
(
    id int NOT NULL,
    address_line1 VARCHAR(255) NOT NULL,
    address_line2 VARCHAR(255),
    address_line3 VARCHAR(255),
    address_line4 VARCHAR(255),
    postcode VARCHAR(10) NOT NULL,
    country CHAR(2) DEFAULT 'GB' NOT NULL, 
    CONSTRAINT pk_addresses_id PRIMARY KEY (id),
    CONSTRAINT fk_addresses_countries_country FOREIGN KEY (country)
        REFERENCES public.countries (id)
);

--rollback DROP TABLE IF EXISTS public.addresses;