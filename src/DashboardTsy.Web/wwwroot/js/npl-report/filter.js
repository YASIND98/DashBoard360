(function () {
    var NPL = window.NplReport = window.NplReport || {};

    var STATIC_FILTERS = {
        period:  { placeholder: 'Dönem', label: '#nplPeriodLabel',  list: '#nplPeriodList',  panel: '#nplPeriodPanel',  items: [{ value: '2026', text: '2026' }, { value: '2025', text: '2025' }] },
        product: { placeholder: 'Ürün',  label: '#nplProductLabel', list: '#nplProductList', panel: '#nplProductPanel', items: [] }
    };
    var _filters = { period: '', product: '' };

    function itemText(key, value) {
        var items = STATIC_FILTERS[key].items;
        for (var i = 0; i < items.length; i++) {
            if (items[i].value === value) return items[i].text;
        }
        return '';
    }

    // FilterCode'lar SP parametre adlarıyla birebir aynı; istek de bu adlarla kuruluyor.
    var PRODUCT_FILTER_CODE = 'URUN';
    var BUSINESS_FILTER_CODE = 'ISKOLU';

    // Servis "Tümü"yü ItemCode = null döner; attribute null tutamadığı için '' e normalize edilir.
    function optionCode(itemCode) {
        return (itemCode == null) ? '' : itemCode;
    }

    function findOption(group, code) {
        if (!group) return null;
        for (var i = 0; i < group.options.length; i++) {
            if (group.options[i].code === (code || '')) return group.options[i];
        }
        return null;
    }

    var _filterGroups = [];
    var _productGroup = null;
    var _businessGroup = null;
    var _businessOptions = [];
    var _businessFilter = '';

    function buildFilterGroups(rows) {
        var byId = {};
        var groups = [];

        (rows || []).forEach(function (r) {
            var g = byId[r.FilterGroupId];
            if (!g) {
                g = byId[r.FilterGroupId] = {
                    id: r.FilterGroupId,
                    code: r.FilterCode,
                    name: r.FilterName,
                    order: r.DisplayOrder,
                    options: []
                };
                groups.push(g);
            }

            // code: DOM/state değeri, itemCode: servise giden ham değer (null olabilir)
            g.options.push({
                id: r.FilterItemId,
                code: optionCode(r.ItemCode),
                itemCode: (r.ItemCode == null) ? null : r.ItemCode,
                name: r.ItemName,
                order: r.ItemOrder
            });
        });

        groups.sort(function (a, b) { return (a.order - b.order) || (a.id - b.id); });
        groups.forEach(function (g) {
            g.options.sort(function (a, b) { return (a.order - b.order) || (a.id - b.id); });
        });
        return groups;
    }

    function loadNplFilters(callback) {
        $.ajax({
            url: '/NplReport/GetNplFilters',
            type: 'POST',
            contentType: 'application/json',
            data: '{}',
            success: function (data) {
                var groups = buildFilterGroups(data);

                // Dropdown "Tümü"yü kendisi basıyor; servisten de gelirse tekrarlamasın.
                _productGroup = groups.filter(function (g) { return g.code === PRODUCT_FILTER_CODE; })[0] || null;
                STATIC_FILTERS.product.items = _productGroup ? _productGroup.options.filter(function (o) {
                    return !!o.code;
                }).map(function (o) {
                    return { value: o.code, text: o.name };
                }) : [];

                _businessGroup = groups.filter(function (g) { return g.code === BUSINESS_FILTER_CODE; })[0] || null;
                _businessOptions = _businessGroup ? _businessGroup.options : [];

                _filterGroups = groups.filter(function (g) {
                    return g.code !== PRODUCT_FILTER_CODE && g.code !== BUSINESS_FILTER_CODE;
                });
                if (callback) callback();
            },
            error: function () { if (callback) callback(); }
        });
    }

    function findGroup(code) {
        for (var i = 0; i < _filterGroups.length; i++) {
            if (_filterGroups[i].code === code) return _filterGroups[i];
        }
        return null;
    }

    // Boş değer = "Tümü" ya da seçimsiz; ikisi de filtre uygulamaz.
    function emptyGroupState() {
        var s = {};
        _filterGroups.forEach(function (g) { s[g.code] = ''; });
        return s;
    }

    function cloneGroupState(state) {
        var s = {};
        _filterGroups.forEach(function (g) { s[g.code] = state[g.code] || ''; });
        return s;
    }

    function hasGroupSelection(state, g) {
        return !!state[g.code];
    }

    function groupSelectionValues(state) {
        var out = {};
        _filterGroups.forEach(function (g) { out[g.code] = state[g.code] || ''; });
        return out;
    }

    var _groupFilters = {}; 
    var _groupDraft = {}; 

    var _selectedRegion = null;
    var _selectedBranch = null;

    // Seçili tüm filtreler; index.js servis isteğini bununla kurar.
    NPL.getFilters = function () {
        return $.extend({
            region: _selectedRegion ? _selectedRegion.name : '',
            regionCode: _selectedRegion ? _selectedRegion.code : null,
            branch: _selectedBranch ? _selectedBranch.name : '',
            branchCode: _selectedBranch ? _selectedBranch.code : null,
            period: _filters.period,
            product: itemText('product', _filters.product),
            productCode: _filters.product || null
        }, groupSelectionValues(_groupFilters), { ISKOLU: _businessFilter });   // iş kolu sekme barından
    };
    
    NPL.getFilterSelections = function () {
        var selections = {};

        function add(code, group, selected) {
            var option = findOption(group, selected);
            if (option) selections[code] = option.itemCode;
        }

        add(PRODUCT_FILTER_CODE, _productGroup, _filters.product);
        add(BUSINESS_FILTER_CODE, _businessGroup, _businessFilter);
        _filterGroups.forEach(function (g) { add(g.code, g, _groupFilters[g.code]); });

        return selections;
    };

    var _filtersReady = false;
    var _pendingReload = false;

    function reload() {
        if (!_filtersReady) { _pendingReload = true; return; }
        if (typeof NPL.reload === 'function') NPL.reload();
    }

    function markFiltersReady() {
        if (_filtersReady) return;
        _filtersReady = true;
        if (_pendingReload) { _pendingReload = false; reload(); }
    }

    NPL.requestReload = reload;

    $(function () {
        if (!document.getElementById('nplChart')) return;

        function persistSelection() {
            saveFilterSelection(_selectedRegion, _selectedBranch);
        }

        function renderRegionDropdown() {
            return renderRegionList('#nplRegionList', _selectedRegion ? _selectedRegion.code : null);
        }

        function renderBranchDropdown() {
            return renderBranchList('#nplBranchList', _selectedBranch ? _selectedBranch.code : null, _selectedRegion ? _selectedRegion.code : null);
        }

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
            persistSelection();
            updateBreadcrumb();
            reload();
        });
        $(document).on('click', '[data-npl-breadcrumb="region"]', function () {
            _selectedBranch = null;
            $('#nplBranchLabel').text('Şube');
            renderBranchDropdown();
            persistSelection();
            updateBreadcrumb();
            reload();
        });

        if (typeof loadRegionFilters === 'function') {
            var savedSelection = initFilterSelection();

            loadRegionFilters(function () {
                if (savedSelection.region && findRegion(savedSelection.region.code)) {
                    _selectedRegion = savedSelection.region;
                    $('#nplRegionLabel').text(_selectedRegion.name);
                }
                var single = renderRegionDropdown();
                if (single) _selectedRegion = { code: single.Code, name: single.Name };

                if (typeof loadBranchFilters !== 'function') { updateBreadcrumb(); return; }
                loadBranchFilters(function () {
                    if (savedSelection.branch && findBranch(savedSelection.branch.code, _selectedRegion ? _selectedRegion.code : null)) {
                        _selectedBranch = savedSelection.branch;
                        $('#nplBranchLabel').text(_selectedBranch.name);
                    }
                    var singleBranch = renderBranchDropdown();
                    if (singleBranch) _selectedBranch = { code: singleBranch.Code, name: singleBranch.Name };
                    persistSelection();
                    updateBreadcrumb();
                    if (_selectedRegion || _selectedBranch) reload();
                });
            });
        }

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

            persistSelection();
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

            persistSelection();
            updateBreadcrumb();
            reload();
        });

        function renderBusinessTabs() {
            var html = '';
            _businessOptions.forEach(function (o) {
                var active = (_businessFilter === o.code) ? ' active' : '';
                html += '<button class="tab' + active + '" data-npltab="' + o.code + '">' + o.name + '</button>';
            });
            $('#nplTabList').html(html);
        }

        $(document).on('click', '#nplTabList .tab', function () {
            var code = $(this).attr('data-npltab') || '';
            if (code === _businessFilter) return;
            _businessFilter = code;
            renderBusinessTabs();
            reload();
        });

        // ===== Dönem / Ürün =====
        function renderStaticFilter(key) {
            var f = STATIC_FILTERS[key];
            var selected = _filters[key];
            var html = '<div class="dropdown-item tumu-item' + (!selected ? ' selected' : '') + '" data-value="">Tümü</div>';
            f.items.forEach(function (it) {
                html += '<div class="dropdown-item' + (selected === it.value ? ' selected' : '') + '" data-value="' + it.value + '">' + it.text + '</div>';
            });
            $(f.list).html(html);
            $(f.label).text(itemText(key, selected) || f.placeholder);
        }

        $(document).on('click', '.dropdown-list[data-filter] .dropdown-item', function () {
            var key = $(this).closest('.dropdown-list').data('filter');
            // .data() sayısal görünen değeri (2026) number'a çevirir; karşılaştırmalar string üzerinden yapılır
            _filters[key] = $(this).attr('data-value') || '';
            renderStaticFilter(key);
            $(STATIC_FILTERS[key].panel).removeClass('open');
            reload();
        });

        // ===== "Filtreler" paneli =====
        function renderFilterGroups() {
            var html = '';
            _filterGroups.forEach(function (g) {
                var selected = _groupDraft[g.code] || '';
                html += '<div class="npl-filter-group">' +
                            '<span class="npl-filter-group-label">' + g.name + '</span>' +
                            '<div class="segmented-control" data-group="' + g.code + '">';
                g.options.forEach(function (o, i) {
                    var active = (selected === o.code);
                    html += (i > 0 ? '<div class="divider"></div>' : '') +
                            '<button type="button" class="segment' + (active ? ' active' : '') + '" data-value="' + o.code + '">' + o.name + '</button>';
                });
                html += '</div></div>';
            });
            $('#nplFilterGroups').html(html);

            var count = _filterGroups.filter(function (g) { return hasGroupSelection(_groupDraft, g); }).length;
            $('#nplFiltersTitle').text(count ? ('Filtreler (' + count + ')') : 'Filtreler');
        }

        function openFiltersPanel() {
            _groupDraft = cloneGroupState(_groupFilters);   // iptal edilirse uygulanmış hâle dönülür
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

        $(document).on('click', '.filter-dropdown:not(#nplFiltersBtn)', closeFiltersPanel);
        $(document).on('keydown', function (e) {
            if (e.key === 'Escape') closeFiltersPanel();
        });

        $('#nplFilterGroups').on('click', '.segment', function () {
            var $seg = $(this);
            var code = $seg.closest('.segmented-control').attr('data-group');
            if (!findGroup(code)) return;

            _groupDraft[code] = $seg.attr('data-value') || '';
            renderFilterGroups();
        });

        $('#nplFiltersClear').on('click', function () {
            var hadApplied = _filterGroups.some(function (g) { return hasGroupSelection(_groupFilters, g); });
            _groupDraft = emptyGroupState();
            _groupFilters = emptyGroupState();
            renderFilterGroups();
            if (hadApplied) reload();
        });

        $('#nplFiltersApply').on('click', function () {
            _groupFilters = cloneGroupState(_groupDraft);
            closeFiltersPanel();
            reload();
        });

        // ===== İlk render =====
        Object.keys(STATIC_FILTERS).forEach(renderStaticFilter);
        renderBusinessTabs();
        loadNplFilters(function () {
            renderBusinessTabs();
            renderStaticFilter('product');
            _groupFilters = emptyGroupState();
            _groupDraft = emptyGroupState();
            renderFilterGroups();
            markFiltersReady();
        });
        updateBreadcrumb();
    });
})();
