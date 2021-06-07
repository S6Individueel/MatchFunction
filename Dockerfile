FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /
EXPOSE 7071

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["MatchFunction.csproj", "./"]
RUN dotnet restore "MatchFunction.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "MatchFunction.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MatchFunction.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MatchFunction.dll"]
