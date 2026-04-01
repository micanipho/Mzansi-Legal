using Ardalis.GuardClauses;
using backend.Domains.QA;
using backend.Services.RagService.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.Services.RagService;

/// <summary>
/// Static helper for constructing the three-part RAG prompt sent to the LLM.
/// Owns all prompt-construction constants plus the mode-aware instructions used for
/// grounded answers, clarification questions, and deterministic insufficient responses.
/// </summary>
public static class RagPromptBuilder
{
    /// <summary>Strong-support similarity threshold used during confidence evaluation.</summary>
    public const float SimilarityThreshold = 0.7f;

    /// <summary>Minimum cosine similarity for the wider semantic candidate pool.</summary>
    public const float SemanticCandidateFloor = 0.45f;

    /// <summary>Maximum number of chunk candidates retained before document reranking.</summary>
    public const int MaxSemanticCandidates = 24;

    /// <summary>Maximum number of chunks passed to the LLM as context per question.</summary>
    public const int MaxContextChunks = 5;

    /// <summary>Maximum number of chunks selected from any single document.</summary>
    public const int MaxChunksPerDocument = 3;

    /// <summary>Minimum score for a document to qualify as a meaningful supporting source.</summary>
    public const float SupportingDocumentFloor = 0.58f;

    /// <summary>
    /// Returns the system message establishing the assistant's identity and citation rules.
    /// When <paramref name="language"/> is not English, appends a directive instructing the LLM
    /// to respond in the user's language while keeping Act names and section numbers in English.
    /// </summary>
    /// <param name="language">The detected language of the user's question. Defaults to English.</param>
    public static string BuildSystemPrompt(
        RagAnswerMode answerMode,
        Language language = Language.English)
    {
        var prompt =
            "You are a South African legal and financial assistant. " +
            "Your role is to help South African residents understand their legal rights and obligations.\n\n" +
            "CRITICAL RULES — follow these without exception:\n" +
            "1. You MUST ONLY answer using information from the legislation context provided below.\n" +
            "2. You MUST ALWAYS include a citation for every claim you make, " +
            "in the format: [Act Name, Section X].\n" +
            "3. If the context does not contain sufficient information to answer the question, " +
            "you MUST say that the available legislation is not enough.\n" +
            "4. Do NOT speculate, infer, or draw on general knowledge outside the provided context.\n" +
            "5. Write in plain, accessible English. Avoid legal jargon where a simpler word exists.";

        if (answerMode == RagAnswerMode.Cautious)
        {
            prompt += "\n6. Make the limits of the available legislation explicit before giving your grounded answer.";
        }

        if (answerMode == RagAnswerMode.Clarification)
        {
            prompt += "\n6. Ask exactly one focused follow-up question and do NOT provide a legal conclusion.";
        }

        var directive = GetLanguageDirective(language);
        if (!string.IsNullOrEmpty(directive))
            prompt += $"\n\n7. {directive}";

        return prompt;
    }

    /// <summary>
    /// Maps a <see cref="Language"/> enum value to its ISO 639-1 code string (e.g. Zulu → "zu").
    /// Returns "en" for English and any unrecognised value.
    /// </summary>
    public static string ToIsoCode(Language language) => language switch
    {
        Language.Zulu      => "zu",
        Language.Sesotho   => "st",
        Language.Afrikaans => "af",
        _                  => "en"
    };

    /// <summary>
    /// Returns the language-response directive for non-English languages, or an empty string for English.
    /// The directive instructs the LLM to answer in the user's language while keeping all
    /// Act names, section numbers, and legal citations in English.
    /// </summary>
    private static string GetLanguageDirective(Language language) => language switch
    {
        Language.Zulu      => "Respond in isiZulu. Keep all Act names, section numbers, and legal citations in English.",
        Language.Sesotho   => "Respond in Sesotho. Keep all Act names, section numbers, and legal citations in English.",
        Language.Afrikaans => "Respond in Afrikaans. Keep all Act names, section numbers, and legal citations in English.",
        _                  => string.Empty
    };

    /// <summary>
    /// Builds a numbered context block containing each chunk's Act name, section number, and content.
    /// Each block is labelled as <c>[ActName — SectionNumber]</c> followed by the chunk text.
    /// </summary>
    /// <param name="chunks">Scored chunks ordered by relevance descending. Must not be null.</param>
    /// <returns>A formatted multi-line string ready to be embedded in the user prompt.</returns>
    public static string BuildContextBlock(IEnumerable<RetrievedChunk> chunks)
    {
        Guard.Against.Null(chunks, nameof(chunks));

        var sb = new StringBuilder();
        foreach (var chunk in chunks)
        {
            sb.AppendLine($"[{chunk.ActName} - {chunk.SectionNumber}]");
            if (!string.IsNullOrWhiteSpace(chunk.SectionTitle))
            {
                sb.AppendLine($"Section title: {chunk.SectionTitle}");
            }
            sb.AppendLine(chunk.Excerpt);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Builds the user turn combining the retrieved context block and the original question.
    /// </summary>
    /// <param name="questionText">The user's question text. Must not be null or whitespace.</param>
    /// <param name="contextBlock">The formatted context string produced by <see cref="BuildContextBlock"/>.</param>
    /// <returns>The full user message string to pass to the LLM.</returns>
    public static string BuildUserPrompt(
        string questionText,
        string contextBlock,
        RagAnswerMode answerMode,
        string clarificationQuestion = null)
    {
        Guard.Against.NullOrWhiteSpace(questionText, nameof(questionText));
        Guard.Against.Null(contextBlock, nameof(contextBlock));

        if (answerMode == RagAnswerMode.Clarification)
        {
            var guidance = string.IsNullOrWhiteSpace(clarificationQuestion)
                ? "Ask one focused follow-up question that would let you identify the correct legal source."
                : $"Ask this follow-up question, or a tighter version of it: {clarificationQuestion}";

            return
                $"Legislation context:\n\n{contextBlock}\n\n" +
                $"Original question: {questionText}\n\n" +
                $"{guidance}\n\n" +
                "Return only the follow-up question.";
        }

        var answerLead = answerMode == RagAnswerMode.Cautious
            ? "Answer carefully, explain any limits in the available legislation, and include citations for every material claim."
            : "Answer directly using only the legislation context and include citations for every material claim.";

        return
            $"Legislation context:\n\n{contextBlock}\n\n" +
            $"Question: {questionText}\n\n" +
            $"{answerLead}\n\n" +
            "Answer:";
    }

    public static double GetChatTemperature(RagAnswerMode answerMode) => answerMode switch
    {
        RagAnswerMode.Direct => 0.2d,
        RagAnswerMode.Cautious => 0.1d,
        RagAnswerMode.Clarification => 0.0d,
        _ => 0.0d
    };

    public static string BuildClarificationLead(Language language) => language switch
    {
        Language.Zulu => "Ngingakusiza, kodwa ngidinga imininingwane eyodwa ngaphambi kokuba nginike impendulo ethembekile.",
        Language.Sesotho => "Nka thusa, empa ke hloka ntlha e le nngwe pele nka fana ka karabo e ka tsheptjwang.",
        Language.Afrikaans => "Ek kan help, maar ek het eers een detail nodig voordat ek 'n betroubare antwoord gee.",
        _ => "I can help with this, but I need one detail first before I give a reliable answer."
    };

    public static string BuildInsufficientResponse(Language language) => language switch
    {
        Language.Zulu => "Angikwazi ukuphendula lo mbuzo ngokuthembeka ngisebenzisa umthetho otholakalayo kuphela. Isiseko somthetho esitholakele asanele okwamanje.",
        Language.Sesotho => "Ha ke kgone ho araba potso ena ka tshepo ke itshetlehile feela ka molao o fumanehang. Motheo wa molao o fumanehileng ha o eso lekane hajoale.",
        Language.Afrikaans => "Ek kan nie hierdie vraag verantwoordelik beantwoord op grond van die beskikbare wetgewing alleen nie. Die huidige regsgrondslag is nog te swak.",
        _ => "I can't answer this responsibly from the available legislation alone. The current legal grounding is too weak."
    };
}
