import type {
  ContractFlag,
  ContractListItem,
  ContractListResponse,
  ContractRecord,
} from "@/components/contracts/contractData";

const API_BASE =
  process.env.NEXT_PUBLIC_API_BASE ??
  process.env.NEXT_PUBLIC_BASE_URL ??
  "http://localhost:21021";

const ABP_TENANT_HEADER = { "Abp-TenantId": "1" };
const LOCAL_API_BASE_CANDIDATES = ["http://localhost:21021", "http://localhost:5000"];

function getCookie(name: string): string | null {
  if (typeof document === "undefined") return null;
  const prefix = `${name}=`;
  for (const part of document.cookie.split(";")) {
    const trimmed = part.trim();
    if (trimmed.startsWith(prefix)) {
      return decodeURIComponent(trimmed.slice(prefix.length));
    }
  }
  return null;
}

function isLocalBrowserSession(): boolean {
  if (typeof window === "undefined") {
    return false;
  }

  return window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1";
}

function getApiBaseCandidates(): string[] {
  if (!isLocalBrowserSession()) {
    return [API_BASE];
  }

  const candidates = [...LOCAL_API_BASE_CANDIDATES];
  if (!candidates.includes(API_BASE)) {
    candidates.push(API_BASE);
  }

  return candidates;
}

function getBaseHeaders(locale?: string): HeadersInit {
  const headers: Record<string, string> = {
    ...ABP_TENANT_HEADER,
  };

  if (locale) {
    headers["Accept-Language"] = locale;
  }

  const token = getCookie("ml_token");
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
}

async function fetchWithFallback(
  path: string,
  init: RequestInit
): Promise<Response> {
  let lastNetworkError: TypeError | null = null;

  for (const apiBase of getApiBaseCandidates()) {
    try {
      return await fetch(`${apiBase}${path}`, init);
    } catch (error) {
      if (!(error instanceof TypeError)) {
        throw error;
      }

      lastNetworkError = error;
    }
  }

  throw lastNetworkError ?? new TypeError("Failed to fetch");
}

function unwrapResult<T>(json: unknown): T {
  if (
    typeof json === "object" &&
    json !== null &&
    "result" in json
  ) {
    return (json as { result: T }).result;
  }

  return json as T;
}

function normalizeFlag(flag: ContractFlag): ContractFlag {
  return {
    severity: flag.severity ?? "amber",
    title: flag.title ?? "",
    description: flag.description ?? "",
    clauseText: flag.clauseText ?? "",
    legislationCitation: flag.legislationCitation ?? null,
  };
}

function normalizeContractRecord(record: ContractRecord): ContractRecord {
  return {
    ...record,
    flags: Array.isArray(record.flags) ? record.flags.map(normalizeFlag) : [],
  };
}

function normalizeContractListItem(item: ContractListItem): ContractListItem {
  return {
    ...item,
    summary: item.summary ?? "",
  };
}

async function parseJsonResponse<T>(response: Response): Promise<T> {
  const json = (await response.json().catch(() => ({}))) as unknown;

  if (!response.ok) {
    const errorEnvelope = json as { error?: { message?: string; details?: string } };
    throw new Error(
      errorEnvelope?.error?.message ??
        errorEnvelope?.error?.details ??
        `Request failed with status ${response.status}`
    );
  }

  return unwrapResult<T>(json);
}

export async function analyseContract(
  file: File,
  locale?: string
): Promise<ContractRecord> {
  const formData = new FormData();
  formData.append("file", file);

  if (locale) {
    formData.append("responseLanguageCode", locale);
  }

  const response = await fetchWithFallback("/api/app/contract/analyse", {
    method: "POST",
    headers: getBaseHeaders(locale),
    body: formData,
  });

  const result = await parseJsonResponse<ContractRecord>(response);
  return normalizeContractRecord(result);
}

export async function getMyContracts(locale?: string): Promise<ContractListResponse> {
  const response = await fetchWithFallback("/api/app/contract/my", {
    method: "GET",
    headers: getBaseHeaders(locale),
  });

  const result = await parseJsonResponse<ContractListResponse>(response);
  return {
    items: Array.isArray(result.items) ? result.items.map(normalizeContractListItem) : [],
    totalCount: result.totalCount ?? 0,
  };
}

export async function getContractAnalysis(
  id: string,
  locale?: string
): Promise<ContractRecord> {
  const response = await fetchWithFallback(`/api/app/contract/${id}`, {
    method: "GET",
    headers: getBaseHeaders(locale),
  });

  const result = await parseJsonResponse<ContractRecord>(response);
  return normalizeContractRecord(result);
}
