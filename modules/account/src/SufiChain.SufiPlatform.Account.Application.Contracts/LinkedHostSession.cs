using System;

namespace SufiChain.SufiPlatform.Account;

public static class LinkedHostSession
{
    public const string SourceUserIdClaim = "sufi_link_source_user";

    public const string SourceIsHostClaim = "sufi_link_source_is_host";

    public static bool IsReturnToHostAvailable(string? sourceUserId, string? sourceIsHost) =>
        string.Equals(sourceIsHost, "true", StringComparison.Ordinal) &&
        Guid.TryParse(sourceUserId, out _);
}
