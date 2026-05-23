using System;
using JetBrains.Annotations;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.BlobStoring.Database;

public class DatabaseBlob : AggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid ContainerId { get; protected set; }

    public virtual Guid? TenantId { get; protected set; }

    public virtual string Name { get; protected set; } = null!;

    [DisableAuditing]
    public virtual byte[] Content { get; protected set; } = null!;

    protected DatabaseBlob()
    {
    }

    public DatabaseBlob(Guid id, Guid containerId, [NotNull] string name, [NotNull] byte[] content, Guid? tenantId = null)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), DatabaseBlobConsts.MaxNameLength);
        ContainerId = containerId;
        Content = CheckContentLength(content);
        TenantId = tenantId;
    }

    public virtual void SetContent(byte[] content)
    {
        Content = CheckContentLength(content);
    }

    protected virtual byte[] CheckContentLength(byte[] content)
    {
        Check.NotNull(content, nameof(content));

        if (content.Length >= DatabaseBlobConsts.MaxContentLength)
        {
            throw new AbpException($"Blob content size cannot be more than {DatabaseBlobConsts.MaxContentLength} Bytes.");
        }

        return content;
    }
}
