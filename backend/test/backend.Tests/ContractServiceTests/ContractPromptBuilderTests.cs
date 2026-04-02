#nullable enable
using backend.Domains.ContractAnalysis;
using backend.Domains.QA;
using backend.Services.ContractService;
using backend.Services.RagService;
using Shouldly;
using System;
using Xunit;

namespace backend.Tests.ContractServiceTests;

public class ContractPromptBuilderTests
{
    [Fact]
    public void BuildSystemPrompt_ForZulu_IncludesJsonAndLanguageDirective()
    {
        var prompt = ContractPromptBuilder.BuildSystemPrompt(Language.Zulu);

        prompt.ShouldContain("Return ONLY valid JSON");
        prompt.ShouldContain("Respond in isiZulu");
        prompt.ShouldContain("Keep Act names, section numbers, and clause excerpts in English");
    }

    [Fact]
    public void BuildUserPrompt_IncludesCoverageAndLegislationContext()
    {
        var context = new ContractLegislationContext(
            new[]
            {
                new RetrievedChunk(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Consumer Protection Act 68 of 2008",
                    "CPA",
                    "Consumer",
                    "Section 14",
                    "Fixed-term agreements",
                    "A consumer may cancel a fixed-term agreement on 20 business days' notice.",
                    "Contracts",
                    Array.Empty<string>(),
                    0.9f,
                    0.9f,
                    42,
                    "Consumer Protection Act 68 of 2008",
                    "Section 14",
                    RagSourceMetadata.BindingLaw,
                    RagSourceMetadata.Primary)
            },
            Array.Empty<RetrievedChunk>(),
            ContractCoverageState.PartialCoverage,
            "The current legislation corpus covers the main baseline issues.");

        var prompt = ContractPromptBuilder.BuildUserPrompt(
            "lease-agreement",
            ContractType.Lease,
            "The tenant must give three months' notice before cancellation.",
            context);

        prompt.ShouldContain("Coverage state: PartialCoverage");
        prompt.ShouldContain("Consumer Protection Act 68 of 2008");
        prompt.ShouldContain("The tenant must give three months' notice");
    }
}
