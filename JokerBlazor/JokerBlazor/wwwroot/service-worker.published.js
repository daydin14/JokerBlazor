// Offline app shell for the published Blazor WebAssembly site.
// Paths are relative to this worker, so the cache stays under /JokerBlazor/.
self.importScripts("./service-worker-assets.js");
self.addEventListener("install", event => event.waitUntil(onInstall()));
self.addEventListener("activate", event => event.waitUntil(onActivate()));
self.addEventListener("fetch", event => event.respondWith(onFetch(event)));

const cacheNamePrefix = "offline-cache-";
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [/\.dll$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff2?$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.webmanifest$/];
const offlineAssetsExclude = [/^service-worker\.js$/];

async function onInstall() {
    const assets = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: "no-cache" }));
    const cache = await caches.open(cacheName);
    await cache.addAll(assets);
}

async function onActivate() {
    const keys = await caches.keys();
    await Promise.all(keys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

async function onFetch(event) {
    if (event.request.method !== "GET") {
        return fetch(event.request);
    }

    const cache = await caches.open(cacheName);
    const isNavigation = event.request.mode === "navigate"
        && !event.request.url.includes("/_framework/")
        && !event.request.url.includes("/_content/");
    const cached = await cache.match(isNavigation ? "index.html" : event.request);
    return cached || fetch(event.request);
}
