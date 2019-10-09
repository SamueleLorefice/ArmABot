FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["ArmA Bot.csproj", ""]
RUN dotnet restore "./ArmA Bot.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ArmA Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmA Bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmA Bot.dll"]