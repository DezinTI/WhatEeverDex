const cardsContainer = document.getElementById('cardsContainer');
const categoriasLista = document.getElementById('categoriasLista');
const filtroInput = document.getElementById('filtroInput');
const tipoFiltro = document.getElementById('tipoFiltro');
const atualizarBtn = document.getElementById('atualizarBtn');
const detalhesModal = document.getElementById('detalhesModal');
const detalhesConteudo = document.getElementById('detalhesConteudo');
const fecharModalBtn = document.getElementById('fecharModal');
const edicaoModal = document.getElementById('edicaoModal');
const fecharEdicaoModalBtn = document.getElementById('fecharEdicaoModal');
const edicaoForm = document.getElementById('edicaoForm');
const editIdInput = document.getElementById('editId');
const editNomeInput = document.getElementById('editNome');
const editDescricaoInput = document.getElementById('editDescricao');
const salvarEdicaoBtn = document.getElementById('salvarEdicaoBtn');
const tipoInput = document.getElementById('tipo');
const novaCategoriaInput = document.getElementById('novaCategoria');
const criarCategoriaBtn = document.getElementById('criarCategoriaBtn');
const novoMonstroForm = document.getElementById('novoMonstroForm');
const userBadge = document.getElementById('userBadge');
const adminLink = document.getElementById('adminLink');
const logoutBtn = document.getElementById('logoutBtn');

const IMAGEM_PADRAO = 'https://via.placeholder.com/480x300?text=DzDex';

let registrosCache = [];
let categoriasCache = [];

function slugCategoria(valor) {
    if (!valor) return '';

    return valor
        .toLowerCase()
        .trim()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/[^a-z0-9\s-_]/g, '')
        .replace(/[\s_]+/g, '-')
        .replace(/-+/g, '-')
        .replace(/^-|-$/g, '');
}

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

    return encodeURI(url.trim().replaceAll('\\', '/'));
}

function mostrarMensagemRegistros(mensagem) {
    cardsContainer.innerHTML = `<p class="empty-state">${mensagem}</p>`;
}

function mostrarMensagemCategorias(mensagem) {
    categoriasLista.innerHTML = `<p class="empty-state">${mensagem}</p>`;
}

function usuarioPodeExcluirCategorias() {
    return window.currentUser?.role === 'Admin';
}

function usuarioPodeExcluirRegistro() {
    return window.currentUser?.role === 'Admin';
}

function usuarioEditaDireto() {
    return window.currentUser?.role === 'Admin';
}

function preencherSelectCategorias() {
    const filtroAtual = tipoFiltro.value;
    const tipoAtual = tipoInput.value;

    tipoFiltro.innerHTML = '<option value="">Todos os tipos</option>';
    tipoInput.innerHTML = '<option value="">Selecione o tipo</option>';

    categoriasCache.forEach(categoria => {
        const optionFiltro = document.createElement('option');
        optionFiltro.value = categoria.valor;
        optionFiltro.textContent = categoria.nome;
        tipoFiltro.appendChild(optionFiltro);

        const optionTipo = document.createElement('option');
        optionTipo.value = categoria.valor;
        optionTipo.textContent = categoria.nome;
        tipoInput.appendChild(optionTipo);
    });

    tipoFiltro.value = categoriasCache.some(item => item.valor === filtroAtual) ? filtroAtual : '';
    tipoInput.value = categoriasCache.some(item => item.valor === tipoAtual) ? tipoAtual : '';
}

async function carregarCategorias() {
    const response = await window.authFetch('/api/categorias');
    if (!response.ok) {
        throw new Error('Falha ao carregar categorias.');
    }

    const categorias = await response.json();
    categoriasCache = categorias
        .map(item => ({
            valor: slugCategoria(item.valor),
            nome: item.nome || valorTipo(slugCategoria(item.valor)),
            totalItens: item.totalItens || 0
        }))
        .filter(item => item.valor)
        .sort((a, b) => a.nome.localeCompare(b.nome));

    preencherSelectCategorias();
    renderizarCategorias();
}

function renderizarCategorias() {
    categoriasLista.innerHTML = '';

    if (!categoriasCache.length) {
        categoriasLista.innerHTML = '<p class="empty-state">Nenhuma categoria criada ainda.</p>';
        return;
    }

    categoriasCache.forEach(categoria => {
        const item = document.createElement('article');
        item.className = 'category-item';
        const podeExcluirCategoria = usuarioPodeExcluirCategorias();
        item.innerHTML = `
            <div>
                <strong>${categoria.nome}</strong>
                <span>${categoria.valor}</span>
            </div>
            <div class="category-meta">
                <small>${categoria.totalItens} registro(s)</small>
                <button type="button" data-id="${categoria.valor}" data-name="${categoria.nome}" class="secondary-btn edit-category-btn">Editar</button>
                ${podeExcluirCategoria ? `<button type="button" data-id="${categoria.valor}" data-total="${categoria.totalItens}" class="danger-btn delete-category-btn">Excluir</button>` : ''}
            </div>
        `;

        categoriasLista.appendChild(item);
    });

    document.querySelectorAll('.edit-category-btn').forEach(button => {
        button.addEventListener('click', async () => {
            const categoriaId = button.dataset.id;
            const nomeAtual = button.dataset.name;
            const novoNome = prompt('Novo nome da categoria:', nomeAtual);

            if (!categoriaId || !novoNome || !novoNome.trim()) return;

            const response = await window.authFetch(`/api/categorias/${categoriaId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ nome: novoNome.trim() })
            });

            const data = await tentarLerJson(response);
            if (!response.ok) {
                alert(data?.message || data?.mensagem || 'Nao foi possivel editar a categoria.');
                return;
            }

            await carregarCategorias();
            await carregarMonstros(filtroInput.value, tipoFiltro.value);
        });
    });

    document.querySelectorAll('.delete-category-btn').forEach(button => {
        button.addEventListener('click', async () => {
            const categoriaId = button.dataset.id;
            const totalItens = Number(button.dataset.total || '0');

            if (!categoriaId) return;

            if (totalItens > 0) {
                alert('Nao e possivel excluir categoria com registros vinculados.');
                return;
            }

            if (!confirm('Deseja excluir esta categoria?')) return;

            const response = await window.authFetch(`/api/categorias/${categoriaId}`, {
                method: 'DELETE'
            });

            const data = await tentarLerJson(response);
            if (!response.ok) {
                alert(data?.message || data?.mensagem || 'Nao foi possivel excluir a categoria.');
                return;
            }

            await carregarCategorias();
        });
    });
}

function converterYoutubeParaEmbed(url) {
    if (!url) return '';

    try {
        const parsed = new URL(url);
        const host = parsed.hostname.toLowerCase();

        if (host.includes('youtu.be')) {
            return `https://www.youtube.com/embed/${parsed.pathname.replace('/', '')}`;
        }

        if (host.includes('youtube.com')) {
            const videoId = parsed.searchParams.get('v');
            if (videoId) return `https://www.youtube.com/embed/${videoId}`;
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
        descricao: item.descricao || '',
        criadoPorNome: item.criadoPorNome || 'Sem autor identificado',
        criadoPorEmail: item.criadoPorEmail || '',
        criadoPorId: item.criadoPorId ?? null,
        atualizadoEm: item.atualizadoEm,
        criadoEm: item.criadoEm
    };
}

function renderizarCards(registros) {
    cardsContainer.innerHTML = '';

    if (!registros.length) {
        cardsContainer.innerHTML = '<p class="empty-state">Nenhum registro encontrado.</p>';
        return;
    }

    registros.forEach(registro => {
        const descricao = pegarDescricao(registro.descricao);
        const imagem = pegarImagem(registro.imagemUrl);
        const podeExcluir = usuarioPodeExcluirRegistro();
        const tituloBotaoEdicao = usuarioEditaDireto() ? 'Editar' : 'Solicitar edicao';
        const classeAcoes = podeExcluir ? '' : 'dual-action';

        const card = document.createElement('article');
        card.className = 'card';
        card.innerHTML = `
            <div class="card-media">
                <img src="${imagem}" alt="${registro.nome}" loading="lazy" onerror="this.onerror=null;this.src='${IMAGEM_PADRAO}'">
            </div>
            <div class="card-body">
                <div class="card-topline">
                    <h3>${registro.nome}</h3>
                    <span class="chip">${valorTipo(registro.tipo)}</span>
                </div>
                <p class="descricao-card">${descricao}</p>
                <p class="card-author">Criado por: <strong>${registro.criadoPorNome}</strong></p>
                <iframe class="youtube-preview" src="${registro.videoYoutubeEmbedUrl}" title="Previa de ${registro.nome}" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                <div class="card-actions ${classeAcoes}">
                    <button data-id="${registro.id}" class="detalhes-btn">Detalhes</button>
                    <button data-id="${registro.id}" class="editar-btn">${tituloBotaoEdicao}</button>
                    ${podeExcluir ? `<button data-id="${registro.id}" class="excluir-btn">Excluir</button>` : ''}
                </div>
            </div>
        `;

        cardsContainer.appendChild(card);
    });

    document.querySelectorAll('.detalhes-btn').forEach(button => {
        button.addEventListener('click', async () => {
            await abrirDetalhes(button.dataset.id);
        });
    });

    document.querySelectorAll('.editar-btn').forEach(button => {
        button.addEventListener('click', async () => {
            const id = Number(button.dataset.id);
            const registro = registrosCache.find(item => item.id === id);
            if (!registro) return;

            abrirModalEdicao(registro);
        });
    });

    document.querySelectorAll('.excluir-btn').forEach(button => {
        button.addEventListener('click', async () => {
            const id = Number(button.dataset.id);
            if (!confirm('Deseja excluir este registro?')) return;

            const response = await window.authFetch(`/api/registros/${id}`, {
                method: 'DELETE'
            });

            if (!response.ok) {
                alert('Nao foi possivel excluir o registro.');
                return;
            }

            await carregarMonstros(filtroInput.value, tipoFiltro.value);
        });
    });
}

function abrirModalEdicao(registro) {
    editIdInput.value = String(registro.id);
    editNomeInput.value = registro.nome;
    editDescricaoInput.value = registro.descricao || '';
    salvarEdicaoBtn.textContent = usuarioEditaDireto() ? 'Salvar alteracao' : 'Enviar para aprovacao';
    edicaoModal.style.display = 'flex';
}

function fecharModalEdicao() {
    edicaoForm.reset();
    editIdInput.value = '';
    edicaoModal.style.display = 'none';
}

async function carregarMonstros(filtro = '', tipo = '') {
    const params = new URLSearchParams();
    if (filtro) params.set('busca', filtro);
    if (tipo) params.set('tipo', tipo);

    const url = params.toString() ? `/api/registros?${params.toString()}` : '/api/registros';
    const response = await window.authFetch(url);
    if (!response.ok) {
        throw new Error('Falha ao carregar registros.');
    }

    registrosCache = (await response.json()).map(normalizarRegistro);
    renderizarCards(registrosCache);
}

async function abrirDetalhes(id) {
    const response = await window.authFetch(`/api/registros/${id}`);
    if (!response.ok) return;

    const registro = normalizarRegistro(await response.json());
    const descricao = pegarDescricao(registro.descricao);
    const imagem = pegarImagem(registro.imagemUrl);

    detalhesConteudo.innerHTML = `
        <img src="${imagem}" alt="${registro.nome}" class="detalhe-imagem" onerror="this.onerror=null;this.src='${IMAGEM_PADRAO}'">
        <h3>${registro.nome}</h3>
        <p><strong>Tipo:</strong> ${valorTipo(registro.tipo)}</p>
        <p><strong>Criado por:</strong> ${registro.criadoPorNome}</p>
        <iframe class="youtube-preview" src="${registro.videoYoutubeEmbedUrl}" title="Previa de ${registro.nome}" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
        <p><strong>Descricao:</strong> ${descricao}</p>
        <p><strong>YouTube:</strong> <a href="${registro.videoYoutubeUrl}" target="_blank" rel="noreferrer">Abrir video</a></p>
    `;

    detalhesModal.style.display = 'flex';
}

async function tentarLerJson(response) {
    try {
        return await response.json();
    } catch {
        return null;
    }
}

novoMonstroForm.addEventListener('submit', async event => {
    event.preventDefault();

    const formData = new FormData();
    formData.append('nome', document.getElementById('nome').value);
    formData.append('tipo', tipoInput.value);
    formData.append('videoYoutubeUrl', document.getElementById('videoYoutubeUrl').value);
    formData.append('descricao', document.getElementById('descricao').value);

    const imagemUrl = document.getElementById('imagemUrl').value;
    if (imagemUrl) {
        formData.append('imagemUrl', imagemUrl);
    }

    const imagemArquivoInput = document.getElementById('imagemArquivo');
    if (imagemArquivoInput.files.length > 0) {
        formData.append('imagemArquivo', imagemArquivoInput.files[0]);
    }

    const response = await window.authFetch('/api/registros', {
        method: 'POST',
        body: formData
    });

    if (!response.ok) {
        const data = await tentarLerJson(response);
        alert(data?.message || 'Nao foi possivel salvar o registro.');
        return;
    }

    novoMonstroForm.reset();
    await carregarCategorias();
    await carregarMonstros(filtroInput.value, tipoFiltro.value);
});

criarCategoriaBtn.addEventListener('click', async () => {
    const valor = slugCategoria(novaCategoriaInput.value);
    if (!valor) {
        alert('Digite um nome valido para a categoria.');
        return;
    }

    const response = await window.authFetch('/api/categorias', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ nome: valor })
    });

    const data = await tentarLerJson(response);
    if (!response.ok && response.status !== 409) {
        alert(data?.message || 'Nao foi possivel cadastrar a categoria.');
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

detalhesModal.addEventListener('click', event => {
    if (event.target === detalhesModal) {
        detalhesModal.style.display = 'none';
    }
});

logoutBtn.addEventListener('click', () => {
    window.logout();
});

fecharEdicaoModalBtn.addEventListener('click', () => {
    fecharModalEdicao();
});

edicaoModal.addEventListener('click', event => {
    if (event.target === edicaoModal) {
        fecharModalEdicao();
    }
});

edicaoForm.addEventListener('submit', async event => {
    event.preventDefault();

    const id = Number(editIdInput.value);
    const nome = editNomeInput.value.trim();
    const descricao = editDescricaoInput.value.trim();

    if (!id || !nome) {
        alert('Informe um nome valido.');
        return;
    }

    let response;

    if (usuarioEditaDireto()) {
        const formData = new FormData();
        formData.append('nome', nome);
        formData.append('descricao', descricao);

        response = await window.authFetch(`/api/registros/${id}`, {
            method: 'PUT',
            body: formData
        });
    } else {
        response = await window.authFetch(`/api/registros/${id}/solicitar-edicao`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ nome, descricao })
        });
    }

    const data = await tentarLerJson(response);
    if (!response.ok) {
        alert(data?.message || data?.mensagem || 'Nao foi possivel enviar a alteracao.');
        return;
    }

    fecharModalEdicao();
    alert(data?.message || (usuarioEditaDireto()
        ? 'Registro atualizado com sucesso.'
        : 'Solicitacao enviada para aprovacao.'));
    await carregarMonstros(filtroInput.value, tipoFiltro.value);
});

document.addEventListener('DOMContentLoaded', async () => {
    await window.requireAuth();

    if (window.currentUser) {
        userBadge.textContent = `${window.currentUser.nome} (${window.currentUser.role})`;
        if (window.currentUser.role === 'Admin') {
            adminLink.classList.remove('hidden');
        }
    }

    const [categoriasResult, registrosResult] = await Promise.allSettled([
        carregarCategorias(),
        carregarMonstros()
    ]);

    if (categoriasResult.status === 'rejected') {
        console.error(categoriasResult.reason);
        mostrarMensagemCategorias('Nao foi possivel carregar as categorias agora.');
    }

    if (registrosResult.status === 'rejected') {
        console.error(registrosResult.reason);
        mostrarMensagemRegistros('Nao foi possivel carregar os registros agora.');
    }
});
