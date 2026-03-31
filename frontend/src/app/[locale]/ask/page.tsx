import { Suspense } from "react";
import QaChatPage from "@/components/chat/QaChatPage";

export default function AskPage() {
  return (
    <Suspense>
      <QaChatPage />
    </Suspense>
  );
}
