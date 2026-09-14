FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /source
COPY Directory.Build.props Directory.Packages.props ManoMana.sln ./
COPY src/ManoMana.Domain/ManoMana.Domain.csproj src/ManoMana.Domain/
COPY src/ManoMana.Application/ManoMana.Application.csproj src/ManoMana.Application/
COPY src/ManoMana.Infrastructure/ManoMana.Infrastructure.csproj src/ManoMana.Infrastructure/
COPY src/ManoMana.Api/ManoMana.Api.csproj src/ManoMana.Api/
RUN dotnet restore src/ManoMana.Api/ManoMana.Api.csproj

FROM restore AS build
COPY src/ src/
RUN dotnet build src/ManoMana.Api/ManoMana.Api.csproj -c Release --no-restore

FROM build AS publish
RUN dotnet publish src/ManoMana.Api/ManoMana.Api.csproj -c Release --no-build -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ManoMana.Api.dll"]
