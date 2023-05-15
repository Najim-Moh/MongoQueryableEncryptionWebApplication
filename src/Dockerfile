#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
#CSFLE implementation
RUN apt-get update && apt-get install -y sudo \
	nano \
	gnupg \
	wget \
	libc6-dev \
	curl

#With CURL example
#RUN curl -fsSL https://pgp.mongodb.com/server-6.0.asc | \
   #sudo gpg -o /usr/share/keyrings/mongodb-server-6.0.gpg \
   #--dearmor
#RUN echo "deb [ signed-by=/usr/share/keyrings/mongodb-server-6.0.gpg ] https://repo.mongodb.com/apt/debian bullseye/mongodb-enterprise/6.0 main" | sudo tee /etc/apt/sources.list.d/mongodb-enterprise.list
#
#RUN sudo apt-get update
#
#RUN sudo apt-get install -y mongodb-enterprise-cryptd
#

#With WGET example
RUN wget -qO - https://www.mongodb.org/static/pgp/server-6.0.asc | sudo apt-key add - 
RUN echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.com/apt/debian bullseye/mongodb-enterprise/6.0 main" | sudo tee /etc/apt/sources.list.d/mongodb-enterprise.list
RUN apt-get update && apt-get install -y mongodb-enterprise-cryptd

WORKDIR /app
ENV ASPNETCORE_URLS=http://*:5000
EXPOSE 5000
#EXPOSE 80
EXPOSE 44305



FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MongoWebApplication/MongoCSFLEWebApplication.csproj", "MongoWebApplication/"]
RUN dotnet restore "MongoWebApplication/MongoCSFLEWebApplication.csproj"
COPY . .
WORKDIR "/src/MongoWebApplication"
RUN dotnet build "MongoCSFLEWebApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MongoCSFLEWebApplication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MongoCSFLEWebApplication.dll"]