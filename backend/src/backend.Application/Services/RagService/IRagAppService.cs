using backend.Services.RagService.DTO;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Services.RagService;

/// <summary>
/// Orchestrates retrieval-augmented generation (RAG) for South African legal Q&amp;A.
/// Loads legislation embeddings at startup, embeds questions, performs cosine similarity
/// retrieval, calls the LLM, and persists the resulting Q&amp;A chain.
/// </summary>
public interface IRagAppService
{
    /// <summary>
    /// Accepts a user's natural-language legal question and returns a grounded, cited answer.
    /// Performs in-memory cosine similarity search, calls GPT-4o, and persists the exchange.
    /// Returns <see cref="RagAnswerResult.IsInsufficientInformation"/> = <c>true</c>
    /// when no chunk scores ≥ 0.7 — no LLM call or DB write occurs in that case.
    /// </summary>
    /// <param name="request">The user's question. <see cref="AskQuestionRequest.QuestionText"/> must not be null or whitespace.</param>
    /// <returns>A <see cref="RagAnswerResult"/> with the answer text, citations, and chunk IDs.</returns>
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
