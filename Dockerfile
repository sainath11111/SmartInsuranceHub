# ----------- BUILD STAGE -----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["SmartInsuranceHub.csproj", "./"]
RUN dotnet restore "SmartInsuranceHub.csproj"

# Copy all files
COPY . .

# Build
RUN dotnet build "SmartInsuranceHub.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "SmartInsuranceHub.csproj" -c Release -o /app/publish /p:UseAppHost=false


# ----------- RUNTIME STAGE -----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# 🔥 IMPORTANT for Render (dynamic port)
ENV ASPNETCORE_URLS=http://+:$PORT

# Optional (production mode)
ENV ASPNETCORE_ENVIRONMENT=Production

# Start app
ENTRYPOINT ["dotnet", "SmartInsuranceHub.dll"]
