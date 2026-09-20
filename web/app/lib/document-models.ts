export type Document = {
  id: number;
  contactId: number | null;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  createdAt: string;
  updatedAt: string;
};

export type CreateUploadUrlResponse = {
  uploadUrl: string;
  storageKey: string;
  uploadHeaders: Record<string, string>;
};

export type DownloadUrlResponse = {
  downloadUrl: string;
};

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} B`;
  }

  const units = ["KB", "MB", "GB", "TB"];
  let value = bytes / 1024;
  let unitIndex = 0;

  while (value >= 1024 && unitIndex < units.length - 1) {
    value /= 1024;
    unitIndex += 1;
  }

  return `${value.toFixed(1)} ${units[unitIndex]}`;
}
