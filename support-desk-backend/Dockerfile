FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SupportDesk.Api.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish SupportDesk.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./
RUN chown -R "$APP_UID:$APP_UID" /app

ENV PORT=8080 \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "SupportDesk.Api.dll"]
