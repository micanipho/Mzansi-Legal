import { Suspense } from "react";
import QaChatPage from "@/components/chat/QaChatPage";
import { ChatProvider } from "@/providers/chat-provider";

export default function AskPage() {
  return (
    <ChatProvider>
      <Suspense>
        <QaChatPage />
      </Suspense>
    </ChatProvider>
  );
}
