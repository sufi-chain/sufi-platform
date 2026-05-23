namespace SufiChain.SufiAbp.Emailing;

public interface IEmailSender
{
    Task SendAsync(string from, string to, string subject, string? body = null);
}
