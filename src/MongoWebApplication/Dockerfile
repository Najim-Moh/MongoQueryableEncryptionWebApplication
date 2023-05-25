#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MongoWebApplication/MongoCSFLEWebApplication.csproj", "MongoWebApplication/"]
RUN dotnet restore "MongoWebApplication/MongoCSFLEWebApplication.csproj"
COPY . .
WORKDIR "/src/MongoWebApplication"
RUN dotnet build "MongoCSFLEWebApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MongoCSFLEWebApplication.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install necessary dependencies
RUN apt-get update && apt-get install -y libc6-dev

ENTRYPOINT ["dotnet", "MongoCSFLEWebApplication.dll"]

