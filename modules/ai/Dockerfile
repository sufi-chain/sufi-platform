FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/SufiChain.SufiPlatform.SufiAI.Domain.Shared/", "SufiChain.SufiPlatform.SufiAI.Domain.Shared/"]
COPY ["src/SufiChain.SufiPlatform.SufiAI.Domain/", "SufiChain.SufiPlatform.SufiAI.Domain/"]
COPY ["src/SufiChain.SufiPlatform.SufiAI.Application.Contracts/", "SufiChain.SufiPlatform.SufiAI.Application.Contracts/"]
COPY ["src/SufiChain.SufiPlatform.SufiAI.Application/", "SufiChain.SufiPlatform.SufiAI.Application/"]
COPY ["src/SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore/", "SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore/"]
COPY ["src/SufiChain.SufiPlatform.SufiAI.HttpApi/", "SufiChain.SufiPlatform.SufiAI.HttpApi/"]

# Restore dependencies
RUN dotnet restore "SufiChain.SufiPlatform.SufiAI.HttpApi/SufiChain.SufiPlatform.SufiAI.HttpApi.csproj"

# Build
RUN dotnet build "SufiChain.SufiPlatform.SufiAI.HttpApi/SufiChain.SufiPlatform.SufiAI.HttpApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SufiChain.SufiPlatform.SufiAI.HttpApi/SufiChain.SufiPlatform.SufiAI.HttpApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SufiChain.SufiPlatform.SufiAI.HttpApi.dll"]
