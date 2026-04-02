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

export interface CitationDto {
  actName: string;
  section: string;
  excerpt: string;
  relevance: number;
}

export interface AnswerDto {
  id: string;
  text: string;
  citations: CitationDto[];
}

export interface QuestionWithAnswerDto {
  id: string;
  conversationId: string;
  text: string;
  language: string;
  answer: AnswerDto;
  disclaimer: string;
}

export interface AskQuestionInput {
  conversationId?: string;
  language: string;
  text: string;
}

export async function askQuestion(
  input: AskQuestionInput,
  token?: string
): Promise<QuestionWithAnswerDto> {
  const res = await fetch(`${API_BASE}/api/app/question/ask`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...ABP_TENANT_HEADER,
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify(input),
  });

  const json = await res.json();

  if (!res.ok) {
    throw new Error(json?.error?.message || "Failed to get answer");
  }

  return json.result;
}

export async function transcribeAudio(
  audioBlob: Blob,
  token?: string
): Promise<{ text: string; language: string }> {
  const formData = new FormData();
  formData.append("audio", audioBlob, "recording.webm");

  const res = await fetch(`${API_BASE}/api/app/voice/transcribe`, {
    method: "POST",
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    body: formData,
  });

  const json = await res.json();
  if (!res.ok) throw new Error(json?.error?.message || "Transcription failed");
  return json.result;
}

export interface ConversationSummary {
  conversationId: string;
  firstQuestion: string;
  questionCount: number;
  startedAt: string;
  locale: string;
}

export interface ConversationHistoryMessage {
  messageId: string;
  type: "user" | "bot";
  text: string;
  createdAt: string;
}

export interface ConversationDetail {
  conversationId: string;
  startedAt: string;
  language: string;
  questionCount: number;
  messages: ConversationHistoryMessage[];
}

export interface ConversationsListResponse {
  items: ConversationSummary[];
  totalCount: number;
}

export async function getConversations(token?: string): Promise<ConversationsListResponse> {
  const authToken = token ?? getCookie("ml_token");
  const res = await fetchWithFallback("/api/app/qa/conversations", {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
      ...ABP_TENANT_HEADER,
      ...(authToken ? { Authorization: `Bearer ${authToken}` } : {}),
    },
  });

  const json = await res.json().catch(() => ({}));

  if (!res.ok) {
    throw new Error(json?.error?.message || "Failed to fetch conversations");
  }

  const result = json.result as ConversationsListResponse;

  return {
    items: Array.isArray(result?.items)
      ? result.items.map((item) => ({
          conversationId: item.conversationId,
          firstQuestion: item.firstQuestion,
          questionCount: item.questionCount,
          startedAt: item.startedAt,
          locale: item.locale ?? (item as ConversationSummary & { language?: string }).language ?? "en",
        }))
      : [],
    totalCount: result?.totalCount ?? 0,
  };
}

export async function getConversation(
  conversationId: string,
  token?: string,
): Promise<ConversationDetail> {
  const authToken = token ?? getCookie("ml_token");
  const res = await fetchWithFallback(`/api/app/qa/conversations/${conversationId}`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
      ...ABP_TENANT_HEADER,
      ...(authToken ? { Authorization: `Bearer ${authToken}` } : {}),
    },
  });

  const json = await res.json().catch(() => ({}));

  if (!res.ok) {
    throw new Error(json?.error?.message || "Failed to fetch conversation");
  }

  const result = (json.result ?? json) as ConversationDetail;

  return {
    conversationId: result.conversationId,
    startedAt: result.startedAt,
    language: result.language ?? "en",
    questionCount: result.questionCount ?? 0,
    messages: Array.isArray(result.messages)
      ? result.messages.map((message) => ({
          messageId: message.messageId,
          type: message.type === "bot" ? "bot" : "user",
          text: message.text ?? "",
          createdAt: message.createdAt,
        }))
      : [],
  };
}

export async function textToSpeech(
  text: string,
  language: string,
  token?: string
): Promise<Blob> {
  const res = await fetch(`${API_BASE}/api/app/voice/speak`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...ABP_TENANT_HEADER,
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify({ text, language }),
  });

  if (!res.ok) throw new Error("TTS failed");
  return res.blob();
}
