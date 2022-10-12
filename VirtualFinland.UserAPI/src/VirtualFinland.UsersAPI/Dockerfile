FROM mcr.microsoft.com/dotnet/sdk:6.0 AS sdk
WORKDIR /app

COPY ./ ./
RUN dotnet publish -p:PublishReadyToRun=True -p:PublishReadyToRunUseCrossgen2=True -c Release -o out -r linux-arm64

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=sdk /app/out .
ENTRYPOINT ["dotnet", "VirtualFinland.UsersAPI.dll"]
