using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;

namespace SufiChain.SufiPlatform.Identity.AspNetCore;

public class LinkUserTokenProvider : DataProtectorTokenProvider<IdentityUser>
{
    private static readonly Encoding TokenEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public LinkUserTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<DataProtectionTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<IdentityUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }

    /// <summary>
    /// Classifies a failed link-login token. The returned value is a reason code.
    /// The token value is not included in the result.
    /// </summary>
    public virtual string DescribeFailure(
        string? token,
        string? expectedPurpose,
        string actualUserId,
        string? actualSecurityStamp,
        bool supportsSecurityStamp)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return LinkLoginRejectionReasons.MissingToken;
        }

        try
        {
            var unprotectedData = Protector.Unprotect(Convert.FromBase64String(token));
            using var reader = new BinaryReader(new MemoryStream(unprotectedData), TokenEncoding, leaveOpen: false);
            var creationTime = new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
            if (creationTime + Options.TokenLifespan < DateTimeOffset.UtcNow)
            {
                return LinkLoginRejectionReasons.TokenExpired;
            }

            var userId = reader.ReadString();
            if (!string.Equals(userId, actualUserId, StringComparison.Ordinal))
            {
                return LinkLoginRejectionReasons.TokenUserMismatch;
            }

            var purpose = reader.ReadString();
            if (!string.Equals(purpose, expectedPurpose ?? string.Empty, StringComparison.Ordinal))
            {
                return LinkLoginRejectionReasons.TokenPurposeMismatch;
            }

            var stamp = reader.ReadString();
            if (reader.PeekChar() != -1)
            {
                return LinkLoginRejectionReasons.TokenInvalid;
            }

            if (supportsSecurityStamp)
            {
                return string.Equals(stamp, actualSecurityStamp ?? string.Empty, StringComparison.Ordinal)
                    ? LinkLoginRejectionReasons.TokenInvalid
                    : LinkLoginRejectionReasons.TokenSecurityStampMismatch;
            }

            return stamp.Length == 0
                ? LinkLoginRejectionReasons.TokenInvalid
                : LinkLoginRejectionReasons.TokenSecurityStampMismatch;
        }
        catch (Exception)
        {
            return LinkLoginRejectionReasons.TokenInvalid;
        }
    }
}
