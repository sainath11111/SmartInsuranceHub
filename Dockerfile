# Use the official .NET SDK image to build and publish the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the csproj file and restore dependencies
COPY ["SmartInsuranceHub.csproj", "./"]
RUN dotnet restore "SmartInsuranceHub.csproj"

# Copy the rest of the application code
COPY . .

# Build the application
RUN dotnet build "SmartInsuranceHub.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SmartInsuranceHub.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set ASP.NET Core to listen on port 8080 (the modern default)
ENV ASPNETCORE_HTTP_PORTS=8080

# Expose port 8080 so Render knows where to route traffic
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "SmartInsuranceHub.dll"]
