using System;
using Microsoft.Extensions.Logging;

namespace SufiChain.SufiPlatform.Identity;

public static class LinkLoginRejectionLog
{
    public const string Message =
        "Link login rejected. Reason {Reason} SourceUserId {SourceUserId} SourceTenantId {SourceTenantId} TargetUserId {TargetUserId} TargetTenantId {TargetTenantId}.";

    public static void Write(
        ILogger logger,
        string reason,
        Guid sourceUserId,
        Guid? sourceTenantId,
        Guid targetUserId,
        Guid? targetTenantId)
    {
        logger.LogWarning(
            Message,
            reason,
            sourceUserId,
            sourceTenantId,
            targetUserId,
            targetTenantId);
    }
}
