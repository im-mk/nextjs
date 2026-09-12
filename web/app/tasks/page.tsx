import Link from "next/link";

import styles from "@/app/tasks/page.module.css";
import { cn } from "@/lib/utils";
import { Button } from "@/shadcn/ui/button";

const sampleTasks = [
    {
        title: "Follow up with Acme proposal",
        meta: "Due tomorrow · Linked to contact: Jane Smith",
        priority: "High",
    },
    {
        title: "Prepare onboarding checklist",
        meta: "Due this week · Linked to contact: Charlie Davis",
        priority: "Medium",
    },
    {
        title: "Archive inactive leads",
        meta: "Due next week · Batch task",
        priority: "Low",
    },
] as const;

export default function TasksPage() {
    return (
        <main className={styles.main}>
            <section className={styles.wrapper}>
                <div className={styles.hero}>
                    <p className={styles.eyebrow}>Tasks</p>
                    <h1 className={styles.title}>Track follow-ups and team actions</h1>
                    <p className={styles.description}>
                        Keep work moving with clear priorities and direct links between tasks and contacts.
                    </p>

                    <div className={styles.actions}>
                        <Button className={styles.primaryButton}>Create task</Button>
                        <Button asChild variant="outline">
                            <Link href="/contacts">Assign from contacts</Link>
                        </Button>
                    </div>
                </div>

                <div className={styles.list}>
                    {sampleTasks.map((task) => {
                        const priorityClass =
                            task.priority === "High"
                                ? styles.high
                                : task.priority === "Medium"
                                    ? styles.medium
                                    : styles.low;

                        return (
                            <article key={task.title} className={styles.task}>
                                <div>
                                    <h2 className={styles.taskTitle}>{task.title}</h2>
                                    <p className={styles.taskMeta}>{task.meta}</p>
                                </div>
                                <span className={cn(styles.badge, priorityClass)}>{task.priority}</span>
                            </article>
                        );
                    })}
                </div>
            </section>
        </main>
    );
}
