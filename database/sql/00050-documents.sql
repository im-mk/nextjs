--liquibase formatted sql

--changeset user:00050
--comment: documents table stores metadata for files uploaded to object storage; the actual file bytes live in the bucket, not the database.
CREATE TABLE IF NOT EXISTS public.documents
(
    id int NOT NULL,
    contact_id INT NULL,
    file_name VARCHAR(255) NOT NULL,
    content_type VARCHAR(127) NOT NULL,
    size_bytes BIGINT NOT NULL,
    storage_key VARCHAR(512) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT now(),
    updated_at TIMESTAMP NOT NULL DEFAULT now(),
    CONSTRAINT pk_documents_id PRIMARY KEY (id),
    CONSTRAINT uq_documents_storage_key UNIQUE (storage_key),
    CONSTRAINT fk_documents_contact_id FOREIGN KEY (contact_id)
        REFERENCES public.contacts (id)
);

--rollback DROP TABLE IF EXISTS public.documents;
