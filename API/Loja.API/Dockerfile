FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Loja.Domain/Loja.Domain.csproj Loja.Domain/
COPY Loja.Application/Loja.Application.csproj Loja.Application/
COPY Loja.Infrastructure/Loja.Infrastructure.csproj Loja.Infrastructure/
COPY Loja.API/Loja.API.csproj Loja.API/
RUN dotnet restore Loja.API/Loja.API.csproj

COPY Loja.Domain/ Loja.Domain/
COPY Loja.Application/ Loja.Application/
COPY Loja.Infrastructure/ Loja.Infrastructure/
COPY Loja.API/ Loja.API/
RUN dotnet publish Loja.API/Loja.API.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Loja.API.dll"]
