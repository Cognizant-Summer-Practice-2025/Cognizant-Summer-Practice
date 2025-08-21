# Multi-stage build for ASP.NET Core backend-AI
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy everything and restore
COPY . .
RUN dotnet restore "backend-AI.csproj"

# Publish
RUN dotnet publish "backend-AI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose and bind
ENV ASPNETCORE_URLS=http://0.0.0.0:5134
EXPOSE 5134

# Secrets are provided via docker-compose at runtime (not here)
ENTRYPOINT ["dotnet", "backend-AI.dll"]


