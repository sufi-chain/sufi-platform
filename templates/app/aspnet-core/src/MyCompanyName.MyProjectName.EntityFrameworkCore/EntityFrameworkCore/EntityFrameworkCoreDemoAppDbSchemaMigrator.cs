using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyCompanyName.MyProjectName.Data;
using Volo.Abp.DependencyInjection;

namespace MyCompanyName.MyProjectName.EntityFrameworkCore;

public class EntityFrameworkCoreDemoAppDbSchemaMigrator : IDemoAppDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreDemoAppDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the DemoAppDbContext
         * from IServiceProvider (not injecting it directly)
         * to properly get a DbContext instance with current connection string */

        var dbContext = _serviceProvider.GetRequiredService<DemoAppDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
