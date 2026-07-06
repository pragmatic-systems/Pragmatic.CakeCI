# Dummy Dockerfile for CI pipeline testing
# This is used as part of the dogfood cycle to validate Docker build/push aliases

FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY ["Pragsys.CakeCI/Pragsys.CakeCI.csproj", "Pragsys.CakeCI/"]
RUN dotnet restore "Pragsys.CakeCI/Pragsys.CakeCI.csproj"
COPY . .
WORKDIR "/src/Pragsys.CakeCI"
RUN dotnet build "Pragsys.CakeCI.csproj" -c Release -o /app/build

FROM build AS test
WORKDIR "/src/Pragsys.CakeCI.Tests"
RUN dotnet test "Pragsys.CakeCI.Tests.csproj" -c Release --no-restore --verbosity normal

FROM build AS publish
RUN dotnet publish "Pragsys.CakeCI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pragsys.CakeCI.dll"]
