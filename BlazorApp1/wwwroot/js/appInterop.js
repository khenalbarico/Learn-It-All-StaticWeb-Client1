export function getItem(key) {
    return localStorage.getItem(key);
}

export function setItem(key, value) {
    localStorage.setItem(key, value);
}

export function removeItem(key) {
    localStorage.removeItem(key);
}

export function setThemeAttribute(theme) {
    document.documentElement.setAttribute('data-theme', theme);
}

export function prefersDarkColorScheme() {
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
}

export function openInNewTab(url) {
    window.open(url, '_blank', 'noopener');
}
