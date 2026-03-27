"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useLocale } from "next-intl";
import { useState } from "react";
import {
  Search, Mic, FileText, MessageCircle, TrendingUp,
  Shield, Home as HomeIcon, Briefcase, CreditCard,
  Calculator, Lock, AlertTriangle,
} from "lucide-react";
import { C, R, shadowOrganic, fontSerif, fontSans } from "@/styles/theme";

const STATS = [
  { num: "2,847", label: "Questions answered", r: R.o1 },
  { num: "13",    label: "Acts indexed",        r: R.o2 },
  { num: "4",     label: "Languages",           r: R.o3 },
  { num: "342",   label: "Contracts analysed",  r: R.o4 },
];

const CATEGORIES = [
  { Icon: Briefcase,     title: "Employment & Labour",   desc: "Dismissals, CCMA, leave, and contracts.",  tag: "Legal",     tagColor: `rgba(93,112,82,0.12)`,  tagText: C.primary, r: R.o1 },
  { Icon: HomeIcon,      title: "Housing & Eviction",    desc: "Leases, deposits, and tenant rights.",      tag: "Legal",     tagColor: `rgba(93,112,82,0.12)`,  tagText: C.primary, r: R.o2 },
  { Icon: Shield,        title: "Consumer Rights",        desc: "Returns, defective goods, and contracts.", tag: "Legal",     tagColor: `rgba(93,112,82,0.12)`,  tagText: C.primary, r: R.o3 },
  { Icon: CreditCard,    title: "Debt & Credit",          desc: "Loans, interest rates, and debt review.",  tag: "Financial", tagColor: `rgba(193,140,93,0.12)`, tagText: C.secondary, r: R.o4 },
  { Icon: Calculator,    title: "Tax",                    desc: "SARS, income tax, and deductions.",         tag: "Financial", tagColor: `rgba(193,140,93,0.12)`, tagText: C.secondary, r: R.o1 },
  { Icon: Lock,          title: "Privacy & Data",         desc: "POPIA, data breaches, and consent.",       tag: "Legal",     tagColor: `rgba(93,112,82,0.12)`,  tagText: C.primary, r: R.o2 },
  { Icon: FileText,      title: "Contract Analysis",      desc: "Review agreements for red flags.",          tag: "Contracts", tagColor: "rgba(109,40,217,0.08)", tagText: "#6D28D9", r: R.o3 },
  { Icon: TrendingUp,    title: "Insurance & Retirement", desc: "Claims, payouts, and provident funds.",    tag: "Financial", tagColor: `rgba(193,140,93,0.12)`, tagText: C.secondary, r: R.o4 },
  { Icon: AlertTriangle, title: "Safety & Harassment",    desc: "Protection orders and workplace safety.",  tag: "Legal",     tagColor: `rgba(93,112,82,0.12)`,  tagText: C.primary, r: R.o1 },
];

const TRENDING: Record<string, { q: string; cat: string }[]> = {
  en: [
    { q: "Can my employer force me to work on public holidays?",      cat: "Employment" },
    { q: "What is the maximum interest rate a mashonisa can charge?", cat: "Debt & Credit" },
    { q: "Can my landlord withhold my deposit?",                      cat: "Housing" },
    { q: "How do I apply for a protection order against harassment?", cat: "Safety" },
    { q: "What are my rights if I am retrenched?",                    cat: "Employment" },
  ],
  zu: [
    { q: "Ingabe umqashi wami angangiphoqa ukusebenza ezinsukwini zomphakathi?", cat: "Umsebenzi" },
    { q: "Ngingayibuyisela yini imoto engiyithenge isinamaphutha?",              cat: "Umuntu othengayo" },
    { q: "Ingabe umnikazi wendlu angabamba idipozithi yami?",                    cat: "Indlu" },
    { q: "Ngifaka kanjani isicelo sokuphephela ekuhlukunyezweni?",               cat: "Ukuphepha" },
    { q: "Yimaphi amalungelo ami uma ngixoshiwe emsebenzini?",                   cat: "Umsebenzi" },
  ],
  st: [
    { q: "Na mophatlalatsi wa ka a ka nkgatelela ho sebetsa matsatsi a boikhohomoso?", cat: "Mosebetsi" },
    { q: "Nka etsa eng ha landlord e sa batle ho kgutlisa deposit ya ka?",            cat: "Ntlo" },
    { q: "Ke bokae bo phahameng ba tswala ba ya ho lefshwa ke mashonisa?",            cat: "Mokoloto" },
    { q: "Ke etsa jwang kopo ya taelo ya tshireletso?",                               cat: "Tshireletso" },
    { q: "Ke na le ditokelo tse feng ha ke ntshwa mosebetsing?",                      cat: "Mosebetsi" },
  ],
  af: [
    { q: "Kan my werkgewer my dwing om op openbare vakansiedae te werk?",     cat: "Diens" },
    { q: "Wat is die maksimum rentekoers wat 'n mashonisa kan hef?",          cat: "Skuld" },
    { q: "Kan my verhuurder my deposito weerhou?",                            cat: "Behuising" },
    { q: "Hoe dien ek aansoek in vir 'n beskermingsbevel teen teistering?",   cat: "Veiligheid" },
    { q: "Wat is my regte as ek afgedank word?",                              cat: "Diens" },
  ],
};

export default function HomePage() {
  const router = useRouter();
  const locale = useLocale();
  const [query, setQuery] = useState("");

  const ask = (q?: string) => {
    const text = (q ?? query).trim();
    if (!text) return;
    router.push(`/${locale}/chat?q=${encodeURIComponent(text)}`);
  };

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
        gap: 96,
        fontFamily: fontSans,
      }}
    >
      {/* ── Hero ─────────────────────────────────────────────── */}
      <section style={{ display: "flex", flexDirection: "column", alignItems: "center", textAlign: "center", maxWidth: 768, margin: "0 auto", marginTop: 32 }}>
        <div
          style={{
            display: "inline-flex",
            alignItems: "center",
            gap: 8,
            background: C.muted,
            padding: "6px 16px",
            borderRadius: 9999,
            marginBottom: 32,
            border: `1px solid rgba(222,216,207,0.5)`,
          }}
        >
          <span style={{ width: 8, height: 8, borderRadius: "50%", background: C.primary, display: "inline-block" }} />
          <span style={{ fontSize: 14, fontWeight: 500, color: C.fg }}>
            Available in English, isiZulu, Sesotho &amp; Afrikaans
          </span>
        </div>

        <h1
          style={{
            fontFamily: fontSerif,
            fontSize: "clamp(3rem, 7vw, 4.5rem)",
            fontWeight: 800,
            color: C.primary,
            lineHeight: 1.1,
            marginBottom: 24,
            letterSpacing: "-0.02em",
          }}
        >
          Know your rights.
          <br />
          <span style={{ color: C.fg }}>In your language.</span>
        </h1>

        <p style={{ fontSize: 18, color: C.mutedFg, marginBottom: 40, maxWidth: 600 }}>
          AI-powered legal and financial guidance backed by actual South African legislation.
        </p>

        {/* Search bar */}
        <div style={{ width: "100%", maxWidth: 640, position: "relative", display: "flex", alignItems: "center", marginBottom: 24 }}>
          <div style={{ position: "absolute", left: 24, color: C.mutedFg, display: "flex" }}>
            <Search size={20} />
          </div>
          <input
            type="text"
            placeholder="Ask about your rights..."
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && ask()}
            style={{
              width: "100%",
              background: "#fff",
              border: `2px solid ${C.border}`,
              borderRadius: 9999,
              padding: "16px 160px 16px 56px",
              fontSize: 18,
              outline: "none",
              fontFamily: fontSans,
              color: C.fg,
            }}
          />
          <div style={{ position: "absolute", right: 8, display: "flex", alignItems: "center", gap: 8 }}>
            <button
              onClick={() => router.push(`/${locale}/chat`)}
              style={{
                width: 40, height: 40,
                borderRadius: 9999,
                background: C.muted,
                border: "none",
                cursor: "pointer",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                color: C.primary,
              }}
              aria-label="Voice search"
            >
              <Mic size={20} />
            </button>
            <button
              onClick={() => ask()}
              style={{
                background: C.primary,
                color: C.primaryFg,
                padding: "12px 24px",
                borderRadius: 9999,
                fontWeight: 700,
                fontSize: 14,
                border: "none",
                cursor: "pointer",
                fontFamily: fontSans,
              }}
            >
              Ask now
            </button>
          </div>
        </div>

        <div style={{ display: "flex", flexWrap: "wrap", justifyContent: "center", gap: 16, fontSize: 14 }}>
          {[
            { text: "Can my landlord evict me?",   href: `/${locale}/chat` },
            { text: "What are my CCMA rights?",    href: `/${locale}/chat` },
            { text: "Analyse my lease",            href: `/${locale}/contracts` },
          ].map(({ text, href }) => (
            <Link
              key={text}
              href={href}
              style={{
                color: C.mutedFg,
                borderBottom: `1px dotted ${C.mutedFg}`,
                textDecoration: "none",
              }}
            >
              {text}
            </Link>
          ))}
        </div>
      </section>

      {/* ── Stats ────────────────────────────────────────────── */}
      <section
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(2, 1fr)",
          gap: 16,
        }}
        className="md-grid-4"
      >
        {STATS.map(({ num, label, r }) => (
          <div
            key={label}
            style={{
              background: C.card,
              border: `1px solid ${C.border}`,
              padding: 24,
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              justifyContent: "center",
              textAlign: "center",
              borderRadius: r,
              boxShadow: shadowOrganic,
            }}
          >
            <span style={{ fontSize: 36, fontFamily: fontSerif, fontWeight: 700, color: C.primary, marginBottom: 8, display: "block" }}>
              {num}
            </span>
            <span style={{ fontSize: 14, fontWeight: 500, color: C.mutedFg }}>
              {label}
            </span>
          </div>
        ))}
      </section>

      {/* ── CTAs ─────────────────────────────────────────────── */}
      <section style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))", gap: 24 }}>
        <Link
          href={`/${locale}/contracts`}
          style={{
            background: C.card,
            border: `2px solid ${C.accent}`,
            padding: 32,
            borderRadius: R.o2,
            boxShadow: shadowOrganic,
            display: "flex",
            flexDirection: "column",
            gap: 16,
            textDecoration: "none",
          }}
        >
          <div style={{ width: 48, height: 48, background: C.accent, borderRadius: 9999, display: "flex", alignItems: "center", justifyContent: "center", color: C.secondary }}>
            <FileText size={24} />
          </div>
          <div>
            <h3 style={{ fontFamily: fontSerif, fontSize: 24, fontWeight: 700, color: C.fg, margin: "0 0 8px" }}>Analyse a contract</h3>
            <p style={{ color: C.mutedFg, margin: 0 }}>Upload a lease, employment, or credit agreement</p>
          </div>
        </Link>

        <Link
          href={`/${locale}/chat`}
          style={{
            background: C.card,
            border: `1px solid ${C.border}`,
            padding: 32,
            borderRadius: R.o1,
            boxShadow: shadowOrganic,
            display: "flex",
            flexDirection: "column",
            gap: 16,
            textDecoration: "none",
          }}
        >
          <div style={{ width: 48, height: 48, background: C.muted, borderRadius: 9999, display: "flex", alignItems: "center", justifyContent: "center", color: C.primary }}>
            <MessageCircle size={24} />
          </div>
          <div>
            <h3 style={{ fontFamily: fontSerif, fontSize: 24, fontWeight: 700, color: C.fg, margin: "0 0 8px" }}>Ask a question</h3>
            <p style={{ color: C.mutedFg, margin: 0 }}>Get cited answers in your language</p>
          </div>
        </Link>
      </section>

      {/* ── Browse by category ───────────────────────────────── */}
      <section>
        <h2 style={{ fontFamily: fontSerif, fontSize: 30, fontWeight: 700, color: C.fg, marginBottom: 32 }}>
          Browse by category
        </h2>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(260px, 1fr))", gap: 24 }}>
          {CATEGORIES.map(({ Icon, title, desc, tag, tagColor, tagText, r }) => (
            <Link
              key={title}
              href={`/${locale}/rights`}
              style={{
                background: C.card,
                border: `1px solid ${C.border}`,
                padding: 24,
                display: "flex",
                flexDirection: "column",
                alignItems: "flex-start",
                gap: 16,
                textDecoration: "none",
                borderRadius: r,
                boxShadow: "0 1px 4px rgba(0,0,0,0.04)",
              }}
            >
              <div style={{ display: "flex", justifyContent: "space-between", width: "100%", alignItems: "flex-start" }}>
                <div style={{ padding: 12, borderRadius: 9999, background: tagColor, color: tagText }}>
                  <Icon size={20} />
                </div>
                <span style={{ fontSize: 11, fontWeight: 700, padding: "4px 12px", borderRadius: 9999, background: tagColor, color: tagText }}>
                  {tag}
                </span>
              </div>
              <div>
                <h3 style={{ fontSize: 17, fontWeight: 700, color: C.fg, margin: "0 0 4px", fontFamily: fontSans }}>{title}</h3>
                <p style={{ fontSize: 14, color: C.mutedFg, margin: 0 }}>{desc}</p>
              </div>
            </Link>
          ))}
        </div>
      </section>

      {/* ── Trending ─────────────────────────────────────────── */}
      <section
        style={{
          background: C.card,
          border: `1px solid ${C.border}`,
          borderRadius: 24,
          padding: "32px 40px",
          boxShadow: shadowOrganic,
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 24 }}>
          <TrendingUp size={22} color={C.secondary} />
          <h2 style={{ fontFamily: fontSerif, fontSize: 26, fontWeight: 700, color: C.fg, margin: 0 }}>
            What South Africans are asking
          </h2>
        </div>
        <div style={{ display: "flex", flexDirection: "column" }}>
          {(TRENDING[locale] ?? TRENDING.en).map(({ q, cat }, i) => (
            <div
              key={i}
              style={{ borderBottom: i < (TRENDING[locale] ?? TRENDING.en).length - 1 ? `1px solid ${C.border}` : undefined }}
            >
            <button
              onClick={() => ask(q)}
              style={{
                display: "flex",
                alignItems: "center",
                gap: 16,
                padding: "20px 0",
                background: "transparent",
                border: "none",
                cursor: "pointer",
                textAlign: "left",
                width: "100%",
              }}
            >
              <div
                style={{
                  width: 40, height: 40,
                  flexShrink: 0,
                  background: `rgba(93,112,82,0.1)`,
                  color: C.primary,
                  fontFamily: fontSerif,
                  fontWeight: 700,
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  borderRadius: R.o1,
                  fontSize: 16,
                }}
              >
                {i + 1}
              </div>
              <p style={{ flex: 1, fontSize: 16, fontWeight: 500, color: C.fg, margin: 0, lineHeight: 1.4 }}>{q}</p>
              <span
                style={{
                  fontSize: 13,
                  color: C.mutedFg,
                  background: C.muted,
                  padding: "4px 12px",
                  borderRadius: 9999,
                  whiteSpace: "nowrap",
                }}
              >
                {cat}
              </span>
            </button>
            </div>
          ))}
        </div>
      </section>
    </main>
  );
}
