FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "VirtualFinland.UsersAPI.csproj"
COPY . .
RUN dotnet build "VirtualFinland.UsersAPI.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "VirtualFinland.UsersAPI.csproj" -p:PublishReadyToRun=True -c Release -o /app/publish --self-contained -r linux-x64 
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VirtualFinland.UsersAPI.dll"]
