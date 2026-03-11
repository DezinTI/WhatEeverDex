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

## Funcoes novas (resumo)

- Autenticacao JWT com login e cadastro.
- Controle de permissao por papel (`Admin` e `User`).
- Painel admin com:
  - gerenciamento de usuarios;
  - aprovacao/recusa de solicitacoes de edicao.
- Registros com autoria (`CriadoPor`).
- Fluxo de solicitacao de edicao para usuario comum.
- Categorias dinamicas com criacao, edicao e exclusao (com regras de seguranca).
- Frontend separado por paginas e CSS dedicado (`login`, `index`, `admin`).

## Login e usuarios

- O login valida email/senha com hash BCrypt.
- Usuarios ja criados continuam funcionando no mesmo banco.
- Admins padrao garantidos na inicializacao:
  - `adm@adm.com` / `adm10`
  - `dezin@dezin.com` / `dezin10`

## Como rodar local

Na pasta `Api Crud`:

```bash
dotnet restore
dotnet build
dotnet run
```

Depois, abra `http://localhost:5000/login.html` ou a porta mostrada no terminal.

## Link publicado

- Railway (producao): `COLE_AQUI_O_LINK_PUBLICO_DO_RAILWAY`

## Aviso importante

- O ambiente gratuito pode hibernar ou expirar.
- Depois de 30 dias+, o link pode parar de responder se o plano/instancia for encerrado.

## Observacoes

- Imagens enviadas por arquivo vao para `wwwroot/uploads/`.
- Categorias com registros vinculados nao podem ser excluidas.
- Categorias so podem ser excluidas por admin.
- Requests recusados nao alteram o item original.
- O banco SQLite principal e `dzdex.db`.
