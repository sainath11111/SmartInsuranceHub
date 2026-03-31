# ----------- BUILD STAGE -----------
<<<<<<< HEAD
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
=======
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
>>>>>>> f3d6b7f (Configured dynamic Render port binding and updated TargetFramework)
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
<<<<<<< HEAD
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
=======
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
>>>>>>> f3d6b7f (Configured dynamic Render port binding and updated TargetFramework)
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

<<<<<<< HEAD
# 🔥 IMPORTANT for Render (dynamic port)
ENV ASPNETCORE_URLS=http://+:$PORT

# Optional (production mode)
ENV ASPNETCORE_ENVIRONMENT=Production

# Start app
ENTRYPOINT ["dotnet", "SmartInsuranceHub.dll"]
=======
# Built-in configuration from environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

# Start app
ENTRYPOINT ["dotnet", "SmartInsuranceHub.dll"]
>>>>>>> f3d6b7f (Configured dynamic Render port binding and updated TargetFramework)
