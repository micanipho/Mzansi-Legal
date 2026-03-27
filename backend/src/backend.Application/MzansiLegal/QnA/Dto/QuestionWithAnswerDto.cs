using System;
using System.Collections.Generic;

namespace backend.MzansiLegal.QnA.Dto;

public class QuestionWithAnswerDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string Text { get; set; }
    public string Language { get; set; }
    public AnswerDto Answer { get; set; }
    public string Disclaimer { get; set; } =
        "This information is for guidance only and does not constitute legal advice. Always consult a qualified legal professional.";
}

public class AnswerDto
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public List<CitationDto> Citations { get; set; } = new();
}

public class CitationDto
{
    public string ActName { get; set; }
    public string Section { get; set; }
    public string Excerpt { get; set; }
    public double Relevance { get; set; }
}
