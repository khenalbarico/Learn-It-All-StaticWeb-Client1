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

function resizeAllPdfViewers() {
    for (const containerId of pdfViewers.keys()) {
        renderCurrentPage(containerId);
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

async function renderCurrentPage(containerId) {
    const state = pdfViewers.get(containerId);
    if (!state || state.rendering) return;

    // Blazor's StateHasChanged patches the DOM synchronously, but the browser
    // hasn't necessarily laid it out yet, so the container can briefly report
    // 0 width right after it becomes visible - wait a few frames for real layout.
    const container = await waitForContainerWidth(containerId, 10);
    if (!container || container.clientWidth === 0) return;

    state.rendering = true;
    try {
        const page = await state.doc.getPage(state.pageNum);
        const unscaledWidth = page.getViewport({ scale: 1 }).width;
        const scale = container.clientWidth / unscaledWidth;
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
    } finally {
        state.rendering = false;
    }
}

export async function loadPdf(containerId, bytes) {
    const pdfjsLib = await getPdfjsLib();

    const existing = pdfViewers.get(containerId);
    if (existing) {
        existing.doc.destroy();
        pdfViewers.delete(containerId);
    }

    const container = document.getElementById(containerId);
    if (container) container.innerHTML = '';

    const loadingTask = pdfjsLib.getDocument({ data: bytes });
    const doc = await loadingTask.promise;

    pdfViewers.set(containerId, { doc, pageNum: 1, rendering: false });
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

export function disposePdf(containerId) {
    const state = pdfViewers.get(containerId);
    if (!state) return;

    state.doc.destroy();
    pdfViewers.delete(containerId);
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
