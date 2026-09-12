"use client";

import { useEffect, useState } from "react";
import { CONTACTS_API_URL } from "@/app/lib/api-config";
import { type Contact } from "@/app/lib/contact-models";

export function useContactDetail(contactId: number) {
  const isValidId = Number.isFinite(contactId);
  const [contact, setContact] = useState<Contact | null>(null);
  const [isLoading, setIsLoading] = useState(isValidId);
  const [errorMessage, setErrorMessage] = useState<string | null>(
    isValidId ? null : "Invalid contact id."
  );

  useEffect(() => {
    if (!isValidId) {
      return;
    }

    let isCancelled = false;

    async function loadContact() {
      try {
        const response = await fetch(`${CONTACTS_API_URL}/${contactId}`, {
          cache: "no-store",
        });

        if (!response.ok) {
          if (response.status === 404) {
            if (!isCancelled) {
              setContact(null);
              setIsLoading(false);
            }
            return;
          }

          throw new Error("Failed to load the contact.");
        }

        const nextContact = (await response.json()) as Contact;

        if (!isCancelled) {
          setContact(nextContact);
          setErrorMessage(null);
          setIsLoading(false);
        }
      } catch {
        if (!isCancelled) {
          setErrorMessage("Unable to load this contact right now.");
          setIsLoading(false);
        }
      }
    }

    void loadContact();

    return () => {
      isCancelled = true;
    };
  }, [contactId, isValidId]);

  return {
    contact,
    isLoading,
    errorMessage,
  };
}
