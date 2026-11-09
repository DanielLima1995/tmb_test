# Loja API

API para cadastro e consulta de pedidos, com histórico de status, processamento em background e notificações de alterações em tempo real.

## Tecnologias e justificativas

- **.NET 9 / ASP.NET Core**: plataforma da API HTTP e dos serviços hospedados, com suporte a injeção de dependência e configuração por ambiente.
- **Entity Framework Core + Npgsql**: mapeia as entidades para o PostgreSQL, executa consultas e persiste pedidos e histórico de status.
- **PostgreSQL 17**: banco relacional para armazenar os pedidos e o histórico de forma persistente.
- **SignalR**: envia alterações de status aos clientes conectados sem que o frontend precise consultar a API repetidamente.
- **`BackgroundService` + `Channel<int>`**: processa os pedidos em segundo plano sem bloquear a requisição HTTP. A fila atual é em memória; mensagens enfileiradas podem ser perdidas se a API reiniciar.
- **Docker e Docker Compose**: facilitam executar a API e o PostgreSQL juntos, com configuração de rede e volume persistente para o banco.
- **Swagger / OpenAPI**: disponibiliza uma interface para explorar e testar os endpoints durante o desenvolvimento.

## Pré-requisitos

Para a execução em contêineres:

- Docker Desktop com Docker Compose.

Para a execução local da API:

- .NET SDK 9.
- PostgreSQL 17 (ou a instância PostgreSQL iniciada pelo Compose).
- Ferramenta `dotnet-ef` compatível com EF Core 9, caso ainda não esteja instalada:

  ```powershell
  dotnet tool install --global dotnet-ef --version 9.0.0
  ```

  Se uma versão mais recente já estiver instalada globalmente, atualize para a versão compatível com este projeto:

  ```powershell
  dotnet tool update --global dotnet-ef --version 9.0.0 --allow-downgrade
  ```

## Configurar variáveis de ambiente

Na pasta `API`, copie o arquivo de exemplo e ajuste os valores:

```powershell
if (-not (Test-Path .env)) { Copy-Item .env.example .env }
```

`.env` é usado pelo Docker Compose para interpolar as variáveis do arquivo Compose. Não use os valores de exemplo em produção; configure senhas e uma chave JWT fortes e privadas.

## Executar com Docker Compose (recomendado)

Execute os comandos desta seção na pasta `API`:

```powershell
docker compose --env-file .env -f .\Loja.API\docker-compose.yml up -d db
```

Aplique a migração inicial ao banco. O comando abaixo configura a conexão local do EF Core para a porta publicada pelo Compose; substitua a senha pelo valor definido em `.env` se tiver alterado o exemplo:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=tmb_test;Username=postgres;Password=localdevpostgres"
dotnet ef database update --project .\Loja.Infrastructure\Loja.Infrastructure.csproj --startup-project .\Loja.API\Loja.API.csproj
```

Depois inicie ou reconstrua a API:

```powershell
docker compose --env-file .env -f .\Loja.API\docker-compose.yml up -d --build
```

Acompanhe os logs e confira o estado dos serviços:

```powershell
docker compose --env-file .env -f .\Loja.API\docker-compose.yml ps
docker compose --env-file .env -f .\Loja.API\docker-compose.yml logs -f api
```

Endereços locais:

- Swagger: <http://localhost:5161/swagger/index.html>
- API: <http://localhost:5161>
- SignalR hub: <http://localhost:5161/hubs/pedidos>
- PostgreSQL: `localhost:5432` (banco `tmb_test`, usuário `postgres`)

Para desligar os serviços sem apagar o banco:

```powershell
docker compose --env-file .env -f .\Loja.API\docker-compose.yml down
```

Os dados do PostgreSQL ficam no volume `postgres_data`. Para apagar também o volume e todos os dados do banco, use `down -v` com cuidado.

## Executar a API localmente

1. Na pasta `API`, inicie o PostgreSQL pelo Compose, conforme o primeiro comando da seção anterior, ou use uma instância PostgreSQL local.
2. Na pasta `API`, defina a conexão do EF Core no terminal PowerShell. Para a instância padrão do Compose:

   ```powershell
   $env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=tmb_test;Username=postgres;Password=localdevpostgres"
   ```

3. Ainda na pasta `API`, aplique as migrações:

   ```powershell
   dotnet ef database update --project .\Loja.Infrastructure\Loja.Infrastructure.csproj --startup-project .\Loja.API\Loja.API.csproj
   ```

4. Inicie a API na pasta `API`:

   ```powershell
   dotnet run --project .\Loja.API\Loja.API.csproj --launch-profile http
   ```

5. Abra <http://localhost:5161/swagger/index.html>.

O perfil `http` define `ASPNETCORE_ENVIRONMENT=Development`, necessário para habilitar o Swagger. A configuração de desenvolvimento de CORS permite por padrão `https://localhost:4200` e `https://localhost:4201`; ajuste `Loja.API\appsettings.Development.json` para a origem do seu frontend, se necessário.

## Notificações em tempo real

O cliente SignalR deve conectar-se ao endereço do hub, sem acrescentar `/api` nem chamar `/negotiate` manualmente:

```text
http://localhost:5161/hubs/pedidos
```

Após a conexão, escute o evento `StatusPedidoAtualizado`. O payload contém `pedidoId`, `status` e `dataAlteracao`. As mudanças são enviadas depois de persistidas no banco. O histórico e o endpoint `GET /api/pedidos/{id}` continuam sendo a fonte persistente do estado atual.
