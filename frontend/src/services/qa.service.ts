const API_BASE =
  process.env.NEXT_PUBLIC_API_BASE ??
  process.env.NEXT_PUBLIC_BASE_URL ??
  "http://localhost:21021";
const ABP_TENANT_HEADER = { "Abp-TenantId": "1" };
const LOCAL_API_BASE_CANDIDATES = ["http://localhost:21021", "http://localhost:5000"];

/**
 * Helper function to read a cookie value by name
 */
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

export interface AskQuestionRequest {
  questionText: string;
}

export interface RagCitationDto {
  chunkId: string;
  actName: string;
  sectionNumber: string;
  excerpt: string;
  relevanceScore: number;
}

export type RagAnswerMode = "direct" | "cautious" | "clarification" | "insufficient";

export type RagConfidenceBand = "high" | "medium" | "low";

export interface RagAnswerResult {
  answerText: string | null;
  isInsufficientInformation: boolean;
  detectedLanguageCode: string;
  answerMode: RagAnswerMode;
  confidenceBand: RagConfidenceBand;
  clarificationQuestion: string | null;
  citations: RagCitationDto[];
  chunkIds: string[];
  answerId: string | null;
}

const ANSWER_MODE_BY_NUMBER: Record<number, RagAnswerMode> = {
  0: "direct",
  1: "cautious",
  2: "clarification",
  3: "insufficient",
};

const CONFIDENCE_BAND_BY_NUMBER: Record<number, RagConfidenceBand> = {
  0: "high",
  1: "medium",
  2: "low",
};

const ANSWER_MODES: RagAnswerMode[] = [
  "direct",
  "cautious",
  "clarification",
  "insufficient",
];

const CONFIDENCE_BANDS: RagConfidenceBand[] = ["high", "medium", "low"];

function isRagAnswerMode(value: unknown): value is RagAnswerMode {
  return (
    typeof value === "string" &&
    ANSWER_MODES.includes(value as RagAnswerMode)
  );
}

function isRagConfidenceBand(value: unknown): value is RagConfidenceBand {
  return (
    typeof value === "string" &&
    CONFIDENCE_BANDS.includes(value as RagConfidenceBand)
  );
}

function normalizeAnswerMode(value: unknown): RagAnswerMode {
  if (isRagAnswerMode(value)) {
    return value;
  }

  if (typeof value === "number" && value in ANSWER_MODE_BY_NUMBER) {
    return ANSWER_MODE_BY_NUMBER[value];
  }

  return "direct";
}

function normalizeConfidenceBand(value: unknown): RagConfidenceBand {
  if (isRagConfidenceBand(value)) {
    return value;
  }

  if (typeof value === "number" && value in CONFIDENCE_BAND_BY_NUMBER) {
    return CONFIDENCE_BAND_BY_NUMBER[value];
  }

  return "medium";
}

function normalizeRagAnswerResult(result: RagAnswerResult): RagAnswerResult {
  return {
    ...result,
    answerMode: normalizeAnswerMode(result.answerMode),
    confidenceBand: normalizeConfidenceBand(result.confidenceBand),
  };
}

function isLocalBrowserSession(): boolean {
  if (typeof window === "undefined") {
    return false;
  }

  return (
    window.location.hostname === "localhost" ||
    window.location.hostname === "127.0.0.1"
  );
}

function getAskApiBaseCandidates(): string[] {
  if (!isLocalBrowserSession()) {
    return [API_BASE];
  }

  const candidates = [...LOCAL_API_BASE_CANDIDATES];

  if (!candidates.includes(API_BASE)) {
    candidates.push(API_BASE);
  }

  for (const localBase of LOCAL_API_BASE_CANDIDATES) {
    if (!candidates.includes(localBase)) {
      candidates.push(localBase);
    }
  }

  return candidates;
}

async function fetchAskResponse(
  request: AskQuestionRequest,
  headers: Record<string, string>
): Promise<Response> {
  let lastNetworkError: TypeError | null = null;

  for (const apiBase of getAskApiBaseCandidates()) {
    try {
      return await fetch(`${apiBase}/api/app/qa/ask`, {
        method: "POST",
        headers,
        body: JSON.stringify(request),
      });
    } catch (error) {
      if (!(error instanceof TypeError)) {
        throw error;
      }

      lastNetworkError = error;
    }
  }

  throw lastNetworkError ?? new TypeError("Failed to fetch");
}

export async function askRagQuestion(
  request: AskQuestionRequest,
  locale?: string
): Promise<RagAnswerResult> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...ABP_TENANT_HEADER,
  };

  if (locale) {
    headers["Accept-Language"] = locale;
  }

  // Read JWT token from cookie and include in Authorization header
  const token = getCookie("ml_token");
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const res = await fetchAskResponse(request, headers);

  if (!res.ok) {
    const json = await res.json().catch(() => ({})) as { error?: { message?: string } };
    throw new Error(json?.error?.message ?? `Request failed with status ${res.status}`);
  }

  const json = await res.json();
  
  // ABP wraps responses in a "result" property
  if (json.result) {
    return normalizeRagAnswerResult(json.result as RagAnswerResult);
  }
  
  return normalizeRagAnswerResult(json as RagAnswerResult);
}
