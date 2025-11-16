FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["DrumBuddy.Endpoint/DrumBuddy.Endpoint.csproj", "DrumBuddy.Endpoint/"]
COPY ["DrumBuddy.IO/DrumBuddy.IO.csproj", "DrumBuddy.IO/"]
COPY ["DrumBuddy.Core/DrumBuddy.Core.csproj", "DrumBuddy.Core/"]

RUN dotnet restore "DrumBuddy.Endpoint/DrumBuddy.Endpoint.csproj"

COPY . .

WORKDIR "/src/DrumBuddy.Endpoint"
RUN dotnet build "DrumBuddy.Endpoint.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DrumBuddy.Endpoint.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DrumBuddy.Endpoint.dll"]
