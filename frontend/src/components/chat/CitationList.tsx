"use client";

import { useState } from "react";
import { Book, ChevronDown, ChevronUp } from "lucide-react";
import { C, fontSans } from "@/styles/theme";
import type { RagCitationDto } from "@/services/qa.service";

interface CitationListProps {
  citations: RagCitationDto[];
}

export default function CitationList({ citations }: CitationListProps) {
  const [open, setOpen] = useState(false);
  const [expandedExcerpts, setExpandedExcerpts] = useState<Set<string>>(new Set());

  if (citations.length === 0) return null;

  const toggleExcerpt = (chunkId: string) => {
    setExpandedExcerpts((prev) => {
      const next = new Set(prev);
      if (next.has(chunkId)) {
        next.delete(chunkId);
      } else {
        next.add(chunkId);
      }
      return next;
    });
  };

  return (
    <div style={{ border: `1px solid ${C.border}`, borderRadius: 16, overflow: "hidden", marginTop: 8 }}>
      <button
        onClick={() => setOpen((v) => !v)}
        style={{
          width: "100%",
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          padding: "12px 16px",
          background: "rgba(240,235,229,0.3)",
          border: "none",
          cursor: "pointer",
          fontFamily: fontSans,
        }}
        aria-expanded={open}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 8, color: C.fg, fontWeight: 500, fontSize: 14 }}>
          <Book size={16} color={C.primary} />
          Legal Sources ({citations.length} {citations.length === 1 ? "section" : "sections"} cited)
        </div>
        {open ? <ChevronUp size={16} color={C.mutedFg} /> : <ChevronDown size={16} color={C.mutedFg} />}
      </button>

      {open && (
        <div
          style={{
            padding: 16,
            display: "flex",
            flexDirection: "column",
            gap: 16,
            background: C.card,
            borderTop: `1px solid ${C.border}`,
          }}
        >
          {citations.map((citation) => (
            <div key={citation.chunkId} style={{ borderLeft: `3px solid ${C.primary}`, paddingLeft: 12 }}>
              <div style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 4 }}>
                <span
                  style={{
                    background: "#DBEAFE",
                    color: "#1E40AF",
                    fontSize: 11,
                    fontWeight: 700,
                    padding: "2px 8px",
                    borderRadius: 9999,
                    fontFamily: fontSans,
                  }}
                >
                  {citation.actName}
                </span>
                <span style={{ fontWeight: 700, fontSize: 13, color: C.fg, fontFamily: fontSans }}>
                  {citation.sectionNumber}
                </span>
              </div>

              <button
                onClick={() => toggleExcerpt(citation.chunkId)}
                style={{
                  background: "transparent",
                  border: "none",
                  cursor: "pointer",
                  padding: 0,
                  fontSize: 12,
                  color: C.primary,
                  fontFamily: fontSans,
                  marginBottom: 4,
                }}
              >
                {expandedExcerpts.has(citation.chunkId) ? "Hide excerpt" : "View source excerpt"}
              </button>

              {expandedExcerpts.has(citation.chunkId) && (
                <p style={{ fontSize: 13, color: C.mutedFg, fontStyle: "italic", margin: 0, fontFamily: fontSans }}>
                  &ldquo;{citation.excerpt}&rdquo;
                </p>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
