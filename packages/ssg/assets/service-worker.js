self.addEventListener('install', (e) => {
    e.waitUntil(
        caches.open('v1').then((cache) => cache.addAll([
            '/index.html',
            '/style.css',
            '/icon.svg',
            '/1-intro.html',
            '/2-setup-env.html',
            '/3-use-shell.html',
            '/10-gui-app.html',
        ]))
    );
});
