FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/web/Ticketing.Web/Ticketing.Web.csproj", "src/web/Ticketing.Web/"]
COPY ["src/shared/Ticketing.Contracts/Ticketing.Contracts.csproj", "src/shared/Ticketing.Contracts/"]
RUN dotnet restore "src/web/Ticketing.Web/Ticketing.Web.csproj"
COPY . .
WORKDIR "/src/src/web/Ticketing.Web"
RUN dotnet build "Ticketing.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ticketing.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ticketing.Web.dll"]

