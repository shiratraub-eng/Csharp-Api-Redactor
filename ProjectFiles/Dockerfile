# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["ApiRedactor.csproj", "./"]
RUN dotnet restore "ApiRedactor.csproj"

# Copy the rest of the source code and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Define the entry point for the container
ENTRYPOINT ["dotnet", "ApiRedactor.dll"]