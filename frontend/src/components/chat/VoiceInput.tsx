"use client";

import { AudioOutlined, StopOutlined } from "@ant-design/icons";
import { Button, message } from "antd";
import { useRef, useState } from "react";
import { transcribeAudio } from "@/services/qaService";

interface VoiceInputProps {
  onTranscribed: (text: string, language: string) => void;
  disabled?: boolean;
  voiceLabel?: string;
  stopLabel?: string;
}

/**
 * Records audio via the browser MediaRecorder API, sends to the Whisper
 * transcription endpoint, and returns the transcribed text and detected language.
 * T019
 */
export default function VoiceInput({
  onTranscribed,
  disabled,
  voiceLabel = "Voice Input",
  stopLabel = "Stop Recording",
}: VoiceInputProps) {
  const [recording, setRecording] = useState(false);
  const mediaRecorder = useRef<MediaRecorder | null>(null);
  const chunks = useRef<Blob[]>([]);

  const startRecording = async () => {
    if (!navigator.mediaDevices?.getUserMedia) {
      message.error("Microphone access is not supported in this browser.");
      return;
    }

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      const recorder = new MediaRecorder(stream);
      chunks.current = [];

      recorder.ondataavailable = (e) => {
        if (e.data.size > 0) chunks.current.push(e.data);
      };

      recorder.onstop = async () => {
        const blob = new Blob(chunks.current, { type: "audio/webm" });
        stream.getTracks().forEach((t) => t.stop());

        try {
          const result = await transcribeAudio(blob);
          onTranscribed(result.text, result.language);
        } catch {
          message.error("Transcription failed. Please try again.");
        }
      };

      recorder.start();
      mediaRecorder.current = recorder;
      setRecording(true);
    } catch {
      message.error("Could not access the microphone. Please check permissions.");
    }
  };

  const stopRecording = () => {
    mediaRecorder.current?.stop();
    setRecording(false);
  };

  return (
    <Button
      icon={recording ? <StopOutlined /> : <AudioOutlined />}
      onClick={recording ? stopRecording : startRecording}
      disabled={disabled}
      danger={recording}
      aria-label={recording ? stopLabel : voiceLabel}
      aria-pressed={recording}
    >
      {recording ? stopLabel : voiceLabel}
    </Button>
  );
}
