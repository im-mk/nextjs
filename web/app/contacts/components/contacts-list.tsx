"use client";

import type { FormEvent } from "react";
import Link from "next/link";

import { ContactModal } from "@/app/contacts/components/contact-modal";
import styles from "@/app/contacts/components/contacts-list.module.css";
import {
    formatTimestamp,
    type Contact,
    type ContactFormValues,
} from "@/app/lib/contact-models";
import { cn } from "@/lib/utils";
import { Button } from "@/shadcn/ui/button";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/shadcn/ui/table";

type ContactsPageViewProps = {
    contacts: Contact[];
    searchTerm?: string;
    onSearchChange?: (value: string) => void;
    createForm: ContactFormValues;
    editForm: ContactFormValues;
    editingId: number | null;
    activeModal: "create" | "edit" | null;
    isLoading: boolean;
    isSaving: boolean;
    errorMessage: string | null;
    successMessage: string | null;
    formErrorMessage: string | null;
    setCreateForm: React.Dispatch<React.SetStateAction<ContactFormValues>>;
    setEditForm: React.Dispatch<React.SetStateAction<ContactFormValues>>;
    onOpenCreateModal: () => void;
    onOpenEditModal: (contact: Contact) => void;
    onCloseModal: () => void;
    onRefresh: () => void;
    onDelete: (contact: Contact) => Promise<void>;
    onCreateSubmit: (event: FormEvent<HTMLFormElement>) => Promise<void>;
    onUpdateSubmit: (event: FormEvent<HTMLFormElement>) => Promise<void>;
};

export function ContactsPageView({
    contacts,
    searchTerm = "",
    onSearchChange,
    createForm,
    editForm,
    editingId,
    activeModal,
    isLoading,
    isSaving,
    errorMessage,
    successMessage,
    formErrorMessage,
    setCreateForm,
    setEditForm,
    onOpenCreateModal,
    onOpenEditModal,
    onCloseModal,
    onRefresh,
    onDelete,
    onCreateSubmit,
    onUpdateSubmit,
}: ContactsPageViewProps) {
    const statusLabel = isLoading ? "Refreshing" : isSaving ? "Saving" : "Ready";
    const selectedLabel = editingId ? "Editing contact" : "Create mode";

    return (
        <main className={styles.main}>
            <div className={styles.topGlow} />
            <div className={styles.container}>
                <section className={styles.hero}>
                    <div className={styles.heroText}>
                        <span className={styles.badge}>
                            Contact desk
                        </span>
                        <div className={styles.heroHeadingWrap}>
                            <h1 className={styles.heroTitle}>
                                Run the whole contacts workflow from one screen.
                            </h1>
                            <p className={styles.heroSubtitle}>
                                Fetch the latest records, add new contacts, edit existing ones,
                                and remove stale entries without leaving the page.
                            </p>
                        </div>
                    </div>

                    <div className={styles.metrics}>
                        <div className={cn(styles.metricCard, styles.metricDark)}>
                            <p className={cn(styles.metricLabel, styles.metricLabelMuted)}>
                                Contacts
                            </p>
                            <p className={styles.metricValueBig}>{contacts.length}</p>
                        </div>
                        <div className={cn(styles.metricCard, styles.metricTeal)}>
                            <p className={cn(styles.metricLabel, styles.metricLabelWhite)}>
                                Status
                            </p>
                            <p className={styles.metricValue}>{statusLabel}</p>
                        </div>
                        <div className={cn(styles.metricCard, styles.metricAmber)}>
                            <p className={cn(styles.metricLabel, styles.metricLabelStone)}>
                                Selected
                            </p>
                            <p className={styles.metricValue}>{selectedLabel}</p>
                        </div>
                    </div>
                </section>

                {(errorMessage || successMessage) && (
                    <section className={styles.messagePanel}>
                        {errorMessage && (
                            <p className={styles.errorMessage}>{errorMessage}</p>
                        )}
                        {successMessage && (
                            <p className={styles.successMessage}>{successMessage}</p>
                        )}
                    </section>
                )}

                <section className={styles.contentGrid}>
                    <section className={styles.directoryCard}>
                        <div className={styles.directoryHeader}>
                            <div>
                                <p className={styles.directoryEyebrow}>
                                    Directory
                                </p>
                                <h2 className={styles.directoryTitle}>
                                    Current contacts
                                </h2>
                            </div>
                            <div className={styles.toolbar}>
                                <p className={styles.toolbarText}>
                                    {isLoading ? "Loading contacts..." : `${contacts.length} loaded`}
                                </p>
                                <Button type="button" onClick={onOpenCreateModal} className={styles.newButton}>
                                    New contact
                                </Button>
                                <Button type="button" onClick={onRefresh} variant="outline" className={styles.refreshButton}>
                                    Refresh
                                </Button>
                            </div>
                        </div>

                        <div className="mb-4">
                            <label className="mb-2 block text-sm font-medium text-stone-700" htmlFor="contact-search">
                                Search contacts
                            </label>
                            <input
                                id="contact-search"
                                type="search"
                                value={searchTerm}
                                onChange={(event) => onSearchChange?.(event.target.value)}
                                placeholder="Search by name, email, company, or phone"
                                className="w-full rounded-xl border border-stone-300 bg-stone-50 px-3 py-2 text-sm text-stone-900 outline-none transition focus:border-stone-500"
                            />
                        </div>

                        {isLoading ? (
                            <div className={`${styles.emptyState} flex items-center justify-center gap-2`} role="status">
                                <span className="inline-block size-4 animate-spin rounded-full border-2 border-stone-400 border-t-transparent" />
                                Fetching the latest contacts from SQLite.
                            </div>
                        ) : contacts.length === 0 ? (
                            <div className={styles.emptyState}>
                                {searchTerm.trim()
                                    ? "No matching contacts found. Try a different search."
                                    : "No contacts yet. Create the first one from the directory toolbar."}
                            </div>
                        ) : (
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead>Contact</TableHead>
                                        <TableHead>Email</TableHead>
                                        <TableHead>Updated</TableHead>
                                        <TableHead className="text-right">Actions</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {contacts.map((contact) => (
                                        <TableRow key={contact.id}>
                                            <TableCell>
                                                <div className={styles.nameRow}>
                                                    <h3 className={styles.contactName}>
                                                        {contact.firstName} {contact.lastName}
                                                    </h3>
                                                    {contact.company && (
                                                        <span className={styles.companyBadge}>
                                                            {contact.company}
                                                        </span>
                                                    )}
                                                </div>
                                                <p className={styles.phoneText}>
                                                    {contact.phone ?? "No phone number"}
                                                </p>
                                            </TableCell>

                                            <TableCell className={styles.emailText}>
                                                {contact.email}
                                            </TableCell>

                                            <TableCell className={styles.updatedText}>
                                                Updated {formatTimestamp(contact.updatedAt)}
                                            </TableCell>

                                            <TableCell className="text-right">
                                                <div className={styles.actions}>
                                                    <Button asChild size="sm" variant="outline">
                                                        <Link href={`/contacts/${contact.id}`}>
                                                            View
                                                        </Link>
                                                    </Button>
                                                    <Button
                                                        type="button"
                                                        onClick={() => {
                                                            onOpenEditModal(contact);
                                                        }}
                                                        size="sm"
                                                        variant="outline"
                                                        className={styles.editButton}
                                                    >
                                                        Edit
                                                    </Button>
                                                    <Button
                                                        type="button"
                                                        onClick={() => {
                                                            void onDelete(contact);
                                                        }}
                                                        size="sm"
                                                        variant="outline"
                                                        className={styles.deleteButton}
                                                    >
                                                        Delete
                                                    </Button>
                                                </div>
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        )}
                    </section>
                </section>

                <ContactModal
                    isOpen={activeModal === "create"}
                    title="Add a new contact"
                    description="Create a contact record without leaving the directory view. Required fields are first name, last name, and email."
                    submitLabel="Create contact"
                    isCreate
                    values={createForm}
                    setValues={setCreateForm}
                    isSaving={isSaving}
                    apiError={formErrorMessage}
                    onSubmit={onCreateSubmit}
                    onClose={onCloseModal}
                />

                <ContactModal
                    isOpen={activeModal === "edit" && editingId !== null}
                    title="Edit contact"
                    description="Update the selected contact from a focused modal instead of editing inline in the grid."
                    submitLabel="Save changes"
                    values={editForm}
                    setValues={setEditForm}
                    editingId={editingId}
                    isSaving={isSaving}
                    apiError={formErrorMessage}
                    onSubmit={onUpdateSubmit}
                    onClose={onCloseModal}
                />
            </div>
        </main>
    );
}