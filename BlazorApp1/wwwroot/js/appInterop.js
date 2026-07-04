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

export function createPdfBlobUrl(bytes) {
    const blob = new Blob([bytes], { type: 'application/pdf' });
    return URL.createObjectURL(blob);
}

export function revokeBlobUrl(url) {
    URL.revokeObjectURL(url);
}

export function pushAdsbygoogle() {
    // Blazor has just inserted the <ins> into the DOM, but under load (e.g. during
    // WASM boot) the browser may not have finished a layout pass yet, so its width
    // can still read as 0. Pushing before that settles makes AdSense error out and
    // permanently mark the slot as dead, so wait until the element actually has
    // width, retrying across a few animation frames before giving up and pushing anyway.
    function tryPush(attemptsLeft) {
        requestAnimationFrame(() => {
            const pending = document.querySelector('ins.adsbygoogle:not([data-ad-status])');
            if (pending && pending.offsetWidth === 0 && attemptsLeft > 0) {
                tryPush(attemptsLeft - 1);
                return;
            }
            try {
                (window.adsbygoogle = window.adsbygoogle || []).push({});
            } catch (e) {
                console.warn('adsbygoogle push failed', e);
            }
        });
    }
    tryPush(10);
}
