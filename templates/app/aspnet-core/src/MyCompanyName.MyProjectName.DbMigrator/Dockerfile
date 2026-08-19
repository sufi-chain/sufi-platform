# MyCompanyName.MyProjectName.DbMigrator - Database migration tool
# Build context MUST be the repository root (project references ../../../src/ modules).
#
# Build from repo root:
#   docker build -f hosts/single/src/MyCompanyName.MyProjectName.DbMigrator/Dockerfile -t sufiplatform/demoapp-single-dbmigrator:latest .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy MSBuild props (required for AbpVersion, etc.)
COPY Directory.Build.props ./
COPY common.props ./
COPY NuGet.Config ./

# Copy solution and project files
COPY hosts/single/ hosts/single/
COPY src/ src/

# Restore
RUN dotnet restore hosts/single/MyCompanyName.MyProjectName.sln

# Publish DbMigrator
WORKDIR /src/hosts/single
RUN dotnet publish src/MyCompanyName.MyProjectName.DbMigrator/MyCompanyName.MyProjectName.DbMigrator.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyCompanyName.MyProjectName.DbMigrator.dll"]
