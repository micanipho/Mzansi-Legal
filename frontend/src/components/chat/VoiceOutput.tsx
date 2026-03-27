"use client";

import { SoundOutlined } from "@ant-design/icons";
import { Button, message } from "antd";
import { useRef, useState } from "react";
import { textToSpeech } from "@/services/qaService";

interface VoiceOutputProps {
  text: string;
  language: string;
  autoPlay?: boolean;
}

/**
 * Converts text to speech via the backend TTS endpoint and plays it.
 * Supports autoPlay for accessibility settings.
 * T020
 */
export default function VoiceOutput({ text, language, autoPlay = false }: VoiceOutputProps) {
  const [playing, setPlaying] = useState(false);
  const audioRef = useRef<HTMLAudioElement | null>(null);

  const play = async () => {
    if (playing) {
      audioRef.current?.pause();
      setPlaying(false);
      return;
    }

    try {
      const blob = await textToSpeech(text, language);
      const url = URL.createObjectURL(blob);

      const audio = new Audio(url);
      audioRef.current = audio;

      audio.onended = () => {
        setPlaying(false);
        URL.revokeObjectURL(url);
      };

      await audio.play();
      setPlaying(true);
    } catch {
      message.error("Could not play audio response.");
      setPlaying(false);
    }
  };

  return (
    <Button
      icon={<SoundOutlined />}
      onClick={play}
      type={playing ? "primary" : "default"}
      size="small"
      aria-label={playing ? "Stop audio playback" : "Play answer aloud"}
    >
      {playing ? "Stop" : "Play"}
    </Button>
  );
}
