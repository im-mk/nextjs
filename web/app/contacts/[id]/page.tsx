"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useMemo, useState } from "react";

import { DocumentsList } from "@/app/documents/components/documents-list";
import { useDocumentsPage } from "@/app/documents/hooks/use-documents-page";
import { useContactDetail } from "@/app/contacts/hooks/use-contact-detail";
import { formatTimestamp } from "@/app/lib/contact-models";
import { type Document } from "@/app/lib/document-models";
import { Button } from "@/shadcn/ui/button";

export default function ContactDetailPage() {
    const params = useParams<{ id: string }>();
    const contactId = Number(params.id);
    const contactDetail = useContactDetail(contactId);
    const [documentSearchTerm, setDocumentSearchTerm] = useState("");

    const documentsPage = useDocumentsPage(contactId);

    const contactDocuments = useMemo(
        () =>
            documentsPage.documents.filter((document: Document) => {
                const matchesContact = document.contactId === contactId;
                const query = documentSearchTerm.trim().toLowerCase();

                if (!matchesContact) {
                    return false;
                }

                if (!query) {
                    return true;
                }

                return document.fileName.toLowerCase().includes(query);
            }),
        [documentsPage.documents, contactId, documentSearchTerm],
    );

    if (!Number.isFinite(contactId)) {
        return (
            <main className="min-h-screen bg-stone-100 px-6 py-12 text-stone-900">
                <div className="mx-auto max-w-5xl rounded-3xl bg-white p-8 shadow-sm ring-1 ring-stone-200">
                    <p className="text-sm font-semibold uppercase tracking-[0.2em] text-stone-500">Contact</p>
                    <h1 className="mt-4 text-3xl font-semibold">Invalid contact id</h1>
                    <p className="mt-3 text-stone-600">The requested contact link is not valid.</p>
                    <div className="mt-6">
                        <Button asChild variant="outline">
                            <Link href="/contacts">Back to contacts</Link>
                        </Button>
                    </div>
                </div>
            </main>
        );
    }

    if (contactDetail.isLoading) {
        return (
            <main className="min-h-screen bg-stone-100 px-6 py-12 text-stone-900">
                <div className="mx-auto max-w-5xl rounded-3xl bg-white p-8 shadow-sm ring-1 ring-stone-200">
                    <span className="flex items-center gap-2" role="status">
                        <span className="inline-block size-4 animate-spin rounded-full border-2 border-stone-400 border-t-transparent" />
                        Loading contact details...
                    </span>
                </div>
            </main>
        );
    }

    if (!contactDetail.contact) {
        return (
            <main className="min-h-screen bg-stone-100 px-6 py-12 text-stone-900">
                <div className="mx-auto max-w-5xl rounded-3xl bg-white p-8 shadow-sm ring-1 ring-stone-200">
                    <p className="text-sm font-semibold uppercase tracking-[0.2em] text-stone-500">Contact</p>
                    <h1 className="mt-4 text-3xl font-semibold">Contact not found</h1>
                    <p className="mt-3 text-stone-600">This contact may have been removed or the ID is invalid.</p>
                    <div className="mt-6">
                        <Button asChild variant="outline">
                            <Link href="/contacts">Back to contacts</Link>
                        </Button>
                    </div>
                </div>
            </main>
        );
    }

    const contact = contactDetail.contact;

    return (
        <main className="min-h-screen bg-stone-100 px-6 py-12 text-stone-900">
            <div className="mx-auto max-w-6xl space-y-6">
                <div className="rounded-3xl border border-stone-200 bg-white p-6 shadow-sm sm:p-8">
                    <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                        <div>
                            <p className="text-sm font-semibold uppercase tracking-[0.2em] text-stone-500">Contact profile</p>
                            <h1 className="mt-3 text-4xl font-semibold tracking-tight text-stone-900">
                                {contact.firstName} {contact.lastName}
                            </h1>
                            {contact.company && (
                                <p className="mt-2 text-lg text-stone-600">{contact.company}</p>
                            )}
                        </div>

                        <div className="flex gap-3">
                            <Button asChild variant="outline">
                                <Link href="/contacts">Back to contacts</Link>
                            </Button>
                        </div>
                    </div>

                    <div className="mt-8 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
                        <div className="rounded-2xl border border-stone-200 bg-stone-50 p-4">
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-stone-500">Email</p>
                            <p className="mt-2 break-all text-base font-medium text-stone-800">{contact.email}</p>
                        </div>
                        <div className="rounded-2xl border border-stone-200 bg-stone-50 p-4">
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-stone-500">Phone</p>
                            <p className="mt-2 text-base font-medium text-stone-800">{contact.phone ?? "No phone number"}</p>
                        </div>
                        <div className="rounded-2xl border border-stone-200 bg-stone-50 p-4">
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-stone-500">Created</p>
                            <p className="mt-2 text-base font-medium text-stone-800">{formatTimestamp(contact.createdAt)}</p>
                        </div>
                        <div className="rounded-2xl border border-stone-200 bg-stone-50 p-4">
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-stone-500">Updated</p>
                            <p className="mt-2 text-base font-medium text-stone-800">{formatTimestamp(contact.updatedAt)}</p>
                        </div>
                    </div>

                    {contact.address && (
                        <div className="mt-6 rounded-2xl border border-stone-200 bg-stone-50 p-4">
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-stone-500">Address</p>
                            <p className="mt-2 text-base font-medium text-stone-800">
                                {[
                                    contact.address.addressLine1,
                                    contact.address.addressLine2,
                                    contact.address.addressLine3,
                                    contact.address.addressLine4,
                                ]
                                    .filter(Boolean)
                                    .join(", ") || "No address line"}
                            </p>
                            <p className="mt-1 text-base text-stone-700">
                                {contact.address.postcode ?? ""} {contact.address.country ?? ""}
                            </p>
                        </div>
                    )}
                </div>

                <div className="rounded-3xl border border-stone-200 bg-white p-6 shadow-sm sm:p-8">
                    <div className="mb-4 flex items-center justify-between gap-3">
                        <div>
                            <p className="text-sm font-semibold uppercase tracking-[0.2em] text-stone-500">Documents</p>
                            <h2 className="mt-2 text-2xl font-semibold text-stone-900">Files linked to this contact</h2>
                        </div>
                        <Button
                            type="button"
                            onClick={() => documentsPage.openFilePicker()}
                            disabled={documentsPage.isUploading}
                        >
                            {documentsPage.isUploading ? "Uploading..." : "Upload document"}
                        </Button>
                    </div>

                    <DocumentsList
                        documents={contactDocuments}
                        searchTerm={documentSearchTerm}
                        onSearchChange={setDocumentSearchTerm}
                        showUploadButton={false}
                        isLoading={documentsPage.isLoading}
                        isUploading={documentsPage.isUploading}
                        errorMessage={documentsPage.errorMessage}
                        successMessage={documentsPage.successMessage}
                        fileInputRef={documentsPage.fileInputRef}
                        onUploadClick={documentsPage.openFilePicker}
                        onFileSelected={documentsPage.handleFileSelected}
                        onDownload={(document) => {
                            void documentsPage.handleDownload(document);
                        }}
                        onDelete={(document) => {
                            void documentsPage.handleDelete(document);
                        }}
                    />
                </div>
            </div>
        </main>
    );
}
