using backend.Debugging;

namespace backend;

public class backendConsts
{
    public const string LocalizationSourceName = "backend";

    public const string ConnectionStringName = "Default";

    public const bool MultiTenancyEnabled = true;


    /// <summary>
    /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
    /// </summary>
    public static readonly string DefaultPassPhrase =
        DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "2772e785a61e4d89bdadf235b628f864";
}


