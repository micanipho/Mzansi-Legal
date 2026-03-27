const API_BASE = process.env.NEXT_PUBLIC_BASE_URL ?? "https://localhost:44311";

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

export async function textToSpeech(
  text: string,
  language: string,
  token?: string
): Promise<Blob> {
  const res = await fetch(`${API_BASE}/api/app/voice/speak`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify({ text, language }),
  });

  if (!res.ok) throw new Error("TTS failed");
  return res.blob();
}
