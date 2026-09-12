"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import styles from "@/app/components/site-nav.module.css";
import { Button } from "@/shadcn/ui/button";
import { cn } from "@/lib/utils";

const navItems = [
    { href: "/", label: "Home" },
    { href: "/contacts", label: "Contacts" },
    { href: "/documents", label: "Documents" },
    { href: "/tasks", label: "Tasks" },
];

export function SiteNav() {
    const pathname = usePathname();

    return (
        <header className={styles.header}>
            <nav className={styles.nav}>
                <Link href="/" className={styles.brand}>
                    Contact Desk
                </Link>

                <ul className={styles.list}>
                    {navItems.map((item) => {
                        const isActive =
                            item.href === "/"
                                ? pathname === "/"
                                : pathname === item.href || pathname.startsWith(`${item.href}/`);

                        return (
                            <li key={item.href}>
                                <Button
                                    asChild
                                    size="sm"
                                    variant="outline"
                                    className={cn(
                                        styles.itemButton,
                                        isActive ? styles.active : styles.inactive
                                    )}
                                >
                                    <Link href={item.href}>{item.label}</Link>
                                </Button>
                            </li>
                        );
                    })}
                </ul>
            </nav>
        </header>
    );
}
