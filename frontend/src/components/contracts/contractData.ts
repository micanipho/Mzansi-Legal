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
  redFlagCount: number;
  amberFlagCount: number;
  greenFlagCount: number;
  flags: ContractFlag[];
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
