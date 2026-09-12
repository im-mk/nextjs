--liquibase formatted sql

--changeset user:00040
--comment: contact_addresses table joins contact with addresses.
CREATE TABLE IF NOT EXISTS public.contact_addresses
(
    id int NOT NULL,
    contact_id INT NOT NULL,
    address_id INT NOT NULL,
    address_type VARCHAR(50) NOT NULL, -- e.g., 'Billing', 'Shipping'
    CONSTRAINT pk_contact_addresses_id PRIMARY KEY (id),
    CONSTRAINT fk_contact_addresses_customer_id FOREIGN KEY (contact_id)
        REFERENCES public.contacts (id),
    CONSTRAINT fk_contact_addresses_address_id FOREIGN KEY (address_id)
        REFERENCES public.addresses (id)
);

--rollback DROP TABLE IF EXISTS public.contact_addresses;