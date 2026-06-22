FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/SufiChain.SufiAbp.AI.Domain.Shared/", "SufiChain.SufiAbp.AI.Domain.Shared/"]
COPY ["src/SufiChain.SufiAbp.AI.Domain/", "SufiChain.SufiAbp.AI.Domain/"]
COPY ["src/SufiChain.SufiAbp.AI.Application.Contracts/", "SufiChain.SufiAbp.AI.Application.Contracts/"]
COPY ["src/SufiChain.SufiAbp.AI.Application/", "SufiChain.SufiAbp.AI.Application/"]
COPY ["src/SufiChain.SufiAbp.AI.EntityFrameworkCore/", "SufiChain.SufiAbp.AI.EntityFrameworkCore/"]
COPY ["src/SufiChain.SufiAbp.AI.HttpApi/", "SufiChain.SufiAbp.AI.HttpApi/"]

# Restore dependencies
RUN dotnet restore "SufiChain.SufiAbp.AI.HttpApi/SufiChain.SufiAbp.AI.HttpApi.csproj"

# Build
RUN dotnet build "SufiChain.SufiAbp.AI.HttpApi/SufiChain.SufiAbp.AI.HttpApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SufiChain.SufiAbp.AI.HttpApi/SufiChain.SufiAbp.AI.HttpApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SufiChain.SufiAbp.AI.HttpApi.dll"]
