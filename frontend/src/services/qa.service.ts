const API_BASE = process.env.NEXT_PUBLIC_BASE_URL ?? "http://localhost:21021";
const ABP_TENANT_HEADER = { "Abp-TenantId": "1" };

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

export interface RagAnswerResult {
  answerText: string | null;
  isInsufficientInformation: boolean;
  citations: RagCitationDto[];
  chunkIds: string[];
  answerId: string | null;
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

  const res = await fetch(`${API_BASE}/api/app/qa/ask`, {
    method: "POST",
    headers,
    body: JSON.stringify(request),
  });

  if (!res.ok) {
    const json = await res.json().catch(() => ({})) as { error?: { message?: string } };
    throw new Error(json?.error?.message ?? `Request failed with status ${res.status}`);
  }

  const json = await res.json();
  
  // ABP wraps responses in a "result" property
  if (json.result) {
    return json.result as RagAnswerResult;
  }
  
  return json as RagAnswerResult;
}
