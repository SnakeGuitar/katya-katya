FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/MemoryGame.API/MemoryGame.API.csproj", "src/MemoryGame.API/"]
COPY ["src/MemoryGame.Application/MemoryGame.Application.csproj", "src/MemoryGame.Application/"]
COPY ["src/MemoryGame.Domain/MemoryGame.Domain.csproj", "src/MemoryGame.Domain/"]
COPY ["src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj", "src/MemoryGame.Infrastructure/"]
RUN dotnet restore "src/MemoryGame.API/MemoryGame.API.csproj"

COPY . .
RUN dotnet build "src/MemoryGame.API/MemoryGame.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/MemoryGame.API/MemoryGame.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MemoryGame.API.dll"]
