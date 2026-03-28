namespace backend.Domains.QA;

/// <summary>
/// Mechanism by which a user submitted their question or received an answer from the assistant.
/// Determines whether audio file references are expected on the related entity.
/// Values are stored as integers in the database for efficiency and compile-time safety.
/// </summary>
public enum InputMethod
{
    /// <summary>User typed or received their interaction as plain text.</summary>
    Text = 0,

    /// <summary>User spoke or received their interaction as audio.</summary>
    Voice = 1
}
