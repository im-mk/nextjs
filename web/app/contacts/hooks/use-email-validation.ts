"use client";

import { CONTACTS_API_URL } from "@/app/lib/api-config";

export function useEmailValidation() {
  async function checkEmailExists(email: string, excludeContactId?: number) {
    try {
      const queryParams = new URLSearchParams({
        email: email.trim(),
      });
      if (excludeContactId) {
        queryParams.append("excludeContactId", excludeContactId.toString());
      }

      const response = await fetch(
        `${CONTACTS_API_URL}/check-email?${queryParams.toString()}`
      );

      if (response.ok) {
        const data = await response.json() as { exists: boolean };
        return data.exists;
      }

      return false;
    } catch (error) {
      console.error("Error checking email:", error);
      return false;
    }
  }

  return {
    checkEmailExists,
  };
}
