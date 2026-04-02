import type {
  RightsAcademyLesson,
  RightsAcademyProgressResponse,
  RightsAcademyResponse,
  RightsAcademyTrack,
  PublicFaqItem,
  PublicFaqListResponse,
} from "@/components/rights/rightsData";
import { localizeRightsAcademyTracks } from "@/components/rights/rightsData";

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

async function fetchWithFallback(path: string, init: RequestInit): Promise<Response> {
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
  if (typeof json === "object" && json !== null && "result" in json) {
    return (json as { result: T }).result;
  }

  return json as T;
}

function normalizeCitation(citation: PublicFaqItem["citations"][number]): PublicFaqItem["citations"][number] {
  return {
    id: citation.id ?? "",
    actName: citation.actName ?? "",
    sectionNumber: citation.sectionNumber ?? "",
    excerpt: citation.excerpt ?? "",
    relevanceScore: citation.relevanceScore ?? 0,
  };
}

function normalizeFaqItem(item: PublicFaqItem): PublicFaqItem {
  return {
    ...item,
    categoryId: item.categoryId ?? null,
    categoryName: item.categoryName ?? "",
    topicKey: item.topicKey ?? "legal",
    title: item.title ?? "",
    summary: item.summary ?? "",
    explanation: item.explanation ?? "",
    sourceQuote: item.sourceQuote ?? null,
    primaryCitation: item.primaryCitation ?? "",
    languageCode: item.languageCode ?? "en",
    publishedAt: item.publishedAt ?? "",
    citations: Array.isArray(item.citations) ? item.citations.map(normalizeCitation) : [],
  };
}

function normalizeAcademyLesson(item: RightsAcademyLesson): RightsAcademyLesson {
  return {
    ...item,
    id: item.id ?? "",
    documentId: item.documentId ?? "",
    topicKey: item.topicKey ?? "legal",
    categoryName: item.categoryName ?? "",
    title: item.title ?? "",
    lawShortName: item.lawShortName ?? "",
    lawTitle: item.lawTitle ?? "",
    summary: item.summary ?? "",
    explanation: item.explanation ?? "",
    sourceQuote: item.sourceQuote ?? null,
    primaryCitation: item.primaryCitation ?? "",
    askQuery: item.askQuery ?? item.title ?? "",
    citations: Array.isArray(item.citations) ? item.citations.map(normalizeCitation) : [],
  };
}

function normalizeAcademyTrack(track: RightsAcademyTrack): RightsAcademyTrack {
  return {
    topicKey: track.topicKey ?? "legal",
    categoryName: track.categoryName ?? "",
    sortOrder: track.sortOrder ?? 0,
    lessons: Array.isArray(track.lessons) ? track.lessons.map(normalizeAcademyLesson) : [],
  };
}

function normalizeAcademyProgress(result: RightsAcademyProgressResponse): RightsAcademyProgressResponse {
  return {
    exploredLessonIds: Array.isArray(result.exploredLessonIds)
      ? result.exploredLessonIds.filter((value): value is string => typeof value === "string" && value.trim().length > 0)
      : [],
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

export async function getPublicFaqs(locale?: string, categoryId?: string): Promise<PublicFaqListResponse> {
  const searchParams = new URLSearchParams();

  if (locale) {
    searchParams.set("languageCode", locale);
  }

  if (categoryId) {
    searchParams.set("categoryId", categoryId);
  }

  const query = searchParams.toString();
  const response = await fetchWithFallback(`/api/app/question/faqs${query ? `?${query}` : ""}`, {
    method: "GET",
    headers: getBaseHeaders(locale),
  });

  const result = await parseJsonResponse<PublicFaqListResponse>(response);

  return {
    items: Array.isArray(result.items) ? result.items.map(normalizeFaqItem) : [],
    totalCount: result.totalCount ?? 0,
  };
}

export async function getRightsAcademy(locale?: string): Promise<RightsAcademyResponse> {
  const response = await fetchWithFallback("/api/app/question/academy", {
    method: "GET",
    headers: getBaseHeaders(locale),
  });

  const result = await parseJsonResponse<RightsAcademyResponse>(response);

  return {
    tracks: localizeRightsAcademyTracks(
      Array.isArray(result.tracks) ? result.tracks.map(normalizeAcademyTrack) : [],
      locale ?? "en",
    ),
    totalLessons: result.totalLessons ?? 0,
  };
}

export async function getRightsAcademyProgress(): Promise<RightsAcademyProgressResponse> {
  const token = getCookie("ml_token");
  if (!token) {
    return { exploredLessonIds: [] };
  }

  const response = await fetchWithFallback("/api/app/question/academy-progress", {
    method: "GET",
    headers: getBaseHeaders(),
  });

  if (response.status === 401 || response.status === 403) {
    return { exploredLessonIds: [] };
  }

  const result = await parseJsonResponse<RightsAcademyProgressResponse>(response);
  return normalizeAcademyProgress(result);
}

export async function saveRightsAcademyProgress(exploredLessonIds: string[]): Promise<RightsAcademyProgressResponse> {
  const token = getCookie("ml_token");
  const normalized = normalizeAcademyProgress({ exploredLessonIds });

  if (!token) {
    return normalized;
  }

  const response = await fetchWithFallback("/api/app/question/academy-progress", {
    method: "PUT",
    headers: {
      ...getBaseHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify(normalized),
  });

  if (response.status === 401 || response.status === 403) {
    return normalized;
  }

  const result = await parseJsonResponse<RightsAcademyProgressResponse>(response);
  return normalizeAcademyProgress(result);
}
