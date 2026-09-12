import type { Dispatch, SetStateAction } from "react";

import styles from "@/app/components/forms/form-fields.module.css";
import { cn } from "@/lib/utils";
import { Input } from "@/shadcn/ui/input";

type FormValues = Record<string, string>;

export type FormFieldConfig<TValues extends FormValues> = {
    key: keyof TValues;
    label: string;
    type: "text" | "email" | "tel";
    placeholder: string;
    required?: boolean;
    requiredIndicator?: boolean;
    loading?: boolean;
    options?: { label: string; value: string }[];
};

type FormFieldsProps<TValues extends FormValues> = {
    fields: FormFieldConfig<TValues>[];
    values: TValues;
    setValues: Dispatch<SetStateAction<TValues>>;
    errors?: Partial<Record<keyof TValues, string>>;
    compact?: boolean;
    onBlur?: (fieldKey: keyof TValues) => void;
};

export function FormFields<TValues extends FormValues>({
    fields,
    values,
    setValues,
    errors,
    compact = false,
    onBlur,
}: FormFieldsProps<TValues>) {
    return fields.map((field) => (
        <label
            key={String(field.key)}
            className={styles.field}
        >
            <span className={styles.label}>
                {field.label}
                {(field.required || field.requiredIndicator) && (
                    <span className={styles.requiredMarker} aria-hidden="true">
                        *
                    </span>
                )}
            </span>
            {errors?.[field.key] && (
                <span className={styles.errorMessage} role="alert">
                    {errors[field.key]}
                </span>
            )}
            {field.options ? (
                <span className={styles.selectWrap}>
                    <select
                        aria-invalid={Boolean(errors?.[field.key])}
                        aria-busy={field.loading}
                        value={values[field.key]}
                        required={field.required}
                        disabled={field.loading}
                        onChange={(event) => {
                            const nextValue = event.target.value;
                            setValues((currentValues) => ({
                                ...currentValues,
                                [field.key]: nextValue,
                            } as TValues));
                        }}
                        className={cn(
                            styles.select,
                            compact
                                ? styles.inputCompact
                                : styles.inputBase
                        )}
                    >
                        {field.loading && (
                            <option value={values[field.key]}>Loading...</option>
                        )}
                        {field.options.map((option) => (
                            <option key={option.value} value={option.value}>
                                {option.label}
                            </option>
                        ))}
                    </select>
                    {field.loading && <span className={styles.spinner} aria-label="Loading" />}
                </span>
            ) : (
                <Input
                    aria-invalid={Boolean(errors?.[field.key])}
                    type={field.type}
                    value={values[field.key]}
                    required={field.required}
                    onChange={(event) => {
                        const nextValue = event.target.value;
                        setValues((currentValues) => ({
                            ...currentValues,
                            [field.key]: nextValue,
                        } as TValues));
                    }}
                    onBlur={() => {
                        onBlur?.(field.key);
                    }}
                    placeholder={field.placeholder}
                    className={cn(
                        compact
                            ? styles.inputCompact
                            : styles.inputBase
                    )}
                />
            )}
        </label>
    ));
}