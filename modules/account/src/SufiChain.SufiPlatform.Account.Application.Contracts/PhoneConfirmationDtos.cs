using System;

namespace SufiChain.SufiPlatform.Account;

public class PhoneConfirmationStateDto
{
    public Guid UserId { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool EmailConfirmed { get; set; }

    public bool EmailConfirmationRequired { get; set; }

    public string? Email { get; set; }
}

public class SendPhoneConfirmationCodeDto
{
    /// <summary>
    /// Server-issued phone confirmation session. Required for anonymous callers.
    /// A signed-in caller may omit it and then acts only for the current user.
    /// </summary>
    public string? SessionToken { get; set; }

    public string? PhoneNumber { get; set; }
}

public class ConfirmPhoneNumberDto
{
    /// <summary>
    /// Server-issued phone confirmation session. Required for anonymous callers.
    /// A signed-in caller may omit it and then acts only for the current user.
    /// </summary>
    public string? SessionToken { get; set; }

    public string Code { get; set; } = string.Empty;
}

public class ConfirmPhoneNumberResultDto
{
    public bool EmailConfirmationStillRequired { get; set; }

    public string? Email { get; set; }
}
