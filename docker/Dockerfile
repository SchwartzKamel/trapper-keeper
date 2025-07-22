# Build stage with secure base image and platform specification
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG BUILD_VERSION=1.0.0

WORKDIR /src

# Restore dependencies first for layer caching
COPY ["src/TrapperKeeper/trapper-keeper.csproj", "src/TrapperKeeper/"]
RUN dotnet restore "src/TrapperKeeper/trapper-keeper.csproj" \
    --runtime linux-x64 \
    --verbosity minimal

# Copy remaining source and build
COPY . .
RUN dotnet publish "src/TrapperKeeper/trapper-keeper.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    --runtime linux-x64 \
    --self-contained false \
    -p:Version=$BUILD_VERSION \
    -p:FileVersion=$BUILD_VERSION \
    -p:DebugType=None \
    -p:EnableCompressionInSingleFile=true

# Runtime stage with security hardening
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bullseye-slim
ARG UID=1001
ARG GID=1001

# Secure entrypoint configuration
ENTRYPOINT ["./trapper-keeper", "--urls", "http://0.0.0.0:5000"]

# Create non-root user and set permissions
RUN mkdir -p /app && \
    groupadd -g $GID appuser && \
    useradd -u $UID -g appuser -s /bin/bash -m appuser && \
    chown -R appuser:appuser /app

WORKDIR /app

# Copy from build stage with proper ownership
COPY --chown=appuser:appuser --from=build /app/publish .

# Security configurations
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

RUN mkdir -p /app/conversations && \
    chown -R appuser:appuser /app/conversations

USER appuser

# Runtime environment settings
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_NOLOGO=true \
    DOTNET_RUNNING_IN_CONTAINER=true

# File permissions hardening
RUN find /app -type d -exec chmod 755 {} \; && \
    find /app -type f -exec chmod 644 {} \; && \
    chmod +x /app/trapper-keeper
