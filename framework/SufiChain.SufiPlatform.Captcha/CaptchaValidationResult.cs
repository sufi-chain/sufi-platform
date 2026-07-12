namespace SufiChain.SufiPlatform.Captcha;

/// <summary>
/// Result of captcha validation.
/// </summary>
public class CaptchaValidationResult
{
    public bool IsValid { get; set; }

    public string? ErrorCode { get; set; }

    public static CaptchaValidationResult Success()
    {
        return new CaptchaValidationResult { IsValid = true };
    }

    public static CaptchaValidationResult Failure(string errorCode)
    {
        return new CaptchaValidationResult
        {
            IsValid = false,
            ErrorCode = errorCode
        };
    }
}
