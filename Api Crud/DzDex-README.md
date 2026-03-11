# DzDex API + Frontend

Aplicacao ASP.NET Core com frontend estatico para cadastrar, listar, editar e excluir registros, com autenticacao JWT, usuarios com papel de admin, fila de aprovacao para edicoes e categorias dinamicas.

## O que o sistema faz

- Cadastro e login de usuarios.
- CRUD de registros em `/api/registros`.
- CRUD de categorias em `/api/categorias`.
- Painel admin para promover usuarios para admin ou excluir usuarios.
- Vinculo de cada registro ao usuario que criou.
- Requests de alteracao de nome/descricao para aprovacao do admin.

## Estrutura principal

### Backend

- `Program.cs`
  - Configura controllers, SQLite, JWT bearer, Swagger e arquivos estaticos.
  - Redireciona `/` para `login.html`.

- `Data/EstoqueContext.cs`
  - Contexto do Entity Framework.
  - Define tabelas de `Itens` e `Usuarios`.

- `Models/Item.cs`
  - Entidade principal dos registros.
  - Guarda nome, tipo, imagem, video, descricao e usuario criador.

- `Models/Usuario.cs`
  - Entidade de usuario e DTOs de login/cadastro/alteracao de role.

- `Controllers/AuthController.cs`
  - Login, cadastro e endpoint `/api/auth/me`.
  - Gera o token JWT.

- `Controllers/ItensController.cs`
  - Lista, cria e exclui registros.
  - Admin aplica edicoes direto; usuario comum envia solicitacao de alteracao.

- `Controllers/RequestsController.cs`
  - Somente admin.
  - Lista requests pendentes e aprova/recusa alteracoes enviadas por usuarios.

- `Controllers/CategoriasController.cs`
  - Lista, cria, edita e exclui categorias.
  - As categorias persistem em `App_Data/categorias.json`.

- `Controllers/UsersController.cs`
  - Somente admin.
  - Lista usuarios, altera role e exclui usuarios.

- `Seed/SeedDatabase.cs`
  - Garante estrutura minima do banco.
  - Cria/atualiza os admins padrao na inicializacao.

- `App_Data/categorias.json`
  - Arquivo com categorias cadastradas manualmente.

### Frontend

- `wwwroot/login.html`
  - Tela de login e cadastro.

- `wwwroot/index.html`
  - Tela principal autenticada.
  - Mostra registros, formulario de novo registro, gerenciamento de categorias e envio de edicao para aprovacao.

- `wwwroot/admin.html`
  - Painel de administracao de usuarios e requests pendentes.

- `wwwroot/auth.js`
  - Helpers de token, sessao, logout e fetch autenticado.

- `wwwroot/login.js`
  - Fluxo de login/cadastro.

- `wwwroot/script.js`
  - Fluxo da tela principal: registros, filtros, modal e categorias.

- `wwwroot/admin.js`
  - Fluxo do painel admin.

- `wwwroot/css/base.css`
  - Estilos compartilhados.

- `wwwroot/css/login.css`
  - Estilos da pagina de login.

- `wwwroot/css/index.css`
  - Estilos da pagina principal.

- `wwwroot/css/admin.css`
  - Estilos do painel admin.

## Fluxo de permissao

- Usuario comum:
  - pode criar registros;
  - pode solicitar edicao de nome e descricao de qualquer registro;
  - pode criar/editar categorias;
  - nao pode excluir registros;
  - nao pode excluir categorias;
  - nao acessa `/admin.html` nem `/api/users`.

- Admin:
  - pode gerenciar qualquer registro;
  - pode aprovar ou recusar requests de alteracao;
  - pode promover usuarios para admin;
  - pode excluir usuarios.

## Como rodar localmente

Na pasta `Api Crud`:

```bash
dotnet restore
dotnet build
dotnet run
```

Depois abra:

- `http://localhost:5000/login.html`
- ou a porta mostrada no terminal.

## Swagger

Em desenvolvimento, o Swagger fica em:

```text
/swagger
```

Para testar endpoints protegidos:

1. faça login em `/api/auth/login`;
2. copie o token JWT;
3. clique em `Authorize` no Swagger;
4. informe `Bearer SEU_TOKEN`.

## Deploy

O projeto .NET fica dentro de `Api Crud/`, entao o caminho mais confiavel para deploy e o `Dockerfile` na raiz do repositorio.

Ele:

- faz restore/publicacao do projeto dentro de `Api Crud`;
- sobe a aplicacao ASP.NET na porta definida por `PORT`.

### Variaveis de ambiente no Railway

Para subir em producao sem depender da configuracao local, defina estas variaveis no Railway:

- `Jwt__Key`
  - chave JWT forte usada para assinar os tokens.
- `Jwt__Issuer`
  - exemplo: `DzDexAPI`.
- `Jwt__Audience`
  - exemplo: `DzDexClient`.
- `ConnectionStrings__DefaultConnection`
  - se usar SQLite com volume, exemplo: `Data Source=/data/dzdex.db`.

Sem `Jwt__Key` em producao a aplicacao nao inicializa.

### Persistencia de banco

O arquivo `dzdex.db` local agora fica fora do commit e fora do build Docker.

Se for usar Railway com SQLite, anexe um volume e aponte `ConnectionStrings__DefaultConnection` para esse caminho persistente. Exemplo:

```text
Data Source=/data/dzdex.db
```

Se nao configurar volume, o banco do container pode ser perdido a cada novo deploy.

## Observacoes

- Imagens enviadas por arquivo vao para `wwwroot/uploads/`.
- Categorias com registros vinculados nao podem ser excluidas.
- Categorias so podem ser excluidas por admin.
- Requests recusados nao alteram o item original.
- O banco SQLite principal e `dzdex.db`.
