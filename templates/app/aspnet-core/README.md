# SufiAbp Application Template

This is the unified template structure for SufiAbp CLI (`sufi new`) command.

## Structure

```
src/
  MyCompanyName.MyProjectName.Domain.Shared/          # Shared (all architectures)
  MyCompanyName.MyProjectName.Domain/                 # Shared (all architectures)
  MyCompanyName.MyProjectName.Application.Contracts/  # Shared (all architectures)
  MyCompanyName.MyProjectName.Application/            # Shared (all architectures)
  MyCompanyName.MyProjectName.EntityFrameworkCore/    # Shared (EF Core only)
  MyCompanyName.MyProjectName.MongoDB/                # Shared (MongoDB only)
  MyCompanyName.MyProjectName.HttpApi/                # Shared (all architectures)
  MyCompanyName.MyProjectName.HttpApi.Client/         # Shared (all architectures)
  MyCompanyName.MyProjectName.DbMigrator/             # Shared (all architectures)
  MyCompanyName.MyProjectName.Blazor.WebApp/          # All architectures
  MyCompanyName.MyProjectName.Blazor.WebApp.Client/   # All architectures
  MyCompanyName.MyProjectName.HttpApi.Host/           # Layered + Tiered
  MyCompanyName.MyProjectName.AuthServer/             # Tiered only
  MyCompanyName.MyProjectName.Blazor.WebSite/        # Optional, tiered only
  MyCompanyName.MyProjectName.Blazor.WebSite.Client/ # Optional, tiered only

etc/docker/
  docker-compose.efcore-sqlserver.yml.template
  docker-compose.efcore-postgresql.yml.template
  docker-compose.efcore-mysql.yml.template
  docker-compose.efcore-mariadb.yml.template
  docker-compose.efcore-sqlite.yml.template
  docker-compose.mongodb.yml.template
```

## Architecture Variants

1. **WebApp**: Blazor.WebApp + Client (auth in server)
2. **Layered**: WebApp + HttpApi.Host (auth still in Blazor.WebApp)
3. **Layered-Tiered**: Layered + AuthServer; optional Blazor.WebSite can be included with `--include-website`

## Database Providers

- **EF Core**: SqlServer, PostgreSQL, MySQL, MariaDB, SQLite
- **MongoDB**: Alternative to EF Core

## Template Markers

- `<TEMPLATE-REMOVE>...</TEMPLATE-REMOVE>`: Removed in generated code
- `<TEMPLATE-ONLY>...</TEMPLATE-ONLY>`: Uncommented in generated code
- `<TEMPLATE-REMOVE IF-NOT="db:efcore">`: Conditional removal
- `<TEMPLATE-REMOVE IF-NOT="efp:postgresql">`: EF provider conditional
- `<TEMPLATE-REMOVE IF-NOT="arch:layered">`: Architecture conditional
- `<TEMPLATE-REMOVE IF-NOT="host:website">`: Optional website host conditional

## CLI Processing

1. User runs: `sufi new MyApp --solution-kind layered --database-provider efcore --ef-provider postgresql`
2. CLI copies template, processes markers
3. CLI selects `docker-compose.efcore-postgresql.yml.template`, renames to `docker-compose.yml`
4. CLI deletes other docker-compose files
5. CLI prompts: "Generate Initial migration? (Y/n)"
6. If yes, runs: `dotnet ef migrations add Initial`
7. CLI prompts: "Run DbMigrator? (Y/n)"
8. If yes, runs DbMigrator

## Version

Template version: 0.0.0-rc.1.0
SufiAbp Framework version: $(SufiVersion) from versions.props
