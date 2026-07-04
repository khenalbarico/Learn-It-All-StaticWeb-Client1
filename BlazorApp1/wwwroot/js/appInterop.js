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

const PDFJS_VERSION = '5.4.149';
const PDFJS_BASE = `https://cdnjs.cloudflare.com/ajax/libs/pdf.js/${PDFJS_VERSION}`;

let pdfjsLibPromise = null;
function getPdfjsLib() {
    // Native browser PDF plugins render at their own idea of "100% zoom", which
    // ignores the iframe's actual width and looks zoomed-in on narrow mobile
    // screens. Rendering pages ourselves onto a canvas sized to the container's
    // width guarantees the page always fits, on any device.
    pdfjsLibPromise ??= import(`${PDFJS_BASE}/pdf.min.mjs`).then(lib => {
        lib.GlobalWorkerOptions.workerSrc = `${PDFJS_BASE}/pdf.worker.min.mjs`;
        return lib;
    });
    return pdfjsLibPromise;
}

const pdfViewers = new Map();
const pinchHandlers = new Map();
const MIN_ZOOM = 0.5;
const MAX_ZOOM = 4;
const ZOOM_STEP = 0.25;

function resizeAllPdfViewers() {
    for (const containerId of pdfViewers.keys()) {
        renderCurrentPage(containerId, true);
    }
}

let resizeTimer = null;
window.addEventListener('resize', () => {
    clearTimeout(resizeTimer);
    resizeTimer = setTimeout(resizeAllPdfViewers, 200);
});

function waitForContainerWidth(containerId, attemptsLeft) {
    return new Promise(resolve => {
        function check() {
            const container = document.getElementById(containerId);
            if ((container && container.clientWidth > 0) || attemptsLeft <= 0) {
                resolve(container);
                return;
            }
            attemptsLeft--;
            requestAnimationFrame(check);
        }
        check();
    });
}

async function renderCurrentPage(containerId, preserveScroll = false) {
    const state = pdfViewers.get(containerId);
    if (!state || state.rendering) return;

    // Blazor's StateHasChanged patches the DOM synchronously, but the browser
    // hasn't necessarily laid it out yet, so the container can briefly report
    // 0 width right after it becomes visible - wait a few frames for real layout.
    const container = await waitForContainerWidth(containerId, 10);
    if (!container || container.clientWidth === 0) return;

    state.rendering = true;
    try {
        const prevLeftRatio = preserveScroll ? container.scrollLeft / (container.scrollWidth || 1) : 0;
        const prevTopRatio = preserveScroll ? container.scrollTop / (container.scrollHeight || 1) : 0;

        const page = await state.doc.getPage(state.pageNum);
        const unscaledWidth = page.getViewport({ scale: 1 }).width;
        const baseScale = container.clientWidth / unscaledWidth;
        const scale = baseScale * state.zoom;
        const dpr = window.devicePixelRatio || 1;
        const viewport = page.getViewport({ scale: scale * dpr });

        let canvas = container.querySelector('canvas');
        if (!canvas) {
            canvas = document.createElement('canvas');
            container.appendChild(canvas);
        }
        canvas.width = viewport.width;
        canvas.height = viewport.height;
        canvas.style.width = `${viewport.width / dpr}px`;
        canvas.style.height = `${viewport.height / dpr}px`;

        const ctx = canvas.getContext('2d');
        await page.render({ canvasContext: ctx, viewport }).promise;

        if (preserveScroll) {
            container.scrollLeft = prevLeftRatio * container.scrollWidth;
            container.scrollTop = prevTopRatio * container.scrollHeight;
        }
    } finally {
        state.rendering = false;
    }
}

function attachPinchZoom(containerId, container) {
    let pinching = false;
    let pinchStartDistance = 0;
    let pinchStartZoom = 1;
    let lastRatio = 1;

    function distance(touches) {
        const dx = touches[0].clientX - touches[1].clientX;
        const dy = touches[0].clientY - touches[1].clientY;
        return Math.sqrt(dx * dx + dy * dy);
    }

    function onTouchStart(e) {
        if (e.touches.length !== 2) return;
        const state = pdfViewers.get(containerId);
        if (!state) return;

        pinching = true;
        pinchStartDistance = distance(e.touches);
        pinchStartZoom = state.zoom;
        lastRatio = 1;
        e.preventDefault();
    }

    function onTouchMove(e) {
        if (!pinching || e.touches.length !== 2) return;
        e.preventDefault();

        lastRatio = distance(e.touches) / pinchStartDistance;
        const canvas = container.querySelector('canvas');
        if (canvas) canvas.style.transform = `scale(${lastRatio})`;
    }

    async function onTouchEnd(e) {
        if (!pinching || e.touches.length >= 2) return;
        pinching = false;

        const canvas = container.querySelector('canvas');
        if (canvas) canvas.style.transform = '';

        const state = pdfViewers.get(containerId);
        if (!state) return;

        state.zoom = Math.max(MIN_ZOOM, Math.min(MAX_ZOOM, pinchStartZoom * lastRatio));
        lastRatio = 1;
        await renderCurrentPage(containerId, true);
    }

    container.addEventListener('touchstart', onTouchStart, { passive: false });
    container.addEventListener('touchmove', onTouchMove, { passive: false });
    container.addEventListener('touchend', onTouchEnd);
    container.addEventListener('touchcancel', onTouchEnd);

    return () => {
        container.removeEventListener('touchstart', onTouchStart);
        container.removeEventListener('touchmove', onTouchMove);
        container.removeEventListener('touchend', onTouchEnd);
        container.removeEventListener('touchcancel', onTouchEnd);
    };
}

export async function loadPdf(containerId, bytes) {
    const pdfjsLib = await getPdfjsLib();

    const existing = pdfViewers.get(containerId);
    if (existing) {
        existing.doc.destroy();
        pdfViewers.delete(containerId);
    }

    const container = document.getElementById(containerId);
    if (container) {
        container.innerHTML = '';
        if (!pinchHandlers.has(containerId)) {
            pinchHandlers.set(containerId, attachPinchZoom(containerId, container));
        }
    }

    const loadingTask = pdfjsLib.getDocument({ data: bytes });
    const doc = await loadingTask.promise;

    pdfViewers.set(containerId, { doc, pageNum: 1, zoom: 1, rendering: false });
    await renderCurrentPage(containerId);

    return doc.numPages;
}

export async function goToPdfPage(containerId, pageNum) {
    const state = pdfViewers.get(containerId);
    if (!state) return 1;

    state.pageNum = Math.max(1, Math.min(pageNum, state.doc.numPages));
    await renderCurrentPage(containerId);
    return state.pageNum;
}

export async function zoomPdf(containerId, delta) {
    const state = pdfViewers.get(containerId);
    if (!state) return 1;

    state.zoom = Math.max(MIN_ZOOM, Math.min(MAX_ZOOM, +(state.zoom + delta * ZOOM_STEP).toFixed(2)));
    await renderCurrentPage(containerId, true);
    return state.zoom;
}

export function disposePdf(containerId) {
    const state = pdfViewers.get(containerId);
    if (state) {
        state.doc.destroy();
        pdfViewers.delete(containerId);
    }

    const detach = pinchHandlers.get(containerId);
    if (detach) {
        detach();
        pinchHandlers.delete(containerId);
    }
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
