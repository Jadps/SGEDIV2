FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BACKSGEDI.csproj", "./"]
RUN dotnet restore "BACKSGEDI.csproj"

COPY . .
RUN dotnet publish "BACKSGEDI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
USER root
RUN mkdir -p /app/uploads /app/logs
RUN chown -R $APP_UID:$APP_UID /app/uploads /app/logs
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BACKSGEDI.dll"]