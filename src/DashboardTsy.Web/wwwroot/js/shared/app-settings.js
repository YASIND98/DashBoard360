// AppSettings client helper.
// Feature-flag'leri /app-settings endpoint'inden bir kez yükler, key -> value map olarak cache'ler.
// Kullanım:
//   AppSettings.isVisible('ProductivityReport.AiInsight.Visible').then(function (visible) { ... });
//   AppSettings.get('SomeKey').then(function (val) { ... });
(function () {
    var settingsPromise = null;

    function loadOnce() {
        if (!settingsPromise) {
            settingsPromise = fetch('/app-settings', { credentials: 'include' })
                .then(function (r) { return r.ok ? r.json() : Promise.reject(r.status); })
                .then(function (list) {
                    var map = {};
                    list.forEach(function (item) { map[item.key] = item.value; });
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
