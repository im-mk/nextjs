"use client";

import Link from "next/link";
import { useState } from "react";

import { DocumentsList } from "@/app/documents/components/documents-list";
import { useDocumentsPage } from "@/app/documents/hooks/use-documents-page";
import { useContactNames } from "@/app/documents/hooks/use-contact-names";
import styles from "@/app/documents/page.module.css";
import { Button } from "@/shadcn/ui/button";

export default function DocumentsPage() {
    const documentsPage = useDocumentsPage();
    const contactNames = useContactNames();
    const [searchTerm, setSearchTerm] = useState("");

    const filteredDocuments = documentsPage.documents.filter((document) => {
        const query = searchTerm.trim().toLowerCase();

        if (!query) {
            return true;
        }

        const fileNameMatch = document.fileName.toLowerCase().includes(query);
        const contactName = document.contactId !== null && document.contactId !== undefined
            ? contactNames.contactNamesById[document.contactId]?.toLowerCase() ?? ""
            : "";

        return fileNameMatch || contactName.includes(query);
    });

    return (
        <main className={styles.main}>
            <section className={styles.shell}>
                <div className={styles.hero}>
                    <p className={styles.eyebrow}>Documents</p>
                    <h1 className={styles.title}>Manage files and references</h1>
                    <p className={styles.description}>
                        Centralize contracts, proposals, and supporting files so each contact record can point to the right source documents.
                    </p>

                    <div className={styles.actions}>
                        <Button
                            className={styles.primaryButton}
                            onClick={documentsPage.openFilePicker}
                            disabled={documentsPage.isUploading}
                        >
                            {documentsPage.isUploading ? "Uploading..." : "Upload document"}
                        </Button>
                        <Button asChild variant="outline">
                            <Link href="/contacts">View related contacts</Link>
                        </Button>
                    </div>
                </div>

                <DocumentsList
                    documents={filteredDocuments}
                    contactNamesById={contactNames.contactNamesById}
                    searchTerm={searchTerm}
                    onSearchChange={setSearchTerm}
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
            </section>
        </main>
    );
}

