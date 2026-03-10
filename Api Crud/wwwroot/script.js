const cardsContainer = document.getElementById('cardsContainer');
const filtroInput = document.getElementById('filtroInput');
const tipoFiltro = document.getElementById('tipoFiltro');
const atualizarBtn = document.getElementById('atualizarBtn');
const detalhesModal = document.getElementById('detalhesModal');
const detalhesConteudo = document.getElementById('detalhesConteudo');
const fecharModalBtn = document.getElementById('fecharModal');
const tipoInput = document.getElementById('tipo');
const novaCategoriaInput = document.getElementById('novaCategoria');
const criarCategoriaBtn = document.getElementById('criarCategoriaBtn');

const novoMonstroForm = document.getElementById('novoMonstroForm');
let modoEstatico = false;
let registrosCache = [];
let categoriasCache = [];
const IMAGEM_PADRAO = 'https://via.placeholder.com/480x300?text=DzDex';

function valorTipo(tipo) {
    const categoria = categoriasCache.find(item => item.valor === tipo);
    if (categoria) return categoria.nome;

    return tipo
        .split('-')
        .filter(Boolean)
        .map(parte => parte.charAt(0).toUpperCase() + parte.slice(1))
        .join(' ');
}

function pegarDescricao(descricao) {
    if (!descricao || !descricao.trim()) {
        return 'Sem descricao ainda.';
    }

    return descricao.trim();
}

function pegarImagem(url) {
    if (!url || !url.trim()) {
        return IMAGEM_PADRAO;
    }

    const imagem = url.trim().replaceAll('\\', '/');

    if (modoEstatico && imagem.startsWith('/uploads/')) {
        return IMAGEM_PADRAO;
    }

    return encodeURI(imagem);
}

function slugCategoria(valor) {
    if (!valor) return '';

    return valor
        .toLowerCase()
        .trim()
        .replace(/[^a-z0-9\s-_]/g, '')
        .replace(/[\s_]+/g, '-')
        .replace(/-+/g, '-')
        .replace(/^-|-$/g, '');
}

function categoriasLocaisBase() {
    const valores = new Set([
        'luta-anime',
        'alien-ben10',
        ...(window.REGISTROS_STATIC || []).map(item => slugCategoria(item.tipo))
    ]);

    return [...valores]
        .filter(Boolean)
        .sort()
        .map(valor => ({
            valor,
            nome: valorTipo(valor)
        }));
}

function preencherSelectCategorias() {
    const filtroAtual = tipoFiltro.value;
    const tipoAtual = tipoInput.value;

    tipoFiltro.innerHTML = '<option value="">Todos os tipos</option>';
    tipoInput.innerHTML = '<option value="">Selecione o tipo</option>';

    categoriasCache.forEach(categoria => {
        const opFiltro = document.createElement('option');
        opFiltro.value = categoria.valor;
        opFiltro.textContent = categoria.nome;
        tipoFiltro.appendChild(opFiltro);

        const opTipo = document.createElement('option');
        opTipo.value = categoria.valor;
        opTipo.textContent = categoria.nome;
        tipoInput.appendChild(opTipo);
    });

    tipoFiltro.value = categoriasCache.some(c => c.valor === filtroAtual) ? filtroAtual : '';
    tipoInput.value = categoriasCache.some(c => c.valor === tipoAtual) ? tipoAtual : '';
}

async function carregarCategorias() {
    if (!modoEstatico) {
        try {
            const response = await fetch('/api/categorias');
            if (!response.ok) throw new Error('Falha ao carregar categorias');

            const categorias = await response.json();
            categoriasCache = categorias
                .map(item => ({
                    valor: slugCategoria(item.valor),
                    nome: item.nome || valorTipo(slugCategoria(item.valor))
                }))
                .filter(item => item.valor);
        } catch {
            ativarModoEstatico();
        }
    }

    if (modoEstatico) {
        categoriasCache = categoriasLocaisBase();
    }

    preencherSelectCategorias();
}

function converterYoutubeParaEmbed(url) {
    if (!url) return '';

    try {
        const parsed = new URL(url);
        const host = parsed.hostname.toLowerCase();

        if (host.includes('youtu.be')) {
            const id = parsed.pathname.replace('/', '');
            return `https://www.youtube.com/embed/${id}`;
        }

        if (host.includes('youtube.com')) {
            const id = parsed.searchParams.get('v');
            if (id) return `https://www.youtube.com/embed/${id}`;
        }

        return url;
    } catch {
        return url;
    }
}

function normalizarRegistro(item) {
    return {
        id: item.id,
        nome: item.nome,
        tipo: item.tipo,
        imagemUrl: item.imagemUrl,
        videoYoutubeUrl: item.videoYoutubeUrl,
        videoYoutubeEmbedUrl: item.videoYoutubeEmbedUrl || converterYoutubeParaEmbed(item.videoYoutubeUrl),
        descricao: item.descricao || ''
    };
}

function ativarModoEstatico() {
    if (modoEstatico) return;

    modoEstatico = true;

    const painel = document.querySelector('.panel');
    if (painel) {
        painel.innerHTML = `<h2>Novo Registro</h2><p>Modo GitHub Pages: aqui so funciona visualizacao.</p>`;
    }
}

function filtrarRegistrosLocais(filtro, tipo) {
    const todos = (window.REGISTROS_STATIC || []).map(normalizarRegistro);

    return todos.filter(item => {
        const passouTipo = !tipo || item.tipo === tipo;
        const texto = `${item.nome} ${item.tipo} ${item.descricao}`.toLowerCase();
        const passouBusca = !filtro || texto.includes(filtro.toLowerCase());
        return passouTipo && passouBusca;
    });
}

function renderizarCards(monstros) {
    cardsContainer.innerHTML = '';

    if (!monstros.length) {
        cardsContainer.innerHTML = '<p>Nenhum registro encontrado.</p>';
        return;
    }

    monstros.forEach(monstro => {
        const descricao = pegarDescricao(monstro.descricao);
        const imagem = pegarImagem(monstro.imagemUrl);

        const card = document.createElement('article');
        card.className = 'card';
        card.innerHTML = `
            <img src="${imagem}" alt="${monstro.nome}" onerror="this.onerror=null;this.src='${IMAGEM_PADRAO}'">
            <div class="card-body">
                <h3>${monstro.nome}</h3>
                <p><strong>Tipo:</strong> ${valorTipo(monstro.tipo)}</p>
                <p class="descricao-card">${descricao}</p>
                <iframe class="youtube-preview" src="${monstro.videoYoutubeEmbedUrl}" title="Previa de ${monstro.nome}" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                <div class="card-actions">
                    <button data-id="${monstro.id}" class="detalhes-btn">Detalhes</button>
                    ${modoEstatico ? '' : `<button data-id="${monstro.id}" class="renomear-btn">Renomear</button><button data-id="${monstro.id}" class="excluir-btn">Excluir</button>`}
                </div>
            </div>
        `;
        cardsContainer.appendChild(card);
    });

    document.querySelectorAll('.detalhes-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
            await abrirDetalhes(btn.getAttribute('data-id'));
        });
    });

    if (modoEstatico) return;

    document.querySelectorAll('.renomear-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
            const id = Number(btn.getAttribute('data-id'));
            const novoNome = prompt('Novo nome:');
            if (!novoNome || !novoNome.trim()) return;

            const responseRename = await fetch(`/api/registros/${id}/renomear`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ nome: novoNome.trim() })
            });

            if (!responseRename.ok) {
                alert('Nao foi possivel renomear.');
                return;
            }

            await carregarMonstros(filtroInput.value, tipoFiltro.value);
        });
    });

    document.querySelectorAll('.excluir-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
            const id = Number(btn.getAttribute('data-id'));
            if (!confirm('Deseja excluir este registro?')) return;

            const responseDelete = await fetch(`/api/registros/${id}`, { method: 'DELETE' });
            if (!responseDelete.ok) {
                alert('Nao foi possivel excluir.');
                return;
            }

            await carregarMonstros(filtroInput.value, tipoFiltro.value);
        });
    });
}

async function carregarMonstros(filtro = '', tipo = '') {
    const params = new URLSearchParams();
    if (filtro) params.set('busca', filtro);
    if (tipo) params.set('tipo', tipo);

    if (!modoEstatico) {
        try {
            const url = params.toString() ? `/api/registros?${params.toString()}` : '/api/registros';
            const response = await fetch(url);

            if (!response.ok) throw new Error('Falha API');

            const monstros = (await response.json()).map(normalizarRegistro);
            registrosCache = monstros;
            renderizarCards(monstros);
            return;
        } catch {
            ativarModoEstatico();
        }
    }

    const locais = filtrarRegistrosLocais(filtro, tipo);
    registrosCache = locais;
    renderizarCards(locais);
}

async function abrirDetalhes(id) {
    let monstro = null;

    if (modoEstatico) {
        monstro = registrosCache.find(item => Number(item.id) === Number(id));
    } else {
        const response = await fetch(`/api/registros/${id}`);
        if (!response.ok) return;
        monstro = normalizarRegistro(await response.json());
    }

    if (!monstro) return;

    const descricao = pegarDescricao(monstro.descricao);
    const imagem = pegarImagem(monstro.imagemUrl);

    detalhesConteudo.innerHTML = `
        <img src="${imagem}" alt="${monstro.nome}" class="detalhe-imagem" onerror="this.onerror=null;this.src='${IMAGEM_PADRAO}'">
        <h3>${monstro.nome}</h3>
        <p><strong>Tipo:</strong> ${valorTipo(monstro.tipo)}</p>
        <iframe class="youtube-preview" src="${monstro.videoYoutubeEmbedUrl}" title="Previa de ${monstro.nome}" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
        <p><strong>Descricao:</strong> ${descricao}</p>
        <p><strong>YouTube:</strong> <a href="${monstro.videoYoutubeUrl}" target="_blank">Abrir video</a></p>
    `;

    detalhesModal.style.display = 'flex';
}

novoMonstroForm.addEventListener('submit', async (event) => {
    event.preventDefault();

    if (modoEstatico) {
        alert('No GitHub Pages essa parte de cadastro nao funciona, so visualizacao.');
        return;
    }

    const formData = new FormData();
    formData.append('nome', document.getElementById('nome').value);
    formData.append('tipo', tipoInput.value);
    formData.append('videoYoutubeUrl', document.getElementById('videoYoutubeUrl').value);
    formData.append('descricao', document.getElementById('descricao').value);

    const imagemUrl = document.getElementById('imagemUrl').value;
    if (imagemUrl) formData.append('imagemUrl', imagemUrl);

    const imagemArquivoInput = document.getElementById('imagemArquivo');
    if (imagemArquivoInput.files.length > 0) {
        formData.append('imagemArquivo', imagemArquivoInput.files[0]);
    }

    const response = await fetch('/api/registros', {
        method: 'POST',
        body: formData
    });

    if (!response.ok) {
        alert('Nao foi possivel salvar o registro.');
        return;
    }

    novoMonstroForm.reset();
    preencherSelectCategorias();
    await carregarMonstros(filtroInput.value, tipoFiltro.value);
});

criarCategoriaBtn.addEventListener('click', async () => {
    const valor = slugCategoria(novaCategoriaInput.value);
    if (!valor) {
        alert('Digite um nome valido para a categoria.');
        return;
    }

    if (modoEstatico) {
        alert('No modo estatico nao e possivel cadastrar categorias.');
        return;
    }

    const response = await fetch('/api/categorias', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ nome: valor })
    });

    if (!response.ok && response.status !== 409) {
        alert('Nao foi possivel cadastrar a categoria.');
        return;
    }

    await carregarCategorias();
    tipoInput.value = valor;
    novaCategoriaInput.value = '';
});

filtroInput.addEventListener('input', async () => {
    await carregarMonstros(filtroInput.value, tipoFiltro.value);
});

tipoFiltro.addEventListener('change', async () => {
    await carregarMonstros(filtroInput.value, tipoFiltro.value);
});

atualizarBtn.addEventListener('click', async () => {
    await carregarMonstros(filtroInput.value, tipoFiltro.value);
});

fecharModalBtn.addEventListener('click', () => {
    detalhesModal.style.display = 'none';
});

detalhesModal.addEventListener('click', (event) => {
    if (event.target === detalhesModal) {
        detalhesModal.style.display = 'none';
    }
});

document.addEventListener('DOMContentLoaded', async () => {
    await carregarCategorias();
    await carregarMonstros();
});