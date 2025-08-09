# Use the official .NET 9 runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Use the .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["CapitalGains/CapitalGains.csproj", "CapitalGains/"]
RUN dotnet restore "CapitalGains/CapitalGains.csproj"

# Copy source code
COPY . .
WORKDIR "/src/CapitalGains"

# Build the application
RUN dotnet build "CapitalGains.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CapitalGains.csproj" -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "CapitalGains.dll"]