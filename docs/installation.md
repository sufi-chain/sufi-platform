# Installation Guide

Use this page when you want the shortest path from an empty machine to a generated Sufi Platform project using the open-source base.

## Prerequisites

- .NET 10.0 SDK or later
- Docker Desktop for local databases
- Git
- A code editor such as VS Code, Visual Studio, or Rider

## Step 1: Install the Sufi CLI

### From NuGet.org

```bash
dotnet tool install --global SufiChain.SufiPlatform.CLI
sufi --version
```

### From nuget.sufichain.com

If your environment uses the SufiChain package feed:

```bash
dotnet nuget add source https://nuget.sufichain.com/v3/index.json \
  --name SufiChain

dotnet tool install --global SufiChain.SufiPlatform.CLI --add-source SufiChain
sufi --version
```

## Step 2: Generate your first project

### Interactive mode

```bash
sufi new
```

Use the wizard to choose:

- solution name, such as `MyCompany.MyProduct`
- solution kind: single or layered
- database provider: MongoDB or EF Core
- multi-tenancy: yes or no
- optional modules

### Non-interactive mode

```bash
sufi new MyCompany.MyProduct \
  --solution-kind layered \
  --database ef \
  --ef-provider postgresql \
  --tiered \
  --multi-tenancy \
  --modules file-manager,audit-logging
```

## Step 3: Start a database

### PostgreSQL

```bash
docker run -d --name my-postgres \
  -e POSTGRES_DB=MyProduct \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:16
```

### MongoDB

```bash
docker run -d --name my-mongo \
  -e MONGO_INITDB_ROOT_USERNAME=admin \
  -e MONGO_INITDB_ROOT_PASSWORD=admin \
  -p 27017:27017 \
  mongo:7
```

## Step 4: Run the application

```bash
cd MyCompany.MyProduct

dotnet restore
dotnet build

# EF Core only
cd src/MyCompany.MyProduct.DbMigrator
dotnet run

# Start the application
cd ../MyCompany.MyProduct
dotnet run
```

## Step 5: Explore the generated project

Most generated solutions include authentication, identity management, optional multi-tenancy, audit logging, settings, localization, Blazor UI with SufiTheme, and Docker support.

## Common commands

```bash
dotnet tool update --global SufiChain.SufiPlatform.CLI
sufi new --list-modules
sufi help
sufi new --help
sufi --version
```

## Next steps

- [Product Overview](product-overview.md)
- [Product Creation Guide](product-creation-guide.md)
- [Getting Started](getting-started.md)
- [CLI Tool](framework/cli.md)
- [Module Catalog](modules/index.md)
- [SufiBlazor](../../independent-projects/sufi-blazor/docs/README.md)
- [SufiTheme](../../independent-projects/sufi-theme/docs/README.md)
