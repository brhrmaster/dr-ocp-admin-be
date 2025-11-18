# Dr. Ocupacional - Backend API

Backend desenvolvido em .NET Core 9 seguindo os princípios de Clean Architecture, Domain Driven Design e SOLID.

## Estrutura do Projeto

```
DrOcupacional.Backend/
├── DrOcupacional.Backend.Api/          # Camada de apresentação (Controllers, Program.cs)
├── DrOcupacional.Backend.Application/  # Camada de aplicação (Services, DTOs, Interfaces)
├── DrOcupacional.Backend.Domain/       # Camada de domínio (Entities, Repositories Interfaces)
└── DrOcupacional.Backend.Infrastructure/# Camada de infraestrutura (Repositories, Data Access)
```

## Tecnologias

- .NET Core 9
- Dapper (ORM leve)
- PostgreSQL
- JWT Authentication
- Swagger/OpenAPI

## Configuração

As configurações estão em `appsettings.json` e `appsettings.Development.json`. As variáveis de ambiente podem ser configuradas no `docker-compose.yaml`.

### Connection Strings

- **Development**: Configurado para usar o PostgreSQL do Docker Compose
- **Production**: Deve ser configurado via variáveis de ambiente

### Identity

- **Authority**: URL do serviço de Identity (OAuth 2.0)
- **Audience**: Escopo da aplicação (ui-app)

## Endpoints

### Menus

- `GET /api/menus?nome={nome}` - Busca menus por nome
- `GET /api/menus/{id}` - Obtém um menu por ID
- `POST /api/menus` - Cria um novo menu
- `PUT /api/menus/{id}` - Atualiza um menu existente
- `DELETE /api/menus/{id}` - Exclui um menu

Todos os endpoints requerem autenticação JWT.

## Executando Localmente

### Via Docker Compose

```bash
cd ../../local
docker-compose up backend
```

### Via .NET CLI

```bash
dotnet run --project DrOcupacional.Backend.Api/DrOcupacional.Backend.Api.csproj
```

## Banco de Dados

O banco de dados PostgreSQL deve ter a seguinte tabela:

```sql
CREATE TABLE tb_menu (
    cod_menu SERIAL PRIMARY KEY,
    nome VARCHAR(255) NOT NULL,
    ordem INTEGER NOT NULL,
    icon VARCHAR(255) NOT NULL
);

CREATE INDEX idx_menu_nome ON tb_menu(nome);
```