import { useState, type FormEvent } from "react";

import { FormFields } from "@/app/components/forms/form-fields";
import { contactFields, type ContactFormValues } from "@/app/lib/contact-models";
import { useCountries } from "@/app/contacts/hooks/use-countries";
import styles from "@/app/contacts/components/contact-modal.module.css";
import { Button } from "@/shadcn/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogTitle,
} from "@/shadcn/ui/dialog";
import { CONTACTS_API_URL } from "@/app/lib/api-config";

const addressFieldKeys = new Set([
    "addressLine1",
    "addressLine2",
    "addressLine3",
    "addressLine4",
    "postcode",
    "country",
]);

const requiredAddressFieldKeys = new Set([
    "addressLine1",
    "postcode",
    "country",
]);

type ContactModalProps = {
    isOpen: boolean;
    title: string;
    description: string;
    submitLabel: string;
    isCreate?: boolean;
    values: ContactFormValues;
    setValues: React.Dispatch<React.SetStateAction<ContactFormValues>>;
    editingId?: number | null;
    isSaving: boolean;
    apiError?: string | null;
    onSubmit: (event: FormEvent<HTMLFormElement>) => Promise<void>;
    onClose: () => void;
};

export function ContactModal({
    isOpen,
    title,
    description,
    submitLabel,
    isCreate = false,
    values,
    setValues,
    editingId,
    isSaving,
    apiError,
    onSubmit,
    onClose,
}: ContactModalProps) {
    const { countryOptions, isLoading: areCountriesLoading } = useCountries();
    const [step, setStep] = useState(1);
    const [validationErrors, setValidationErrors] = useState<
        Partial<Record<keyof ContactFormValues, string>>
    >({});

    const getEmailError = (email: string) => {
        const trimmedEmail = email.trim();

        if (!trimmedEmail) {
            return "Email is required.";
        }

        if (!/^\S+@\S+\.\S+$/.test(trimmedEmail)) {
            return "Enter a valid email address.";
        }

        return undefined;
    };

    const handleEmailBlur = async () => {
        const basicError = getEmailError(values.email);
        if (basicError) {
            setValidationErrors((currentErrors) => ({
                ...currentErrors,
                email: basicError,
            }));
            return;
        }

        // Check if email exists in the database
        try {
            const queryParams = new URLSearchParams({
                email: values.email.trim(),
            });
            if (editingId) {
                queryParams.append("excludeContactId", editingId.toString());
            }

            const response = await fetch(
                `${CONTACTS_API_URL}/check-email?${queryParams.toString()}`
            );

            if (response.ok) {
                const data = await response.json() as { exists: boolean };
                if (data.exists) {
                    setValidationErrors((currentErrors) => ({
                        ...currentErrors,
                        email: "Email already exists.",
                    }));
                } else {
                    setValidationErrors((currentErrors) => ({
                        ...currentErrors,
                        email: undefined,
                    }));
                }
            }
        } catch (error) {
            console.error("Error checking email:", error);
            // Don't set an error here; let submission handle it
        }
    };

    const handleFieldSet = (updater: React.SetStateAction<ContactFormValues>) => {
        setValues((currentValues) => {
            const nextValues = typeof updater === "function"
                ? (updater as (previousState: ContactFormValues) => ContactFormValues)(currentValues)
                : updater;

            return nextValues;
        });
    };

    const handleClose = () => {
        setStep(1);
        setValidationErrors({});
        onClose();
    };

    const handleNext = () => {
        if (isCreate) {
            const errors: Partial<Record<keyof ContactFormValues, string>> = {};

            if (!values.firstName.trim()) {
                errors.firstName = "First name is required.";
            }

            if (!values.lastName.trim()) {
                errors.lastName = "Last name is required.";
            }

            const emailError = getEmailError(values.email);
            if (emailError) {
                errors.email = emailError;
            }

            if (Object.keys(errors).length > 0) {
                setValidationErrors(errors);
                return;
            }
        }

        setValidationErrors({});
        setStep(2);
    };

    const hasAddressInput = [
        values.addressLine1,
        values.addressLine2,
        values.addressLine3,
        values.addressLine4,
        values.postcode,
    ].some((value) => value.trim() !== "");

    const fields = contactFields
        .filter((field) => (
            step === 2
                ? addressFieldKeys.has(field.key)
                : !addressFieldKeys.has(field.key)
        ))
        .map((field) => requiredAddressFieldKeys.has(field.key)
            ? {
                ...field,
                required: hasAddressInput,
                requiredIndicator: true,
            }
            : field)
        .map((field) => field.key === "country"
            ? { ...field, options: countryOptions, loading: areCountriesLoading }
            : field);

    return (
        <Dialog open={isOpen} onOpenChange={(open) => {
            if (!open) {
                handleClose();
            }
        }}>
            <DialogContent className={styles.dialogContent}>
                <form
                    onSubmit={onSubmit}
                    className={styles.formShell}
                >
                    <div className={styles.headerRow}>
                        <div>
                            <p className={styles.eyebrow}>
                                Contact editor
                            </p>
                            <DialogTitle asChild>
                                <h2 className={styles.modalTitle}>
                                    {title}
                                </h2>
                            </DialogTitle>
                            <DialogDescription asChild>
                                <p className={styles.modalDescription}>
                                    {description}
                                </p>
                            </DialogDescription>
                            <p className={styles.modalDescription}>
                                Step {step} of 2
                            </p>
                            {step === 2 && (
                                <p className={styles.modalDescription}>
                                    Address line 1, postcode, and country are required when adding an address.
                                </p>
                            )}
                        </div>
                        <Button
                            type="button"
                            onClick={handleClose}
                            size="sm"
                            variant="outline"
                        >
                            Close
                        </Button>
                    </div>

                    {apiError && (
                        <div className={styles.apiErrorStack} role="alert">
                            <p className={styles.apiErrorTitle}>Couldn’t save contact</p>
                            <p className={styles.apiErrorMessage}>{apiError}</p>
                        </div>
                    )}

                    <div className={styles.fieldsGrid}>
                        <FormFields
                            fields={fields}
                            values={values}
                            setValues={handleFieldSet}
                            errors={step === 1 ? validationErrors : {}}
                            onBlur={(fieldKey) => {
                                if (fieldKey === "email") {
                                    handleEmailBlur();
                                }
                            }}
                        />
                    </div>

                    <div className={styles.actions}>
                        <Button
                            type="button"
                            onClick={handleClose}
                            variant="outline"
                        >
                            Cancel
                        </Button>
                        {step === 1 ? (
                            <Button
                                type="button"
                                onClick={handleNext}
                            >
                                Next
                            </Button>
                        ) : (
                            <>
                                <Button
                                    type="button"
                                    onClick={() => setStep(1)}
                                    variant="outline"
                                >
                                    Previous
                                </Button>
                                <Button
                                    type="submit"
                                    disabled={isSaving}
                                >
                                    {isSaving ? "Saving..." : submitLabel}
                                </Button>
                            </>
                        )}
                    </div>
                </form>
            </DialogContent>
        </Dialog>
    );
}

export function CreateContactPanel(props: ContactModalProps) {
    return (
        <ContactModal
            {...props}
            title={props.title}
            description={props.description}
            submitLabel={props.submitLabel}
        />
    );
}