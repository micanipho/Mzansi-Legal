using Abp.Application.Services;
using backend.Services.RagService.DTO;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Services.RagService;

/// <summary>
/// Orchestrates retrieval-augmented generation (RAG) for South African legal Q&amp;A.
/// Loads legislation embeddings at startup, plans document-aware retrieval from user intent,
/// calls the LLM for grounded answers or clarification questions, and persists grounded answers.
/// </summary>
public interface IRagAppService : IApplicationService
{
    /// <summary>
    /// Accepts a user's natural-language legal question and returns a structured response.
    /// The pipeline detects whether the user wrote in English, isiZulu, Sesotho, or Afrikaans,
    /// translates non-English input to English for retrieval, and then instructs the answer model
    /// to respond in the user's language while keeping legal source references in English.
    /// The answer may be direct, cautious, clarification-seeking, or insufficient depending
    /// on how strongly the indexed legislation supports the question, whether official guidance
    /// supplements the law, and whether urgent risk indicators require a safer posture.
    /// </summary>
    /// <param name="request">
    /// The user's question. <see cref="AskQuestionRequest.QuestionText"/> must not be null or whitespace.
    /// <see cref="AskQuestionRequest.ConversationId"/> may be supplied to continue an existing conversation owned by the current user.
    /// </param>
    /// <returns>A <see cref="RagAnswerResult"/> with the answer text, citations, and persistence identifiers.</returns>
    Task<RagAnswerResult> AskAsync(AskQuestionRequest request);

    /// <summary>
    /// Returns the conversation history for the currently authenticated user,
    /// ordered by most recent first. Each item includes the first question text
    /// and the total number of questions in that conversation.
    /// </summary>
    Task<ConversationsListDto> GetConversationsAsync();

    /// <summary>
    /// Loads all <see cref="backend.Domains.LegalDocuments.DocumentChunk"/> embeddings into memory.
    /// Must be called once at application startup before the first <see cref="AskAsync"/> call.
    /// Safe to call multiple times — each call replaces the in-memory store.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the startup load if the host shuts down.</param>
    Task InitialiseAsync(CancellationToken cancellationToken = default);
}
