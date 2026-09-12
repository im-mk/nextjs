"use client";

import { useEffect, useState } from "react";
import { CONTACTS_API_URL, DOCUMENTS_API_URL } from "@/app/lib/api-config";
import { type Document } from "@/app/lib/document-models";

export function useDashboardData() {
  const [totalContacts, setTotalContacts] = useState(0);
  const [documents, setDocuments] = useState<Document[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    let isCancelled = false;

    async function loadDashboardData() {
      try {
        const [contactsResponse, documentsResponse] = await Promise.all([
          fetch(CONTACTS_API_URL, { cache: "no-store" }),
          fetch(DOCUMENTS_API_URL, { cache: "no-store" }),
        ]);

        if (!contactsResponse.ok || !documentsResponse.ok) {
          throw new Error("Failed to load dashboard data.");
        }

        const [contacts, documents] = (await Promise.all([
          contactsResponse.json() as Promise<unknown[]>,
          documentsResponse.json() as Promise<Document[]>,
        ])) as [unknown[], Document[]];

        if (!isCancelled) {
          setTotalContacts(contacts.length);
          setDocuments(documents);
          setErrorMessage(null);
        }
      } catch {
        if (!isCancelled) {
          setErrorMessage("Unable to load dashboard data right now.");
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false);
        }
      }
    }

    void loadDashboardData();

    return () => {
      isCancelled = true;
    };
  }, []);

  return {
    totalContacts,
    documents,
    isLoading,
    errorMessage,
  };
}
