# DzDex - WhateverDex

Sistema de gerenciamento de cards com imagens, vídeos do YouTube e categorias personalizáveis.

## 📋 Descrição

O DzDex é uma aplicação web para criar e gerenciar uma coleção de cards com:
- Imagens e vídeos do YouTube
- Categorias personalizáveis
- Sistema de filtro por nome e tipo
- Interface moderna e responsiva
- Armazenamento local (LocalStorage)

## 🚀 Funcionalidades

### ✅ Principais Recursos
- **CRUD Completo**: Criar, visualizar, editar e excluir registros
- **Categorias Dinâmicas**: Criar e gerenciar tipos personalizados
- **Filtro Avançado**: Buscar por nome, descrição ou tipo
- **Modal Interativo**: Visualização detalhada com vídeo embutido
- **Armazenamento Local**: Dados salvos no navegador
- **Interface Responsiva**: Funciona em desktop e mobile

### 📱 Interface
- Design moderno com tema escuro
- Cards com hover effects
- Modal para visualização detalhada
- Formulário intuitivo para cadastro
- Sistema de notificações

## 🛠️ Tecnologias

- **Frontend**: HTML5, CSS3, JavaScript ES6+
- **Armazenamento**: LocalStorage API
- **Estilo**: CSS Grid e Flexbox
- **Responsividade**: Mobile-first design

## 📦 Instalação

### Pré-requisitos
- Navegador web moderno (Chrome, Firefox, Safari, Edge)
- Servidor web local (opcional, para desenvolvimento)

### Passos para Instalação

1. **Clone o repositório**:
```bash
git clone https://github.com/DezinTI/Rocket-Program-Alpar.git
cd Rocket-Program-Alpar
```

2. **Acesse os arquivos do DzDex**:
```bash
cd WhatEeverDex/Api\ Crud/wwwroot
```

3. **Abra a aplicação**:
- Opção 1: Abra `index.html` diretamente no navegador
- Opção 2: Use um servidor local:
```bash
# Python 3
python -m http.server 8000

# Node.js (se tiver instalado)
npx serve .

# VS Code com Live Server
# Clique com botão direito em index.html > "Open with Live Server"
```

## 🎯 Como Usar

### 1. **Visualizar Registros**
- Abra a aplicação
- Use a barra de busca para filtrar
- Selecione categorias no dropdown
- Clique nos cards para ver detalhes

### 2. **Adicionar Novo Registro**
1. Preencha o formulário "Novo Registro"
2. Campos obrigatórios: Nome e Tipo
3. Campos opcionais: Descrição, URL da imagem, URL do vídeo
4. Clique em "Adicionar"

### 3. **Criar Categorias**
1. No campo "Nova categoria", digite o nome (ex: "memes-anime")
2. Clique em "Cadastrar categoria"
3. A nova categoria aparecerá nos selects

### 4. **Editar Registros**
1. Clique em qualquer card para abrir o modal
2. Clique no botão "Editar"
3. Modifique as informações desejadas
4. Salve as alterações

### 5. **Excluir Registros**
1. Abra o modal do registro
2. Clique em "Excluir"
3. Confirme a exclusão

## 📁 Estrutura de Arquivos

```
WhatEeverDex/Api Crud/wwwroot/
├── index.html              # Página principal
├── styles.css              # Estilos CSS
├── script.js               # Lógica JavaScript
├── registros-static.js     # Dados de exemplo
├── uploads/                # Pasta de imagens
└── dashboard.html          # Dashboard (em desenvolvimento)
```

## 💡 Dicas de Uso

### **URLs de Vídeos**
O sistema aceita estes formatos de URL do YouTube:
- `https://www.youtube.com/watch?v=VIDEO_ID`
- `https://youtu.be/VIDEO_ID`
- `https://www.youtube.com/embed/VIDEO_ID`

### **Imagens**
- Use URLs de imagens online ou
- Faça upload para a pasta `uploads/`
- Imagens com erro usam placeholder automático

### **Categorias**
- Use nomes descritivos (ex: "alien-ben10", "luta-anime")
- Evite espaços e caracteres especiais
- Use hífen para separar palavras

## 🔧 Configuração

### **Dados Iniciais**
O sistema carrega dados de exemplo automaticamente na primeira execução:
- 6 registros pré-cadastrados
- 2 categorias padrão
- Imagens placeholder para testes

### **Backup dos Dados**
Os dados são salvos no LocalStorage do navegador:
- `dzdex-registros`: Lista de todos os registros
- `dzdex-categorias`: Lista de categorias personalizadas

### **Limpar Dados**
Para resetar a aplicação:
1. Abra o DevTools (F12)
2. Vá para aba Application/Storage
3. Localize "Local Storage"
4. Remova as chaves `dzdex-registros` e `dzdex-categorias`
5. Recarregue a página

## 🚀 Deploy

### **GitHub Pages**
1. Faça upload dos arquivos para o repositório
2. Ative GitHub Pages nas configurações
3. Selecione a branch `main`
4. Acesse: `https://username.github.io/repo/WhatEeverDex/Api%20Crud/wwwroot/`

### **Outras Plataformas**
- **Netlify**: Arraste e solte a pasta `wwwroot`
- **Vercel**: Importe o repositório GitHub
- **Firebase Hosting**: Use o Firebase CLI

## 🐛 Troubleshooting

### **Problemas Comuns**

** Vídeos não aparecem**
- Verifique se a URL do YouTube está correta
- Confirme se o vídeo não está privado ou restrito

** Imagens não carregam**
- Verifique se a URL da imagem está acessível
- Teste abrir a URL da imagem em outra aba

** Dados não são salvos**
- Verifique se o navegador permite LocalStorage
- Limpe o cache e cookies do navegador

** Modal não abre**
- Verifique o console do navegador por erros
- Confirme se o JavaScript está habilitado

## 🤝 Contribuição

Contribuições são bem-vindas! Para contribuir:

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/nova-funcionalidade`)
3. Faça commit das mudanças (`git commit -m 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 📞 Contato

- **Autor**: André Augustinho
- **Email**: dezin.ti01@gmail.com
- **GitHub**: [@DezinTI](https://github.com/DezinTI)
- **LinkedIn**: [André Augustinho](https://www.linkedin.com/in/dezin-ti-da-costa/)

---

**DzDex v1.0** - Sistema de gerenciamento de cards com JavaScript puro 🚀
