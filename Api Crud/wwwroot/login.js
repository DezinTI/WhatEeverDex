const loginTab = document.getElementById('loginTab');
const registerTab = document.getElementById('registerTab');
const loginForm = document.getElementById('loginForm');
const registerForm = document.getElementById('registerForm');
const loginMensagem = document.getElementById('loginMensagem');
const registerMensagem = document.getElementById('registerMensagem');

function alternarAba(modo) {
    const isLogin = modo === 'login';
    loginTab.classList.toggle('active', isLogin);
    registerTab.classList.toggle('active', !isLogin);
    loginForm.classList.toggle('hidden', !isLogin);
    registerForm.classList.toggle('hidden', isLogin);
    loginMensagem.textContent = '';
    registerMensagem.textContent = '';
}

loginTab.addEventListener('click', () => alternarAba('login'));
registerTab.addEventListener('click', () => alternarAba('register'));

loginForm.addEventListener('submit', async event => {
    event.preventDefault();

    loginMensagem.textContent = '';

    const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            email: document.getElementById('loginEmail').value,
            senha: document.getElementById('loginSenha').value
        })
    });

    const data = await response.json().catch(() => ({}));
    if (!response.ok) {
        loginMensagem.textContent = data.message || 'Nao foi possivel fazer login.';
        loginMensagem.className = 'form-message error';
        return;
    }

    localStorage.setItem('token', data.token);
    localStorage.setItem('user', JSON.stringify(data.user));
    window.location.href = '/index.html';
});

registerForm.addEventListener('submit', async event => {
    event.preventDefault();

    registerMensagem.textContent = '';

    const response = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            nome: document.getElementById('registerNome').value,
            email: document.getElementById('registerEmail').value,
            senha: document.getElementById('registerSenha').value
        })
    });

    const data = await response.json().catch(() => ({}));
    if (!response.ok) {
        registerMensagem.textContent = data.message || 'Nao foi possivel criar a conta.';
        registerMensagem.className = 'form-message error';
        return;
    }

    registerMensagem.textContent = 'Conta criada com sucesso. Agora faca login.';
    registerMensagem.className = 'form-message success';
    registerForm.reset();
    setTimeout(() => alternarAba('login'), 1200);
});

document.addEventListener('DOMContentLoaded', () => {
    const token = window.getToken();
    const user = window.getCurrentUser();
    if (token && user) {
        window.location.href = '/index.html';
    }
});
