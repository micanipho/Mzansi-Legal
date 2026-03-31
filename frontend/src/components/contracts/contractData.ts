export interface ContractFlag {
  title: string;
  detail: string;
  citation?: string;
}

export interface ContractRecord {
  id: string;
  title: string;
  category: string;
  uploadedAt: string;
  pages: number;
  clauses: number;
  language: string;
  score: number;
  verdict: "good" | "review" | "high-risk";
  summary: string;
  recommendation: string;
  tags: string[];
  redFlags: ContractFlag[];
  cautionFlags: ContractFlag[];
  standardCount: number;
}

export const demoContracts: ContractRecord[] = [
  {
    id: "maple-street-lease",
    title: "Lease agreement - 42 Maple Street",
    category: "Rental / lease",
    uploadedAt: "24 March 2026",
    pages: 12,
    clauses: 47,
    language: "English",
    score: 62,
    verdict: "review",
    summary:
      "This residential lease is mostly standard, but several clauses shift too much power to the landlord and deserve negotiation before signing.",
    recommendation:
      "Ask for the deposit, cancellation notice period, and access clauses to be revised before you commit.",
    tags: ["Tenant rights", "Deposit risk", "Notice periods"],
    redFlags: [
      {
        title: "Deposit exceeds standard practice",
        detail:
          "The lease asks for a 3-month deposit, which is unusually heavy for this rent band and may be challenged as unfair.",
        citation: "Rental Housing Act, Section 5(3)(g)",
      },
      {
        title: "Early cancellation notice is too long",
        detail:
          "The draft requires 3 months' notice, while the CPA typically allows a tenant to cancel on 20 business days' notice.",
        citation: "Consumer Protection Act, Section 14",
      },
      {
        title: "Landlord access clause is too broad",
        detail:
          "The contract allows entry without reasonable notice, which undermines the tenant's privacy and peaceful occupation.",
        citation: "Constitution, Section 14",
      },
    ],
    cautionFlags: [
      {
        title: "Annual escalation is above market average",
        detail: "A 10% increase is steeper than the current rental market trend for similar units.",
      },
      {
        title: "Maintenance cap is vague",
        detail:
          "A blanket tenant obligation for all maintenance under R2,000 could shift structural repairs onto the tenant.",
      },
    ],
    standardCount: 40,
  },
  {
    id: "warehouse-employment-offer",
    title: "Warehouse operations employment offer",
    category: "Employment contract",
    uploadedAt: "18 March 2026",
    pages: 9,
    clauses: 31,
    language: "English",
    score: 78,
    verdict: "good",
    summary:
      "This employment offer is fairly balanced, with clear leave, probation, and remuneration clauses, though overtime language should be tightened.",
    recommendation:
      "Confirm how overtime is approved and paid, then request that process in writing before acceptance.",
    tags: ["Employment", "Overtime", "Leave"],
    redFlags: [
      {
        title: "Overtime approval process is unclear",
        detail:
          "The contract expects reasonable overtime but does not specify approval steps or compensation timing.",
        citation: "BCEA, Sections 10 and 17",
      },
    ],
    cautionFlags: [
      {
        title: "Probation review lacks dates",
        detail: "The probation clause refers to reviews without setting milestones or review owners.",
      },
    ],
    standardCount: 29,
  },
  {
    id: "starter-credit-agreement",
    title: "Starter credit agreement",
    category: "Credit agreement",
    uploadedAt: "09 March 2026",
    pages: 15,
    clauses: 52,
    language: "English",
    score: 48,
    verdict: "high-risk",
    summary:
      "This credit agreement contains fee stacking and aggressive default terms that create a much higher repayment risk than the headline price suggests.",
    recommendation:
      "Do not sign until the initiation fee, default charges, and collection language are independently explained or corrected.",
    tags: ["Debt & credit", "Default fees", "Collections"],
    redFlags: [
      {
        title: "Charges compound too aggressively after default",
        detail:
          "The default clause layers penalties, service fees, and collection charges in a way that can inflate the debt quickly.",
        citation: "National Credit Act, Section 103(5)",
      },
      {
        title: "Collection language is overbroad",
        detail:
          "The agreement suggests immediate asset recovery rights without a clear legal process or notice period.",
      },
    ],
    cautionFlags: [
      {
        title: "Plain-language summary is missing",
        detail: "The pricing section is dense and difficult to compare against what a consumer was told verbally.",
      },
      {
        title: "Insurance add-on is not clearly optional",
        detail: "The document does not separate credit life insurance from the base cost in a clear way.",
      },
    ],
    standardCount: 34,
  },
];

export function getContractById(id: string): ContractRecord | undefined {
  return demoContracts.find((contract) => contract.id === id);
}
