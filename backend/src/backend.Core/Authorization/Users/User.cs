using Abp.Authorization.Users;
using Abp.Extensions;
using backend.Domains.QA;
using System;
using System.Collections.Generic;

namespace backend.Authorization.Users;

public class User : AbpUser<User>
{
    public const string DefaultPassword = "123qwe";

    /// <summary>
    /// The user's preferred interface language for all responses and UI labels.
    /// Defaults to English when not explicitly set during registration.
    /// </summary>
    public Language PreferredLanguage { get; set; } = Language.English;

    /// <summary>
    /// When true, the UI applies dyslexia-friendly font and spacing adjustments.
    /// Defaults to false — standard typography is used unless the user opts in.
    /// </summary>
    public bool DyslexiaMode { get; set; } = false;

    /// <summary>
    /// When true, audio responses are played automatically without requiring user interaction.
    /// Defaults to false — audio must be explicitly triggered by the user unless opted in.
    /// </summary>
    public bool AutoPlayAudio { get; set; } = false;

    public static string CreateRandomPassword()
    {
        return Guid.NewGuid().ToString("N").Truncate(16);
    }

    public static User CreateTenantAdminUser(int tenantId, string emailAddress)
    {
        var user = new User
        {
            TenantId = tenantId,
            UserName = AdminUserName,
            Name = AdminUserName,
            Surname = AdminUserName,
            EmailAddress = emailAddress,
            Roles = new List<UserRole>()
        };

        user.SetNormalizedNames();

        return user;
    }
}


