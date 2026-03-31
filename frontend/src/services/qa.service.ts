const API_BASE = process.env.NEXT_PUBLIC_BASE_URL ?? "https://localhost:44311";

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
  answerText: string;
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
  };

  if (locale) {
    headers["Accept-Language"] = locale;
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

  return res.json() as Promise<RagAnswerResult>;
}
