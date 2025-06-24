# Use official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY PayrollPortal.sln .                             # ✅ This line is good
COPY PayrollPortal/ PayrollPortal/                   # ✅ This copies the full folder contents

# Restore dependencies
RUN dotnet restore PayrollPortal.sln                 # ✅ Explicitly restore using .sln

# Build and publish the app
WORKDIR /src/PayrollPortal
RUN dotnet publish -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PayrollPortal.dll"]
