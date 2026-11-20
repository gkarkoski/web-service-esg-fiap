FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY src/EsgTrainingManagement.Api/EsgTrainingManagement.Api.csproj src/EsgTrainingManagement.Api/
RUN dotnet restore src/EsgTrainingManagement.Api/EsgTrainingManagement.Api.csproj
COPY src/EsgTrainingManagement.Api/ src/EsgTrainingManagement.Api/
RUN dotnet publish src/EsgTrainingManagement.Api/EsgTrainingManagement.Api.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "EsgTrainingManagement.Api.dll"]
