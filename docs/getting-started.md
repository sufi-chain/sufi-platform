# Getting Started

This guide is the main onboarding path for teams that want to install `sufi`, generate a new solution from the open-source platform templates, start the required infrastructure, and begin building a vertical product. Product owners can start with [Product Overview](product-overview.md) and [Product Creation Guide](product-creation-guide.md) before using this technical setup guide. If your goal is to contribute to the framework or to reusable modules in `src/`, use the workspace-level contributor guide at `docs/framework/contributing/source-contributors.md` instead.

## Prerequisites

Before generating a solution, make sure your machine has:

- .NET 10 SDK
- Docker and Docker Compose support
- Git
- a local shell with permission to install a .NET global tool

## Install the `sufi` CLI

The Sufi CLI is distributed as a .NET global tool.

### Install from NuGet.org

```bash
dotnet tool install --global SufiChain.SufiAbp.CLI
sufi --version
```

### Install from nuget.sabp.ir

If your environment uses the SufiChain package feed, add it once and install from that source:

```bash
dotnet nuget add source https://nuget.sabp.ir/v3/index.json \
  --name SufiChain

dotnet tool install --global SufiChain.SufiAbp.CLI --add-source SufiChain
sufi --version
```

### Install from Local Build (Contributors Only)

If you are working from this repository and want to install the local build:

```bash
dotnet pack src/framework/SufiChain.SufiAbp.CLI -o ./nupkg
dotnet tool install --global --add-source ./nupkg SufiChain.SufiAbp.CLI
sufi --version
```

### Update the CLI

To update to the latest version:

```bash
dotnet tool update --global SufiChain.SufiAbp.CLI

# Or from the SufiChain feed
dotnet tool update --global SufiChain.SufiAbp.CLI --add-source SufiChain
```

## Explore the available templates

Check the available templates and generation modes before creating the solution:

```bash
sufi list
sufi help
```

The CLI supports both interactive and non-interactive generation. Running `sufi new` with no name starts the wizard. Supplying a solution name lets you script the generation.

## Generate a new solution

### Interactive path

```bash
sufi new
```

Use this when you want the CLI wizard to guide you through the solution type, database provider, multi-tenancy, and optional module choices.

### Non-interactive path

```bash
sufi new MyCompany.MyProduct \
  --database ef \
  --ef-provider postgresql \
  --solution-kind layered \
  --tiered \
  --multi-tenancy \
  --output ./output
```

Common options:

- `--database` chooses `ef` or `mongo`
- `--ef-provider` chooses the EF provider when using `ef`
- `--solution-kind` chooses `single` or `layered`
- `--tiered` enables separate auth and API hosts for layered solutions
- `--multi-tenancy` enables multi-tenant setup
- `--modules` adds optional modules

## Start the infrastructure with Docker

Most generated solutions need at least a database. Many teams also run Redis locally for caching and background processing support.

### PostgreSQL and Redis example

```bash
docker run -d --name sufi-postgres \
  -e POSTGRES_DB=SufiApp \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 postgres:16

docker run -d --name sufi-redis \
  -p 6379:6379 redis:7
```

### MongoDB and Redis example

```bash
docker run -d --name sufi-mongo \
  -p 27017:27017 mongo:7

docker run -d --name sufi-redis \
  -p 6379:6379 redis:7
```

After the containers are up, update the generated `appsettings.json` files or user secrets so the connection strings match your local ports, database names, and credentials.

## Restore and build the generated solution

From the generated solution root:

```bash
dotnet restore
dotnet build
```

If the solution includes a `DbMigrator` project and you generated an EF Core solution, run it before starting the host:

```bash
dotnet run --project src/MyCompany.MyProduct.DbMigrator
```

Replace `MyCompany.MyProduct` with your generated solution name.

## Run the application without Docker

### Single solution

For a single-host solution, run the main web host:

```bash
dotnet run --project src/MyCompany.MyProduct.Blazor.WebApp
```

If your generated solution uses a different host project name, use that generated host instead.

### Layered solution

For a layered non-tiered solution, run the main web app host:

```bash
dotnet run --project src/MyCompany.MyProduct.Blazor.WebApp
```

### Layered tiered solution

For a layered tiered solution, run the hosts in this order:

1. Auth server
2. API host
3. Web app host
4. Public host if the solution includes one

Example:

```bash
dotnet run --project src/MyCompany.MyProduct.AuthServer
dotnet run --project src/MyCompany.MyProduct.HttpApi.Host
dotnet run --project src/MyCompany.MyProduct.Blazor.WebApp
```

## Run with Docker as part of your delivery flow

The default local development path is `dotnet run`, because it is the fastest way to confirm the generated solution is healthy. If your team deploys with Docker, build container images for the generated host projects after the local run path is working.

That usually means:

- confirming connection strings and environment variables first
- verifying the app starts correctly with `dotnet run`
- then building images for the specific host projects your solution actually uses

## What to read next

Once the generated solution is running, continue with:

- [Product Overview](product-overview.md)
- [Architecture](architecture.md)
- [Module Catalog](modules/index.md)
- [SufiBlazor Overview](sufi-blazor/overview.md)
- [SufiTheme Overview](sufi-theme/sufi-theme-overview.md)
