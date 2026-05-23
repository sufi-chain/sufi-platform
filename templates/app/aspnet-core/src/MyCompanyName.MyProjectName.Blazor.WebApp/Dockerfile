# MyCompanyName.MyProjectName.Blazor.WebApp - Single host web application.
# Build context MUST be the repository root.
#
# Build from repo root:
#   docker build -f .dev/hosts/single/src/MyCompanyName.MyProjectName.Blazor.WebApp/Dockerfile .
#
# Build from .dev/hosts/single (context must be repo root):
#   docker build -f src/MyCompanyName.MyProjectName.Blazor.WebApp/Dockerfile ../../..

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy MSBuild props (required for AbpVersion, etc.)
COPY Directory.Build.props ./
COPY common.props ./
COPY NuGet.Config ./

# Copy solution and project files
COPY .dev/hosts/single/ .dev/hosts/single/
COPY src/ src/
COPY independent-projects/ independent-projects/

# Restore
RUN dotnet restore .dev/hosts/single/MyCompanyName.MyProjectName.sln

# Publish (Release for production-like; ASPNETCORE_ENVIRONMENT=Development set at runtime for dev)
WORKDIR /src/.dev/hosts/single
RUN dotnet publish src/MyCompanyName.MyProjectName.Blazor.WebApp/MyCompanyName.MyProjectName.Blazor.WebApp.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
EXPOSE 8080
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyCompanyName.MyProjectName.Blazor.WebApp.dll"]
