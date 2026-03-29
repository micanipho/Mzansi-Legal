using backend.Domains.LegalDocuments;
using backend.Services.PdfIngestionService.DTO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace backend.Services.PdfIngestionService;

/// <summary>
/// Static helper class that implements the chunking strategies for SA legislation PDFs.
/// Extracted from PdfIngestionAppService to keep each class within the 350-line limit.
/// Contains: SA legislation regex patterns, section detection, section-level chunking,
/// fixed-size fallback chunking, subsection splitting, and token count estimation.
/// </summary>
internal static class PdfChunkingHelper
{
    // ── Constants ────────────────────────────────────────────────────────────

    /// <summary>Minimum number of detected sections required to use section-level chunking.</summary>
    internal const int MinSectionsForAuto = 3;

    /// <summary>Token threshold above which a detected section is split by subsection markers.</summary>
    internal const int MaxTokensPerChunk = 800;

    /// <summary>Window size in tokens for fixed-size fallback chunking.</summary>
    internal const int FixedChunkTokens = 500;

    /// <summary>Overlap in tokens between adjacent fixed-size chunks to preserve context at boundaries.</summary>
    internal const int OverlapTokens = 50;

    /// <summary>
    /// Approximate number of characters per token for English legal prose.
    /// Based on OpenAI cl100k_base tokenizer average of ~4 chars/token.
    /// </summary>
    internal const int CharsPerTokenEstimate = 4;

    // ── SA legislation regex patterns ────────────────────────────────────────

    /// <summary>
    /// Matches chapter boundaries in SA legislation format.
    /// Examples: "Chapter 2 — Fundamental Rights", "CHAPTER II - General Provisions"
    /// Group 1: chapter number (Arabic or Roman). Group 2: chapter title.
    /// </summary>
    private static readonly Regex ChapterPattern = new(
        @"^Chapter\s+(\d+|[IVX]+)\s*[\u2014\-\u2013]+\s*(.+)$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Matches section boundaries in SA legislation format.
    /// Examples: "12. Freedom of expression", "12A. Amendment", "Section 12. Rights"
    /// Group 1: section number (e.g., "12" or "12A"). Group 2: section title.
    /// </summary>
    private static readonly Regex SectionPattern = new(
        @"^(?:Section\s+)?(\d+[A-Z]?)\.\s+(.+)$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>
    /// Matches subsection markers at the start of a line or paragraph.
    /// Example: "(1) The employer shall..." matches with group 1 = "1".
    /// Used to split large sections that exceed MaxTokensPerChunk.
    /// </summary>
    private static readonly Regex SubsectionPattern = new(
        @"(?:^|\n)\((\d+)\)\s+",
        RegexOptions.Compiled);

    // ── Section detection ────────────────────────────────────────────────────

    /// <summary>
    /// Scans fullText for SA legislation chapter and section boundary markers.
    /// Each returned DetectedSection carries the most recently seen chapter title
    /// so that section-level chunks inherit their chapter context.
    /// Sections before any chapter match receive a null ChapterTitle — this is expected and handled.
    /// Returns an empty list when no section markers are found.
    /// </summary>
    internal static IReadOnlyList<DetectedSection> DetectSections(string fullText)
    {
        var chapterMatches = ChapterPattern.Matches(fullText);
        var sectionMatches = SectionPattern.Matches(fullText);

        if (sectionMatches.Count == 0)
        {
            return Array.Empty<DetectedSection>();
        }

        // Map each chapter match to its start index for O(n) chapter lookup.
        var chapterByIndex = new SortedList<int, string>(chapterMatches.Count);
        foreach (Match chapter in chapterMatches)
        {
            // Combine number and title into the full chapter identifier stored on chunks.
            var chapterTitle = $"Chapter {chapter.Groups[1].Value.Trim()} \u2014 {chapter.Groups[2].Value.Trim()}";
            chapterByIndex[chapter.Index] = chapterTitle;
        }

        var results = new List<DetectedSection>(sectionMatches.Count);
        string currentChapterTitle = null;
        int chapterCursor = 0;
        var chapterKeys = new List<int>(chapterByIndex.Keys);

        for (int i = 0; i < sectionMatches.Count; i++)
        {
            var sectionMatch = sectionMatches[i];

            // Advance cursor to the most recent chapter preceding this section.
            while (chapterCursor < chapterKeys.Count && chapterKeys[chapterCursor] <= sectionMatch.Index)
            {
                currentChapterTitle = chapterByIndex[chapterKeys[chapterCursor]];
                chapterCursor++;
            }

            // Group 1: section number; Group 2: section title — trimmed for clean metadata.
            var sectionNumber = sectionMatch.Groups[1].Value.Trim();
            var sectionTitle  = sectionMatch.Groups[2].Value.Trim();

            var endIndex = (i + 1 < sectionMatches.Count)
                ? sectionMatches[i + 1].Index
                : fullText.Length;

            results.Add(new DetectedSection(currentChapterTitle, sectionNumber, sectionTitle, sectionMatch.Index, endIndex));
        }

        return results;
    }

    // ── Chunk builders ───────────────────────────────────────────────────────

    /// <summary>
    /// Maps each DetectedSection to one or more DocumentChunkResults.
    /// Sections exceeding MaxTokensPerChunk are split at subsection markers.
    /// Assigns sequential SortOrder values (0-based) across all produced chunks.
    /// </summary>
    internal static List<DocumentChunkResult> BuildSectionChunks(
        IReadOnlyList<DetectedSection> sections,
        string actName,
        string fullText)
    {
        var chunks = new List<DocumentChunkResult>(sections.Count);

        foreach (var section in sections)
        {
            var sectionText = fullText.Substring(section.StartIndex, section.EndIndex - section.StartIndex).Trim();

            if (EstimateTokenCount(sectionText) > MaxTokensPerChunk)
            {
                // Large section: split at subsection markers and inherit parent metadata.
                chunks.AddRange(SplitLargeSectionBySubsections(section, sectionText, actName, chunks.Count));
            }
            else
            {
                chunks.Add(BuildSectionChunk(actName, section, sectionText, chunks.Count));
            }
        }

        return chunks;
    }

    /// <summary>
    /// Splits a single large section at subsection markers (N).
    /// If no subsection markers are present, returns the section as one oversized chunk.
    /// All sub-chunks inherit the parent section's chapter, section number, and section title.
    /// </summary>
    internal static List<DocumentChunkResult> SplitLargeSectionBySubsections(
        DetectedSection section,
        string sectionText,
        string actName,
        int sortOrderBase)
    {
        var subsectionMatches = SubsectionPattern.Matches(sectionText);

        if (subsectionMatches.Count == 0)
        {
            return new List<DocumentChunkResult> { BuildSectionChunk(actName, section, sectionText, sortOrderBase) };
        }

        var subChunks = new List<DocumentChunkResult>(subsectionMatches.Count);
        int localSortOrder = sortOrderBase;

        for (int i = 0; i < subsectionMatches.Count; i++)
        {
            var start   = subsectionMatches[i].Index;
            var end     = (i + 1 < subsectionMatches.Count) ? subsectionMatches[i + 1].Index : sectionText.Length;
            var subText = sectionText.Substring(start, end - start).Trim();

            if (!string.IsNullOrWhiteSpace(subText))
            {
                subChunks.Add(BuildSectionChunk(actName, section, subText, localSortOrder++));
            }
        }

        return subChunks;
    }

    /// <summary>
    /// Splits fullText into fixed-size sliding windows of FixedChunkTokens with OverlapTokens overlap.
    /// Used as a fallback when section-level detection finds fewer than MinSectionsForAuto sections.
    /// ChapterTitle, SectionNumber, and SectionTitle are null for all FixedSize chunks by design.
    /// ActName is always set from the caller to enable document attribution.
    /// </summary>
    internal static List<DocumentChunkResult> BuildFixedSizeChunks(string fullText, string actName)
    {
        var windowChars = FixedChunkTokens * CharsPerTokenEstimate;
        var stepChars   = (FixedChunkTokens - OverlapTokens) * CharsPerTokenEstimate;
        var chunks      = new List<DocumentChunkResult>();
        int position    = 0;
        int sortOrder   = 0;

        while (position < fullText.Length)
        {
            var content = fullText.Substring(position, Math.Min(windowChars, fullText.Length - position)).Trim();

            if (!string.IsNullOrWhiteSpace(content))
            {
                chunks.Add(new DocumentChunkResult
                {
                    ActName       = actName,
                    ChapterTitle  = null,
                    SectionNumber = null,
                    SectionTitle  = null,
                    Content       = content,
                    TokenCount    = EstimateTokenCount(content),
                    SortOrder     = sortOrder++,
                    Strategy      = ChunkStrategy.FixedSize
                });
            }

            position += stepChars;
        }

        return chunks;
    }

    // ── Utilities ────────────────────────────────────────────────────────────

    /// <summary>
    /// Estimates the number of tokens in a text using character-based approximation.
    /// Formula: (text.Length + 3) / CharsPerTokenEstimate.
    /// The +3 ensures rounding up rather than truncation.
    /// Accurate to within ~15% of the actual cl100k_base token count for English legal prose.
    /// Always returns at least 1 for non-empty text (minimum for a 1-char string is 1).
    /// </summary>
    internal static int EstimateTokenCount(string text)
    {
        return (text.Length + 3) / CharsPerTokenEstimate;
    }

    // ── Private factory ──────────────────────────────────────────────────────

    private static DocumentChunkResult BuildSectionChunk(
        string actName,
        DetectedSection section,
        string content,
        int sortOrder)
    {
        return new DocumentChunkResult
        {
            ActName       = actName,
            ChapterTitle  = section.ChapterTitle,
            SectionNumber = section.SectionNumber,
            SectionTitle  = section.SectionTitle,
            Content       = content,
            TokenCount    = EstimateTokenCount(content),
            SortOrder     = sortOrder,
            Strategy      = ChunkStrategy.SectionLevel
        };
    }
}

/// <summary>
/// Represents a single section boundary detected in a legislation PDF.
/// ChapterTitle is null when the section precedes any chapter heading in the document.
/// </summary>
internal sealed record DetectedSection(
    string ChapterTitle,
    string SectionNumber,
    string SectionTitle,
    int StartIndex,
    int EndIndex);
