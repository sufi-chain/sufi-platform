# CLI Tool

The `sufi` CLI is the starting point for vertical solution teams using Sufi Platform. Its main job is to generate a solution from the platform templates so the team can start building a product without assembling the baseline architecture by hand.

If your team is consuming the platform rather than contributing to `src/`, this is the command surface you care about first.

## Install

### From NuGet.org (Recommended)

The Sufi CLI is published to NuGet.org as a .NET global tool:

```bash
dotnet tool install --global SufiChain.SufiAbp.CLI
sufi --version
```

### From nuget.sabp.ir

If your environment uses the SufiChain package feed:

```bash
# Add the feed (one-time setup)
dotnet nuget add source https://nuget.sabp.ir/v3/index.json \
  --name SufiChain

# Install the CLI
dotnet tool install --global SufiChain.SufiAbp.CLI --add-source SufiChain
sufi --version
```


### Update the CLI

To update to the latest version:

```bash
# From NuGet.org
dotnet tool update --global SufiChain.SufiAbp.CLI

# From nuget.sabp.ir
dotnet tool update --global SufiChain.SufiAbp.CLI --add-source SufiChain

# From local build
cd /path/to/sufi-orchestrator
dotnet pack src/framework/SufiChain.SufiAbp.CLI -o ./nupkg
dotnet tool update --global --add-source ./nupkg SufiChain.SufiAbp.CLI
```

### Uninstall

To remove the CLI:

```bash
dotnet tool uninstall --global SufiChain.SufiAbp.CLI
```

## Main commands

| Command | Purpose |
| --- | --- |
| `sufi new` | Start interactive solution generation |
| `sufi new <Name>` | Generate a solution non-interactively |
| `sufi list` | List available templates |
| `sufi help` | Show command help |
| `sufi version` or `sufi --version` | Show CLI version |


## `new` command options

| Option | Purpose | Notes |
| --- | --- | --- |
| `-d`, `--database` | Choose `ef` or `mongo` | Defaults to `mongo` |
| `--solution-kind` | Choose `single` or `layered` | Defaults to `layered` |
| `--tiered` | Split auth and API hosts in layered solutions | Valid for layered only |
| `--multi-tenancy` | Enable multi-tenancy | Pulls tenant-aware setup into the solution |
| `--ef-provider` | Choose the EF provider | `sqlserver`, `postgresql`, `mysql`, `mariadb`, or `sqlite` |
| `--connection-string` | Provide the initial database connection string | Optional |
| `--modules` | Add optional modules | Core modules are still included |
| `-o`, `--output` | Choose the output directory | Defaults to the current directory |
| `--list-modules` | Show available modules and exit | Useful before generation |
| `--no-interactive` | Force non-interactive mode | Requires a solution name |


## Typical generation examples

Interactive wizard:

```bash
sufi new
```

Layered EF Core solution with PostgreSQL, tiered architecture, and multi-tenancy:

```bash
sufi new MyCompany.MyProduct \
  --database ef \
  --ef-provider postgresql \
  --solution-kind layered \
  --tiered \
  --multi-tenancy
```

Single-host MongoDB solution:

```bash
sufi new MyCompany.MyProduct \
  --database mongo \
  --solution-kind single
```

Layered EF Core solution with SQL Server and all modules:

```bash
sufi new MyCompany.MyProduct \
  --database ef \
  --ef-provider sqlserver \
  --solution-kind layered \
  --multi-tenancy \
  --modules file-manager,audit-logging,background-jobs,localization-management,short-link-generator
```

Single-host EF Core solution with MySQL:

```bash
sufi new MyCompany.MyProduct \
  --database ef \
  --ef-provider mysql \
  --solution-kind single \
  --output ./my-projects
```


## What the CLI generates

Depending on the options you choose, the generated solution will typically include:

- standard SufiAbp and ABP layering
- the selected host style, such as single or layered
- the selected database setup
- the baseline platform modules
- optional modules requested through the CLI

For tiered layered solutions, expect separate auth, API, and UI hosts. For single-host solutions, expect a simpler run path centered around one main host.


## After generation

Your next steps are usually:

1. start the database and Redis locally, often with Docker
2. update connection strings if needed
3. run `dotnet restore`
4. run `dotnet build`
5. run `DbMigrator` when the solution uses EF Core
6. run the generated host projects with `dotnet run`

The full end-to-end path is described in [Getting Started](../getting-started.md).


## Available Templates

The CLI includes templates for different solution architectures and database providers:

| Template | Solution Kind | Database | Description |
|----------|---------------|----------|-------------|
| Single + MongoDB | Single | MongoDB | All-in-one Blazor host with MongoDB |
| Single + EF Core | Single | EF Core | All-in-one Blazor host with SQL database |
| Layered + MongoDB | Layered | MongoDB | Separated layers with MongoDB |
| Layered + EF Core | Layered | EF Core | Separated layers with SQL database |

When you choose EF Core, you can select from these providers:
- PostgreSQL (recommended)
- SQL Server
- MySQL
- MariaDB
- SQLite


## Troubleshooting

### CLI not found after install

Add .NET tools to your PATH:

```bash
# Linux/Mac
export PATH="$PATH:$HOME/.dotnet/tools"

# Windows PowerShell
$env:PATH += ";$HOME\.dotnet\tools"
```
