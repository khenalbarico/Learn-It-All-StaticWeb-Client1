self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => {
    // Only intercept what this worker could actually serve. Presigned R2 book URLs are
    // cross-origin and can never be cached here, so routing them through respondWith()
    // puts the worker in the path of a request it cannot help with — and turns a plain
    // network or CORS rejection into a second, unhandled rejection inside the worker.
    // Declining to respond hands the request straight back to the browser.
    if (!shouldHandle(event.request)) return;

    event.respondWith(onFetch(event));
});

function shouldHandle(request) {
    return request.method === 'GET' && new URL(request.url).origin === self.origin;
}

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/, /\.webmanifest$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/, /^staticwebapp\.config\.json$/ ];

const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    // Take over as soon as install finishes, instead of waiting for every
    // already-open tab to close - without this, a tab left open across a
    // deployment keeps running the previous service worker indefinitely,
    // which can end up fetching against assets that no longer match what's live.
    self.skipWaiting();

    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));

    // Immediately control already-open tabs rather than only the next navigation.
    await self.clients.claim();
}

// Reached only for same-origin GETs; shouldHandle() has already filtered the rest.
async function onFetch(event) {
    const shouldServeIndexHtml = event.request.mode === 'navigate'
        && !manifestUrlList.some(url => url === event.request.url);

    const request = shouldServeIndexHtml ? 'index.html' : event.request;
    const cache = await caches.open(cacheName);
    const cachedResponse = await cache.match(request);

    return cachedResponse || fetch(event.request);
}
