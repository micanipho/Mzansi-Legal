export type ContractSeverity = "red" | "amber" | "green";

export type ContractVerdict = "good" | "review" | "high-risk";

export interface ContractFlag {
  severity: ContractSeverity;
  title: string;
  description: string;
  clauseText: string;
  legislationCitation?: string | null;
}

export interface ContractRecord {
  id: string;
  displayTitle: string;
  contractType: string;
  healthScore: number;
  summary: string;
  language: string;
  analysedAt: string;
  pageCount?: number | null;
  redFlagCount: number;
  amberFlagCount: number;
  greenFlagCount: number;
  strengths: ContractFlag[];
  concerns: ContractFlag[];
  flags: ContractFlag[];
}

export type ContractAnswerMode = "direct" | "cautious" | "insufficient";
export type ContractConfidenceBand = "high" | "medium" | "low";

export interface ContractFollowUpCitation {
  sourceTitle: string;
  sourceLocator: string;
  authorityType: string;
  sourceRole: string;
  excerpt: string;
}

export interface ContractConversationMessage {
  role: "user" | "assistant";
  text: string;
}

export interface ContractFollowUpAnswer {
  answerText: string;
  answerMode: ContractAnswerMode;
  confidenceBand: ContractConfidenceBand;
  requiresUrgentAttention: boolean;
  detectedLanguageCode: string;
  contractExcerpts: string[];
  citations: ContractFollowUpCitation[];
}

export interface ContractListItem {
  id: string;
  displayTitle: string;
  contractType: string;
  healthScore: number;
  summary: string;
  language: string;
  analysedAt: string;
  redFlagCount: number;
  amberFlagCount: number;
  greenFlagCount: number;
}

export interface ContractListResponse {
  items: ContractListItem[];
  totalCount: number;
}

export function getContractVerdict(score: number): ContractVerdict {
  if (score >= 75) {
    return "good";
  }

  if (score >= 55) {
    return "review";
  }

  return "high-risk";
}

export function getContractTypeLabel(contractType: string): string {
  switch (contractType) {
    case "employment":
      return "Employment";
    case "lease":
      return "Lease";
    case "credit":
      return "Credit";
    case "service":
      return "Service";
    default:
      return contractType;
  }
}

export function formatContractDate(value: string, locale: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat(locale, {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(date);
}

export function groupFlags(flags: ContractFlag[]) {
  return {
    redFlags: flags.filter((flag) => flag.severity === "red"),
    cautionFlags: flags.filter((flag) => flag.severity === "amber"),
    standardFlags: flags.filter((flag) => flag.severity === "green"),
  };
}
