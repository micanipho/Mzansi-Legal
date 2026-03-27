using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenAI.Audio;
using System;
using System.Threading.Tasks;

namespace backend.Controllers.MzansiLegal
{
    [Route("api/app/voice")]
    [ApiController]
    [AbpMvcAuthorize]
    public class VoiceController : backendControllerBase
    {
        private readonly AudioClient _whisperClient;
        private readonly AudioClient _ttsClient;

        public VoiceController(IConfiguration configuration)
        {
            var apiKey = configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

            var openAiClient = new OpenAI.OpenAIClient(apiKey);
            _whisperClient = openAiClient.GetAudioClient("whisper-1");
            _ttsClient = openAiClient.GetAudioClient("tts-1");
        }

        /// <summary>
        /// POST /api/app/voice/transcribe
        /// Upload audio and receive transcribed text with detected language.
        /// </summary>
        [HttpPost("transcribe")]
        public async Task<TranscribeResponseDto> TranscribeAsync(IFormFile audio)
        {
            if (audio == null || audio.Length == 0)
                throw new ArgumentException("Audio file is required.");

            using (var stream = audio.OpenReadStream())
            {
                var options = new AudioTranscriptionOptions
                {
                    ResponseFormat = AudioTranscriptionFormat.Verbose,
                };

                var result = await _whisperClient.TranscribeAudioAsync(stream, audio.FileName, options);

                return new TranscribeResponseDto
                {
                    Text = result.Value.Text,
                    Language = result.Value.Language ?? "en"
                };
            }
        }

        /// <summary>
        /// POST /api/app/voice/speak
        /// Convert text to speech and return audio/mpeg stream.
        /// </summary>
        [HttpPost("speak")]
        public async Task<IActionResult> SpeakAsync([FromBody] SpeakRequestDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Text))
                return BadRequest("Text is required.");

            var options = new SpeechGenerationOptions
            {
                SpeedRatio = 1.0f
            };

            var result = await _ttsClient.GenerateSpeechAsync(
                input.Text,
                GeneratedSpeechVoice.Alloy,
                options);

            var bytes = result.Value.ToArray();
            return File(bytes, "audio/mpeg");
        }
    }

    public class TranscribeResponseDto
    {
        public string Text { get; set; }
        public string Language { get; set; }
    }

    public class SpeakRequestDto
    {
        public string Text { get; set; }
        public string Language { get; set; }
    }
}
