"use client";

import { useEffect, useState } from "react";

import { COUNTRIES_API_URL } from "@/app/lib/api-config";

type Country = {
  id: string;
  name: string;
};

type CountryOption = {
  label: string;
  value: string;
};

export function useCountries() {
  const [countryOptions, setCountryOptions] = useState<CountryOption[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isCancelled = false;

    async function loadCountries() {
      try {
        const response = await fetch(COUNTRIES_API_URL, { cache: "no-store" });

        if (!response.ok) {
          throw new Error("Failed to load countries.");
        }

        const countries = (await response.json()) as Country[];

        if (!isCancelled) {
          setCountryOptions(
            countries.map((country) => ({
              label: country.name,
              value: country.id,
            })),
          );
          setIsLoading(false);
        }
      } catch {
        if (!isCancelled) {
          setCountryOptions([]);
          setIsLoading(false);
        }
      }
    }

    void loadCountries();

    return () => {
      isCancelled = true;
    };
  }, []);

  return { countryOptions, isLoading };
}
