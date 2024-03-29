FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY ./src/*.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o publishdir

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/publishdir .
ENTRYPOINT ["dotnet", "PuttPutt.dll"]
