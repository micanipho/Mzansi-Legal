using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services.RagService;

public sealed class RagDocumentProfileBuilder
{
    public IReadOnlyList<DocumentProfile> Build(IEnumerable<IndexedChunk> indexedChunks)
    {
        Guard.Against.Null(indexedChunks, nameof(indexedChunks));

        return indexedChunks
            .GroupBy(chunk => chunk.DocumentId)
            .Select(BuildProfile)
            .ToList();
    }

    private static DocumentProfile BuildProfile(IGrouping<Guid, IndexedChunk> group)
    {
        var sample = group.First();
        var metadataPhrases = new HashSet<string>(StringComparer.Ordinal);
        var metadataTerms = new HashSet<string>(StringComparer.Ordinal);
        var authorityType = RagSourceMetadata.DeriveAuthorityType(
            sample.ActName,
            sample.ActShortName,
            sample.ActNumber);
        var sourceFamily = RagSourceMetadata.BuildSourceFamily(sample.ActName, sample.ActShortName);

        AddMetadata(sample.ActName, metadataPhrases, metadataTerms);
        AddMetadata(sample.ActShortName, metadataPhrases, metadataTerms);
        AddMetadata(sample.ActNumber, metadataPhrases, metadataTerms);
        AddMetadata(sample.CategoryName, metadataPhrases, metadataTerms);
        AddMetadata(sourceFamily, metadataPhrases, metadataTerms);

        foreach (var chunk in group)
        {
            AddMetadata(chunk.SectionTitle, metadataPhrases, metadataTerms);
            AddMetadata(RagSourceHintExtractor.ExtractLeadingHeading(chunk.Excerpt), metadataPhrases, metadataTerms);
            AddMetadata(chunk.TopicClassification, metadataPhrases, metadataTerms);

            foreach (var keyword in chunk.Keywords)
            {
                AddMetadata(keyword, metadataPhrases, metadataTerms);
            }
        }

        return new DocumentProfile(
            sample.DocumentId,
            sample.ActName,
            sample.ActShortName,
            sample.CategoryName,
            metadataTerms.ToArray(),
            metadataPhrases.ToArray(),
            BuildCentroidVector(group.Select(chunk => chunk.Vector).ToList()),
            authorityType,
            sourceFamily);
    }

    private static void AddMetadata(
        string rawValue,
        ISet<string> metadataPhrases,
        ISet<string> metadataTerms)
    {
        var normalized = RagSourceHintExtractor.Normalize(rawValue);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        metadataPhrases.Add(normalized);

        foreach (var term in RagSourceHintExtractor.TokenizeNormalized(normalized))
        {
            metadataTerms.Add(term);
        }

        foreach (var alias in RagSourceHintExtractor.BuildAliases(normalized))
        {
            metadataPhrases.Add(alias);
            metadataTerms.Add(alias);
        }
    }

    private static float[] BuildCentroidVector(IReadOnlyList<float[]> vectors)
    {
        if (vectors.Count == 0)
        {
            return Array.Empty<float>();
        }

        var dimensions = vectors[0].Length;
        var centroid = new float[dimensions];

        foreach (var vector in vectors)
        {
            for (var index = 0; index < dimensions; index++)
            {
                centroid[index] += vector[index];
            }
        }

        for (var index = 0; index < dimensions; index++)
        {
            centroid[index] /= vectors.Count;
        }

        return centroid;
    }
}

public sealed record DocumentProfile(
    Guid DocumentId,
    string ActName,
    string ActShortName,
    string CategoryName,
    IReadOnlyCollection<string> MetadataTerms,
    IReadOnlyCollection<string> MetadataPhrases,
    float[] Vector,
    string AuthorityType = RagSourceMetadata.BindingLaw,
    string SourceFamily = "");
