"use client";

import type { FormEvent } from "react";
import { useState } from "react";
import { useRouter } from "next/navigation";

import {
  type Contact,
  type ContactFormValues,
} from "@/app/lib/contact-models";
import { CONTACTS_API_URL } from "@/app/lib/api-config";

export function useContactModal(onSuccess?: () => Promise<void>) {
  const router = useRouter();
  const [isSaving, setIsSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  function serializeContactValues(values: ContactFormValues) {
    const hasAddress = [
      values.addressLine1,
      values.addressLine2,
      values.addressLine3,
      values.addressLine4,
      values.postcode,
    ].some((value) => value.trim() !== "");

    const address = {
      addressLine1: values.addressLine1.trim(),
      addressLine2: values.addressLine2.trim(),
      addressLine3: values.addressLine3.trim(),
      addressLine4: values.addressLine4.trim(),
      postcode: values.postcode.trim(),
      country: values.country.trim() || "GB",
    };

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

  async function handleCreateSubmit(
    event: FormEvent<HTMLFormElement>,
    createForm: ContactFormValues,
    onModalClose: () => void
  ) {
    event.preventDefault();
    setFormError(null);
    setIsSaving(true);

    const result = await sendContactRequest(CONTACTS_API_URL, "POST", createForm);

    if (result.error) {
      setFormError(result.error);
      setIsSaving(false);
      return;
    }

    onModalClose();

    const createdContact = result.data;
    if (createdContact?.id) {
      router.push(`/contacts/${createdContact.id}`);
      setIsSaving(false);
      return;
    }

    await onSuccess?.();
    setIsSaving(false);
  }

  async function handleUpdateSubmit(
    event: FormEvent<HTMLFormElement>,
    editForm: ContactFormValues,
    editingId: number | null,
    onModalClose: () => void
  ) {
    event.preventDefault();

    if (!editingId) {
      return;
    }

    setFormError(null);
    setIsSaving(true);

    const result = await sendContactRequest(
      `${CONTACTS_API_URL}/${editingId}`,
      "PATCH",
      editForm
    );

    if (result.error) {
      setFormError(result.error);
      setIsSaving(false);
      return;
    }

    onModalClose();
    await onSuccess?.();
    setIsSaving(false);
  }

  function clearError() {
    setFormError(null);
  }

  return {
    isSaving,
    formError,
    handleCreateSubmit,
    handleUpdateSubmit,
    clearError,
  };
}
