// ===== NPL Girişleri - filtre alanı =====
// Bölge / Şube / Dönem / Tahsis Kolu / Ürün dropdown'ları, "Filtreler" paneli ve breadcrumb.
// Seçili değerler NplReport.getFilters() ile okunur; her değişimde NplReport.reload() (index.js) çağrılır.
(function () {
    var NPL = window.NplReport = window.NplReport || {};

    // ===== Statik filtreler (Dönem / Ürün) =====
    // Servis bağlanınca seçenekler buradan değil ilgili endpoint'ten gelecek; seçim akışı aynı kalır.
    var STATIC_FILTERS = {
        period:  { placeholder: 'Dönem', label: '#nplPeriodLabel',  list: '#nplPeriodList',  panel: '#nplPeriodPanel',  items: ['2026', '2025', '2024'] },
        product: { placeholder: 'Ürün',  label: '#nplProductLabel', list: '#nplProductList', panel: '#nplProductPanel', items: ['İhtiyaç', 'KMH', 'KK', 'ÜK', 'Konut', 'Taşıt', 'Ticari', 'Diğer'] }
    };
    var _filters = { period: '', product: '' };   // boş = Tümü

    // ===== "Filtreler" paneli =====
    // Eski ekranlardaki 0/1 kolonlarının karşılığı; her grup tek seçim, boş değer = Tümü.
    // Tahsis Kolu yalnız bu modalda (üstteki dropdown kaldırıldı).
    var FILTER_GROUPS = [
        { key: 'allocation',           label: 'Tahsis Kolu',          options: ['Bireysel', 'İşletme', 'Tarım', 'Tanımsız'] },
        { key: 'authorityCode',        label: 'Yetki Kodu',           options: ['BY', 'Diğer', 'GM', 'SY'] },
        { key: 'bonusBusiness',        label: 'Bonus Business',       options: ['Hayır', 'Evet'] },
        { key: 'retailMicro',          label: 'Bireysel Mikro',       options: ['Hayır', 'Evet'] },
        { key: 'irs',                  label: 'IRS',                  options: ['Hayır', 'Evet'] },
        { key: 'restructuredCustomer', label: 'Yapılandırma Müşteri', options: ['Hayır', 'Evet'] },
        { key: 'restructuredLoan',     label: 'Yapılandırma Kredi',   options: ['Yapılandırma', 'Modifikasyon', 'Yok'] },
        { key: 'commercialConsumer',   label: 'İhtiyaç Ticari',       options: ['Hayır', 'Evet'] },
        { key: 'kgfLoan',              label: "KGF'li Kredi",         options: ['Hayır', 'Evet'] },
        { key: 'retired',              label: 'Emekli',               options: ['Hayır', 'Evet'] },
        { key: 'salaryCustomer',       label: 'Maaş Müşterisi',       options: ['Hayır', 'Evet'] }
    ];

    function emptyGroupState() {
        var s = {};
        FILTER_GROUPS.forEach(function (g) { s[g.key] = ''; });
        return s;
    }

    var _groupFilters = emptyGroupState();   // uygulanmış seçim
    var _groupDraft = emptyGroupState();     // panel açıkken üzerinde oynanan kopya

    // Seçili tüm filtreler; index.js servis isteğini bununla kurar.
    NPL.getFilters = function () {
        // allocation (Tahsis Kolu) artık modal grubudur; _groupFilters içinden gelir.
        return $.extend({
            region: $('#nplRegionLabel').text(),
            branch: $('#nplBranchLabel').text(),
            period: _filters.period,
            product: _filters.product
        }, _groupFilters);
    };

    function reload() {
        if (typeof NPL.reload === 'function') NPL.reload();
    }

    $(function () {
        if (!document.getElementById('nplChart')) return;

        // ===== Breadcrumb (Tüm Bölgeler / Bölge / Şube) =====
        function updateBreadcrumb() {
            var region = $('#nplRegionLabel').text();
            var branch = $('#nplBranchLabel').text();
            var items = ['<span class="breadcrumb-item' + ((region === 'Bölge' || !region) ? ' active' : '') + '">Tüm Bölgeler</span>'];
            if (region && region !== 'Bölge') {
                items.push('<span class="breadcrumb-separator">/</span>');
                items.push('<span class="breadcrumb-item' + ((branch === 'Şube' || !branch) ? ' active' : '') + '">' + region + '</span>');
            }
            if (branch && branch !== 'Şube') {
                items.push('<span class="breadcrumb-separator">/</span>');
                items.push('<span class="breadcrumb-item active">' + branch + '</span>');
            }
            $('#nplBreadcrumb').html(items.join(''));
        }

        // ===== Bölge / Şube filtreleri (paylaşılan veri) =====
        if (typeof loadRegionFilters === 'function') {
            loadRegionFilters(function () { renderRegionList('#nplRegionList', null); updateBreadcrumb(); });
        }
        if (typeof loadBranchFilters === 'function') {
            loadBranchFilters(function () { renderBranchList('#nplBranchList', null, null); });
        }
        $(document).on('click', '#nplRegionList .dropdown-item', function () {
            var code = $(this).data('code') || '';
            $('#nplRegionLabel').text($(this).text());
            $('#nplRegionPanel').removeClass('open');
            // Bölge değişince şube listesini filtrele ve şube seçimini sıfırla
            $('#nplBranchLabel').text('Şube');
            if (typeof renderBranchList === 'function') renderBranchList('#nplBranchList', null, code || null);
            updateBreadcrumb();
            reload();
        });
        $(document).on('click', '#nplBranchList .dropdown-item', function () {
            $('#nplBranchLabel').text($(this).text());
            $('#nplBranchPanel').removeClass('open');
            updateBreadcrumb();
            reload();
        });

        // ===== Dönem / Ürün =====
        // Bölge/şube listeleriyle aynı düzen: başta "Tümü", seçili olan .selected
        function renderStaticFilter(key) {
            var f = STATIC_FILTERS[key];
            var selected = _filters[key];
            var html = '<div class="dropdown-item tumu-item' + (!selected ? ' selected' : '') + '" data-value="">Tümü</div>';
            f.items.forEach(function (v) {
                html += '<div class="dropdown-item' + (selected === v ? ' selected' : '') + '" data-value="' + v + '">' + v + '</div>';
            });
            $(f.list).html(html);
            $(f.label).text(selected || f.placeholder);
        }

        $(document).on('click', '.dropdown-list[data-filter] .dropdown-item', function () {
            var key = $(this).closest('.dropdown-list').data('filter');
            _filters[key] = $(this).data('value') || '';
            renderStaticFilter(key);
            $(STATIC_FILTERS[key].panel).removeClass('open');
            reload();
        });

        // ===== "Filtreler" paneli =====
        // Segment kutuları paylaşılan .segmented-control bileşeniyle çizilir
        function renderFilterGroups() {
            var html = '';
            FILTER_GROUPS.forEach(function (g) {
                var selected = _groupDraft[g.key];
                html += '<div class="npl-filter-group">' +
                            '<span class="npl-filter-group-label">' + g.label + '</span>' +
                            '<div class="segmented-control" data-group="' + g.key + '">' +
                                '<button type="button" class="segment' + (!selected ? ' active' : '') + '" data-value="">Tümü</button>';
                g.options.forEach(function (o) {
                    html += '<div class="divider"></div>' +
                            '<button type="button" class="segment' + (selected === o ? ' active' : '') + '" data-value="' + o + '">' + o + '</button>';
                });
                html += '</div></div>';
            });
            $('#nplFilterGroups').html(html);

            var count = FILTER_GROUPS.filter(function (g) { return _groupDraft[g.key]; }).length;
            $('#nplFiltersTitle').text(count ? ('Filtreler (' + count + ')') : 'Filtreler');
        }

        function openFiltersPanel() {
            _groupDraft = $.extend({}, _groupFilters);   // iptal edilirse uygulanmış hâle dönülür
            renderFilterGroups();
            $('.dropdown-panel').removeClass('open');    // açık bir filtre dropdown'ı varsa kapansın
            // Panel + karartma birlikte açılır (mobil/tablet'te bottom-sheet + overlay)
            $('#nplFiltersPanel, #nplFiltersOverlay').addClass('open');
        }

        function closeFiltersPanel() {
            $('#nplFiltersPanel, #nplFiltersOverlay').removeClass('open');
        }

        $('#nplFiltersBtn').on('click', function (e) {
            e.stopPropagation();
            if ($('#nplFiltersPanel').hasClass('open')) closeFiltersPanel(); else openFiltersPanel();
        });
        $('#nplFiltersClose').on('click', closeFiltersPanel);
        // Karartmaya tıklanınca kapat (bottom-sheet dışına dokunma)
        $('#nplFiltersOverlay').on('click', closeFiltersPanel);

        // Panelin içine tıklayınca kapanmasın; dışına tıklayınca / Esc ile kapansın
        $('#nplFiltersPanel').on('click', function (e) { e.stopPropagation(); });
        $(document).on('click', function () { closeFiltersPanel(); });
        $(document).on('keydown', function (e) {
            if (e.key === 'Escape') closeFiltersPanel();
        });

        $('#nplFilterGroups').on('click', '.segment', function () {
            var $seg = $(this);
            _groupDraft[$seg.closest('.segmented-control').data('group')] = $seg.data('value') || '';
            renderFilterGroups();
        });

        $('#nplFiltersClear').on('click', function () {
            _groupDraft = emptyGroupState();
            renderFilterGroups();
        });

        $('#nplFiltersApply').on('click', function () {
            _groupFilters = $.extend({}, _groupDraft);
            closeFiltersPanel();
            reload();
        });

        // ===== İlk render =====
        Object.keys(STATIC_FILTERS).forEach(renderStaticFilter);
        updateBreadcrumb();
    });
})();
