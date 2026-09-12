"use client";

import Link from "next/link";
import type { ChangeEvent, RefObject } from "react";

import styles from "@/app/documents/page.module.css";
import { formatFileSize, type Document } from "@/app/lib/document-models";
import { formatTimestamp } from "@/app/lib/contact-models";
import { Button } from "@/shadcn/ui/button";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/shadcn/ui/table";

type DocumentsListProps = {
    documents: Document[];
    contactNamesById?: Record<number, string>;
    searchTerm?: string;
    onSearchChange?: (value: string) => void;
    showUploadButton?: boolean;
    isLoading: boolean;
    isUploading: boolean;
    errorMessage: string | null;
    successMessage: string | null;
    fileInputRef: RefObject<HTMLInputElement | null>;
    onUploadClick: () => void;
    onFileSelected: (event: ChangeEvent<HTMLInputElement>) => void;
    onDownload: (document: Document) => void;
    onDelete: (document: Document) => void;
};

export function DocumentsList({
    documents,
    contactNamesById = {},
    searchTerm = "",
    onSearchChange,
    showUploadButton = true,
    isLoading,
    isUploading,
    errorMessage,
    successMessage,
    fileInputRef,
    onUploadClick,
    onFileSelected,
    onDownload,
    onDelete,
}: DocumentsListProps) {
    return (
        <section className={styles.card}>
            <input
                ref={fileInputRef}
                type="file"
                className="hidden"
                onChange={onFileSelected}
            />

            <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
                <div>
                    <h2 className={styles.cardTitle}>Uploaded files</h2>
                    <p className="text-sm text-stone-500">
                        {isLoading ? "Loading documents..." : `${documents.length} loaded`}
                    </p>
                </div>
                {showUploadButton && (
                    <Button type="button" onClick={onUploadClick} disabled={isUploading}>
                        {isUploading ? "Uploading..." : "Upload document"}
                    </Button>
                )}
            </div>

            <div className="mb-4">
                <label className="mb-2 block text-sm font-medium text-stone-700" htmlFor="document-search">
                    Search documents
                </label>
                <input
                    id="document-search"
                    type="search"
                    value={searchTerm}
                    onChange={(event) => onSearchChange?.(event.target.value)}
                    placeholder="Search by file name or contact name"
                    className="w-full rounded-xl border border-stone-300 bg-stone-50 px-3 py-2 text-sm text-stone-900 outline-none ring-0 transition focus:border-stone-500"
                />
            </div>

            {(errorMessage || successMessage) && (
                <p
                    className={
                        errorMessage
                            ? "mb-4 text-sm font-medium text-red-700"
                            : "mb-4 text-sm font-medium text-emerald-700"
                    }
                >
                    {errorMessage ?? successMessage}
                </p>
            )}

            {isLoading ? (
                <p className="flex items-center gap-2 text-stone-500" role="status">
                    <span className="inline-block size-4 animate-spin rounded-full border-2 border-stone-400 border-t-transparent" />
                    Fetching the latest documents.
                </p>
            ) : documents.length === 0 ? (
                <p className="text-stone-500">
                    No documents yet. Upload the first one from the button above.
                </p>
            ) : (
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>File</TableHead>
                            <TableHead>Contact</TableHead>
                            <TableHead>Type</TableHead>
                            <TableHead>Size</TableHead>
                            <TableHead>Updated</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {documents.map((document) => (
                            <TableRow key={document.id}>
                                <TableCell className="font-medium text-stone-900">
                                    {document.fileName}
                                </TableCell>
                                <TableCell className="text-stone-600">
                                    {document.contactId !== null && document.contactId !== undefined ? (
                                        contactNamesById[document.contactId] ? (
                                            <Link
                                                href={`/contacts/${document.contactId}`}
                                                className="font-medium text-stone-900 underline decoration-stone-400 underline-offset-4 hover:text-stone-700"
                                            >
                                                {contactNamesById[document.contactId]}
                                            </Link>
                                        ) : (
                                            <span>Linked contact</span>
                                        )
                                    ) : (
                                        <span className="text-stone-400">Unassigned</span>
                                    )}
                                </TableCell>
                                <TableCell className="text-stone-600">
                                    {document.contentType}
                                </TableCell>
                                <TableCell className="text-stone-600">
                                    {formatFileSize(document.sizeBytes)}
                                </TableCell>
                                <TableCell className="text-stone-600">
                                    {formatTimestamp(document.updatedAt)}
                                </TableCell>
                                <TableCell className="text-right">
                                    <div className="flex justify-end gap-2">
                                        <Button
                                            type="button"
                                            size="sm"
                                            variant="outline"
                                            onClick={() => onDownload(document)}
                                        >
                                            Download
                                        </Button>
                                        <Button
                                            type="button"
                                            size="sm"
                                            variant="outline"
                                            onClick={() => onDelete(document)}
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
    );
}
