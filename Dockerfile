FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Api Crud/GestaoEstoque.API.csproj", "Api Crud/"]
RUN dotnet restore "Api Crud/GestaoEstoque.API.csproj"

COPY ["Api Crud/", "Api Crud/"]
WORKDIR "/src/Api Crud"
RUN dotnet publish "GestaoEstoque.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet GestaoEstoque.API.dll"]