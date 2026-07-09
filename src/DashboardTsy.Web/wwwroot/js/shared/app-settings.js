// AppSettings client helper.
// Feature-flag'leri /app-settings endpoint'inden bir kez yükler, key -> value map olarak cache'ler.
// Cache sessionStorage'da tutulur: siteye ilk girişte bir kez istek atılır, sekme kapanana kadar tekrar çağrılmaz. Yeni bir sayfada ihtiyaç olursa da aynı cache kullanılır, ek istek atılmaz.
// Kullanım:
//   AppSettings.isVisible('ProductivityReport.AiInsight.Visible').then(function (visible) { ... });
//   AppSettings.get('SomeKey').then(function (val) { ... });
(function () {
    var STORAGE_KEY = '_appSettings';
    var settingsPromise = null;

    function loadOnce() {
        if (!settingsPromise) {
            var cached = sessionStorage.getItem(STORAGE_KEY);
            if (cached) {
                try {
                    return (settingsPromise = Promise.resolve(JSON.parse(cached)));
                } catch (e) {
                    sessionStorage.removeItem(STORAGE_KEY);
                }
            }
            settingsPromise = fetch('/app-settings', { credentials: 'include' })
                .then(function (r) { return r.ok ? r.json() : Promise.reject(r.status); })
                .then(function (list) {
                    var map = {};
                    list.forEach(function (item) { map[item.key] = item.value; });
                    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(map));
                    return map;
                })
                .catch(function () { return {}; });
        }
        return settingsPromise;
    }

    window.AppSettings = {
        getAll: function () { return loadOnce(); },
        get: function (key) { return loadOnce().then(function (m) { return m[key]; }); },
        isVisible: function (key) { return loadOnce().then(function (m) { return m[key] === true; }); }
    };
})();
