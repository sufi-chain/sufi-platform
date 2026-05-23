using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Emailing;

public class SufiAbpEmailSender : IEmailSender, ITransientDependency
{
    protected Volo.Abp.Emailing.IEmailSender InnerEmailSender { get; }

    public SufiAbpEmailSender(Volo.Abp.Emailing.IEmailSender innerEmailSender)
    {
        InnerEmailSender = innerEmailSender;
    }

    public virtual Task SendAsync(string from, string to, string subject, string? body = null)
    {
        return InnerEmailSender.SendAsync(from, to, subject, body);
    }
}
