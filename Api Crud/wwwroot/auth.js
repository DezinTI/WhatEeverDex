window.currentUser = null;

window.getToken = function getToken() {
    return localStorage.getItem('token');
};

window.getCurrentUser = function getCurrentUser() {
    const user = localStorage.getItem('user');
    if (!user) return null;

    try {
        return JSON.parse(user);
    } catch {
        return null;
    }
};

window.logout = function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/login.html';
};

window.authFetch = async function authFetch(url, options = {}) {
    const token = window.getToken();
    const headers = new Headers(options.headers || {});

    if (token) {
        headers.set('Authorization', `Bearer ${token}`);
    }

    const response = await fetch(url, {
        ...options,
        headers
    });

    if (response.status === 401) {
        window.logout();
    }

    return response;
};

window.requireAuth = async function requireAuth(role = null) {
    const token = window.getToken();
    const user = window.getCurrentUser();

    if (!token || !user) {
        window.logout();
        return;
    }

    window.currentUser = user;

    const response = await window.authFetch('/api/auth/me');
    if (!response.ok) {
        return;
    }

    const data = await response.json();
    window.currentUser = data;
    localStorage.setItem('user', JSON.stringify(data));

    if (role && data.role !== role) {
        window.location.href = '/index.html';
    }
};
