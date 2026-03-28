namespace backend.Domains.QA;

/// <summary>
/// Supported user-facing languages for the MzansiLegal assistant.
/// All four official South African languages required by the constitution are represented.
/// Values are stored as integers in the database for efficiency and compile-time safety.
/// </summary>
public enum Language
{
    /// <summary>English (ISO 639-1: en).</summary>
    English = 0,

    /// <summary>isiZulu (ISO 639-1: zu).</summary>
    Zulu = 1,

    /// <summary>Sesotho (ISO 639-1: st).</summary>
    Sesotho = 2,

    /// <summary>Afrikaans (ISO 639-1: af).</summary>
    Afrikaans = 3
}
