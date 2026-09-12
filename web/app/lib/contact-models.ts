export type ContactAddress = {
  addressLine1?: string | null;
  addressLine2?: string | null;
  addressLine3?: string | null;
  addressLine4?: string | null;
  postcode?: string | null;
  country?: string | null;
};

export type Contact = {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  company: string | null;
  address?: ContactAddress | null;
  createdAt: string;
  updatedAt: string;
};

export type ContactFormValues = {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  company: string;
  addressLine1: string;
  addressLine2: string;
  addressLine3: string;
  addressLine4: string;
  postcode: string;
  country: string;
};

export type ContactFieldConfig = {
  key: keyof ContactFormValues;
  label: string;
  type: "text" | "email" | "tel";
  placeholder: string;
  required?: boolean;
  options?: { label: string; value: string }[];
};

export const emptyContactForm: ContactFormValues = {
  firstName: "",
  lastName: "",
  email: "",
  phone: "",
  company: "",
  addressLine1: "",
  addressLine2: "",
  addressLine3: "",
  addressLine4: "",
  postcode: "",
  country: "GB",
};

export const contactFields: ContactFieldConfig[] = [
  {
    key: "firstName",
    label: "First name",
    type: "text",
    placeholder: "Avery",
    required: true,
  },
  {
    key: "lastName",
    label: "Last name",
    type: "text",
    placeholder: "Stone",
    required: true,
  },
  {
    key: "email",
    label: "Email",
    type: "email",
    placeholder: "avery@studio.example",
    required: true,
  },
  {
    key: "phone",
    label: "Phone",
    type: "tel",
    placeholder: "+49 171 555 0100",
  },
  {
    key: "company",
    label: "Company",
    type: "text",
    placeholder: "Northline Studio",
  },
  {
    key: "addressLine1",
    label: "Address line 1",
    type: "text",
    placeholder: "123 Main St",
  },
  {
    key: "addressLine2",
    label: "Address line 2",
    type: "text",
    placeholder: "Suite 200",
  },
  {
    key: "addressLine3",
    label: "Address line 3",
    type: "text",
    placeholder: "City",
  },
  {
    key: "addressLine4",
    label: "Address line 4",
    type: "text",
    placeholder: "Region",
  },
  {
    key: "postcode",
    label: "Postcode",
    type: "text",
    placeholder: "SW1A 1AA",
  },
  {
    key: "country",
    label: "Country",
    type: "text",
    placeholder: "GB",
  },
];

export function toFormValues(contact: Contact): ContactFormValues {
  return {
    firstName: contact.firstName,
    lastName: contact.lastName,
    email: contact.email,
    phone: contact.phone ?? "",
    company: contact.company ?? "",
    addressLine1: contact.address?.addressLine1 ?? "",
    addressLine2: contact.address?.addressLine2 ?? "",
    addressLine3: contact.address?.addressLine3 ?? "",
    addressLine4: contact.address?.addressLine4 ?? "",
    postcode: contact.address?.postcode ?? "",
    country: contact.address?.country ?? "GB",
  };
}

export function formatTimestamp(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}