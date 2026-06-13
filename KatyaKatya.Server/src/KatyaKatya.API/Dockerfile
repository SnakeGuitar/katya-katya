FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/KatyaKatya.API/KatyaKatya.API.csproj", "src/KatyaKatya.API/"]
COPY ["src/KatyaKatya.Application/KatyaKatya.Application.csproj", "src/KatyaKatya.Application/"]
COPY ["src/KatyaKatya.Domain/KatyaKatya.Domain.csproj", "src/KatyaKatya.Domain/"]
COPY ["src/KatyaKatya.Infrastructure/KatyaKatya.Infrastructure.csproj", "src/KatyaKatya.Infrastructure/"]
RUN dotnet restore "src/KatyaKatya.API/KatyaKatya.API.csproj"

COPY . .
RUN dotnet build "src/KatyaKatya.API/KatyaKatya.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/KatyaKatya.API/KatyaKatya.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KatyaKatya.API.dll"]
