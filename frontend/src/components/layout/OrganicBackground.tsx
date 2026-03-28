import { alpha, RGB } from "@/styles/theme";

export default function OrganicBackground() {
  return (
    <div
      style={{
        position: "fixed",
        inset: 0,
        overflow: "hidden",
        pointerEvents: "none",
        zIndex: -10,
      }}
      aria-hidden="true"
    >
      <div
        style={{
          position: "absolute",
          inset: 0,
          background:
            "radial-gradient(circle at top left, rgba(255,255,255,0.66), transparent 36%)," +
            "radial-gradient(circle at bottom right, rgba(255,255,255,0.5), transparent 30%)",
        }}
      />
      <div
        style={{
          position: "absolute",
          top: "-10%",
          left: "-10%",
          width: "50vw",
          height: "50vw",
          background: alpha(RGB.primary, 0.08),
          filter: "blur(100px)",
          borderRadius: "60% 40% 30% 70% / 60% 30% 70% 40%",
        }}
      />
      <div
        style={{
          position: "absolute",
          top: "18%",
          right: "-5%",
          width: "40vw",
          height: "60vw",
          background: alpha(RGB.secondary, 0.08),
          filter: "blur(120px)",
          borderRadius: "60% 40% 30% 70% / 60% 30% 70% 40%",
        }}
      />
      <div
        style={{
          position: "absolute",
          bottom: "-20%",
          left: "10%",
          width: "60vw",
          height: "40vw",
          background: alpha(RGB.primary, 0.05),
          filter: "blur(100px)",
          borderRadius: "60% 40% 30% 70% / 60% 30% 70% 40%",
        }}
      />
      <div
        style={{
          position: "absolute",
          inset: 0,
          opacity: 0.18,
          mixBlendMode: "multiply",
          backgroundImage:
            "radial-gradient(circle at 20% 20%, rgba(44,44,36,0.08) 0 0.7px, transparent 0.8px)," +
            "radial-gradient(circle at 70% 35%, rgba(93,112,82,0.08) 0 0.6px, transparent 0.7px)," +
            "radial-gradient(circle at 45% 75%, rgba(193,140,93,0.08) 0 0.7px, transparent 0.8px)",
          backgroundSize: "14px 14px, 16px 16px, 18px 18px",
        }}
      />
    </div>
  );
}
