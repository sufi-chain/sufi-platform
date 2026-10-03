using System;

namespace SufiChain.SufiPlatform.Account;

public class PhoneConfirmationStateDto
{
    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool EmailConfirmed { get; set; }

    public bool EmailConfirmationRequired { get; set; }

    public string? Email { get; set; }
}

public class SendPhoneConfirmationCodeDto
{
    public Guid UserId { get; set; }

    public string? PhoneNumber { get; set; }
}

public class ConfirmPhoneNumberDto
{
    public Guid UserId { get; set; }

    public string Code { get; set; } = string.Empty;
}

public class ConfirmPhoneNumberResultDto
{
    public bool EmailConfirmationStillRequired { get; set; }

    public string? Email { get; set; }
}
