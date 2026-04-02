import Link from "next/link";
import { Home, Search } from "lucide-react";
import { C, fontSans, fontSerif, shadowOrganic } from "@/styles/theme";

export default function NotFound() {
  return (
    <main
      className="page-shell page-shell--compact"
      style={{
        display: "grid",
        placeItems: "center",
        minHeight: "100vh",
        fontFamily: fontSans,
      }}
    >
      <section
        className="surface-card grain-panel"
        style={{
          width: "100%",
          padding: 32,
          borderRadius: 32,
          boxShadow: shadowOrganic,
          display: "grid",
          gap: 18,
          textAlign: "center",
        }}
      >
        <span
          style={{
            color: C.primary,
            fontSize: 13,
            fontWeight: 800,
            letterSpacing: "0.08em",
            textTransform: "uppercase",
          }}
        >
          Error 404
        </span>
        <h1
          style={{
            margin: 0,
            color: C.fg,
            fontFamily: fontSerif,
            fontSize: "clamp(2.3rem, 7vw, 3.8rem)",
          }}
        >
          This page could not be found
        </h1>
        <p style={{ margin: 0, color: C.mutedFg, lineHeight: 1.7 }}>
          The link may be outdated, or the page might have moved. You can head
          back home or start a fresh question instead.
        </p>
        <div
          style={{
            display: "flex",
            gap: 12,
            justifyContent: "center",
            flexWrap: "wrap",
          }}
        >
          <Link
            href="/"
            style={{
              minHeight: 44,
              display: "inline-flex",
              alignItems: "center",
              gap: 8,
              padding: "12px 18px",
              borderRadius: 9999,
              textDecoration: "none",
              background: C.primary,
              color: C.primaryFg,
              fontWeight: 700,
            }}
          >
            <Home size={16} />
            Go home
          </Link>
          <Link
            href="/en/ask"
            style={{
              minHeight: 44,
              display: "inline-flex",
              alignItems: "center",
              gap: 8,
              padding: "12px 18px",
              borderRadius: 9999,
              textDecoration: "none",
              border: `1px solid ${C.border}`,
              color: C.fg,
              fontWeight: 700,
              background: "rgba(255,255,255,0.72)",
            }}
          >
            <Search size={16} />
            Ask a question
          </Link>
        </div>
      </section>
    </main>
  );
}
