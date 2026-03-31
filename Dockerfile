# ----------- BUILD STAGE -----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["SmartInsuranceHub.csproj", "./"]
RUN dotnet restore "SmartInsuranceHub.csproj"

# Copy all files
COPY . .

# Build explicitly targeting net8.0 since the csproj now multi-targets
RUN dotnet build "SmartInsuranceHub.csproj" -c Release -o /app/build

# Publish explicitly targeting net8.0
RUN dotnet publish "SmartInsuranceHub.csproj" -c Release -o /app/publish /p:UseAppHost=false


# ----------- RUNTIME STAGE -----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Built-in configuration from environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

# Start app
ENTRYPOINT ["dotnet", "SmartInsuranceHub.dll"]
