--liquibase formatted sql

--changeset user:00030
--comment: Contacts table for storing contact information

CREATE TABLE IF NOT EXISTS public.contacts
(
    id int NOT NULL,
    first_name VARCHAR(255) NOT NULL,
    last_name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    phone VARCHAR(20),
    company VARCHAR(255),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_contacts_id PRIMARY KEY (id)
);

CREATE INDEX idx_contacts_email ON public.contacts(email);
CREATE INDEX idx_contacts_created_at ON public.contacts(created_at DESC);

--rollback DROP INDEX IF EXISTS public.idx_contacts_created_at;
--rollback DROP INDEX IF EXISTS public.idx_contacts_email;
--rollback DROP TABLE IF EXISTS public.contacts;
