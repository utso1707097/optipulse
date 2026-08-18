# syntax=docker/dockerfile:1

# Multi-stage: the SDK image is ~800MB and contains a compiler; the runtime image does not.
# Shipping the SDK would mean shipping a build toolchain into production.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy manifests first so `restore` is cached independently of source edits — a source-only
# change then skips the slowest step entirely.
COPY Directory.Build.props Directory.Packages.props ./
COPY OptiPulse.slnx ./
COPY src/ ./src/

RUN dotnet restore src/OptiPulse.Api/OptiPulse.Api.csproj

# PublishAot is deliberately NOT used (constitution v2.2.0): it bakes
# IsDynamicCodeSupported=false into the runtimeconfig, which makes EF Core refuse to build a
# model and stops the host from starting. See the note in Directory.Build.props / T085.
RUN dotnet publish src/OptiPulse.Api/OptiPulse.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root by default. The base image provides the `app` user; running as root would give a
# process that only needs to read config and talk to Postgres the ability to rewrite its own
# filesystem if it were ever compromised.
USER app

COPY --from=build /app/publish .

# Hosts that inject a PORT (Render, Fly, Cloud Run) are honoured; 8080 is the fallback so the
# image also runs locally with no configuration at all.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "OptiPulse.Api.dll"]
