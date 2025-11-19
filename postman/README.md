# Postman Collection - Dr. Ocupacional Admin API

Esta collection contém todos os endpoints necessários para testar a API administrativa do Dr. Ocupacional.

## Configuração Inicial

### 1. Importar a Collection

1. Abra o Postman
2. Clique em "Import"
3. Selecione o arquivo `Dr-Ocupacional-Admin-API.postman_collection.json`

### 2. Configurar Variáveis de Ambiente

Crie um ambiente no Postman com as seguintes variáveis:

- `base_url`: `http://localhost:8080` (URL da Admin API)
- `identity_url`: `http://localhost:8081` (URL do Identity Manager)
- `access_token`: (será preenchido automaticamente após login)
- `menu_id`: (será preenchido automaticamente após criar/buscar um menu)

**Nota**: Se estiver executando localmente via .NET CLI, use `http://localhost:5031` para `base_url`.

## Autenticação

A Admin API utiliza **OAuth2 Token Introspection** (RFC 7662) para validar tokens. Todos os endpoints (exceto Swagger) requerem autenticação via Bearer token.

### Fluxo de Autenticação

1. **Obter Access Token**: Use o endpoint "Obter Access Token (Login Direto)" na seção "1. Autenticação"
   - Este endpoint faz login no Identity Manager e obtém um access_token
   - O token é salvo automaticamente na variável `access_token`
   - Use as credenciais: `admin@dr-ocupacional.com` / `Admin@123`

2. **Usar o Token**: Todos os endpoints da API já estão configurados para usar o token automaticamente via `{{access_token}}`

3. **Validar Token**: Use o endpoint "Validar Token (Introspect)" para verificar se o token ainda está válido

## Endpoints Disponíveis

### 1. Autenticação

- **Obter Access Token (Login Direto)**: Realiza login no Identity Manager e obtém tokens
- **Validar Token (Introspect)**: Valida o access_token atual

### 2. Menus

Todos os endpoints de menus requerem autenticação via Bearer token.

#### Buscar Menus

- **Buscar Todos os Menus (Sem Paginação)**: `GET /api/menus/all?nome={nome}`
  - Retorna todos os menus ou filtra por nome
  - Não utiliza paginação

- **Buscar Menus com Paginação**: `GET /api/menus?nome={nome}&page={page}&pageSize={pageSize}`
  - Retorna menus paginados
  - Parâmetros:
    - `nome` (opcional): Filtro por nome
    - `page` (padrão: 1): Número da página
    - `pageSize` (padrão: 10, máximo: 100): Itens por página

- **Buscar Menu por ID**: `GET /api/menus/{id}`
  - Obtém um menu específico pelo seu ID
  - O ID é salvo automaticamente na variável `menu_id` após busca bem-sucedida

#### Gerenciar Menus

- **Criar Novo Menu**: `POST /api/menus`
  - Cria um novo menu
  - Body:
    ```json
    {
        "nome": "Nome do Menu",
        "ordem": 1,
        "icone": "menu-icon"
    }
    ```
  - O ID do menu criado é salvo automaticamente em `menu_id`

- **Atualizar Menu**: `PUT /api/menus/{id}`
  - Atualiza um menu existente
  - Body:
    ```json
    {
        "nome": "Nome Atualizado",
        "ordem": 2,
        "icone": "updated-icon"
    }
    ```
  - O nome do menu deve ser único

- **Excluir Menu**: `DELETE /api/menus/{id}`
  - Exclui um menu pelo seu ID
  - Retorna 204 No Content em caso de sucesso

### 3. Swagger

- **Swagger UI**: `GET /swagger` - Interface interativa de documentação
- **Swagger JSON**: `GET /swagger/v1/swagger.json` - Especificação OpenAPI

## Fluxo de Testes Recomendado

### 1. Autenticação

1. Execute "Obter Access Token (Login Direto)"
   - Verifique se o token foi salvo na variável `access_token`
   - O console do Postman mostrará o token obtido

2. (Opcional) Execute "Validar Token (Introspect)" para verificar o token

### 2. Testar Endpoints de Menus

1. **Listar Menus**:
   - Execute "Buscar Todos os Menus" ou "Buscar Menus com Paginação"
   - Verifique a resposta

2. **Criar Menu**:
   - Execute "Criar Novo Menu"
   - Modifique o body conforme necessário
   - O ID do menu criado será salvo automaticamente

3. **Buscar Menu Específico**:
   - Execute "Buscar Menu por ID"
   - O endpoint usará automaticamente o `menu_id` salvo

4. **Atualizar Menu**:
   - Execute "Atualizar Menu"
   - Modifique o body conforme necessário
   - O endpoint usará automaticamente o `menu_id` salvo

5. **Excluir Menu**:
   - Execute "Excluir Menu"
   - O endpoint usará automaticamente o `menu_id` salvo

## Estrutura de Dados

### MenuDto

```json
{
    "codMenu": 1,
    "nome": "Nome do Menu",
    "ordem": 1,
    "icone": "menu-icon"
}
```

### CreateMenuDto / UpdateMenuDto

```json
{
    "nome": "Nome do Menu",
    "ordem": 1,
    "icone": "menu-icon"
}
```

### PagedResultDto<MenuDto>

```json
{
    "items": [
        {
            "codMenu": 1,
            "nome": "Menu 1",
            "ordem": 1,
            "icone": "icon1"
        }
    ],
    "totalItems": 10,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
}
```

## Códigos de Resposta HTTP

- **200 OK**: Requisição bem-sucedida
- **201 Created**: Recurso criado com sucesso
- **204 No Content**: Recurso excluído com sucesso
- **400 Bad Request**: Dados inválidos ou validação falhou
- **401 Unauthorized**: Token ausente, inválido ou expirado
- **404 Not Found**: Recurso não encontrado
- **409 Conflict**: Conflito (ex: nome de menu já existe)
- **500 Internal Server Error**: Erro interno do servidor

## Notas Importantes

1. **Autenticação**: Todos os endpoints (exceto Swagger) requerem autenticação via Bearer token
2. **Validação de Token**: A API valida tokens via Token Introspection (RFC 7662) com o Identity Manager
3. **Nome Único**: O nome do menu deve ser único no sistema
4. **Variáveis Automáticas**: 
   - `access_token` é preenchido automaticamente após login
   - `menu_id` é preenchido automaticamente após criar ou buscar um menu
5. **Paginação**: O tamanho máximo de página é 100 itens

## Troubleshooting

### 401 Unauthorized

- Verifique se o token está válido e não expirado
- Execute "Obter Access Token" novamente para obter um novo token
- Verifique se o token está sendo enviado no header Authorization

### 404 Not Found

- Verifique se o ID do menu existe
- Verifique se a URL está correta

### 409 Conflict

- O nome do menu já existe no sistema
- Escolha um nome diferente

### 400 Bad Request

- Verifique os dados enviados no body
- Verifique se todos os campos obrigatórios estão presentes
- Verifique os tipos de dados (nome: string, ordem: int, icone: string)

## Exemplos de Uso

### Criar um Menu

```http
POST /api/menus
Authorization: Bearer {access_token}
Content-Type: application/json

{
    "nome": "Dashboard",
    "ordem": 1,
    "icone": "dashboard"
}
```

### Buscar Menus com Paginação

```http
GET /api/menus?nome=Dashboard&page=1&pageSize=10
Authorization: Bearer {access_token}
```

### Atualizar um Menu

```http
PUT /api/menus/1
Authorization: Bearer {access_token}
Content-Type: application/json

{
    "nome": "Dashboard Atualizado",
    "ordem": 2,
    "icone": "dashboard-updated"
}
```

### Excluir um Menu

```http
DELETE /api/menus/1
Authorization: Bearer {access_token}
```

