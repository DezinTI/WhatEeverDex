const usersList = document.getElementById('usersList');
const requestsList = document.getElementById('requestsList');
const logoutBtnAdmin = document.getElementById('logoutBtn');
const adminInfo = document.getElementById('adminInfo');
const requestsInfo = document.getElementById('requestsInfo');

async function carregarUsuarios() {
    const response = await window.authFetch('/api/users');
    const data = await response.json().catch(() => []);

    if (!response.ok) {
        usersList.innerHTML = '<p class="empty-state">Nao foi possivel carregar os usuarios.</p>';
        return;
    }

    adminInfo.textContent = `${data.length} usuario(s) encontrados.`;
    usersList.innerHTML = '';

    data.forEach(user => {
        const item = document.createElement('article');
        item.className = 'user-card';
        item.innerHTML = `
            <div>
                <h3>${user.nome}</h3>
                <p>${user.email}</p>
                <small>${user.totalRegistros} registro(s)</small>
            </div>
            <div class="user-actions">
                <span class="role-pill ${user.role === 'Admin' ? 'admin' : 'user'}">${user.role}</span>
                <button type="button" class="secondary-btn role-btn" data-id="${user.id}" data-role="${user.role === 'Admin' ? 'User' : 'Admin'}">
                    ${user.role === 'Admin' ? 'Remover admin' : 'Tornar admin'}
                </button>
                <button type="button" class="danger-btn delete-user-btn" data-id="${user.id}">Excluir</button>
            </div>
        `;

        usersList.appendChild(item);
    });

    document.querySelectorAll('.role-btn').forEach(button => {
        button.addEventListener('click', async () => {
            const userId = button.dataset.id;
            const role = button.dataset.role;
            if (!userId || !role) return;

            const response = await window.authFetch(`/api/users/${userId}/role`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ role })
            });

            if (!response.ok) {
                alert('Nao foi possivel alterar o privilegio do usuario.');
                return;
            }

            await carregarUsuarios();
        });
    });

    document.querySelectorAll('.delete-user-btn').forEach(button => {
        button.addEventListener('click', async () => {
            const userId = button.dataset.id;
            if (!userId || !confirm('Deseja excluir este usuario?')) return;

            const response = await window.authFetch(`/api/users/${userId}`, {
                method: 'DELETE'
            });

            if (!response.ok) {
                alert('Nao foi possivel excluir o usuario.');
                return;
            }

            await carregarUsuarios();
        });
    });
}

async function carregarRequests() {
    const response = await window.authFetch('/api/requests');
    const data = await response.json().catch(() => []);

    if (!response.ok) {
        requestsList.innerHTML = '<p class="empty-state">Nao foi possivel carregar os requests.</p>';
        return;
    }

    requestsInfo.textContent = data.length
        ? `${data.length} request(s) pendente(s).`
        : 'Nenhuma solicitacao pendente no momento.';

    requestsList.innerHTML = '';

    if (!data.length) {
        requestsList.innerHTML = '<p class="empty-state">Nenhuma solicitacao pendente.</p>';
        return;
    }

    data.forEach(request => {
        const item = document.createElement('article');
        item.className = 'request-card';
        item.innerHTML = `
            <div class="request-copy">
                <h3>${request.solicitadoPorNome} solicita alteracao no item "${request.itemNome}"</h3>
                <p>${request.solicitadoPorEmail}</p>
                <div class="request-grid">
                    <div>
                        <small>Nome atual</small>
                        <strong>${request.nomeAtual}</strong>
                    </div>
                    <div>
                        <small>Nome proposto</small>
                        <strong>${request.nomeProposto}</strong>
                    </div>
                </div>
                <div class="request-grid">
                    <div>
                        <small>Descricao atual</small>
                        <p>${request.descricaoAtual || 'Sem descricao.'}</p>
                    </div>
                    <div>
                        <small>Descricao proposta</small>
                        <p>${request.descricaoProposta || 'Sem descricao.'}</p>
                    </div>
                </div>
            </div>
            <div class="user-actions">
                <button type="button" class="secondary-btn approve-request-btn" data-id="${request.id}">Aprovar</button>
                <button type="button" class="danger-btn reject-request-btn" data-id="${request.id}">Recusar</button>
            </div>
        `;

        requestsList.appendChild(item);
    });

    document.querySelectorAll('.approve-request-btn').forEach(button => {
        button.addEventListener('click', async () => {
            const requestId = button.dataset.id;
            if (!requestId) return;

            const response = await window.authFetch(`/api/requests/${requestId}/aprovar`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({})
            });

            if (!response.ok) {
                alert('Nao foi possivel aprovar a solicitacao.');
                return;
            }

            await Promise.all([carregarUsuarios(), carregarRequests()]);
        });
    });

    document.querySelectorAll('.reject-request-btn').forEach(button => {
        button.addEventListener('click', async () => {
            const requestId = button.dataset.id;
            if (!requestId) return;

            const response = await window.authFetch(`/api/requests/${requestId}/recusar`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({})
            });

            if (!response.ok) {
                alert('Nao foi possivel recusar a solicitacao.');
                return;
            }

            await carregarRequests();
        });
    });
}

logoutBtnAdmin.addEventListener('click', () => {
    window.logout();
});

document.addEventListener('DOMContentLoaded', async () => {
    await window.requireAuth('Admin');
    await Promise.all([carregarUsuarios(), carregarRequests()]);
});
