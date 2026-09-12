"use client";

import Link from "next/link";
import { useMemo, useState } from "react";

import { useDashboardData } from "@/app/hooks/use-dashboard-data";
import styles from "@/app/page.module.css";
import { Button } from "@/shadcn/ui/button";

const periodOptions = [
  { key: "day", label: "Day" },
  { key: "month", label: "Month" },
  { key: "year", label: "Year" },
  { key: "all", label: "All time" },
] as const;

type PeriodKey = (typeof periodOptions)[number]["key"];

export default function Home() {
  const dashboardData = useDashboardData();
  const [selectedPeriod, setSelectedPeriod] = useState<PeriodKey>("month");

  const documentsInPeriod = useMemo(() => {
    if (selectedPeriod === "all") {
      return dashboardData.documents.length;
    }

    const cutoffMs = {
      day: 1000 * 60 * 60 * 24,
      month: 1000 * 60 * 60 * 24 * 30,
      year: 1000 * 60 * 60 * 24 * 365,
    }[selectedPeriod];

    // eslint-disable-next-line react-hooks/purity
    const cutoff = Date.now() - cutoffMs;

    return dashboardData.documents.filter((document) => {
      const timestamp = new Date(document.createdAt).getTime();
      return !Number.isNaN(timestamp) && timestamp >= cutoff;
    }).length;
  }, [dashboardData.documents, selectedPeriod]);

  return (
    <main className={styles.main}>
      <section className={styles.panel}>
        <div className={styles.headlineBlock}>
          <p className={styles.eyebrow}>Home</p>
          <h1 className={styles.title}>Contacts at a glance</h1>
          <p className={styles.description}>
            Quick summary of your contact directory, with a direct shortcut to create a new record.
          </p>
        </div>

        <div className={styles.statGrid}>
          <div className={styles.statCard}>
            <p className={styles.statLabel}>Total contacts</p>
            <p className={styles.statValue}>{dashboardData.isLoading ? "..." : dashboardData.totalContacts}</p>
          </div>

          <div className={styles.statCardAccent}>
            <div className={styles.statHeaderRow}>
              <p className={styles.statLabel}>Documents</p>
              <span className={styles.periodBadge}>{selectedPeriod}</span>
            </div>
            <p className={styles.statValue}>{dashboardData.isLoading ? "..." : documentsInPeriod}</p>
            <div className={styles.periodSelector} aria-label="Select a document period">
              {periodOptions.map((period) => {
                const isActive = period.key === selectedPeriod;

                return (
                  <button
                    key={period.key}
                    type="button"
                    className={`${styles.periodButton} ${isActive ? styles.periodButtonActive : ""}`.trim()}
                    onClick={() => setSelectedPeriod(period.key)}
                    aria-pressed={isActive}
                  >
                    {period.label}
                  </button>
                );
              })}
            </div>
          </div>
        </div>

        {dashboardData.errorMessage && <p className={styles.errorText}>{dashboardData.errorMessage}</p>}

        <div className={styles.actions}>
          <Button asChild className={styles.createButton}>
            <Link href="/contacts?create=1">Create new contact</Link>
          </Button>
        </div>
      </section>
    </main>
  );
}
