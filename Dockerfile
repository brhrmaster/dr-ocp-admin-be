FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY ["DrOcupacional.Backend.Api/DrOcupacional.Backend.Api.csproj", "DrOcupacional.Backend.Api/"]
COPY ["DrOcupacional.Backend.Application/DrOcupacional.Backend.Application.csproj", "DrOcupacional.Backend.Application/"]
COPY ["DrOcupacional.Backend.Domain/DrOcupacional.Backend.Domain.csproj", "DrOcupacional.Backend.Domain/"]
COPY ["DrOcupacional.Backend.Infrastructure/DrOcupacional.Backend.Infrastructure.csproj", "DrOcupacional.Backend.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "DrOcupacional.Backend.Api/DrOcupacional.Backend.Api.csproj"

# Copiar todo o código
COPY . .

# Build
WORKDIR "/src/DrOcupacional.Backend.Api"
RUN dotnet build "DrOcupacional.Backend.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DrOcupacional.Backend.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DrOcupacional.Backend.Api.dll"]

