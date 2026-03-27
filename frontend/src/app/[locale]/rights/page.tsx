"use client";

import { useLocale } from "next-intl";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Minus, Plus, Share2, Play } from "lucide-react";
import { C, R, shadowOrganic, fontSerif, fontSans } from "@/styles/theme";

const FILTERS = ["All", "Employment", "Housing", "Consumer", "Debt & Credit", "Tax", "Privacy"];

interface RightCard {
  title: string;
  cite: string;
  desc: string;
  detail: string;
  quote: string;
  category: string;
  r: string;
}

const CARDS: RightCard[] = [
  {
    title: "You cannot be evicted without a court order",
    cite: "Constitution, Section 26(3)",
    desc: "A landlord cannot remove you without a court order under the PIE Act.",
    detail:
      "Even if you are behind on rent, a landlord cannot simply lock you out, change the locks, cut off your water or electricity, or remove your belongings. They must apply for an eviction order through a court under the Prevention of Illegal Eviction (PIE) Act. The court will consider all circumstances, including whether you have alternative accommodation, before granting an order.",
    quote:
      "\"No one may be evicted from their home, or have their home demolished, without an order of court made after considering all the relevant circumstances. No legislation may permit arbitrary evictions.\"",
    category: "Housing",
    r: R.o1,
  },
  {
    title: "Your employer must give you a payslip",
    cite: "BCEA, Section 33",
    desc: "You have the right to written particulars of your payment.",
    detail: "Every employer must give each employee a payslip when paying remuneration. It must include the employer's details, period covered, remuneration in money, deductions, and actual amount paid.",
    quote: "",
    category: "Employment",
    r: R.o2,
  },
  {
    title: "You can return defective goods within 6 months",
    cite: "CPA, Section 56",
    desc: "Suppliers must repair, replace, or refund unsafe or defective goods.",
    detail: "If goods are unsafe, defective or fail to meet quality standards within 6 months of delivery, you can return them without penalty and at the supplier's risk and expense, and demand a full refund, replacement, or repair.",
    quote: "",
    category: "Consumer",
    r: R.o3,
  },
  {
    title: "Maximum interest rates on loans are capped",
    cite: "NCA, Section 105",
    desc: "Credit providers cannot charge interest exceeding the prescribed limits.",
    detail: "The National Credit Regulator sets maximum interest rates for different credit products. No credit agreement may charge more than the prescribed rate, and any excess charge is void and unenforceable.",
    quote: "",
    category: "Debt & Credit",
    r: R.o4,
  },
  {
    title: "You can't be fired without a hearing",
    cite: "LRA, Section 188",
    desc: "Dismissals must be substantively and procedurally fair.",
    detail: "A dismissal is unfair if the employer fails to prove that it was for a fair reason and that a fair procedure was followed. You have the right to be told the reasons for a proposed dismissal and the right to respond.",
    quote: "",
    category: "Employment",
    r: R.o1,
  },
  {
    title: "Companies must get consent to use your data",
    cite: "POPIA, Section 11",
    desc: "Personal information may only be processed with your consent.",
    detail: "Personal information may only be processed if you consent, if it is necessary to carry out a contract, or if required by law. You can withdraw consent at any time, and the responsible party must stop processing.",
    quote: "",
    category: "Privacy",
    r: R.o2,
  },
];

export default function MyRightsPage() {
  const locale = useLocale();
  const router = useRouter();
  const [activeFilter, setActiveFilter] = useState("All");
  const [expanded, setExpanded]         = useState<string | null>(CARDS[0].title);

  const filtered = activeFilter === "All"
    ? CARDS
    : CARDS.filter((c) => c.category === activeFilter);

  return (
    <main
      style={{
        minHeight: "100vh",
        paddingTop: 96,
        paddingBottom: 80,
        paddingLeft: 16,
        paddingRight: 16,
        maxWidth: 1280,
        margin: "0 auto",
        display: "flex",
        flexDirection: "column",
        gap: 40,
        fontFamily: fontSans,
      }}
    >
      {/* Header */}
      <section style={{ textAlign: "center", maxWidth: 640, margin: "0 auto" }}>
        <h1 style={{ fontFamily: fontSerif, fontSize: "clamp(2rem,5vw,3rem)", fontWeight: 700, color: C.fg, marginBottom: 16 }}>
          Know your rights
        </h1>
        <p style={{ fontSize: 17, color: C.mutedFg }}>
          Explore your legal and financial rights by topic. Tap any card to learn more.
        </p>
      </section>

      {/* Knowledge score */}
      <section
        style={{
          background: C.muted,
          border: `1px solid ${C.border}`,
          borderRadius: R.o2,
          padding: 32,
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          gap: 24,
          flexWrap: "wrap",
          boxShadow: "0 1px 4px rgba(0,0,0,0.04)",
        }}
      >
        <div style={{ flex: 1, minWidth: 200 }}>
          <h2 style={{ fontSize: 17, fontWeight: 700, color: C.fg, marginBottom: 16, fontFamily: fontSans }}>
            Your knowledge score — you&rsquo;ve explored 7 of 20 rights topics
          </h2>
          <div style={{ height: 16, background: C.border, borderRadius: 9999, overflow: "hidden" }}>
            <div style={{ height: "100%", background: C.primary, borderRadius: 9999, width: "35%", transition: "width 1s ease" }} />
          </div>
        </div>
        <div style={{ fontFamily: fontSerif, fontSize: 48, fontWeight: 700, color: C.primary, flexShrink: 0 }}>
          35%
        </div>
      </section>

      {/* Filter tabs */}
      <section
        style={{
          display: "flex",
          gap: 12,
          overflowX: "auto",
          paddingBottom: 4,
        }}
        className="hide-scrollbar"
      >
        {FILTERS.map((f, i) => (
          <button
            key={f}
            onClick={() => setActiveFilter(f)}
            style={{
              whiteSpace: "nowrap",
              padding: "10px 24px",
              borderRadius: 9999,
              fontSize: 14,
              fontWeight: 700,
              border: `1px solid ${f === activeFilter ? C.primary : C.border}`,
              background: f === activeFilter ? C.primary : "transparent",
              color: f === activeFilter ? C.primaryFg : C.fg,
              cursor: "pointer",
              fontFamily: fontSans,
            }}
          >
            {f}
          </button>
        ))}
      </section>

      {/* Rights grid */}
      <section style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(320px, 1fr))", gap: 24 }}>
        {filtered.map((card) => {
          const isExpanded = expanded === card.title;
          return (
            <div
              key={card.title}
              style={{
                gridColumn: isExpanded ? "1 / -1" : undefined,
                background: C.card,
                border: `1px solid ${C.border}`,
                borderRadius: card.r,
                boxShadow: isExpanded ? shadowOrganic : "0 1px 4px rgba(0,0,0,0.04)",
                overflow: "hidden",
              }}
            >
              {/* Card header */}
              <div
                onClick={() => setExpanded(isExpanded ? null : card.title)}
                style={{
                  padding: "24px 32px",
                  display: "flex",
                  alignItems: "flex-start",
                  justifyContent: "space-between",
                  gap: 16,
                  cursor: "pointer",
                }}
              >
                <div>
                  <h3
                    style={{
                      fontFamily: fontSans,
                      fontSize: isExpanded ? 22 : 17,
                      fontWeight: 700,
                      color: C.fg,
                      margin: "0 0 8px",
                      lineHeight: 1.3,
                    }}
                  >
                    {card.title}
                  </h3>
                  <span style={{ fontSize: 13, fontWeight: 700, color: C.primary }}>{card.cite}</span>
                  {!isExpanded && (
                    <p style={{ fontSize: 14, color: C.mutedFg, margin: "8px 0 0", lineHeight: 1.5 }}>{card.desc}</p>
                  )}
                </div>
                <button
                  style={{
                    width: 40, height: 40,
                    borderRadius: 9999,
                    background: C.muted,
                    border: "none",
                    cursor: "pointer",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    color: C.fg,
                    flexShrink: 0,
                  }}
                  aria-label={isExpanded ? "Collapse" : "Expand"}
                >
                  {isExpanded ? <Minus size={20} /> : <Plus size={20} />}
                </button>
              </div>

              {/* Expanded content */}
              {isExpanded && (
                <div style={{ borderTop: `1px solid ${C.border}`, padding: "24px 32px", background: C.card }}>
                  <p style={{ fontSize: 17, color: C.fg, lineHeight: 1.7, marginBottom: 24 }}>
                    {card.detail}
                  </p>

                  {card.quote && (
                    <div
                      style={{
                        background: C.muted,
                        padding: 24,
                        borderRadius: 16,
                        borderLeft: `4px solid ${C.primary}`,
                        marginBottom: 32,
                      }}
                    >
                      <p style={{ fontFamily: fontSerif, fontSize: 17, color: C.mutedFg, fontStyle: "italic", margin: 0 }}>
                        {card.quote}
                      </p>
                    </div>
                  )}

                  <div style={{ display: "flex", flexWrap: "wrap", gap: 16 }}>
                    <button
                      onClick={() => router.push(`/${locale}/chat?q=${encodeURIComponent(`Tell me more about: ${card.title}`)}`)}
                      style={{
                        background: C.primary,
                        color: C.primaryFg,
                        padding: "12px 24px",
                        borderRadius: 9999,
                        fontSize: 14,
                        fontWeight: 700,
                        border: "none",
                        cursor: "pointer",
                        fontFamily: fontSans,
                      }}
                    >
                      Ask a follow-up
                    </button>
                    <button
                      style={{
                        background: "transparent",
                        border: `2px solid ${C.border}`,
                        color: C.fg,
                        padding: "12px 24px",
                        borderRadius: 9999,
                        fontSize: 14,
                        fontWeight: 700,
                        cursor: "pointer",
                        display: "flex",
                        alignItems: "center",
                        gap: 8,
                        fontFamily: fontSans,
                      }}
                    >
                      <Play size={16} /> Listen in isiZulu
                    </button>
                    <button
                      style={{
                        background: "transparent",
                        border: `2px solid ${C.border}`,
                        color: C.fg,
                        padding: "12px 24px",
                        borderRadius: 9999,
                        fontSize: 14,
                        fontWeight: 700,
                        cursor: "pointer",
                        display: "flex",
                        alignItems: "center",
                        gap: 8,
                        fontFamily: fontSans,
                      }}
                    >
                      <Share2 size={16} /> Share
                    </button>
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </section>
    </main>
  );
}
