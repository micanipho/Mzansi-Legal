"use client";

import { useState } from "react";
import { Send } from "lucide-react";
import { C, fontSans, shadowOrganic } from "@/styles/theme";

interface ChatInputProps {
  onSend: (text: string) => void;
  disabled?: boolean;
  placeholder?: string;
}

export default function ChatInput({
  onSend,
  disabled = false,
  placeholder = "Ask a legal question…",
}: ChatInputProps) {
  const [value, setValue] = useState("");

  const handleSend = () => {
    const trimmed = value.trim();
    if (!trimmed || disabled) return;
    onSend(trimmed);
    setValue("");
  };

  return (
    <div
      className="grain-panel"
      style={{
        position: "relative",
        display: "flex",
        alignItems: "center",
        background: C.card,
        border: `2px solid ${C.border}`,
        borderRadius: 28,
        padding: 8,
        boxShadow: shadowOrganic,
        width: "100%",
      }}
    >
      <input
        type="text"
        placeholder={placeholder}
        value={value}
        onChange={(e) => setValue(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === "Enter") handleSend();
        }}
        disabled={disabled}
        style={{
          flex: 1,
          minWidth: 0,
          background: "transparent",
          border: "none",
          outline: "none",
          padding: "14px 18px",
          fontSize: 16,
          fontFamily: fontSans,
          color: C.fg,
          paddingRight: 64,
        }}
        aria-label="Question input"
      />
      <button
        onClick={handleSend}
        disabled={disabled || !value.trim()}
        style={{
          position: "absolute",
          right: 8,
          width: 44,
          height: 44,
          borderRadius: 9999,
          background: C.primary,
          border: "none",
          cursor: disabled || !value.trim() ? "not-allowed" : "pointer",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          color: C.primaryFg,
          opacity: disabled || !value.trim() ? 0.5 : 1,
        }}
        aria-label="Send message"
      >
        <Send size={16} style={{ marginLeft: 1 }} />
      </button>
    </div>
  );
}
