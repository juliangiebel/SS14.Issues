FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SS14.Issues/SS14.Issues.csproj", "SS14.Issues/"]
RUN apt update -y
RUN apt install nodejs npm -y
RUN dotnet restore "SS14.Issues/SS14.Issues.csproj"
COPY . .
WORKDIR "/src/SS14.Issues"
RUN dotnet build "SS14.Issues.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SS14.Issues.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SS14.Issues.dll"]