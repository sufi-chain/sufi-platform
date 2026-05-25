using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.TagsManagement.Tags;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.TagsManagement.EntityFrameworkCore;

[ConnectionStringName(TagsManagementDbProperties.ConnectionStringName)]
public interface ITagsManagementDbContext : IEfCoreDbContext
{
    DbSet<Tag> Tags { get; }
    DbSet<TagLink> TagLinks { get; }
}
