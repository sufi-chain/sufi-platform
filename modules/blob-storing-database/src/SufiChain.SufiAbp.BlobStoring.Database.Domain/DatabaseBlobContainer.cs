using System;
using JetBrains.Annotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.BlobStoring.Database;

public class DatabaseBlobContainer : AggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual string Name { get; protected set; } = null!;

    protected DatabaseBlobContainer()
    {
    }

    public DatabaseBlobContainer(Guid id, [NotNull] string name, Guid? tenantId = null)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), DatabaseContainerConsts.MaxNameLength);
        TenantId = tenantId;
    }
}
