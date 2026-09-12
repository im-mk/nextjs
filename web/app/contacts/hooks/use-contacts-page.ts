"use client";

import { useEffect, useState } from "react";

import {
  emptyContactForm,
  toFormValues,
  type Contact,
  type ContactFormValues,
} from "@/app/lib/contact-models";
import { CONTACTS_API_URL } from "@/app/lib/api-config";

type ActiveModal = "create" | "edit" | null;

export function useContactsPage() {
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [createForm, setCreateForm] = useState<ContactFormValues>(emptyContactForm);
  const [editForm, setEditForm] = useState<ContactFormValues>(emptyContactForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [activeModal, setActiveModal] = useState<ActiveModal>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  function resetMessages() {
    setErrorMessage(null);
    setSuccessMessage(null);
  }

  function resetForms() {
    setCreateForm(emptyContactForm);
    setEditForm(emptyContactForm);
  }

  function closeModal() {
    setActiveModal(null);
    setEditingId(null);
    resetForms();
  }

  async function fetchContacts() {
    const response = await fetch(CONTACTS_API_URL, {
      cache: "no-store",
    });

    if (!response.ok) {
      throw new Error("Failed to load contacts.");
    }

    return (await response.json()) as Contact[];
  }

  async function refreshContacts() {
    try {
      setIsLoading(true);
      const nextContacts = await fetchContacts();
      setContacts(nextContacts);
      setErrorMessage(null);
    } catch {
      setErrorMessage("Unable to load contacts right now.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    let isCancelled = false;

    async function loadInitialContacts() {
      try {
        const nextContacts = await fetchContacts();

        if (isCancelled) {
          return;
        }

        setContacts(nextContacts);
        setErrorMessage(null);
      } catch {
        if (!isCancelled) {
          setErrorMessage("Unable to load contacts right now.");
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false);
        }
      }
    }

    void loadInitialContacts();

    return () => {
      isCancelled = true;
    };
  }, []);

  function serializeContactValues(values: ContactFormValues) {
    const address = {
      addressLine1: values.addressLine1.trim(),
      addressLine2: values.addressLine2.trim(),
      addressLine3: values.addressLine3.trim(),
      addressLine4: values.addressLine4.trim(),
      postcode: values.postcode.trim(),
      country: values.country.trim() || "GB",
    };

    const hasAddress = Object.values(address).some((value) => value !== "");

    return {
      firstName: values.firstName.trim(),
      lastName: values.lastName.trim(),
      email: values.email.trim(),
      phone: values.phone.trim(),
      company: values.company.trim(),
      ...(hasAddress ? { address } : { address: null }),
    };
  }

  async function sendContactRequest(
    url: string,
    method: "POST" | "PATCH" | "DELETE",
    values?: ContactFormValues
  ) {
    const response = await fetch(url, {
      method,
      headers: values ? { "Content-Type": "application/json" } : undefined,
      body: values ? JSON.stringify(serializeContactValues(values)) : undefined,
    });

    if (response.ok) {
      try {
        return { error: null, data: (await response.json()) as Contact | null };
      } catch {
        return { error: null, data: null };
      }
    }

    const payload = (await response.json().catch(() => null)) as
      | { message?: string }
      | null;

    return {
      error: payload?.message ?? "The request could not be completed.",
      data: null,
    };
  }



  async function handleDelete(contact: Contact) {
    const confirmed = window.confirm(
      `Delete ${contact.firstName} ${contact.lastName}?`
    );

    if (!confirmed) {
      return;
    }

    resetMessages();
    setIsSaving(true);

    const result = await sendContactRequest(
      `${CONTACTS_API_URL}/${contact.id}`,
      "DELETE"
    );

    if (result.error) {
      setErrorMessage(result.error);
      setIsSaving(false);
      return;
    }

    if (editingId === contact.id) {
      setEditingId(null);
      setActiveModal(null);
      setEditForm(emptyContactForm);
    }

    setSuccessMessage("Contact deleted.");
    await refreshContacts();
    setIsSaving(false);
  }

  function openCreateModal() {
    resetMessages();
    setCreateForm(emptyContactForm);
    setActiveModal("create");
  }

  function openEditModal(contact: Contact) {
    setEditingId(contact.id);
    setEditForm(toFormValues(contact));
    setActiveModal("edit");
    resetMessages();
  }

  return {
    contacts,
    createForm,
    editForm,
    editingId,
    activeModal,
    isLoading,
    isSaving,
    errorMessage,
    successMessage,
    setCreateForm,
    setEditForm,
    closeModal,
    refreshContacts,
    handleDelete,
    openCreateModal,
    openEditModal,
  };
}