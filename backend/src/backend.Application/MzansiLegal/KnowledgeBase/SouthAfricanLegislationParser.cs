using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace backend.MzansiLegal.KnowledgeBase;

/// <summary>
/// Parses South African legislation PDFs into structured chunks bounded by
/// CHAPTER and Section markers, enabling precise Act/section citations.
/// </summary>
public static class SouthAfricanLegislationParser
{
    // Matches: "CHAPTER 1", "CHAPTER ONE", "PART A"
    private static readonly Regex ChapterPattern =
        new(@"^(CHAPTER\s+[\dA-Z]+|PART\s+[A-Z]+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Matches: "1.", "12.", "Section 3", "3A."
    private static readonly Regex SectionPattern =
        new(@"^(Section\s+\d+[A-Z]?|\d+[A-Z]?\.)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static List<LegislationChunk> Parse(Stream pdfStream)
    {
        var chunks = new List<LegislationChunk>();
        var rawLines = ExtractLines(pdfStream);

        string currentChapter = string.Empty;
        string currentSection = string.Empty;
        string currentSectionTitle = string.Empty;
        var contentBuffer = new StringBuilder();
        int sortOrder = 0;

        void FlushChunk()
        {
            var content = contentBuffer.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(content) && !string.IsNullOrWhiteSpace(currentSection))
            {
                chunks.Add(new LegislationChunk
                {
                    ChapterTitle = currentChapter,
                    SectionNumber = currentSection,
                    SectionTitle = currentSectionTitle,
                    Content = content,
                    SortOrder = sortOrder++
                });
            }
            contentBuffer.Clear();
        }

        foreach (var line in rawLines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            if (ChapterPattern.IsMatch(trimmed))
            {
                FlushChunk();
                currentChapter = trimmed;
                currentSection = string.Empty;
                currentSectionTitle = string.Empty;
                continue;
            }

            var sectionMatch = SectionPattern.Match(trimmed);
            if (sectionMatch.Success)
            {
                FlushChunk();
                currentSection = sectionMatch.Value.TrimEnd('.', ' ');
                currentSectionTitle = trimmed.Substring(sectionMatch.Length).Trim();
                // Start content with the section title
                contentBuffer.AppendLine(currentSectionTitle);
                continue;
            }

            contentBuffer.AppendLine(trimmed);
        }

        FlushChunk();
        return chunks;
    }

    private static IEnumerable<string> ExtractLines(Stream pdfStream)
    {
        var lines = new List<string>();

        using var doc = PdfDocument.Open(pdfStream);
        foreach (Page page in doc.GetPages())
        {
            var words = page.GetWords();
            var lineBuffer = new StringBuilder();
            double lastY = -1;

            foreach (var word in words)
            {
                double currentY = Math.Round(word.BoundingBox.Bottom, 0);

                if (lastY >= 0 && Math.Abs(currentY - lastY) > 2)
                {
                    lines.Add(lineBuffer.ToString());
                    lineBuffer.Clear();
                }

                if (lineBuffer.Length > 0) lineBuffer.Append(' ');
                lineBuffer.Append(word.Text);
                lastY = currentY;
            }

            if (lineBuffer.Length > 0)
                lines.Add(lineBuffer.ToString());
        }

        return lines;
    }
}
