# Painel de pedidos

Frontend React para listar, cadastrar e consultar pedidos, com atualização de status em tempo real via SignalR.

## Requisitos

- Node.js 22 ou superior e npm para desenvolvimento local.
- Docker com Docker Compose para executar em container.
- Uma API de pedidos acessível pelo frontend.

## Configuração do ambiente

Copie o arquivo de exemplo:

```powershell
if (-not (Test-Path .env)) { Copy-Item .env.example .env }
```

Variáveis disponíveis:

| Variável | Uso |
| --- | --- |
| `VITE_API_PROXY_TARGET` | Destino do proxy da API durante `npm run dev`. Padrão: `http://localhost:5161`. |
| `VITE_PEDIDOS_HUB_URL` | URL do hub SignalR incluída no build do frontend. Padrão: `/hubs/pedidos`, servida pela mesma origem. |
| `API_PROXY_TARGET` | URL base da API vista de dentro do container; usada pelo Nginx para encaminhar `/api` e `/hubs`. |

Ao executar a API no host junto com o frontend em Docker, `API_PROXY_TARGET` deve usar `http://host.docker.internal:5161`. Se a API estiver em outro container na mesma rede Docker, use o nome do serviço e a porta interna, por exemplo `http://api:5161`.

## Desenvolvimento local

Instale as dependências e inicie o Vite:

```bash
npm install
npm run dev
```

Abra a URL exibida pelo Vite. O proxy de desenvolvimento encaminha `/api` e `/hubs` para `VITE_API_PROXY_TARGET`; a configuração de WebSocket do hub também é encaminhada. O arquivo `.env` é carregado automaticamente pelo Vite.

## Subir backend e frontend com Docker Compose

Para executar os dois containers a partir da raiz do repositório, crie os arquivos `.env` a partir dos exemplos, caso ainda não existam:

```powershell
if (-not (Test-Path .\API\.env)) { Copy-Item .\API\.env.example .\API\.env }
if (-not (Test-Path .\Front\.env)) { Copy-Item .\Front\.env.example .\Front\.env }
```

Inicie primeiro o PostgreSQL:

```powershell
docker compose --env-file .\API\.env -f .\API\Loja.API\docker-compose.yml up -d db
```

Antes de iniciar a API, aplique as migrações do banco conforme as instruções da [documentação da API](../API/README.md#executar-com-docker-compose-recomendado). Em seguida, suba o backend:

```powershell
docker compose --env-file .\API\.env -f .\API\Loja.API\docker-compose.yml up --build -d
```

Em seguida, suba o frontend:

```powershell
docker compose --env-file .\Front\.env -f .\Front\docker-compose.yml up --build -d
```

O frontend estará disponível em `http://localhost:8080`. O `.env` desta pasta configura a URL do hub e o destino da API visto pelo container; por padrão, `API_PROXY_TARGET` usa `http://host.docker.internal:5161`, que requer que o backend publique a porta `5161` no host.

Para acompanhar os logs ou parar os serviços, execute os comandos a partir da raiz do repositório:

```powershell
docker compose --env-file .\API\.env -f .\API\Loja.API\docker-compose.yml logs -f
docker compose --env-file .\Front\.env -f .\Front\docker-compose.yml logs -f
docker compose --env-file .\Front\.env -f .\Front\docker-compose.yml down
docker compose --env-file .\API\.env -f .\API\Loja.API\docker-compose.yml down
```

Também é possível construir e executar somente o frontend sem Compose:

```bash
docker build -t pedidos-front .
docker run --rm -p 8080:80 --env-file .env pedidos-front
```

O `API_PROXY_TARGET` é configurado em tempo de execução pelo Nginx. Já `VITE_PEDIDOS_HUB_URL` é incorporada ao bundle durante a construção da imagem; para alterá-la, atualize-a no `.env` antes de executar novamente o comando do frontend com `--build`. Em produção, o valor padrão relativo `/hubs/pedidos` mantém a conexão SignalR na mesma origem e o Nginx encaminha o tráfego à API.

## API e eventos esperados

A API deve disponibilizar:

- `GET /api/Pedidos?page={page}&perPage={perPage}`
- `POST /api/Pedidos`
- `GET /api/Pedidos/{id}`
- Hub SignalR em `/hubs/pedidos`

O frontend escuta o evento `StatusPedidoAtualizado`, com `pedidoId`, `status` e `dataAlteracao`, e atualiza o status e o histórico do pedido correspondente.

## Tecnologias escolhidas

- **React**: componentização da interface e gerenciamento reativo dos estados da listagem, detalhes e notificações.
- **Vite**: servidor de desenvolvimento rápido, proxy local para API/SignalR e build estático para produção.
- **ASP.NET Core SignalR Client** (`@microsoft/signalr`): cliente oficial para receber atualizações de status em tempo real e reconectar automaticamente.
- **Docker multi-stage**: usa Node.js somente para instalar dependências e gerar o bundle; a imagem final usa Nginx, reduzindo o tamanho e a superfície do container de produção.
- **Nginx**: entrega os arquivos estáticos, dá suporte a rotas SPA e encaminha chamadas HTTP e WebSocket para a API sem exigir CORS entre frontend e backend.
