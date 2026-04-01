# South African Legal Corpus for a Non‑Lawyer AI Assistant

## Executive summary

This report identifies the South African legal *documents* (beyond the already-considered Legal Practice Act / POPIA materials) that should form a legally grounded corpus for a RAG assistant aimed at ordinary, non‑lawyer users in South Africa. The recommended strategy is to ingest “domain bundles” that pair: (1) **primary legislation + rules/regulations**, (2) **official procedural forms and government guidance**, and (3) **a small set of leading apex-court judgments** that concretely interpret/operationalize those laws—especially where courts have developed standards like “just and equitable,” “procedural fairness,” or arrest/bail discretion. This keeps retrieval efficient while preserving legal reliability through authoritative citation chains. citeturn30view0turn27view0turn33view3turn44view0

Phase 1 should prioritize **high-frequency user journeys with public forms and standardized processes**: tenant disputes/evictions (Rental Housing Act + PIE Act), labour dismissal disputes (LRA + BCEA + CCMA rules/forms via Government Gazette), domestic violence/harassment protection orders (Domestic Violence Act + Protection from Harassment Act + DOJ forms), small claims (Small Claims Courts Act + DOJ small claims guidance + rules), administrative review and access-to-information (PAJA materials + PAIA + Information Regulator forms/guides), and baseline criminal procedure rights (Criminal Procedure Act + leading rights case law). citeturn31view0turn30view0turn27view0turn28view1turn32view0turn34view0turn22search5turn23search6turn41search2turn44view0

A critical implementation nuance: several “officially hosted” consolidated PDFs include publisher copyright watermarks (notably **entity["company","Juta and Company (Pty) Ltd","legal publisher, south africa"]**) even when the *law itself* is public; treat such PDFs as **higher licensing risk for ingestion** and prefer Government Gazette PDFs on gov.za when feasible (or obtain explicit permission/license for the formatted compilation you ingest). citeturn38view0turn39view0turn32view2turn34view0

Unspecified constraints that materially affect corpus design: (a) budget for paid databases (Juta/LexisNexis headnotes and law reports), (b) whether the product must support languages beyond English, (c) whether the assistant must provide “how to litigate” step-by-step workflows vs. high-level information and referrals, and (d) whether you will redistribute text or only provide snippets with links/citations. This report assumes English-first and a public-sources-first approach. citeturn25search11turn41search2turn22search5

## Purpose and scope

### User problems this corpus should support

Your stated target problems (ordinary users, no legal training) map cleanly to a small number of repeatable procedural pathways and therefore to document bundles:

- **Tenant disputes and evictions**: tenant/landlord rights and duties; rental tribunal processes; eviction procedures and required notices; “just and equitable” factors in eviction litigation. citeturn31view0turn30view0turn14search1turn17search0  
- **Labour dismissals**: “unfair dismissal” definitions/standards; CCMA referral and dispute resolution workflow; notice/severance basics; review standards for arbitration awards. citeturn27view0turn28view1turn14search0  
- **Domestic violence / harassment**: protection order workflows (interim → return date → final); police duties; warrants tied to orders; official protection-order forms. citeturn33view3turn34view0turn42search0  
- **Small claims**: jurisdiction, representation limits, procedure, enforcement/“execution,” and official clerks/commissioner guidance. citeturn38view0turn22search5  
- **Administrative review**: requests for reasons, internal remedies, and judicial review concepts (PAJA), plus access-to-information procedures (PAIA) and standardized forms. citeturn23search6turn18search0turn41search2  
- **Criminal rights**: arrest without warrant, post‑arrest procedure, bail standards, and confession admissibility; plus leading constitutional jurisprudence that ordinary users commonly rely on when asserting rights. citeturn44view0turn44view3turn44view4turn39view1turn19search13  
- **Tax queries**: dispute-resolution steps and timing (objection/appeal), “pay now argue later” mechanics, and practical SARS-admin guidance on ADR and extensions. citeturn40view2turn40view3turn40view0turn43search2turn43search1  

### Unspecified problem areas

Not specified (but commonly demanded in public-facing legal assistants) include consumer debt/credit enforcement (NCA s129 notices, debt review), family law beyond protection orders (divorce, parental responsibilities), immigration and asylum, wills/estates, road traffic fines, and social grants. If these are in scope later, treat them as separate domain bundles because they introduce distinct tribunals, statutes, and form sets. citeturn23search6turn38view0

## Priority primary legal documents to ingest

This section lists the statutes/rules that most directly power the listed user problems. “Official source” is satisfied through citations to official gov.za / justice.gov.za / regulator sites, rather than raw URLs.

### Phase 1 statutes, rules, and key sections to index

The Phase 1 set below is chosen for (i) high real-world demand, (ii) strong availability of public forms/guides, and (iii) “procedural” clarity that supports reliable, step-by-step answers with citations.

| Domain | Statute / instrument | What it supports | Key sections to index (minimal, high-yield) | Official source | Freshness & update notes | Licensing / ingestion risk notes |
|---|---|---|---|---|---|---|
| Tenant disputes / eviction | Rental Housing Act 50 of 1999 | Tenant/landlord rights, leases, rental-housing tribunal complaint process | Ch 3: **s 4** (general provisions/rights), **s 5** (leases); Ch 4: tribunal structure; **s 13** (complaints/rulings) | gov.za PDF citeturn26view2turn31view0turn31view3 | gov.za attachment is a Gazette-era text; treat as potentially not reflecting every later amendment unless version-controlled against Gazette amendments. citeturn26view2 | Lower risk than commercial databases, but still confirm reuse terms for bulk ingestion if redistributing; safest posture is “index + cite + link.” citeturn26view2 |
| Tenant disputes / eviction | PIE Act 19 of 1998 (Prevention of Illegal Eviction…) | Eviction procedure; notice requirements; “just and equitable” factors | **s 4** (owner eviction proceedings, notice), **s 5** (urgent eviction), **s 6** (state eviction), **s 7** (mediation) | gov.za PDF citeturn29view0turn30view0turn30view1 | Short statute; stable core, but interpretation is heavily case-driven; pair with leading Constitutional Court cases. citeturn14search1turn15search10 | Prefer pairing statute text with paragraph-cited case law to reduce “template hallucination” in eviction notices. citeturn30view0turn14search1 |
| Labour dismissals | Labour Relations Act 66 of 1995 | Unfair dismissal framework; CCMA referral path; remedies | Ch VIII: **ss 185–195**; especially **ss 185–188** and **s 191**; plus linked provisions on dispute referral and representation rules around s 191 disputes | gov.za PDF citeturn26view0turn27view0turn27view3 | gov.za copy is original Act publication; ensure you track later amendments separately where needed for production reliability. citeturn26view0 | Use with CCMA rules/forms (below) so the assistant can produce procedure-aligned guidance grounded in Government Gazette instruments. citeturn40view2turn13search0 |
| Labour dismissals | Basic Conditions of Employment Act 75 of 1997 | Leave, notice, severance basics commonly asked in dismissals | **s 20–21** (annual leave), Ch 5: **ss 37–39** (notice/payment), **s 41** (severance) | gov.za PDF citeturn26view1turn28view1turn28view2turn28view3 | BCEA is frequently amended; treat this as “base text” and add an amendment-tracking policy. citeturn26view1 | Low-to-medium risk; prefer official gov.za PDFs. citeturn26view1 |
| Labour dismissals procedure | CCMA Rules (Government Gazette) | How conciliation/arbitration processes run; timelines; service/filing norms | Index rule headings + defined time periods; treat as a “procedural” corpus separate from statutes | gov.za Gazette PDF (48445 / GN 3318) citeturn13search0turn12search1 | Rules change over time; build a Gazette-monitoring feed for replacements/supersessions. citeturn13search0 | Gazette PDFs are preferable to third‑party reproductions. citeturn13search0 |
| Labour forms | CCMA / LRA forms (Government Gazette forms set) | Referral forms and standardized pleadings that enable reliable templates | Index each form as a structured object (fields + instructions), not only free text | gov.za Gazette PDF (48445 / GN 3317) citeturn10search2turn12search1 | Same monitoring as CCMA rules; forms are a high UX leverage point. citeturn12search1 | Prefer Gazette originals; avoid unofficial “download mirrors.” citeturn10search2 |
| Domestic violence | Domestic Violence Act 116 of 1998 | Protection order process; police duties; improved mechanisms (e.g., safety monitoring notices) | **s 2** (duty to assist), **s 3** (arrest without warrant), **s 4–6** (applications/interim/final process), **s 8** (warrant of arrest), plus **ss 2A–2B** (added obligations/reporting) | justice.gov.za PDF citeturn32view0turn33view3turn33view1 | DOJ copy explicitly notes update to **GG 48419 (14 Apr 2023)** and lists amendments including Domestic Violence Amendment Act 14 of 2021 (some provisions not yet proclaimed). citeturn32view0 | Treat as authoritative, but confirm whether the specific PDF edition carries third-party rights; some DOJ-hosted PDFs include publisher notices in other Acts. citeturn38view0turn39view0 |
| Domestic violence forms | DOJ Domestic Violence forms set | Reliable templates for interim orders, notices, subpoenas, monitoring notices | Index each form number + fields; align the assistant’s template outputs to exact form language | DOJ forms page citeturn42search0 | Forms can change with amendments; crawl and diff by form number and last-modified. citeturn42search0 | Low risk vs commercial; still keep “source-of-truth” links in outputs. citeturn42search0 |
| Harassment | Protection from Harassment Act 17 of 2011 | Protection orders outside “domestic relationship”; cyber-harassment process | **s 2** (apply), **s 3** (interim order), **s 4** (electronic service provider info), **s 9** (final order), **s 11** (warrant), **s 13** (variation), **s 17–18** (appeal/offences) | justice.gov.za PDF citeturn32view1turn34view0turn34view3 | DOJ/PDF includes governance-transfer note (Proclamation 199 in GG 51368 of 11 Oct 2024) in the text extract—capture “administrative authority” metadata when present. citeturn34view0 | The PDF extract shows a publisher copyright line; treat that specific formatted compilation as higher-risk for bulk ingestion unless permission is clear. citeturn34view0 |
| Small claims | Small Claims Courts Act 61 of 1984 | Eligibility, representation limits, jurisdiction, procedure, enforcement | **s 2** (establishment), **s 7** (appearance/representation rules), Ch III (**ss 12–24**) jurisdiction, Ch V (**ss 26–33**) procedure/evidence, Ch VI (**ss 34–37**) judgment/costs, Ch VII (**ss 38–44**) execution, Ch VIII (**ss 45–46**) review | justice.gov.za PDF citeturn38view0 | This DOJ-hosted copy contains an amendment/commencement table; capture as “version metadata” rather than assuming it’s current. citeturn38view0 | The PDF includes a publisher copyright line; if you ingest the *text as displayed*, obtain clarity/permission or reconstruct from Gazette originals. citeturn38view0 |
| Small claims guides | DOJ Small Claims Court guidance page | Plain-language process guides and commissioner/clerk guidance | Index headings + checklists + referenced tariffs (as guidance, not “law”) | DOJ small claims page citeturn22search5 | Guidance references tariffs updated by Government Notices; adopt a freshness rule for these citations. citeturn22search5 | Guidance is not law but is core to user success; label it “procedural guidance.” citeturn22search5 |
| Administrative review | PAJA (Promotion of Administrative Justice Act 3 of 2000) + citizen materials | Requests for reasons, procedural fairness, internal remedies, review | Core procedural fairness and review provisions (commonly **ss 3–8**) plus definitions; also ingest citizen/NGO booklets and forms | justice.gov.za Act PDF + citizen booklet citeturn23search4turn23search6turn23search7 | Pair the Act with DOJ’s “Info for Citizens” materials to reduce misapplication by non-lawyers. citeturn23search6turn23search7 | Low-to-medium risk; DOJ training materials are designed for public comprehension. citeturn23search6 |
| Access to information | PAIA (Promotion of Access to Information Act 2 of 2000) + Information Regulator forms/guide | PAIA requests, internal appeals, complaints, and standardized forms | Index PAIA Act (especially request/appeal mechanisms) and the Information Regulator **PAIA forms 1–5** + PAIA Guide | gov.za PAIA Act page + Regulator forms + guide citeturn3search2turn41search2turn41search4turn41search10 | The Regulator publishes updated manuals/guides; treat these as high-freshness documents. citeturn41search9turn41search4 | These are official public forms/guides; safest to ingest with “versioned form IDs.” citeturn41search2 |
| Criminal procedure | Criminal Procedure Act 51 of 1977 | Arrest, custody, bail, confession admissibility | **s 40** (arrest w/o warrant), **s 50** (post-arrest procedure), **s 59–60** (bail), **s 217** (confessions), plus schedule references used in bail analyses | justice.gov.za PDF citeturn39view0turn44view0turn44view3turn44view4 | CPA is heavily amended; DOJ/PDF includes extensive amendment table—capture as metadata and add an amendment-monitoring plan. citeturn39view0 | DOJ-hosted PDF includes publisher copyright line; treat edition rights as nontrivial for ingestion at scale. citeturn39view0 |
| Tax disputes | Tax Administration Act 28 of 2011 | Objections/appeals, ADR, payment pending dispute | Ch 9 (dispute resolution); **s 104** (objection), **s 107** (appeal), **s 164** (payment pending objection/appeal) | gov.za PDF citeturn40view0turn40view2turn40view3turn40view3 | Core dispute mechanics are stable but procedural rules and SARS guidance evolve; monitor SARS and Gazette updates. citeturn43search2turn43search1 | Low risk for statute; pair with SARS guidance clearly labeled as guidance (not binding law). citeturn43search2turn43search1 |

### Phase 2 expansion documents

Phase 2 should broaden coverage to later-stage disputes, cross‑cutting procedure, and “adjacent” everyday life problems that quickly appear once users trust the system.

- **Housing Act 107 of 1997** (policy/programmes referenced by Rental Housing Act) to support “where does the municipality fit” explanations. citeturn29view1turn31view0  
- **Magistrates’ Courts Act 32 of 1944** for civil procedure concepts that spill over (execution, jurisdiction, etc.), especially when users move from small claims to ordinary civil claims. citeturn25search3turn25search11  
- **Income Tax Act 58 of 1962** and **Value‑Added Tax Act 89 of 1991**, but consider ingesting them as “selected-topic slices” first (definitions, filing obligations, basic liability concepts) rather than the full Acts immediately, because they are large and frequently amended. citeturn7search6turn7search3turn43search2  
- **Consumer Protection Act 68 of 2008** for everyday contract disputes (including many “small claim” narratives) and plain-language rights framing. citeturn29view2  
- Property registry and community‑scheme disputes: **Deeds Registries Act 47 of 1937**, **Community Schemes Ombud Service Act 9 of 2011**, **Sectional Titles Schemes Management Act 8 of 2011** + Deeds Office procedural pages/FAQs. citeturn24search4turn25search0turn25search1turn24search2turn24search6  

## Key judgments and repositories to include

### Repositories to ingest

- **Constitutional Court repository**: ingest full judgments with paragraph numbering plus metadata (case number, neutral citation, date), prioritizing cases that operationalize statutory tests (e.g., PIE “just and equitable,” Rental Housing Act tribunal powers, PAJA review standards). Use entity["organization","Constitutional Court of South Africa","apex court, south africa"] sources as the “top authority” layer in retrieval ranking for constitutional and rights‑driven issues. citeturn14search1turn17search0turn18search0  
- **Supreme Court of Appeal repository**: ingest judgments in areas where the SCA is the key interpreter (civil procedure principles, arrest without warrant, tax appeal standards). Use entity["organization","Supreme Court of Appeal of South Africa","appellate court, south africa"] PDFs where available. citeturn19search13turn21search10  
- **High Court judgments**: there is no single “official” consolidated national High Court judgment portal; in practice, systems commonly rely on entity["organization","Southern African Legal Information Institute","legal information institute, south africa"] as a comprehensive, citable repository, with court-level metadata captured carefully. citeturn18search1turn22search1turn20search3  

image_group{"layout":"carousel","aspect_ratio":"16:9","query":["Constitutional Court of South Africa building Johannesburg","Supreme Court of Appeal of South Africa building Bloemfontein","South African Government Gazette cover page"],"num_per_query":1}

### Representative leading cases per priority domain

These are “high-yield” judgments for a non-lawyer assistant because they translate abstract rights/tests into concrete factors and procedural expectations.

**Labour (dismissals and review of arbitration outcomes)**  
- *Sidumo and Another v Rustenburg Platinum Mines Ltd and Others* **[2007] ZACC 22** — establishes the constitutional/administrative standard applied when reviewing CCMA arbitration awards (“reasonableness” framing) and is repeatedly used in fairness/review explanations. Source: Constitutional Court repository. citeturn14search0  

**Housing/eviction and rental disputes**  
- *Port Elizabeth Municipality v Various Occupiers* **[2004] ZACC 7** — foundational PIE interpretation emphasizing “justice and equity” in eviction decisions and balancing property rights with dignity/housing considerations. citeturn14search1  
- *Occupiers of 51 Olivia Road, Berea Township and 197 Main Street Johannesburg v City of Johannesburg and Others* **[2008] ZACC 1** — establishes the importance of meaningful engagement and practical accommodation outcomes in eviction contexts. citeturn15search10  
- *City of Johannesburg Metropolitan Municipality v Blue Moonlight Properties 39 (Pty) Ltd and Another* **[2011] ZACC 33** — clarifies municipal obligations around temporary accommodation in eviction matters (often decisive for user questions involving “where do I go if evicted?”). citeturn15search12turn15search7  
- *Maphango and Others v Aengus Lifestyle Properties (Pty) Ltd* **[2012] ZACC 2** — key for Rental Housing Act users: addresses lease termination in a rental dispute and the role of regulatory/tribunal mechanisms. citeturn17search0  

**Domestic violence / family protection orders**  
- *S v Baloyi (Minister of Justice and Another Intervening)* **[1999] ZACC 19** — leading constitutional framing of domestic violence protections and the state’s obligations; highly citeable when explaining why protection orders exist and how they’re enforced. citeturn16search1  
- *Carmichele v Minister of Safety and Security and Another* **[2001] ZACC 22** — anchors explanations about state duties and potential liability where protection systems fail. citeturn16search2  
- *Bannatyne v Bannatyne (Commission for Gender Equality as Amicus Curiae)* **[2002] ZACC 31** — leading maintenance enforcement case (useful once domestic violence inquiries expand into financial support and enforcement realities). citeturn16search3  

**Small claims / access to courts**  
- *Chief Lesapo v North West Agricultural Bank and Another* **[1999] ZACC 16** — constitutional “access to courts” and anti–self‑help principles; extremely useful when users ask whether the other party may “just take” property or act unilaterally. citeturn22search0turn22search3  
- *Chrish v Commissioner, Small Claims Court* (Eastern Cape High Court) **[2007] ZAECHC 114** — directly engages constitutionality questions in the small-claims context; a good High Court exemplar for constraints/structure of SCC proceedings. citeturn22search1  

**Administrative law (reviews, reasons, internal remedies)**  
- *Bato Star Fishing (Pty) Ltd v Minister of Environmental Affairs and Tourism and Others* **[2004] ZACC 15** — core administrative review principles and reasonableness framing that non-lawyer explanations can safely cite. citeturn14search3  
- *Koyabe and Others v Minister of Home Affairs and Others* **[2009] ZACC 23** — clarifies exhaustion of internal remedies before judicial review; maps directly onto user questions like “can I go to court now?” citeturn18search0turn18search4  
- *Trencon Construction (Pty) Ltd v Industrial Development Corporation of South Africa Limited and Another* **[2015] ZACC 22** — practical guidance on remedies (substitution vs remit), relevant to explaining likely outcomes of review proceedings. citeturn18search2  
- *Oudekraal Estates (Pty) Ltd v City of Cape Town and Others* **[2004] ZASCA 48** — leading SCA authority on the effect of unlawful administrative acts until set aside; frequently cited in review litigation reasoning. citeturn18search1  

**Criminal procedure (arrest, bail, confessions)**  
- *S v Zuma and Others* **[1995] ZACC 1** — foundational confession admissibility constitutional analysis (useful when users ask about coerced confessions). citeturn39view1turn44view4  
- *S v Dlamini; S v Dladla and Others; S v Joubert; S v Schietekat* **[1999] ZACC 8** — key constitutional bail case; supports user-facing explanations of bail tests and procedure. citeturn20search3turn44view3  
- *Minister of Safety and Security v Sekhoto and Another* **[2010] ZASCA 141** — leading SCA authority on arrest without warrant and the scope of police discretion (commonly relevant to “was my arrest lawful?” questions). citeturn19search13turn44view0  

## Official forms, procedural guides, and regulator/agency guidance to ingest

This category is disproportionately valuable for non-lawyers because it enables the assistant to generate **accurate, jurisdiction-specific templates** and to explain “what happens next” without inventing procedure.

### Labour dismissal pathway

- **CCMA Rules (Government Gazette)**: ingest as a standalone “procedure corpus” and annotate with effective dates and Gazette identifiers. citeturn13search0turn12search1  
- **CCMA/LRA forms set (Government Gazette)**: ingest each form as a structured template (field name → instruction → evidence checklist). citeturn10search2turn12search1  

### Domestic violence and harassment protection orders

- **Domestic Violence forms** (DOJ forms page): ingest the full set and map each to the Domestic Violence Act stage (application, interim order, notice to respondent, subpoenas, safety monitoring notice). citeturn42search0turn33view1  
- **DOJ domestic violence public guidance page**: useful for plain-language summaries and safety-planning content that can drive escalation/referral UX. citeturn42search4turn42search3  
- Harassment Act procedural components (interim orders, warrants, variation, appeal) are in the statute itself; consider also ingesting any DOJ harassment forms page if you include it later (not retrieved here; unspecified). citeturn34view0  

### Maintenance (adjacent but commonly co-occurring with domestic violence)

- DOJ maintenance guidance page and its referenced forms list (starting with J101/Form A as described by DOJ). citeturn42search1turn32view2  

### Small claims

- DOJ small claims guidance page (includes official guidance and references to controlling instruments). citeturn22search5  
- Small Claims Courts Act itself is essential for eligibility rules (who can sue, representation limits) and enforcement structures. citeturn38view0  
- Rules regulating Small Claims Court procedure should be included; a commonly used public version is hosted in open repositories (not exhaustively validated here—treat as **unspecified** until you select an official Gazette PDF or a DOJ-hosted rules PDF endpoint). citeturn22search5turn36search5  

### Administrative review and access to information

- DOJ PAJA citizen/NGO booklet materials (designed for public comprehension and includes practical steps like requesting reasons). citeturn23search6turn23search11turn23search7  
- Information Regulator PAIA portal: ingest the PAIA forms (notably Form 2 for requests; Form 4 for internal appeal; Form 5 for complaints) and the PAIA Guide for plain-language explanations. citeturn41search2turn41search10turn41search4  

### Tax queries and disputes

- **VAT 404 – Guide for Vendors**: while not “the law,” it contains procedural explanations (e.g., ADR steps, NOA references) that help ordinary users navigate disputes and compliance. citeturn43search1  
- SARS interpretive guidance relevant to straightforward procedural questions: **Interpretation Note 15 (Issue 6)** on extending time periods for objections/appeals, which directly links to Tax Administration Act ss 104 and 107 mechanics. citeturn43search2turn40view2turn40view3  

### Property registry (Phase 2, unless property is core now)

- Deeds Office public guidance pages (how to obtain deed copies; system FAQs), plus the enabling statute(s). citeturn24search2turn24search6turn24search4  

### Legal aid access and referrals

For ordinary users, corpus should include materials that support “get help now” routing and eligibility understanding. A readily citable official document is Legal Aid South Africa’s PAIA manual (useful as a proxy for organization structure and access channels); eligibility/means-test material is **not retrieved here** and remains **unspecified**. citeturn41search0  

## Secondary but useful materials and licensing posture

### What to do with commercial law reports, textbooks, and law firm notes

- **Commercial law reports (Juta / LexisNexis)** add significant value (headnotes, consistent citations, curated case significance), but are typically license-restricted; for many RAG assistants the best compromise is **link-only** (do not ingest full text), or ingest only if you have a negotiated license explicitly covering indexing/embedding and model-assisted excerpting. citeturn21search10turn38view0turn39view0  
- **Law firm client guides / practice notes** can improve plain-language explanations and checklists, but should generally be **link-only** unless you have written permission, because these are copyrighted secondary works and can be updated without notice. citeturn21search18turn18search9  
- **Academic articles/journals**: use sparingly for non-lawyer workflows. They are usually license-restricted; preferred stance is “cite and link,” not ingest. citeturn17search4turn19search3  

### Evidence that “officially hosted” does not always mean “low licensing risk”

Multiple justice.gov.za statute PDFs show publisher copyright lines (e.g., within the Small Claims Courts Act and the Criminal Procedure Act text extracts), indicating that the *specific compiled edition* may carry third‑party rights even if the underlying law is public. This is a practical reason to prefer Government Gazette PDFs on gov.za as your ingestion base, or to obtain explicit permission for the compiled format you index. citeturn38view0turn39view0turn34view0turn32view2  

## Metadata, indexing notes, and prioritized ingestion checklist

### Chunking and citation granularity by document type

- **Statutes / Acts**: chunk by *section* (and subsection where long). Preserve canonical identifiers: Act number/year + section/subsection. This supports precise retrieval like “PIE s 4(2) notice.” citeturn30view0turn27view0turn28view1turn44view0turn40view3  
- **Rules and Government Gazette forms**: chunk by rule number / form number, with each form also stored as a structured template (field schema). citeturn13search0turn42search0  
- **Judgments**: chunk by paragraph blocks (e.g., 1–3 paragraphs per chunk) and retain neutral citation + court + date + paragraph anchors. This enables reliable “standard-of-review” explanations with pinpoint quotes. citeturn14search1turn18search2turn19search13turn20search3  
- **Government/Regulator guidance**: chunk by heading/FAQ; label as “guidance” (non-binding) but preserve referenced legal hooks (e.g., “Tax Administration Act s 107”). citeturn22search5turn41search4turn43search2  

Recommended metadata fields (minimum viable):

- `domain` (tenant/labour/DV/small_claims/admin/criminal/tax)  
- `document_type` (act | rule | form | judgment | guidance)  
- `source_authority` (gov.za | justice.gov.za | concourt repository | SCA site | regulator) citeturn26view0turn42search0turn41search2turn19search13  
- `act_number_year` and `section_path` (e.g., “66/1995 s191(1)”) citeturn27view3  
- `court_level` and `neutral_citation` where applicable (ZACC/ZASCA/etc.) citeturn16search1turn19search13turn18search2  
- `effective_date` / `gazette_id` for rules/forms; `last_seen` timestamp for freshness.

### Prioritized ingestion checklist with timelines and minimal licensing actions

**Phase 1 (about 10–14 days total, if you already have ingestion infrastructure):**

- Days 1–2: ingest and normalize statutes for the six core domains (Rental Housing Act; PIE Act; LRA; BCEA; Domestic Violence Act; Protection from Harassment Act; Small Claims Courts Act; PAJA; PAIA; Criminal Procedure Act; Tax Administration Act). citeturn26view2turn30view0turn27view0turn28view1turn32view0turn34view0turn38view0turn23search4turn3search2turn44view0turn40view3  
- Days 3–5: ingest **forms and procedural guides** (DV forms set; maintenance entry page; small claims guidance; PAJA citizen materials; Information Regulator PAIA forms/guide; CCMA Gazette rules/forms). citeturn42search0turn42search1turn22search5turn23search6turn41search2turn41search4turn13search0turn10search2  
- Days 6–9: ingest “leading cases” listed above with paragraph-level chunking and neutral-citation metadata; build domain‑specific case bundles (eviction bundle; DV bundle; admin bundle; criminal bundle). citeturn14search1turn15search10turn15search12turn17search0turn16search1turn18search0turn19search13turn20search3  
- Days 10–14: licensing review and hardening:
  - Flag any DOJ-hosted statute PDFs that carry publisher copyright lines; decide whether to (a) replace with Gazette originals, or (b) obtain permission for the compiled edition. citeturn38view0turn39view0turn34view0turn32view2  
  - Add a “guidance vs law” labeling policy in your corpus so retrieval can prefer statutes/judgments over guidance where conflict exists. citeturn43search1turn22search5  

**Phase 2 (about 10–20 days, depending on breadth and licensing):**

- Add property registry and community scheme dispute materials (Deeds statute + CSOS + STSM + DeedsWEB FAQs). citeturn24search4turn25search0turn25search1turn24search6  
- Add tax substantive Acts (Income Tax Act, VAT Act) as “slices” first; then expand if needed. citeturn7search6turn7search3turn43search1  
- Add Magistrates’ Courts Act for civil procedure expansions beyond small claims. citeturn25search11turn25search3  

### Short trade-off table: ingestion and licensing posture

| Source type | Strengths for South African non‑lawyer RAG | Key risks / downsides | Recommended posture |
|---|---|---|---|
| Public government sources (gov.za, justice.gov.za forms/pages) | High authority; best for statutes, forms, and “how to” portals; easiest to cite and audit citeturn42search0turn22search5turn26view0 | Versions may be non‑consolidated; some “officially hosted” PDFs carry publisher copyright lines citeturn39view0turn38view0 | Ingest as primary; version-control by Gazette IDs; keep citations to source pages |
| Apex court repositories (Constitutional Court; SCA) | Highest precedential value; enables reliable interpretation layer; supports paragraph-level citations citeturn14search1turn19search13 | Coverage gaps for some domains; High Court completeness varies citeturn18search1 | Ingest as “authority layer” with highest retrieval rank |
| Commercial law reports / textbooks | Adds editorial insight, headnotes, and curated doctrine | Licensing and redistribution constraints; expensive; content may not be ingestible at all without contract citeturn38view0turn39view0 | Link-only unless you have an explicit ingestion license |
| Law firm guides / blogs | Excellent plain-language explanations; strong UX value | Not authoritative; copyrighted; may drift from current law without clear versioning citeturn21search18turn18search9 | Link-only; optionally ingest only with written permission + version commitments |

## Sources to prioritize for crawling

Primary crawling targets for this corpus (in priority order) should be the official statute/form/guidance endpoints and apex-court repositories: **gov.za** (Acts and Gazette PDFs), **justice.gov.za** (Acts, forms, PAJA materials, small claims portal, maintenance portal), Constitutional Court repository, Supreme Court of Appeal judgments portal, **inforegulator.org.za** (PAIA forms, PAIA Guide), and **sars.gov.za** (guides and interpretation notes for practical tax workflows). citeturn26view0turn42search0turn23search7turn22search5turn41search2turn43search2turn19search13

Unspecified constraints: whether you can pay for any commercial ingestion licenses (Juta/LexisNexis, law firm content licenses) is not stated; this report therefore defaults to a public-sources‑first corpus that maximizes legal authority and citation verifiability. citeturn38view0turn39view0