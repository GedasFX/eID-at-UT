﻿FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AuthServer/AuthServer.csproj", "AuthServer/"]
COPY ["Microsoft.AspNetCore.Authentication.Dokobit/Microsoft.AspNetCore.Authentication.Dokobit.csproj", "Microsoft.AspNetCore.Authentication.Dokobit/"]
COPY ["Microsoft.AspNetCore.Authentication.WebEid/Microsoft.AspNetCore.Authentication.WebEid.csproj", "Microsoft.AspNetCore.Authentication.WebEid/"]
RUN dotnet restore "AuthServer/AuthServer.csproj"
COPY . .
WORKDIR "/src/AuthServer"
RUN dotnet build "AuthServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthServer.dll"]