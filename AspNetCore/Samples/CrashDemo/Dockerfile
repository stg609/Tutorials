#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CrashDemo/CrashDemo.csproj", "CrashDemo/"]
COPY ["nuget.config", ""]
RUN dotnet restore "CrashDemo/CrashDemo.csproj"
COPY . .
WORKDIR "/src/CrashDemo"
RUN dotnet build "CrashDemo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CrashDemo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CrashDemo.dll"]