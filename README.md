# Game Catalogue API

API REST para gerenciamento de um catálogo de jogos, construída com **ASP.NET Core (.NET 10)** e **MongoDB**, organizada em camadas no estilo Clean Architecture.

## Sumário

- [Tecnologias](#tecnologias)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Como executar](#como-executar)
- [Modelo de dados](#modelo-de-dados)
- [Endpoints](#endpoints)
- [Exemplos de teste](#exemplos-de-teste)

## Tecnologias

| Tecnologia | Uso |
|---|---|
| .NET 10 / ASP.NET Core | Web API com Controllers |
| MongoDB 8 (`MongoDB.Driver` 3.12) | Banco de dados |
| Docker Compose | Container do MongoDB para desenvolvimento |
| OpenAPI + Scalar | Documentação interativa da API |

## Estrutura do projeto

```
game-catalogue/
├── docker-compose.yml                  # Container do MongoDB
├── GameCatalogue.slnx                  # Solution
└── src/
    ├── GameCatalogue.Domain/           # Entidades de domínio
    │   └── Entities/
    │       └── Game.cs
    ├── GameCatalogue.Application/      # Regras de aplicação e contratos
    │   ├── Interfaces/
    │   │   ├── IGameRepository.cs
    │   │   └── IGameService.cs
    │   └── Services/
    │       └── GameService.cs
    ├── GameCatalogue.Infrastructure/   # Acesso a dados (MongoDB)
    │   ├── Data/
    │   │   ├── MongoDbContext.cs
    │   │   └── MongoDbSettings.cs
    │   └── Repositories/
    │       └── GameRepository.cs
    └── GameCatalogue.Api/              # Camada de apresentação (HTTP)
        ├── Controllers/
        │   └── GameController.cs       # GamesController → /api/games
        ├── Program.cs                  # Configuração e injeção de dependência
        ├── appsettings.json
        ├── appsettings.Development.json
        └── GameCatalogue.Api.http      # Requisições de teste (VS / VS Code / Rider)
```

### Camadas e dependências

```
Api ──► Application ──► Domain
 │                        ▲
 └──► Infrastructure ─────┘
         (implementa as interfaces de Application)
```

| Camada | Responsabilidade |
|---|---|
| **Domain** | Entidade `Game`, mapeada para o MongoDB com `[BsonId]`. |
| **Application** | Interfaces `IGameService` / `IGameRepository` e o `GameService`, que orquestra as operações. |
| **Infrastructure** | `MongoDbContext` (conexão e ping no banco), `MongoDbSettings` e `GameRepository` (consultas e agregação na coleção `Games`). |
| **Api** | `GamesController`, configuração do pipeline, OpenAPI/Scalar e injeção de dependência. |

## Como executar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) em execução

### Subindo a aplicação

```bash
dotnet run --project src/GameCatalogue.Api
```

Em builds **Debug**, o projeto da API executa `docker compose up -d --wait` antes de compilar. Assim, o container do MongoDB sobe sozinho e a API só inicia depois que o banco está pronto. O mesmo vale para o F5 no Visual Studio.

- API: `http://localhost:5270` (perfil `https`: `https://localhost:7046`)
- Documentação Scalar: `http://localhost:5270/scalar/v1`
- Especificação OpenAPI: `http://localhost:5270/openapi/v1.json`

Para rodar **sem** subir o container (por exemplo, usando o MongoDB Atlas):

```bash
dotnet run --project src/GameCatalogue.Api -p:SkipMongoContainer=true
```

### Banco de dados

| Item | Valor |
|---|---|
| Container | `gamecatalogue-mongodb` |
| Porta | `27017` |
| Usuário / senha | `admin` / `admin123` (alteráveis via `MONGO_USERNAME` / `MONGO_PASSWORD`) |
| Database | `GameCatalogue` |
| Coleção | `Games` |
| Volume | `mongodb-data` (os dados persistem entre reinicializações) |

A connection string de desenvolvimento fica em `appsettings.Development.json`:

```json
"MongoDb": {
  "ConnectionString": "mongodb://admin:admin123@localhost:27017/?authSource=admin",
  "DatabaseName": "GameCatalogue"
}
```

Comandos úteis:

```bash
docker compose up -d       # sobe o MongoDB manualmente
docker compose down        # para o container (mantém os dados)
docker compose down -v     # para o container e APAGA os dados
```

## Modelo de dados

### `Game`

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `id` | `string` (ObjectId) | não | Gerado pelo MongoDB na criação |
| `title` | `string` | sim | Título do jogo |
| `genre` | `string` | sim | Gênero |
| `platform` | `string` | sim | Plataforma |
| `releaseDate` | `DateTime` (ISO 8601) | não | Data de lançamento |
| `publisher` | `string` | sim | Publicadora |
| `price` | `decimal` | não | Preço |

```json
{
  "id": "6acd934aa85911a8e72ec2f6",
  "title": "Elden Ring",
  "genre": "RPG",
  "platform": "PC",
  "releaseDate": "2022-02-25T00:00:00Z",
  "publisher": "Bandai Namco",
  "price": 229.90
}
```

## Endpoints

Rota base: `/api/games`

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| `GET` | `/api/games` | Lista todos os jogos | `200` |
| `GET` | `/api/games/{id}` | Busca um jogo pelo ID | `200`, `404` |
| `POST` | `/api/games` | Cadastra um jogo | `201`, `400` |
| `PUT` | `/api/games/{id}` | Atualiza (substitui) um jogo | `200`, `400`, `404` |
| `DELETE` | `/api/games/{id}` | Remove um jogo | `204`, `404` |
| `GET` | `/api/games/genre/{genre}` | Lista jogos de um gênero | `200` |
| `GET` | `/api/games/platform/{platform}` | Lista jogos de uma plataforma | `200` |
| `GET` | `/api/games/average-price-by-genre` | Preço médio agrupado por gênero | `200` |

**Observações**

- Os filtros por gênero e plataforma comparam o **valor exato**, diferenciando maiúsculas de minúsculas: `RPG` ≠ `rpg`. Valores com espaço devem ser codificados na URL (`Nintendo%20Switch`).
- O `PUT` substitui o documento inteiro, então envie todos os campos.
- Um `{id}` que não seja um ObjectId válido (24 caracteres hexadecimais) retorna `500`.
- Um corpo sem `title`, `genre`, `platform` ou `publisher` retorna `400` com os erros de validação.

## Exemplos de teste

Os exemplos usam `curl` e a porta HTTP padrão. Substitua `{id}` pelo ID retornado na criação.

> **Dica:** o arquivo [`src/GameCatalogue.Api/GameCatalogue.Api.http`](src/GameCatalogue.Api/GameCatalogue.Api.http) tem as mesmas requisições, prontas para executar no Visual Studio, VS Code (extensão REST Client) ou Rider. Também é possível testar pela interface do Scalar em `/scalar/v1`.

### Cadastrar um jogo — `POST /api/games`

```bash
curl -X POST http://localhost:5270/api/games \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Elden Ring",
    "genre": "RPG",
    "platform": "PC",
    "releaseDate": "2022-02-25T00:00:00Z",
    "publisher": "Bandai Namco",
    "price": 229.90
  }'
```

Resposta `201 Created` (com o header `Location: /api/Games/{id}`):

```json
{
  "id": "6acd934aa85911a8e72ec2f6",
  "title": "Elden Ring",
  "genre": "RPG",
  "platform": "PC",
  "releaseDate": "2022-02-25T00:00:00Z",
  "publisher": "Bandai Namco",
  "price": 229.90
}
```

Corpo inválido, resposta `400 Bad Request`:

```bash
curl -X POST http://localhost:5270/api/games \
  -H "Content-Type: application/json" \
  -d '{ "title": "Sem dados" }'
```

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Genre": ["The Genre field is required."],
    "Platform": ["The Platform field is required."],
    "Publisher": ["The Publisher field is required."]
  }
}
```

### Listar todos — `GET /api/games`

```bash
curl http://localhost:5270/api/games
```

Resposta `200 OK`:

```json
[
  {
    "id": "6acd934aa85911a8e72ec2f5",
    "title": "The Legend of Zelda: Tears of the Kingdom",
    "genre": "Adventure",
    "platform": "Nintendo Switch",
    "releaseDate": "2023-05-12T00:00:00Z",
    "publisher": "Nintendo",
    "price": 349.90
  },
  {
    "id": "6acd934aa85911a8e72ec2f6",
    "title": "Elden Ring",
    "genre": "RPG",
    "platform": "PC",
    "releaseDate": "2022-02-25T00:00:00Z",
    "publisher": "Bandai Namco",
    "price": 229.90
  }
]
```

### Buscar por ID — `GET /api/games/{id}`

```bash
curl http://localhost:5270/api/games/6acd934aa85911a8e72ec2f6
```

Retorna `200 OK` com o jogo, ou `404 Not Found` se ele não existir:

```bash
curl -i http://localhost:5270/api/games/000000000000000000000000
# HTTP/1.1 404 Not Found
```

### Atualizar — `PUT /api/games/{id}`

```bash
curl -X PUT http://localhost:5270/api/games/6acd934aa85911a8e72ec2f6 \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Elden Ring",
    "genre": "RPG",
    "platform": "PC",
    "releaseDate": "2022-02-25T00:00:00Z",
    "publisher": "Bandai Namco",
    "price": 179.90
  }'
```

Retorna `200 OK` com o jogo atualizado, ou `404 Not Found` se o ID não existir.

### Remover — `DELETE /api/games/{id}`

```bash
curl -i -X DELETE http://localhost:5270/api/games/6acd934aa85911a8e72ec2f6
# HTTP/1.1 204 No Content
```

Retorna `404 Not Found` se o ID não existir.

### Filtrar por gênero — `GET /api/games/genre/{genre}`

```bash
curl http://localhost:5270/api/games/genre/RPG
```

```json
[
  {
    "id": "6acd934aa85911a8e72ec2f6",
    "title": "Elden Ring",
    "genre": "RPG",
    "platform": "PC",
    "releaseDate": "2022-02-25T00:00:00Z",
    "publisher": "Bandai Namco",
    "price": 229.90
  },
  {
    "id": "6acd934aa85911a8e72ec2f7",
    "title": "Baldur's Gate 3",
    "genre": "RPG",
    "platform": "PC",
    "releaseDate": "2023-08-03T00:00:00Z",
    "publisher": "Larian Studios",
    "price": 199.90
  }
]
```

### Filtrar por plataforma — `GET /api/games/platform/{platform}`

```bash
curl "http://localhost:5270/api/games/platform/Nintendo%20Switch"
```

```json
[
  {
    "id": "6acd934aa85911a8e72ec2f5",
    "title": "The Legend of Zelda: Tears of the Kingdom",
    "genre": "Adventure",
    "platform": "Nintendo Switch",
    "releaseDate": "2023-05-12T00:00:00Z",
    "publisher": "Nintendo",
    "price": 349.90
  }
]
```

### Preço médio por gênero — `GET /api/games/average-price-by-genre`

```bash
curl http://localhost:5270/api/games/average-price-by-genre
```

Resposta `200 OK`. A chave é o gênero e o valor é o preço médio:

```json
{
  "Adventure": 349.90,
  "RPG": 214.90
}
```

### Testando pelo PowerShell

No Windows, a alternativa ao `curl` é o `Invoke-RestMethod`:

```powershell
$base = "http://localhost:5270/api/games"

# Criar
$game = @{
  title       = "Elden Ring"
  genre       = "RPG"
  platform    = "PC"
  releaseDate = "2022-02-25T00:00:00Z"
  publisher   = "Bandai Namco"
  price       = 229.90
} | ConvertTo-Json
$created = Invoke-RestMethod -Method Post -Uri $base -Body $game -ContentType "application/json; charset=utf-8"

# Listar / buscar / filtrar
Invoke-RestMethod $base
Invoke-RestMethod "$base/$($created.id)"
Invoke-RestMethod "$base/genre/RPG"
Invoke-RestMethod "$base/average-price-by-genre"

# Remover
Invoke-RestMethod -Method Delete -Uri "$base/$($created.id)"
```
## Integrantes

| Nome | RM |
|---|---|
| Arthur Graciani | RM561728 |
| Gustavo Oliveira | RM566358 |
| Lucas Hideki | RM565355 |