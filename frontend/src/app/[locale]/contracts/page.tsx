"use client";

import Link from "next/link";
import { useLocale } from "next-intl";
import { useState } from "react";
import {
  ArrowLeft, FileText, AlertCircle, AlertTriangle,
  CheckCircle, Mic, Send, UploadCloud,
} from "lucide-react";
import { C, R, shadowOrganic, fontSerif, fontSans } from "@/styles/theme";

interface ContractFlag {
  title: string;
  desc: string;
  cite?: string;
}

interface AnalysisResult {
  score: number;
  name: string;
  date: string;
  tags: string[];
  summary: string;
  red: ContractFlag[];
  caution: ContractFlag[];
  standardCount: number;
}

const DEMO: AnalysisResult = {
  score: 62,
  name: "Lease agreement — 42 Maple Street",
  date: "Uploaded 24 March 2026 · Analysed in 8 seconds",
  tags: ["Rental / lease", "12 pages · 47 clauses", "Analysed in English"],
  summary:
    "This is a standard residential lease agreement for a period of 12 months. The rent is set at R8,500 per month. However, there are several clauses that heavily favor the landlord and may violate the Rental Housing Act and Consumer Protection Act. Specifically, the deposit requirements and notice periods are irregular. You should negotiate these points before signing.",
  red: [
    {
      title: "Deposit exceeds legal limit",
      desc: "The contract demands a 3-month deposit. Standard practice and tribunal rulings generally limit this to 1-2 months unless specifically justified.",
      cite: "Rental Housing Act, Section 5(3)(g)",
    },
    {
      title: "Notice period exceeds standard",
      desc: "The landlord requires 3 months notice for early cancellation. Under the CPA, a tenant can cancel with 20 business days' notice.",
      cite: "Consumer Protection Act, Section 14",
    },
    {
      title: "Landlord can enter without consent",
      desc: "Clause 12.4 allows the landlord to enter the property at any time without notice. This violates your constitutional right to privacy.",
      cite: "Constitution, Section 14",
    },
  ],
  caution: [
    {
      title: "Escalation rate above market average",
      desc: "The annual rent increase is set at 10%. The current market average is between 5-7%.",
    },
    {
      title: "Tenant responsible for all maintenance under R2,000",
      desc: "While tenants are responsible for minor maintenance, setting a high blanket threshold of R2,000 might force you to pay for structural wear and tear.",
    },
  ],
  standardCount: 40,
};

export default function ContractsPage() {
  const locale = useLocale();
  const [result, setResult]   = useState<AnalysisResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [followUp, setFollowUp] = useState("");

  const handleUpload = () => {
    setLoading(true);
    setTimeout(() => { setResult(DEMO); setLoading(false); }, 1800);
  };

  if (!result) {
    return (
      <main
        style={{
          minHeight: "100vh",
          paddingTop: 96,
          paddingBottom: 80,
          paddingLeft: 16,
          paddingRight: 16,
          maxWidth: 896,
          margin: "0 auto",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          fontFamily: fontSans,
        }}
      >
        <div
          style={{
            background: C.card,
            border: `1px solid ${C.border}`,
            borderRadius: R.o2,
            padding: 48,
            textAlign: "center",
            boxShadow: shadowOrganic,
            width: "100%",
            maxWidth: 520,
          }}
        >
          <div
            style={{
              width: 64, height: 64,
              background: C.accent,
              borderRadius: 9999,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              color: C.secondary,
              margin: "0 auto 24px",
            }}
          >
            <FileText size={28} />
          </div>
          <h1 style={{ fontFamily: fontSerif, fontSize: 28, fontWeight: 700, color: C.fg, margin: "0 0 12px" }}>
            Upload your contract
          </h1>
          <p style={{ color: C.mutedFg, marginBottom: 32, fontSize: 15 }}>
            Upload a lease, employment, or credit agreement to analyse for risks and red flags.
          </p>
          <label
            style={{
              display: "inline-flex",
              alignItems: "center",
              gap: 8,
              background: C.primary,
              color: C.primaryFg,
              padding: "14px 32px",
              borderRadius: 9999,
              fontWeight: 700,
              fontSize: 15,
              cursor: loading ? "not-allowed" : "pointer",
              fontFamily: fontSans,
              opacity: loading ? 0.7 : 1,
            }}
          >
            <UploadCloud size={20} />
            {loading ? "Analysing…" : "Upload & Analyse"}
            <input type="file" accept=".pdf" style={{ display: "none" }} onChange={handleUpload} />
          </label>
          <p style={{ fontSize: 12, color: C.mutedFg, marginTop: 16 }}>
            PDF up to 10 MB · Lease, employment, credit, or service agreement
          </p>
        </div>
      </main>
    );
  }

  return (
    <main
      style={{
        minHeight: "100vh",
        paddingTop: 96,
        paddingBottom: 128,
        paddingLeft: 16,
        paddingRight: 16,
        maxWidth: 896,
        margin: "0 auto",
        display: "flex",
        flexDirection: "column",
        gap: 40,
        fontFamily: fontSans,
      }}
    >
      {/* Back */}
      <button
        onClick={() => setResult(null)}
        style={{
          display: "inline-flex",
          alignItems: "center",
          gap: 8,
          color: C.mutedFg,
          background: "transparent",
          border: "none",
          cursor: "pointer",
          fontWeight: 500,
          fontSize: 14,
          fontFamily: fontSans,
          padding: 0,
          width: "fit-content",
        }}
      >
        <ArrowLeft size={16} />
        Back to contracts
      </button>

      {/* Header */}
      <section style={{ display: "flex", flexDirection: "column", alignItems: "center", gap: 24 }} className="md-flex-row">
        <div
          style={{
            flexShrink: 0,
            width: 160,
            height: 160,
            border: `4px solid ${C.secondary}`,
            borderRadius: R.o1,
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            justifyContent: "center",
            background: C.card,
            boxShadow: shadowOrganic,
            transform: "rotate(-1deg)",
          }}
        >
          <span style={{ fontFamily: fontSerif, fontSize: 60, fontWeight: 700, color: C.fg, lineHeight: 1 }}>
            {result.score}
          </span>
          <span style={{ fontSize: 14, fontWeight: 500, color: C.mutedFg, marginTop: 4 }}>/100</span>
        </div>

        <div style={{ flex: 1, textAlign: "center" }}>
          <h1 style={{ fontFamily: fontSerif, fontSize: 32, fontWeight: 700, color: C.fg, margin: "0 0 12px" }}>
            {result.name}
          </h1>
          <p style={{ color: C.mutedFg, marginBottom: 24 }}>{result.date}</p>
          <div style={{ display: "flex", flexWrap: "wrap", justifyContent: "center", gap: 8 }}>
            {result.tags.map((tag) => (
              <span
                key={tag}
                style={{
                  background: C.muted,
                  color: C.fg,
                  fontSize: 14,
                  fontWeight: 500,
                  padding: "6px 16px",
                  borderRadius: 9999,
                }}
              >
                {tag}
              </span>
            ))}
          </div>
        </div>
      </section>

      {/* Summary */}
      <section
        style={{
          background: "rgba(230,220,205,0.3)",
          border: `1px solid ${C.accent}`,
          borderRadius: R.o1,
          padding: 32,
        }}
      >
        <h2 style={{ fontFamily: fontSerif, fontSize: 20, fontWeight: 700, color: C.fg, margin: "0 0 12px", display: "flex", alignItems: "center", gap: 8 }}>
          <FileText size={20} color={C.secondary} />
          Plain-language summary
        </h2>
        <p style={{ color: C.fg, lineHeight: 1.7, margin: 0, fontSize: 15 }}>{result.summary}</p>
      </section>

      {/* Breakdown row */}
      <section style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 16 }}>
        <div style={{ background: `rgba(168,84,72,0.05)`, border: `1px solid rgba(168,84,72,0.2)`, borderRadius: R.o2, padding: 24, textAlign: "center" }}>
          <span style={{ fontFamily: fontSerif, fontSize: 36, fontWeight: 700, color: C.destructive, display: "block", marginBottom: 4 }}>3</span>
          <span style={{ fontSize: 14, fontWeight: 500, color: C.destructive }}>Red flags</span>
        </div>
        <div style={{ background: `rgba(193,140,93,0.05)`, border: `1px solid rgba(193,140,93,0.2)`, borderRadius: R.o3, padding: 24, textAlign: "center" }}>
          <span style={{ fontFamily: fontSerif, fontSize: 36, fontWeight: 700, color: C.secondary, display: "block", marginBottom: 4 }}>4</span>
          <span style={{ fontSize: 14, fontWeight: 500, color: C.secondary }}>Caution</span>
        </div>
        <div style={{ background: `rgba(93,112,82,0.05)`, border: `1px solid rgba(93,112,82,0.2)`, borderRadius: R.o4, padding: 24, textAlign: "center" }}>
          <span style={{ fontFamily: fontSerif, fontSize: 36, fontWeight: 700, color: C.primary, display: "block", marginBottom: 4 }}>{result.standardCount}</span>
          <span style={{ fontSize: 14, fontWeight: 500, color: C.primary }}>Standard</span>
        </div>
      </section>

      {/* Red Flags */}
      <section style={{ display: "flex", flexDirection: "column", gap: 16 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 8 }}>
          <h2 style={{ fontFamily: fontSerif, fontSize: 24, fontWeight: 700, color: C.fg, margin: 0 }}>Red flags</h2>
          <span style={{ background: C.destructive, color: "#fff", fontSize: 12, fontWeight: 700, padding: "3px 12px", borderRadius: 9999 }}>
            {result.red.length} issues
          </span>
        </div>
        {result.red.map((flag, i) => (
          <div
            key={i}
            style={{
              background: `rgba(168,84,72,0.05)`,
              border: `1px solid rgba(168,84,72,0.2)`,
              borderLeft: `4px solid ${C.destructive}`,
              borderRadius: "0 16px 16px 0",
              padding: 24,
            }}
          >
            <div style={{ display: "flex", alignItems: "flex-start", gap: 12, marginBottom: 8 }}>
              <AlertCircle size={20} color={C.destructive} style={{ flexShrink: 0, marginTop: 2 }} />
              <h3 style={{ fontSize: 17, fontWeight: 700, color: C.destructive, margin: 0 }}>{flag.title}</h3>
            </div>
            <p style={{ color: C.fg, marginLeft: 32, marginBottom: 16, lineHeight: 1.6 }}>{flag.desc}</p>
            {flag.cite && (
              <div style={{ marginLeft: 32, display: "inline-block", background: `rgba(168,84,72,0.1)`, color: `rgba(168,84,72,0.9)`, fontSize: 13, fontWeight: 500, padding: "4px 12px", borderRadius: 9999 }}>
                {flag.cite}
              </div>
            )}
          </div>
        ))}
      </section>

      {/* Caution */}
      <section style={{ display: "flex", flexDirection: "column", gap: 16 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 8 }}>
          <h2 style={{ fontFamily: fontSerif, fontSize: 24, fontWeight: 700, color: C.fg, margin: 0 }}>Caution</h2>
          <span style={{ background: C.secondary, color: "#fff", fontSize: 12, fontWeight: 700, padding: "3px 12px", borderRadius: 9999 }}>
            {result.caution.length} items
          </span>
        </div>
        {result.caution.map((item, i) => (
          <div
            key={i}
            style={{
              background: `rgba(193,140,93,0.05)`,
              border: `1px solid rgba(193,140,93,0.2)`,
              borderLeft: `4px solid ${C.secondary}`,
              borderRadius: "0 16px 16px 0",
              padding: 24,
            }}
          >
            <div style={{ display: "flex", alignItems: "flex-start", gap: 12, marginBottom: 8 }}>
              <AlertTriangle size={20} color={C.secondary} style={{ flexShrink: 0, marginTop: 2 }} />
              <h3 style={{ fontSize: 17, fontWeight: 700, color: C.secondary, margin: 0 }}>{item.title}</h3>
            </div>
            <p style={{ color: C.fg, marginLeft: 32, margin: "0 0 0 32px", lineHeight: 1.6 }}>{item.desc}</p>
          </div>
        ))}
      </section>

      {/* Standard */}
      <section style={{ display: "flex", flexDirection: "column", gap: 16 }}>
        <h2 style={{ fontFamily: fontSerif, fontSize: 24, fontWeight: 700, color: C.fg, margin: "0 0 8px" }}>Standard clauses</h2>
        <div
          style={{
            background: `rgba(93,112,82,0.05)`,
            border: `1px solid rgba(93,112,82,0.2)`,
            borderRadius: 16,
            padding: 24,
            display: "flex",
            alignItems: "center",
            gap: 16,
          }}
        >
          <CheckCircle size={24} color={C.primary} style={{ flexShrink: 0 }} />
          <div>
            <h3 style={{ fontSize: 17, fontWeight: 700, color: C.primary, margin: "0 0 4px" }}>All standard clauses are in order</h3>
            <p style={{ color: C.fg, margin: 0, lineHeight: 1.6 }}>
              {result.standardCount} clauses cover standard definitions, jurisdiction, and severability. Nothing unusual detected.
            </p>
          </div>
        </div>
      </section>

      {/* Fixed follow-up input */}
      <div
        style={{
          position: "fixed",
          bottom: 24,
          left: 0,
          right: 0,
          paddingLeft: 16,
          paddingRight: 16,
          zIndex: 40,
          pointerEvents: "none",
        }}
      >
        <div style={{ maxWidth: 768, margin: "0 auto", pointerEvents: "auto", boxShadow: shadowOrganic, borderRadius: 9999 }}>
          <div
            style={{
              position: "relative",
              display: "flex",
              alignItems: "center",
              background: C.card,
              border: `2px solid ${C.border}`,
              borderRadius: 9999,
              padding: 8,
            }}
          >
            <input
              type="text"
              placeholder="Ask about this contract..."
              value={followUp}
              onChange={(e) => setFollowUp(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter" && followUp.trim()) {
                  window.location.href = `/${locale}/chat?q=${encodeURIComponent(followUp)}`;
                }
              }}
              style={{
                flex: 1,
                background: "transparent",
                border: "none",
                outline: "none",
                padding: "12px 24px",
                fontSize: 16,
                fontFamily: fontSans,
                color: C.fg,
                paddingRight: 96,
              }}
            />
            <div style={{ position: "absolute", right: 8, display: "flex", alignItems: "center", gap: 8 }}>
              <button
                style={{ width: 40, height: 40, borderRadius: 9999, background: C.muted, border: "none", cursor: "pointer", display: "flex", alignItems: "center", justifyContent: "center", color: C.primary }}
                aria-label="Voice input"
              >
                <Mic size={20} />
              </button>
              <button
                onClick={() => followUp.trim() && (window.location.href = `/${locale}/chat?q=${encodeURIComponent(followUp)}`)}
                style={{ width: 40, height: 40, borderRadius: 9999, background: C.primary, border: "none", cursor: "pointer", display: "flex", alignItems: "center", justifyContent: "center", color: C.primaryFg }}
                aria-label="Send"
              >
                <Send size={16} style={{ marginLeft: 1 }} />
              </button>
            </div>
          </div>
        </div>
      </div>
    </main>
  );
}
