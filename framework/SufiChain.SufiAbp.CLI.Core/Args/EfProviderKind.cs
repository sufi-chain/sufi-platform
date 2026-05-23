namespace SufiChain.SufiAbp.CLI.Args;

/// <summary>
/// Entity Framework Core database provider sub-types.
/// Only relevant when <see cref="DatabaseProvider"/> is <see cref="DatabaseProvider.EntityFrameworkCore"/>.
/// </summary>
public enum EfProviderKind
{
    /// <summary>
    /// Microsoft SQL Server (Microsoft.EntityFrameworkCore.SqlServer)
    /// </summary>
    SqlServer,

    /// <summary>
    /// PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL)
    /// </summary>
    PostgreSQL,

    /// <summary>
    /// MySQL (Pomelo.EntityFrameworkCore.MySql)
    /// </summary>
    MySQL,

    /// <summary>
    /// MariaDB (Pomelo.EntityFrameworkCore.MySql with MariaDB server version)
    /// </summary>
    MariaDB,

    /// <summary>
    /// SQLite (Microsoft.EntityFrameworkCore.Sqlite)
    /// </summary>
    Sqlite
}
