// NPL Girişleri - filtre alanı (Bölge/Şube/Dönem/Ürün dropdown'ları, Filtreler paneli, breadcrumb)
(function () {
    var NPL = window.NplReport = window.NplReport || {};

    var STATIC_FILTERS = {
        period:  { placeholder: 'Dönem', label: '#nplPeriodLabel',  list: '#nplPeriodList',  panel: '#nplPeriodPanel',  items: ['2026', '2025', '2024'] },
        product: { placeholder: 'Ürün',  label: '#nplProductLabel', list: '#nplProductList', panel: '#nplProductPanel', items: ['İhtiyaç', 'KMH', 'KK', 'ÜK', 'Konut', 'Taşıt', 'Ticari', 'Diğer'] }
    };
    var _filters = { period: '', product: '' };   // boş = Tümü

    // Filtreler paneli grupları (her grup tek seçim, boş = Tümü)
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

    var _selectedRegion = null;   // { code, name }
    var _selectedBranch = null;   // { code, name }

    // Seçili tüm filtreler; index.js servis isteğini bununla kurar.
    NPL.getFilters = function () {
        return $.extend({
            region: _selectedRegion ? _selectedRegion.name : '',
            regionCode: _selectedRegion ? _selectedRegion.code : null,
            branch: _selectedBranch ? _selectedBranch.name : '',
            branchCode: _selectedBranch ? _selectedBranch.code : null,
            period: _filters.period,
            product: _filters.product
        }, _groupFilters);
    };

    function reload() {
        if (typeof NPL.reload === 'function') NPL.reload();
    }

    $(function () {
        if (!document.getElementById('nplChart')) return;

        // ===== Bölge / Şube listeleri (paylaşılan veri) =====
        function renderRegionDropdown() {
            return renderRegionList('#nplRegionList', _selectedRegion ? _selectedRegion.code : null);
        }

        function renderBranchDropdown() {
            return renderBranchList('#nplBranchList', _selectedBranch ? _selectedBranch.code : null, _selectedRegion ? _selectedRegion.code : null);
        }

        // ===== Breadcrumb (Verim Raporları ile aynı) =====
        function crumb(text, action, disabled) {
            var cls = disabled ? ' disabled' : '';
            var attr = (!disabled && action) ? ' data-npl-breadcrumb="' + action + '"' : '';
            return '<span class="breadcrumb-item' + cls + '"' + attr + '>' + text + '</span>';
        }

        function updateBreadcrumb() {
            if (!_selectedRegion && !_selectedBranch) {
                $('#nplBreadcrumbBar').hide();
                return;
            }
            $('#nplBreadcrumbBar').show();

            // Tek bölge dönen kullanıcıda (şube/bölge müdürü) üst kademeye dönüş anlamsızdır
            var singleRegion = (typeof _regionFilters !== 'undefined') && _regionFilters.length === 1;
            var parts = [crumb('Tüm Bölgeler', 'allRegions', singleRegion)];

            if (_selectedRegion) {
                parts.push(crumb(_selectedRegion.name, 'region', singleRegion || !_selectedBranch));
            }
            if (_selectedBranch) {
                parts.push(crumb(_selectedBranch.name, null, true));
            }
            $('#nplBreadcrumb').html(parts.join('<span class="breadcrumb-separator">/</span>'));
        }

        $(document).on('click', '[data-npl-breadcrumb="allRegions"]', function () {
            _selectedRegion = null;
            _selectedBranch = null;
            $('#nplRegionLabel').text('Bölge');
            $('#nplBranchLabel').text('Şube');
            renderRegionDropdown();
            renderBranchDropdown();
            updateBreadcrumb();
            reload();
        });
        $(document).on('click', '[data-npl-breadcrumb="region"]', function () {
            _selectedBranch = null;
            $('#nplBranchLabel').text('Şube');
            renderBranchDropdown();
            updateBreadcrumb();
            reload();
        });

        // ===== İlk yükleme: tek bölge/şube dönerse otomatik seçili kabul edilir =====
        if (typeof loadRegionFilters === 'function') {
            loadRegionFilters(function () {
                var single = renderRegionDropdown();
                if (single) _selectedRegion = { code: single.Code, name: single.Name };

                if (typeof loadBranchFilters !== 'function') { updateBreadcrumb(); return; }
                loadBranchFilters(function () {
                    var singleBranch = renderBranchDropdown();
                    if (singleBranch) _selectedBranch = { code: singleBranch.Code, name: singleBranch.Name };
                    updateBreadcrumb();
                    if (_selectedRegion || _selectedBranch) reload();
                });
            });
        }

        // ===== Bölge seçimi =====
        $(document).on('click', '#nplRegionList .dropdown-item', function () {
            var code = $(this).attr('data-code') || '';
            var name = $(this).text();

            _selectedRegion = code ? { code: code, name: name } : null;
            $('#nplRegionLabel').text(code ? name : 'Bölge');

            // Bölge değişince şube seçimi sıfırlanır, liste yeni bölgeye göre filtrelenir
            _selectedBranch = null;
            $('#nplBranchLabel').text('Şube');

            renderRegionDropdown();
            renderBranchDropdown();
            $('#nplRegionPanel').removeClass('open');
            $('#nplRegionSearch').val('');

            updateBreadcrumb();
            reload();
        });

        // ===== Şube seçimi =====
        $(document).on('click', '#nplBranchList .dropdown-item', function () {
            var code = $(this).attr('data-code') || '';
            var name = $(this).text();

            if (!code) {
                _selectedBranch = null;
                $('#nplBranchLabel').text('Şube');
            } else {
                _selectedBranch = { code: code, name: name };
                $('#nplBranchLabel').text(name);

                // Şube doğrudan seçilse de ait olduğu bölge otomatik seçilir
                var region = (typeof findRegion === 'function') ? findRegion($(this).attr('data-region')) : null;
                if (region) {
                    _selectedRegion = { code: region.Code, name: region.Name };
                    $('#nplRegionLabel').text(region.Name);
                }
            }

            renderRegionDropdown();
            renderBranchDropdown();
            $('#nplBranchPanel').removeClass('open');
            $('#nplBranchSearch').val('');

            updateBreadcrumb();
            reload();
        });

        // ===== Dönem / Ürün =====
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
            $('.dropdown-panel').removeClass('open');
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
            var hadApplied = FILTER_GROUPS.some(function (g) { return _groupFilters[g.key]; });
            _groupDraft = emptyGroupState();
            _groupFilters = emptyGroupState();
            renderFilterGroups();
            if (hadApplied) reload();
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
