FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/SufiChain.SufiAbp.AIManagement.Domain.Shared/", "SufiChain.SufiAbp.AIManagement.Domain.Shared/"]
COPY ["src/SufiChain.SufiAbp.AIManagement.Domain/", "SufiChain.SufiAbp.AIManagement.Domain/"]
COPY ["src/SufiChain.SufiAbp.AIManagement.Application.Contracts/", "SufiChain.SufiAbp.AIManagement.Application.Contracts/"]
COPY ["src/SufiChain.SufiAbp.AIManagement.Application/", "SufiChain.SufiAbp.AIManagement.Application/"]
COPY ["src/SufiChain.SufiAbp.AIManagement.EntityFrameworkCore/", "SufiChain.SufiAbp.AIManagement.EntityFrameworkCore/"]
COPY ["src/SufiChain.SufiAbp.AIManagement.HttpApi/", "SufiChain.SufiAbp.AIManagement.HttpApi/"]

# Restore dependencies
RUN dotnet restore "SufiChain.SufiAbp.AIManagement.HttpApi/SufiChain.SufiAbp.AIManagement.HttpApi.csproj"

# Build
RUN dotnet build "SufiChain.SufiAbp.AIManagement.HttpApi/SufiChain.SufiAbp.AIManagement.HttpApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SufiChain.SufiAbp.AIManagement.HttpApi/SufiChain.SufiAbp.AIManagement.HttpApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SufiChain.SufiAbp.AIManagement.HttpApi.dll"]
