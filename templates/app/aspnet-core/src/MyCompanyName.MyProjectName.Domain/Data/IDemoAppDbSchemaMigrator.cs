using System.Threading.Tasks;

namespace MyCompanyName.MyProjectName.Data
{
    public interface IDemoAppDbSchemaMigrator
    {
        Task MigrateAsync();
    }
}
