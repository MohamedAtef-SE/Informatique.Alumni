using System.Threading.Tasks;

namespace Informatique.Alumni.Data;

public interface IAlumniDbSchemaMigrator
{
    Task MigrateAsync();
}
