FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EduTrack.csproj", "."]
RUN dotnet restore "./EduTrack.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "EduTrack.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EduTrack.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EduTrack.dll"]