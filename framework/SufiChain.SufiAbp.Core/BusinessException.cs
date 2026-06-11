namespace SufiChain.SufiAbp;

public class BusinessException : Volo.Abp.BusinessException
{
    public BusinessException(string? code = null, string? message = null, string? details = null, Exception? innerException = null)
        : base(code, message, details, innerException)
    {
    }
}
