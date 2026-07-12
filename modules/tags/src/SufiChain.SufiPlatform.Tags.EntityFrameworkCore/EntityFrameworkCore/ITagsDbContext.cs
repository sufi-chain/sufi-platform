using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Tags.EntityFrameworkCore;

[ConnectionStringName(SufiTagsDbProperties.ConnectionStringName)]
public interface ITagsDbContext : IEfCoreDbContext
{
    DbSet<Tag> Tags { get; }
    DbSet<TagLink> TagLinks { get; }
}