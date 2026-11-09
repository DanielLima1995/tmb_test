# TMB Test

Aplicação de pedidos com API em ASP.NET Core e frontend em React. A API persiste os pedidos no PostgreSQL e publica atualizações de status em tempo real via SignalR.

## Pré-requisitos

- .NET SDK 9
- Node.js 22 ou superior e npm
- Docker Desktop com Docker Compose, para executar o PostgreSQL
- Ferramenta `dotnet-ef` 9 para aplicar as migrações:

  ```powershell
  dotnet tool install --global dotnet-ef --version 9.0.0
  ```

  Se uma versão mais recente já estiver instalada globalmente, atualize para a versão compatível com este projeto:

  ```powershell
  dotnet tool update --global dotnet-ef --version 9.0.0 --allow-downgrade
  ```

## Executar em desenvolvimento

Os comandos abaixo são para PowerShell e devem ser executados a partir da raiz deste repositório.

### 1. Configurar e iniciar o banco

```powershell
if (-not (Test-Path .\API\.env)) { Copy-Item .\API\.env.example .\API\.env }
docker compose --env-file .\API\.env -f .\API\Loja.API\docker-compose.yml up -d db
```

### 2. Aplicar as migrações

O exemplo usa a senha padrão de desenvolvimento definida em `API/.env.example`. Se você alterou `POSTGRES_PASSWORD` em `API/.env`, use o mesmo valor na connection string:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=tmb_test;Username=postgres;Password=localdevpostgres"
dotnet ef database update --project .\API\Loja.Infrastructure\Loja.Infrastructure.csproj --startup-project .\API\Loja.API\Loja.API.csproj
```

### 3. Iniciar a API e o frontend

Abra dois terminais na raiz do repositório.

No primeiro, inicie a API:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=tmb_test;Username=postgres;Password=localdevpostgres"
dotnet run --project .\API\Loja.API\Loja.API.csproj --launch-profile http
```

No segundo, instale as dependências e inicie o frontend:

```powershell
Set-Location .\Front
if (-not (Test-Path .env)) { Copy-Item .env.example .env }
npm install
npm run dev
```

Abra a URL exibida pelo Vite (normalmente <http://localhost:5173>). O proxy do frontend encaminha as chamadas da API e do SignalR para `http://localhost:5161`.

- Swagger da API: <http://localhost:5161/swagger/index.html>
- Hub SignalR: <http://localhost:5161/hubs/pedidos>

Para encerrar, use `Ctrl+C` nos terminais da API e do frontend. Para parar o banco:

```powershell
docker compose --env-file .\API\.env -f .\API\Loja.API\docker-compose.yml down
```

## Documentação dos projetos

- [API](./API/README.md): configuração, execução com Docker Compose ou local, endpoints e eventos SignalR.
- [Frontend](./Front/README.md): desenvolvimento local, proxy, execução em container e integração com a API.
