FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy backend project files
COPY backend/ ./backend/

# Restore and publish the project
WORKDIR /app/backend
RUN dotnet restore src/Api/Api.csproj
RUN dotnet publish src/Api/Api.csproj -c Release -o /app/out

# Lightweight Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy build binaries to runtime working directory
COPY --from=build /app/out ./

# Kestrel port variable and Injectable OpenAI Environment variable
ENV ASPNETCORE_URLS=http://+:5181
ENV OpenAI__ApiKey=""

EXPOSE 5181
ENTRYPOINT ["dotnet", "GroceryStore.Chatbot.Api.dll"]
