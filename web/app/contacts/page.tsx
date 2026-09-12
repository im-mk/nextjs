"use client";

import { useEffect, useRef, useState } from "react";
import { ContactsPageView } from "@/app/contacts/components/contacts-list";
import { useContactsPage } from "@/app/contacts/hooks/use-contacts-page";
import { useContactModal } from "@/app/contacts/hooks/use-contact-modal";

export default function ContactsPage() {
    const contactsPage = useContactsPage();
    const contactModal = useContactModal(() => contactsPage.refreshContacts());
    const didAutoOpenCreate = useRef(false);
    const [searchTerm, setSearchTerm] = useState("");

    const filteredContacts = contactsPage.contacts.filter((contact) => {
        const query = searchTerm.trim().toLowerCase();

        if (!query) {
            return true;
        }

        const searchTarget = [
            contact.firstName,
            contact.lastName,
            contact.email,
            contact.company ?? "",
            contact.phone ?? "",
        ]
            .join(" ")
            .toLowerCase();

        return searchTarget.includes(query);
    });

    useEffect(() => {
        if (didAutoOpenCreate.current) {
            return;
        }

        const searchParams = new URLSearchParams(window.location.search);

        if (searchParams.get("create") === "1") {
            contactsPage.openCreateModal();
            didAutoOpenCreate.current = true;
        }
    }, [contactsPage]);

    return (
        <ContactsPageView
            {...contactsPage}
            contacts={filteredContacts}
            searchTerm={searchTerm}
            onSearchChange={setSearchTerm}
            onRefresh={() => {
                void contactsPage.refreshContacts();
            }}
            onDelete={contactsPage.handleDelete}
            onCreateSubmit={(event) => contactModal.handleCreateSubmit(event, contactsPage.createForm, contactsPage.closeModal)}
            onUpdateSubmit={(event) => contactModal.handleUpdateSubmit(event, contactsPage.editForm, contactsPage.editingId, contactsPage.closeModal)}
            isSaving={contactModal.isSaving}
            formErrorMessage={contactModal.formError}
            onOpenCreateModal={contactsPage.openCreateModal}
            onOpenEditModal={contactsPage.openEditModal}
            onCloseModal={contactsPage.closeModal}
        />
    );
}
