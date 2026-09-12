"use client";

import { useEffect, useState } from "react";
import { CONTACTS_API_URL } from "@/app/lib/api-config";
import { type Contact } from "@/app/lib/contact-models";

export function useContactNames() {
  const [contactNamesById, setContactNamesById] = useState<Record<number, string>>({});

  useEffect(() => {
    let isCancelled = false;

    async function loadContacts() {
      try {
        const response = await fetch(CONTACTS_API_URL, { cache: "no-store" });

        if (!response.ok) {
          return;
        }

        const contacts = (await response.json()) as Contact[];

        if (isCancelled) {
          return;
        }

        setContactNamesById(
          Object.fromEntries(
            contacts.map((contact) => [contact.id, `${contact.firstName} ${contact.lastName}`]),
          ),
        );
      } catch {
        // Ignore contact lookup failures; the documents page still works without names.
      }
    }

    void loadContacts();

    return () => {
      isCancelled = true;
    };
  }, []);

  return {
    contactNamesById,
  };
}
