using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace backend.Configuration;

public static class RightsAcademySeedData
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly Lazy<string> CatalogJson = new(() => JsonSerializer.Serialize(BuildCatalog(), SerializerOptions));

    public static string GetCatalogJson()
    {
        return CatalogJson.Value;
    }

    private static RightsAcademySeedCatalog BuildCatalog()
    {
        var tracks = new List<RightsAcademySeedTrack>
        {
            new()
            {
                TopicKey = "legal",
                CategoryName = "Constitutional Rights",
                SortOrder = 1,
                Lessons =
                {
                    CreateLesson(
                        id: "academy-legal-equality-dignity",
                        documentId: "11111111-1111-1111-1111-111111111111",
                        topicKey: "legal",
                        categoryName: "Constitutional Rights",
                        title: "Equality and dignity come first",
                        lawShortName: "Constitution",
                        lawTitle: "Constitution of the Republic of South Africa",
                        summary: "The Constitution protects equal treatment and dignity in everyday decisions by employers, landlords, schools, and service providers.",
                        explanation: "Sections 9 and 10 of the Constitution set the baseline for how people must be treated. If a rule, decision, or contract targets you unfairly because of who you are, or strips away your dignity, that is not just bad practice. It can be unconstitutional and may support a complaint or court challenge.",
                        sourceQuote: "Everyone is equal before the law and has the right to equal protection and benefit of the law.",
                        primaryCitation: "Constitution of the Republic of South Africa, sections 9 and 10",
                        askQuery: "Explain how the Constitution protects my dignity and equality in daily life.",
                        citations:
                        [
                            CreateCitation("11111111-aaaa-1111-aaaa-111111111111", "Constitution of the Republic of South Africa", "Sections 9 and 10", "The equality clause and dignity clause are the starting point whenever treatment is unfair or humiliating.")
                        ]),
                    CreateLesson(
                        id: "academy-legal-courts-and-fair-process",
                        documentId: "11111111-1111-1111-1111-111111111112",
                        topicKey: "legal",
                        categoryName: "Constitutional Rights",
                        title: "You can challenge unfair decisions",
                        lawShortName: "Constitution",
                        lawTitle: "Constitution of the Republic of South Africa",
                        summary: "You have a right to have legal disputes decided fairly by an independent tribunal or court.",
                        explanation: "Section 34 is one of the public's most practical rights. It means you are not expected to simply accept an unfair eviction, dismissal, debt step, or administrative decision. There must be a fair process, and the legal system must remain open to you when rights are affected.",
                        sourceQuote: "Everyone has the right to have any dispute that can be resolved by the application of law decided in a fair public hearing before a court.",
                        primaryCitation: "Constitution of the Republic of South Africa, section 34",
                        askQuery: "What does section 34 mean if I want to challenge an unfair decision?",
                        citations:
                        [
                            CreateCitation("11111111-bbbb-1111-bbbb-111111111111", "Constitution of the Republic of South Africa", "Section 34", "Section 34 protects access to a fair hearing before a court or another independent tribunal.")
                        ])
                }
            },
            new()
            {
                TopicKey = "employment",
                CategoryName = "Employment & Labour",
                SortOrder = 2,
                Lessons =
                {
                    CreateLesson(
                        id: "academy-employment-written-terms",
                        documentId: "22222222-2222-2222-2222-222222222221",
                        topicKey: "employment",
                        categoryName: "Employment & Labour",
                        title: "Your job terms should be in writing",
                        lawShortName: "BCEA",
                        lawTitle: "Basic Conditions of Employment Act",
                        summary: "Employees should receive clear written terms covering pay, hours, leave, and other key conditions.",
                        explanation: "The BCEA requires employers to give employees written particulars of employment and proper pay information. That matters because workers often get pressured with shifting terms or unclear deductions. Written terms help you compare what was promised with what is actually happening, and they become important evidence in disputes.",
                        sourceQuote: "An employer must supply an employee, when the employee commences employment, with the following particulars in writing.",
                        primaryCitation: "Basic Conditions of Employment Act, sections 29 and 33",
                        askQuery: "What written employment information must my employer give me under the BCEA?",
                        citations:
                        [
                            CreateCitation("22222222-aaaa-2222-aaaa-222222222222", "Basic Conditions of Employment Act", "Sections 29 and 33", "The Act requires written particulars of employment and proper information about remuneration and deductions.")
                        ]),
                    CreateLesson(
                        id: "academy-employment-dismissal",
                        documentId: "22222222-2222-2222-2222-222222222223",
                        topicKey: "employment",
                        categoryName: "Employment & Labour",
                        title: "A dismissal must be fair",
                        lawShortName: "LRA",
                        lawTitle: "Labour Relations Act",
                        summary: "Workers have the right not to be unfairly dismissed and can challenge dismissal through labour processes.",
                        explanation: "The Labour Relations Act says employees have the right not to be unfairly dismissed. Fairness usually requires both a fair reason and a fair process. If you are dismissed suddenly, without a hearing, or for a weak reason, the CCMA or bargaining council process may be available to challenge it.",
                        sourceQuote: "Every employee has the right not to be unfairly dismissed.",
                        primaryCitation: "Labour Relations Act, sections 185, 188 and 191",
                        askQuery: "How do I know whether my dismissal was unfair under the LRA?",
                        citations:
                        [
                            CreateCitation("22222222-bbbb-2222-bbbb-222222222222", "Labour Relations Act", "Sections 185, 188 and 191", "The LRA protects workers from unfair dismissal and sets out the dispute route for challenging it.")
                        ])
                }
            },
            new()
            {
                TopicKey = "housing",
                CategoryName = "Housing & Eviction",
                SortOrder = 3,
                Lessons =
                {
                    CreateLesson(
                        id: "academy-housing-eviction-court-order",
                        documentId: "33333333-3333-3333-3333-333333333331",
                        topicKey: "housing",
                        categoryName: "Housing & Eviction",
                        title: "You cannot be evicted without a court process",
                        lawShortName: "PIE",
                        lawTitle: "Prevention of Illegal Eviction from and Unlawful Occupation of Land Act",
                        summary: "A landlord or owner cannot lawfully remove you from a home without a court order and a just process.",
                        explanation: "Housing disputes often escalate through threats, lockouts, or utility cut-offs, but the legal position is stricter. PIE and section 26 of the Constitution require a court process before an eviction. The court must look at justice and fairness, which is especially important when children, age, disability, or long occupation are involved.",
                        sourceQuote: "No one may be evicted from their home, or have their home demolished, without an order of court made after considering all the relevant circumstances.",
                        primaryCitation: "Constitution section 26 and PIE section 4",
                        askQuery: "Can a landlord evict me or lock me out without a court order?",
                        citations:
                        [
                            CreateCitation("33333333-aaaa-3333-aaaa-333333333333", "Constitution of the Republic of South Africa", "Section 26(3)", "Evictions from a home require a court order after all relevant circumstances are considered."),
                            CreateCitation("33333333-bbbb-3333-bbbb-333333333333", "Prevention of Illegal Eviction from and Unlawful Occupation of Land Act", "Section 4", "PIE sets the court-driven eviction process and notice requirements.")
                        ]),
                    CreateLesson(
                        id: "academy-housing-lease-and-deposit",
                        documentId: "33333333-3333-3333-3333-333333333332",
                        topicKey: "housing",
                        categoryName: "Housing & Eviction",
                        title: "Lease terms and deposits should be handled fairly",
                        lawShortName: "RHA",
                        lawTitle: "Rental Housing Act",
                        summary: "Tenants are entitled to a written lease on request and fair handling of deposits, inspections, and disputes.",
                        explanation: "The Rental Housing Act gives practical protections around lease terms, inspections, deposits, and unfair practice complaints. It helps tenants push back when a landlord refuses to document the agreement, withholds deposit money unfairly, or uses vague lease wording to shift everything onto the tenant.",
                        sourceQuote: "A lease between a tenant and a landlord, subject to subsection (6), need not be in writing or be subject to a standard format.",
                        primaryCitation: "Rental Housing Act, section 5",
                        askQuery: "What protections do I have around rental deposits and written leases?",
                        citations:
                        [
                            CreateCitation("33333333-cccc-3333-cccc-333333333333", "Rental Housing Act", "Section 5", "Section 5 covers lease content, joint inspections, deposits, receipts, and related housing protections.")
                        ])
                }
            },
            new()
            {
                TopicKey = "consumer",
                CategoryName = "Consumer Rights",
                SortOrder = 4,
                Lessons =
                {
                    CreateLesson(
                        id: "academy-consumer-unfair-terms",
                        documentId: "44444444-4444-4444-4444-444444444441",
                        topicKey: "consumer",
                        categoryName: "Consumer Rights",
                        title: "Suppliers cannot rely on unfair contract terms",
                        lawShortName: "CPA",
                        lawTitle: "Consumer Protection Act",
                        summary: "Consumer contracts must be fair, understandable, and not one-sided in how risk is allocated.",
                        explanation: "The Consumer Protection Act is important beyond shopping. It also shapes service agreements, hidden fees, waiver clauses, and standard-form contracts. If a term is excessively one-sided, confusing, or shifts unreasonable risk onto the consumer, sections 48 and 49 may help challenge it or force better disclosure.",
                        sourceQuote: "A supplier must not offer to supply, supply, or enter into an agreement to supply, any goods or services on terms that are unfair, unreasonable or unjust.",
                        primaryCitation: "Consumer Protection Act, sections 48 and 49",
                        askQuery: "What makes a consumer contract term unfair under the CPA?",
                        citations:
                        [
                            CreateCitation("44444444-aaaa-4444-aaaa-444444444444", "Consumer Protection Act", "Sections 48 and 49", "The CPA limits unfair, unreasonable, or unjust terms and requires certain risky clauses to be drawn to the consumer's attention.")
                        ]),
                    CreateLesson(
                        id: "academy-consumer-quality-repair",
                        documentId: "44444444-4444-4444-4444-444444444442",
                        topicKey: "consumer",
                        categoryName: "Consumer Rights",
                        title: "Goods should work and services should meet a reasonable standard",
                        lawShortName: "CPA",
                        lawTitle: "Consumer Protection Act",
                        summary: "Consumers can demand quality goods and may have repair, replacement, or refund rights in the right circumstances.",
                        explanation: "Sections 55 and 56 of the CPA give consumers a practical route when products are faulty or not suitable for their ordinary purpose. The law is useful when a seller tries to push you into endless repairs, ignores short-term defects, or denies the implied warranty that comes with supply.",
                        sourceQuote: "Every consumer has a right to receive goods that are reasonably suitable for the purposes for which they are generally intended.",
                        primaryCitation: "Consumer Protection Act, sections 55 and 56",
                        askQuery: "What can I ask for when a product is defective under the CPA?",
                        citations:
                        [
                            CreateCitation("44444444-bbbb-4444-bbbb-444444444444", "Consumer Protection Act", "Sections 55 and 56", "The CPA gives consumers rights to safe, good-quality goods and creates an implied warranty of quality.")
                        ])
                }
            },
            new()
            {
                TopicKey = "debtCredit",
                CategoryName = "Debt & Credit",
                SortOrder = 5,
                Lessons =
                {
                    CreateLesson(
                        id: "academy-debt-credit-affordability",
                        documentId: "55555555-5555-5555-5555-555555555551",
                        topicKey: "debtCredit",
                        categoryName: "Debt & Credit",
                        title: "Credit should not be granted recklessly",
                        lawShortName: "NCA",
                        lawTitle: "National Credit Act",
                        summary: "Lenders must assess affordability before granting credit, and reckless credit can be challenged.",
                        explanation: "The National Credit Act expects providers to assess whether you can afford credit before approving it. When that does not happen, or when the lender ignores obvious signs of over-indebtedness, the agreement may be attacked as reckless credit. That can affect enforcement, fees, and repayment steps.",
                        sourceQuote: "A credit provider must not enter into a credit agreement without first taking reasonable steps to assess the proposed consumer's general understanding and debt repayment history.",
                        primaryCitation: "National Credit Act, sections 80 and 81",
                        askQuery: "What is reckless credit and how can I challenge it under the NCA?",
                        citations:
                        [
                            CreateCitation("55555555-aaaa-5555-aaaa-555555555555", "National Credit Act", "Sections 80 and 81", "The NCA links reckless credit to failed affordability checks and over-indebted lending.")
                        ]),
                    CreateLesson(
                        id: "academy-debt-credit-enforcement",
                        documentId: "55555555-5555-5555-5555-555555555552",
                        topicKey: "debtCredit",
                        categoryName: "Debt & Credit",
                        title: "Debt enforcement has steps and limits",
                        lawShortName: "NCA",
                        lawTitle: "National Credit Act",
                        summary: "Credit providers must follow proper notice and fee rules before enforcing many agreements.",
                        explanation: "The NCA does not allow every threatened repossession or summons to happen immediately. There are notice requirements, limits on what may be charged, and restrictions on the build-up of certain amounts. Section 129 notices and section 103(5) often matter when a consumer needs time to respond or wants to scrutinize the amount claimed.",
                        sourceQuote: "A credit provider may not commence any legal proceedings to enforce the agreement before first providing notice to the consumer.",
                        primaryCitation: "National Credit Act, sections 101, 103 and 129",
                        askQuery: "What steps must a lender follow before enforcing a debt under the NCA?",
                        citations:
                        [
                            CreateCitation("55555555-bbbb-5555-bbbb-555555555555", "National Credit Act", "Sections 101, 103 and 129", "The NCA regulates what can be charged and requires notice before many enforcement steps start.")
                        ])
                }
            },
            new()
            {
                TopicKey = "privacy",
                CategoryName = "Privacy & Data",
                SortOrder = 6,
                Lessons =
                {
                    CreateLesson(
                        id: "academy-privacy-lawful-collection",
                        documentId: "66666666-6666-6666-6666-666666666661",
                        topicKey: "privacy",
                        categoryName: "Privacy & Data",
                        title: "Your personal information cannot be collected without a proper basis",
                        lawShortName: "POPIA",
                        lawTitle: "Protection of Personal Information Act",
                        summary: "Organisations need a lawful reason to collect and use personal information, and they must tell you what they are doing.",
                        explanation: "POPIA matters any time an employer, school, landlord, lender, or app asks for personal information. The law requires a lawful ground for processing and clear notice about who is collecting the data, why it is needed, and what will happen if you do not provide it. Hidden or excessive data collection is exactly what the Act tries to limit.",
                        sourceQuote: "Personal information may only be processed if the processing complies with the conditions for the lawful processing of personal information.",
                        primaryCitation: "POPIA, sections 11 and 18",
                        askQuery: "When can someone lawfully collect and use my personal information under POPIA?",
                        citations:
                        [
                            CreateCitation("66666666-aaaa-6666-aaaa-666666666666", "Protection of Personal Information Act", "Sections 11 and 18", "POPIA requires a lawful basis for processing and obliges responsible parties to notify people about the collection.")
                        ]),
                    CreateLesson(
                        id: "academy-privacy-access-correction",
                        documentId: "66666666-6666-6666-6666-666666666662",
                        topicKey: "privacy",
                        categoryName: "Privacy & Data",
                        title: "You can ask to see and correct your data",
                        lawShortName: "POPIA",
                        lawTitle: "Protection of Personal Information Act",
                        summary: "POPIA gives people rights to access, correct, and in some cases object to how personal information is being used.",
                        explanation: "If a company, school, or employer holds inaccurate or outdated data about you, POPIA gives you practical rights to ask what they hold and to request correction or deletion where appropriate. These rights are important when poor records start affecting employment, credit, services, or reputational harm.",
                        sourceQuote: "A data subject, having provided adequate proof of identity, has the right to request whether a responsible party holds personal information about the data subject.",
                        primaryCitation: "POPIA, sections 23 and 24",
                        askQuery: "How do I use my POPIA rights to access or correct information about me?",
                        citations:
                        [
                            CreateCitation("66666666-bbbb-6666-bbbb-666666666666", "Protection of Personal Information Act", "Sections 23 and 24", "POPIA allows data subjects to request access to personal information and correction or deletion in the proper circumstances.")
                        ])
                }
            },
            new()
            {
                TopicKey = "safety",
                CategoryName = "Safety & Harassment",
                SortOrder = 7,
                Lessons =
                {
                    CreateLesson(
                        id: "academy-safety-protection-order",
                        documentId: "77777777-7777-7777-7777-777777777771",
                        topicKey: "safety",
                        categoryName: "Safety & Harassment",
                        title: "Harassment can be stopped through a protection order",
                        lawShortName: "PHA",
                        lawTitle: "Protection from Harassment Act",
                        summary: "The law provides a court-based route to seek protection from stalking, threats, and repeated unwanted conduct.",
                        explanation: "Harassment is not limited to one setting. It can happen at home, at work, online, or through people acting on someone else's behalf. The Protection from Harassment Act allows a person to apply for a protection order when conduct causes harm or inspires a reasonable belief that harm may follow. That makes it a practical safety tool, not just a criminal-law issue.",
                        sourceQuote: "A complainant may apply to the court for a protection order if any person is engaging or has engaged in harassment.",
                        primaryCitation: "Protection from Harassment Act, sections 2 and 3",
                        askQuery: "How do protection orders work under the Protection from Harassment Act?",
                        citations:
                        [
                            CreateCitation("77777777-aaaa-7777-aaaa-777777777777", "Protection from Harassment Act", "Sections 2 and 3", "The Act creates the process for obtaining a protection order against harassment.")
                        ]),
                    CreateLesson(
                        id: "academy-safety-interim-order",
                        documentId: "77777777-7777-7777-7777-777777777772",
                        topicKey: "safety",
                        categoryName: "Safety & Harassment",
                        title: "Courts can grant urgent interim protection",
                        lawShortName: "PHA",
                        lawTitle: "Protection from Harassment Act",
                        summary: "Where the facts justify it, a court can issue interim protection before the matter is finally decided.",
                        explanation: "Speed matters in harassment matters. The Act allows courts to grant interim protection orders when there is prima facie evidence and the balance of hardship favours immediate protection. That can be critical where ongoing threats, tracking, contact, or intimidation create real risk while a final hearing is still pending.",
                        sourceQuote: "The court must issue an interim protection order if it is satisfied that there is prima facie evidence that the respondent is engaging or has engaged in harassment.",
                        primaryCitation: "Protection from Harassment Act, section 3",
                        askQuery: "When can a court issue an interim protection order for harassment?",
                        citations:
                        [
                            CreateCitation("77777777-bbbb-7777-bbbb-777777777777", "Protection from Harassment Act", "Section 3", "Interim protection is available when the court sees prima facie harassment and urgency justifies it.")
                        ])
                }
            }
        };

        return new RightsAcademySeedCatalog
        {
            Tracks = tracks,
            TotalLessons = tracks.Sum(track => track.Lessons.Count)
        };
    }

    private static RightsAcademySeedLesson CreateLesson(
        string id,
        string documentId,
        string topicKey,
        string categoryName,
        string title,
        string lawShortName,
        string lawTitle,
        string summary,
        string explanation,
        string sourceQuote,
        string primaryCitation,
        string askQuery,
        List<RightsAcademySeedCitation> citations)
    {
        return new RightsAcademySeedLesson
        {
            Id = id,
            DocumentId = Guid.Parse(documentId),
            TopicKey = topicKey,
            CategoryName = categoryName,
            Title = title,
            LawShortName = lawShortName,
            LawTitle = lawTitle,
            Summary = summary,
            Explanation = explanation,
            SourceQuote = sourceQuote,
            PrimaryCitation = primaryCitation,
            AskQuery = askQuery,
            Citations = citations
        };
    }

    private static RightsAcademySeedCitation CreateCitation(string id, string actName, string sectionNumber, string excerpt)
    {
        return new RightsAcademySeedCitation
        {
            Id = Guid.Parse(id),
            ActName = actName,
            SectionNumber = sectionNumber,
            Excerpt = excerpt,
            RelevanceScore = 1m
        };
    }

    private sealed class RightsAcademySeedCatalog
    {
        public List<RightsAcademySeedTrack> Tracks { get; set; } = new();

        public int TotalLessons { get; set; }
    }

    private sealed class RightsAcademySeedTrack
    {
        public string TopicKey { get; set; }

        public string CategoryName { get; set; }

        public int SortOrder { get; set; }

        public List<RightsAcademySeedLesson> Lessons { get; set; } = new();
    }

    private sealed class RightsAcademySeedLesson
    {
        public string Id { get; set; }

        public Guid DocumentId { get; set; }

        public string TopicKey { get; set; }

        public string CategoryName { get; set; }

        public string Title { get; set; }

        public string LawShortName { get; set; }

        public string LawTitle { get; set; }

        public string Summary { get; set; }

        public string Explanation { get; set; }

        public string SourceQuote { get; set; }

        public string PrimaryCitation { get; set; }

        public string AskQuery { get; set; }

        public List<RightsAcademySeedCitation> Citations { get; set; } = new();
    }

    private sealed class RightsAcademySeedCitation
    {
        public Guid Id { get; set; }

        public string ActName { get; set; }

        public string SectionNumber { get; set; }

        public string Excerpt { get; set; }

        public decimal RelevanceScore { get; set; }
    }
}
