# STAGE 1: Build the frontend

# Use Node.js 20-alpine (lightweight Linux distribution) for frontend build
FROM node:20-alpine AS frontend-build

WORKDIR /app/src/frontend

# Copy package files first for better layer caching
COPY src/frontend/package*.json ./

# Install dependencies (including dev dependencies for build)
RUN npm ci --prefer-offline --no-audit

# Copy frontend source code
COPY src/frontend/ ./

# Copy shared folder (needed for role-permissions.json import)
COPY src/shared/ ../shared/

# Build the frontend (outputs to /app/wwwroot per vite.config.js)
RUN npm run build

# Stage 2: Build and run the ASP.NET Core application

# Use ASP.NET instead of .NET SDK for only runtime tools
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 3: Build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies (better caching)
COPY ["Starter-App.csproj", "./"]
RUN dotnet restore "Starter-App.csproj" --verbosity quiet

# Copy source code (excluding what's in .dockerignore)
COPY . .

# Copy the built frontend from frontend-build stage
COPY --from=frontend-build /app/wwwroot ./wwwroot

# Build the .NET application (skip frontend build since it's already done)
RUN dotnet build "Starter-App.csproj" -c Release -o /app/build \
    --no-restore \
    /p:SKIP_FRONTEND_BUILD=true

# Publish the application
FROM build AS publish
RUN dotnet publish "Starter-App.csproj" -c Release -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:SKIP_FRONTEND_BUILD=true

FROM base AS final
WORKDIR /app

# Create a non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Starter-App.dll"]