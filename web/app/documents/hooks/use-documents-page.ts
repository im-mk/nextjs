"use client";

import { useCallback, useEffect, useRef, useState } from "react";

import { DOCUMENTS_API_URL } from "@/app/lib/api-config";
import type {
  CreateUploadUrlResponse,
  DownloadUrlResponse,
  Document,
} from "@/app/lib/document-models";

export function useDocumentsPage(contactId?: number | null) {
  const [documents, setDocuments] = useState<Document[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isUploading, setIsUploading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);

  function resetMessages() {
    setErrorMessage(null);
    setSuccessMessage(null);
  }

  const fetchDocuments = useCallback(async () => {
    const url = new URL(DOCUMENTS_API_URL);

    if (typeof contactId === "number") {
      url.searchParams.set("contactId", String(contactId));
    }

    const response = await fetch(url.toString(), { cache: "no-store" });

    if (!response.ok) {
      throw new Error("Failed to load documents.");
    }

    return (await response.json()) as Document[];
  }, [contactId]);

  async function refreshDocuments() {
    try {
      setIsLoading(true);
      const nextDocuments = await fetchDocuments();
      setDocuments(nextDocuments);
      setErrorMessage(null);
    } catch {
      setErrorMessage("Unable to load documents right now.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    let isCancelled = false;

    async function loadInitialDocuments() {
      try {
        const nextDocuments = await fetchDocuments();

        if (isCancelled) {
          return;
        }

        setDocuments(nextDocuments);
        setErrorMessage(null);
      } catch {
        if (!isCancelled) {
          setErrorMessage("Unable to load documents right now.");
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false);
        }
      }
    }

    void loadInitialDocuments();

    return () => {
      isCancelled = true;
    };
  }, [fetchDocuments]);

  function openFilePicker() {
    resetMessages();
    fileInputRef.current?.click();
  }

  async function handleFileSelected(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    event.target.value = "";

    if (!file) {
      return;
    }

    resetMessages();
    setIsUploading(true);

    try {
      const uploadUrlResponse = await fetch(`${DOCUMENTS_API_URL}/upload-url`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          contactId,
          fileName: file.name,
          contentType: file.type || "application/octet-stream",
        }),
      });

      if (!uploadUrlResponse.ok) {
        throw new Error("Failed to prepare the upload.");
      }

      const { uploadUrl, storageKey } =
        (await uploadUrlResponse.json()) as CreateUploadUrlResponse;

      const putResponse = await fetch(uploadUrl, {
        method: "PUT",
        headers: { "Content-Type": file.type || "application/octet-stream" },
        body: file,
      });

      if (!putResponse.ok) {
        throw new Error("Failed to upload the file to storage.");
      }

      const completeResponse = await fetch(DOCUMENTS_API_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          contactId,
          fileName: file.name,
          contentType: file.type || "application/octet-stream",
          sizeBytes: file.size,
          storageKey,
        }),
      });

      if (!completeResponse.ok) {
        throw new Error("Failed to save the document record.");
      }

      setSuccessMessage("Document uploaded.");
      await refreshDocuments();
    } catch {
      setErrorMessage("Unable to upload the document right now.");
    } finally {
      setIsUploading(false);
    }
  }

  async function handleDownload(document: Document) {
    resetMessages();

    try {
      const response = await fetch(`${DOCUMENTS_API_URL}/${document.id}/download-url`);

      if (!response.ok) {
        throw new Error("Failed to create a download link.");
      }

      const { downloadUrl } = (await response.json()) as DownloadUrlResponse;
      window.open(downloadUrl, "_blank", "noopener,noreferrer");
    } catch {
      setErrorMessage("Unable to download this document right now.");
    }
  }

  async function handleDelete(document: Document) {
    const confirmed = window.confirm(`Delete ${document.fileName}?`);

    if (!confirmed) {
      return;
    }

    resetMessages();

    try {
      const response = await fetch(`${DOCUMENTS_API_URL}/${document.id}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error("Failed to delete the document.");
      }

      setSuccessMessage("Document deleted.");
      await refreshDocuments();
    } catch {
      setErrorMessage("Unable to delete this document right now.");
    }
  }

  return {
    documents,
    isLoading,
    isUploading,
    errorMessage,
    successMessage,
    fileInputRef,
    openFilePicker,
    handleFileSelected,
    handleDownload,
    handleDelete,
    refreshDocuments,
  };
}
