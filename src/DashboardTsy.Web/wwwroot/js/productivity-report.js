var _selectedDate =  _todayDate;

// ===== Yield Table Skeleton + Loading =====
var _yieldLoadingActive = false;
var _yieldHasSecondTable = false;

function showYieldTableLoading() {
    _yieldLoadingActive = true;
    _yieldHasSecondTable = false;
    // Tablo container'ları gizle, skeleton göster
    $('#dynamicTableContainer').hide();
    $('#dynamicTableContainer2').hide();
    $('#dynamicTableHead').html('');
    $('#dynamicTableHead2').html('');
    $('#dynamicTableBody').html('');
    $('#dynamicTableBody2').html('');
    $('#pageSkeleton').show();
    $('body').loading({
        stoppable: false,
        message: '<div><div class="brand-spinner"></div><p class="loading-text">Yükleniyor<span class="loading-dots"><span>.</span><span>.</span><span>.</span></span></p></div>'
    });

    // Hiç istek açılmayan kombinasyonlarda ajaxStop tetiklenmez
    setTimeout(function () {
        if (_yieldLoadingActive && !$.active) hideYieldTableLoading();
    }, 0);
}

function hideYieldTableLoading() {
    _yieldLoadingActive = false;
    $('#pageSkeleton').hide();
    $('#dynamicTableContainer').show();
    $('#dynamicTableContainer2').toggle(_yieldHasSecondTable);
    // Tablo kendi kabında kayar; container gizlenip gösterilince tarayıcı eski konumu koruyor
    resetTableScroll('#dynamicTableBody');
    $('body').loading('stop');
}

// Loading'i son biten istek kapatır
$(document).ajaxStop(function () {
    if (_yieldLoadingActive) hideYieldTableLoading();
});

// ===== Nested Response Helpers =====
function flattenRows(items, depth) {
    var rows = [];
    if (!items) return rows;
    items.forEach(function (item) {
        item._depth = depth || 0;
        item._hasChildren = item.SubProducts && item.SubProducts.length > 0;
        rows.push(item);
        if (item._hasChildren) {
            rows = rows.concat(flattenRows(item.SubProducts, (depth || 0) + 1));
        }
    });
    return rows;
}

// ===== PDF verisi (window.PdfReport) — servis cevabından kurulur, DOM'dan okunmaz =====
// Kolon başlıkları _cachedHeaders'ın leaf (en alt) başlıklarından; değerler response objesinin
// alanlarından (meta + *Code/*Id hariç) sırayla alınır. Tüm rapor türleri için tek noktadan çalışır.
function _yieldLeafHeaderNames() {
    var headers = (typeof _cachedHeaders !== 'undefined' && _cachedHeaders) ? _cachedHeaders : [];
    var top = headers.filter(function (h) { return h.ParentId === 0; }).sort(function (a, b) { return a.OrderNo - b.OrderNo; });
    var childMap = {};
    headers.forEach(function (h) {
        if (h.ParentId !== 0) { (childMap[h.ParentId] = childMap[h.ParentId] || []).push(h); }
    });
    Object.keys(childMap).forEach(function (k) { childMap[k].sort(function (a, b) { return a.OrderNo - b.OrderNo; }); });
    var leaves = [];
    top.forEach(function (h) {
        var kids = childMap[h.Id];
        if (kids && kids.length) { kids.forEach(function (c) { leaves.push({ name: c.HeaderName, group: h.HeaderName }); }); }
        else { leaves.push({ name: h.HeaderName, group: null }); }
    });
    return leaves.filter(function (l) { return l.name !== '#'; });   // index kolonu hariç
}

function _yieldFields(sample) {
    if (!sample) return [];
    var skip = { _depth: 1, _hasChildren: 1, SubProducts: 1 };
    return Object.keys(sample).filter(function (k) {
        if (skip[k]) return false;
        if (/Code$|Id$|Diff$/.test(k)) return false;   // gösterilmeyen anahtarlar
        var v = sample[k];
        if (v && typeof v === 'object') return false;   // nested / dizi
        return true;
    });
}

function _yieldFmt(field, v) {
    if (v == null || v === '') return '-';
    if (typeof v === 'number') {
        if (/Rate$|Ratio$|Percent/i.test(field)) return formatPercent(v);
        return (typeof formatNumber === 'function') ? formatNumber(v) : String(v);
    }
    return String(v);
}

function _yieldRows(items, fields) {
    return (items || []).map(function (it) {
        var o = {};
        fields.forEach(function (f, i) { o['c' + i] = _yieldFmt(f, it[f]); });
        if (it.SubProducts && it.SubProducts.length) o.children = _yieldRows(it.SubProducts, fields);
        return o;
    });
}

function _yieldInfoLines() {
    var date = ($('.date-text').text() || '').trim();
    var region = ($('#yieldBolgeLabel').text() || '').trim(); if (region === 'Bölge' || !region) region = 'Tüm Bölgeler';
    var branch = ($('#yieldSubeLabel').text() || '').trim(); if (branch === 'Şube' || !branch) branch = 'Tüm Şubeler';
    var type = ($('#yieldToggle .segment.active').text() || '').trim();   // Hacim / Adet (varsa)
    var tab = ($('#tabBarList .tab.active').text() || '').trim();
    var subtab = ($('#subTabBarList .sub-tab.active').text() || '').trim();

    var lines = [];
    lines.push((date ? date + ' tarihine ait ' : '') + region + ' / ' + branch);
    if (type) lines.push('Rapor Türü: ' + type);
    var segment = [tab, subtab].filter(Boolean).join(' - ');
    if (segment) lines.push('Segment: ' + segment);
    return lines;
}

function setYieldPdfReport(data) {
    var leaves = _yieldLeafHeaderNames();
    var fields = _yieldFields((data && data[0]) || null);
    var n = Math.min(leaves.length, fields.length);
    var columns = [];
    for (var i = 0; i < n; i++) columns.push({ header: leaves[i].name, group: leaves[i].group || undefined, key: 'c' + i, align: i === 0 ? 'left' : undefined });   // ilk kolon (ad) sola dayalı
    var title = ($('.page-title').text() || 'Verim Raporu').trim();
    window.PdfReport = {
        title: title,
        infoLines: _yieldInfoLines(),
        columns: columns,
        rows: _yieldRows(data || [], fields.slice(0, n)),
        childrenKey: 'children',
        footerNote: 'Tablodaki değerler /1000 olarak verilmektedir.',
        filename: (title.replace(/[\\/:*?"<>|]+/g, '').trim() || 'Verim-Raporu') + '.pdf'
    };
}

// Response'tan asıl data array'ini çıkar (ilk array property veya direkt array)
function extractResponseData(response) {
    var data = response;
    if (!Array.isArray(response)) {
        for (var key in response) {
            if (response.hasOwnProperty(key) && Array.isArray(response[key])) { data = response[key]; break; }
        }
    }
    try { setYieldPdfReport(Array.isArray(data) ? data : []); } catch (e) { /* PDF verisi opsiyonel */ }
    return data;
}

// ===== Tab State =====
var _productivityTabs = [];
var _activeToggleId = null;
var _activeTabId = null;
var _activeSubTabId = null;

// ===== Sort State =====
var _yieldSortBy = null;
var _yieldSortAsc = true;

// ===== In-flight Request Tracking =====
// Hızlı tab/şube/bölge değişiminde önceki isteğin geç gelen cevabı ekranı ezmesin diye
// her yeni tablo isteğinden önce bir öncekini iptal ederiz.
var _yieldHeadersXhr = null;
var _yieldTableXhr = null;
var _yieldTable2Xhr = null;

// ===== Load Tabs =====
function loadProductivityTabs(filterType, callback) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityReportTabs',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ sessionId: '1', filterType: filterType }),
        success: function (data) {
            _productivityTabs = data;
            renderMainTabs();
            if (callback) callback();
        }
    });
}

// ===== Render Main Tabs (Level 1) =====
// Genel banka geneli rapordur; bölge veya şube seçiliyken sekmelerde gösterilmez.
var GENERAL_TAB_ID = 1;
var _yieldScopeSelected = false;

function applyYieldScope(selected) {
    selected = !!selected;
    if (selected === _yieldScopeSelected) return;
    _yieldScopeSelected = selected;
    renderMainTabs();
}

function renderMainTabs() {
    var mainTabs = _productivityTabs.filter(function (t) {
        if (t.TabLevel !== 1) return false;
        return !(_yieldScopeSelected && t.TabId === GENERAL_TAB_ID);
    });
    var $toggle = $('#yieldToggle');
    $toggle.empty();

    mainTabs.forEach(function (tab, i) {
        if (i > 0) {
            $toggle.append('<div class="divider"></div>');
        }
        var activeClass = (i === 0) ? ' active' : '';
        $toggle.append('<button class="segment' + activeClass + '" data-tab-id="' + tab.TabId + '">' + tab.TabName + '</button>');
    });

    // İlk tab'ı aktif yap
    if (mainTabs.length > 0) {
        _activeToggleId = mainTabs[0].TabId;
    }

    renderTabBar();
}

// ===== Render Tab Bar (Level 2 children of active main tab) =====
function renderTabBar() {
    var tabs = _productivityTabs.filter(function (t) { return t.ParentId === _activeToggleId && t.TabLevel === 2; });
    var $bar = $('#tabBar');
    var $list = $('#tabBarList');

    $list.empty();
    _activeTabId = null;
    _activeSubTabId = null;

    if (tabs.length === 0) {
        $bar.hide();
        renderSubTabBar();
        return;
    }

    tabs.forEach(function (tab, i) {
        if (i > 0) {
            $list.append('<span class="tab-divider">|</span>');
        }
        var activeClass = (i === 0) ? ' active' : '';
        $list.append('<button class="tab' + activeClass + '" data-tabbar-id="' + tab.TabId + '">' + tab.TabName + '</button>');
    });

    _activeTabId = tabs[0].TabId;
    $bar.show();
    renderSubTabBar();
}

// ===== Render Sub Tab Bar (Level 3 children of active tab bar item) =====
function renderSubTabBar() {
    var parentId = _activeTabId || _activeToggleId;
    var tabs = _productivityTabs.filter(function (t) { return t.ParentId === parentId && t.TabLevel === 3; });
    var $bar = $('#subTabBar');
    var $list = $('#subTabBarList');

    $list.empty();
    _activeSubTabId = null;

    if (tabs.length === 0) {
        $bar.hide();
        return;
    }

    tabs.forEach(function (tab, i) {
        var activeClass = (i === 0) ? ' active' : '';
        $list.append('<button class="sub-tab' + activeClass + '" data-subtabbar-id="' + tab.TabId + '">' + tab.TabName + '</button>');
    });

    _activeSubTabId = tabs[0].TabId;
    $bar.show();
}

// ===== Get Active Tab IDs =====
function getActiveTabIds() {
    return {
        toggleId: _activeToggleId,
        tabId: _activeTabId,
        subTabId: _activeSubTabId
    };
}

// ===== Event Handlers (delegated) =====

// Main tab click
$(document).on('click', '#yieldToggle .segment', function () {
    $('#yieldToggle .segment').removeClass('active');
    $(this).addClass('active');
    _activeToggleId = parseInt($(this).data('tab-id'));
    _yieldSortBy = null; _yieldSortAsc = true;
    renderTabBar();
    if (typeof onProductivityTabChange === 'function') {
        onProductivityTabChange();
    }
});

// Tab bar click (Level 2)
$(document).on('click', '#tabBarList .tab', function () {
    $('#tabBarList .tab').removeClass('active');
    $(this).addClass('active');
    _activeTabId = parseInt($(this).data('tabbar-id'));
    renderSubTabBar();
    if (typeof onProductivityTabChange === 'function') {
        onProductivityTabChange();
    }
});

// Sub tab bar click (Level 3)
$(document).on('click', '#subTabBarList .sub-tab', function () {
    $('#subTabBarList .sub-tab').removeClass('active');
    $(this).addClass('active');
    _activeSubTabId = parseInt($(this).data('subtabbar-id'));
    if (typeof onProductivityTabChange === 'function') {
        onProductivityTabChange();
    }
});

// Branch name click in table -> select matching branch filter
$(document).on('click', '.branch-link', function (e) {
    e.stopPropagation();
    var code = $(this).attr('data-branch-code');
    if (!code) return;
    var $item = $('#yieldSubeList .dropdown-item[data-code="' + code + '"]').first();
    if ($item.length) $item.trigger('click');
});

// ===== Sort Click Handler =====
$(document).on('click', '#dynamicTable thead th, #dynamicTable2 thead th', function () {
    var $icon = $(this).find('.sort-icon[data-sort-id]');
    if (!$icon.length) return;

    var sortId = parseInt($icon.data('sort-id'));

    if (_yieldSortBy === sortId) {
        if (_yieldSortAsc) {
            _yieldSortAsc = false;
        } else {
            _yieldSortBy = null;
            _yieldSortAsc = true;
        }
    } else {
        _yieldSortBy = sortId;
        _yieldSortAsc = true;
    }

    // Reload table
    if (typeof onProductivityTabChange === 'function') {
        onProductivityTabChange();
    }
});

function loadGeneralRegionReport(tabId) {
    var tab = (tabId != null) ? String(tabId) : null;

    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityGeneralRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            isKoluAdi: tab,
            segment: tab
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderGeneralRegionTable(items);
        }
    });
}

function renderGeneralRegionTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.Urun + '</span>' : item.Urun;
        html += '<td class="col-left">' + indent + '</td>';

        html += '<td>' + formatNumber(item.BankaGecenYil) + '</td>';
        html += '<td>' + formatNumber(item.BankaGerceklesen) + '</td>';
        html += '<td>' + formatNumber(item.BankaOrt) + '</td>';
        html += '<td>' + formatNumber(item.BankaHedef) + '</td>';
        html += '<td class="' + percentColor(item.HgYuzde) + '">' + formatPercent(item.HgYuzde) + '</td>';
        html += '<td>' + formatNumber(item.NetBuyumeBanka) + '</td>';
        html += '<td>' + formatNumber(item.NetBuyumeBankaOrt) + '</td>';
        html += '<td>' + formatPercent(item.YtdBanka) + '</td>';
        html += '<td>' + formatPercent(item.QtdBanka) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Load Table Headers =====
var _cachedHeaders = [];
var _lastHeaderParams = null;

function loadTableHeaders(toggleId, tabId, subTabId, filterType, callback) {
    var paramKey = toggleId + '-' + (tabId || 0) + '-' + (subTabId || 0) + '-' + filterType;

    if (paramKey === _lastHeaderParams && _cachedHeaders.length > 0) {
        if (callback) callback();
        return;
    }

    abortXhr(_yieldHeadersXhr);
    _yieldHeadersXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityReportTableHeaders',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            mainTabId: toggleId,
            midTabId: tabId || null,
            subTabId: subTabId || null,
            filterType: filterType,
            reportDate: _selectedDate
        }),
        success: function (data) {
            _cachedHeaders = data;
            _lastHeaderParams = paramKey;
            renderDynamicHeaders(data, false);
            if (callback) callback();
        }
    });
}

// hasExpandable: data'da parentId olan satır varsa true
function sortIcon(h) {
    if (!h.Sortable) return '';
    return ' <i class="sort-icon" data-sort-id="' + h.Id + '"><img class="sort-up" src="/images/sort-asc.svg" alt="" /><img class="sort-down" src="/images/sort-dec.svg" alt="" /></i>';
}

function renderDynamicHeaders(headers, hasExpandable, withDetail, selectedGroupCount) {
    selectedGroupCount = selectedGroupCount || 1;
    var $thead = $('#dynamicTableHead');
    $thead.empty();

    if (!headers || headers.length === 0) return;

    function getClasses(h, isRowspanned) {
        var cls = [];
        if (h.HeaderName === '#') cls.push('col-index');
        if (isRowspanned) cls.push('valign-bottom');
        return cls.length > 0 ? ' class="' + cls.join(' ') + '"' : '';
    }

    var topHeaders = headers.filter(function (h) { return h.ParentId === 0; }).sort(function (a, b) { return a.OrderNo - b.OrderNo; });
    var childMap = {};
    headers.forEach(function (h) {
        if (h.ParentId !== 0) {
            if (!childMap[h.ParentId]) childMap[h.ParentId] = [];
            childMap[h.ParentId].push(h);
        }
    });
    for (var key in childMap) {
        childMap[key].sort(function (a, b) { return a.OrderNo - b.OrderNo; });
    }

    var hasGroupHeaders = Object.keys(childMap).length > 0;
    var rowspan = hasGroupHeaders ? 2 : 1;
    var expandTh = hasExpandable ? '<th rowspan="' + rowspan + '" class="col-expand"></th>' : '';
    // Detay (kırılım) kolonu şimdilik kapalı; geri açmak için alttaki satırı yorumdan çıkarın.
    // var detailTh = withDetail ? '<th rowspan="' + rowspan + '" class="col-detail"></th>' : '';
    var detailTh = '';

    var INFO_ICON = '<span class="info-icon" data-tooltip="Ürünler konsolide değil solo kalemleri içermektedir." tabindex="0"><img src="/images/info.svg" alt="" /></span>';

    function headerContent(h, idx) {
        var text = h.HeaderName + sortIcon(h);
        if (idx === 1) {
            return '<div class="col-group-header-content" style="justify-content:flex-start;">' + INFO_ICON + '<span>' + h.HeaderName + '</span>' + sortIcon(h) + '</div>';
        }
        return text;
    }

    if (!hasGroupHeaders) {
        var row = '<tr>';
        topHeaders.forEach(function (h, i) {
            row += '<th' + getClasses(h, false) + '>' + headerContent(h, i) + '</th>';
            if (i === 0) row += expandTh;
        });
        row += detailTh;
        row += '</tr>';
        $thead.append(row);
    } else {
        var row1 = '<tr>';
        var row2 = '<tr>';

        var firstGroupFound = 0;
        topHeaders.forEach(function (h, i) {
            var children = childMap[h.Id];
            if (children && children.length > 0) {
                var isFirstGroup = firstGroupFound < selectedGroupCount;
                if (isFirstGroup) firstGroupFound++;
                var groupCls = isFirstGroup ? 'col-group-header selected' : 'col-group-header';
                row1 += '<th colspan="' + children.length + '" class="' + groupCls + '">' + h.HeaderName + '</th>';
                children.forEach(function (c, cIdx) {
                    // "Bu Yıl (30.06.2026)" -> tarih alt satıra iner
                    var m = /^(.*?)\s*\((.+)\)$/.exec(c.HeaderName);
                    var leafHtml = m ? m[1] + '<small>(' + m[2] + ')</small>' : c.HeaderName;
                    var childCls = '';
                    if (isFirstGroup) {
                        if (cIdx === 0) childCls = ' class="col-selected-first"';
                        else if (cIdx === children.length - 1) childCls = ' class="col-selected-last"';
                        else childCls = ' class="col-selected-mid"';
                    }
                    row2 += '<th' + childCls + '>' + leafHtml + sortIcon(c) + '</th>';
                });
            } else {
                row1 += '<th rowspan="2"' + getClasses(h, true) + '>' + headerContent(h, i) + '</th>';
            }
            if (i === 0) row1 += expandTh;
        });

        row1 += detailTh;
        row1 += '</tr>';
        row2 += '</tr>';
        $thead.append(row1);
        $thead.append(row2);

        var $groups = $thead.find('.col-group-header');
        $groups.removeClass('selected');
        $groups.slice(0, selectedGroupCount).addClass('selected');

        // Ofset, önceki grupların colspan toplamı (gruplar row2'de sırayla yer alır).
        var $row2ths = $thead.find('tr:last th');
        $row2ths.removeClass('col-selected-first col-selected-mid col-selected-last');
        var leafOffset = 0;
        $groups.each(function (gIdx) {
            var span = parseInt($(this).attr('colspan')) || 1;
            if (gIdx < selectedGroupCount) {
                $row2ths.eq(leafOffset).addClass('col-selected-first');
                for (var m = 1; m < span - 1; m++) {
                    $row2ths.eq(leafOffset + m).addClass('col-selected-mid');
                }
                $row2ths.eq(leafOffset + span - 1).addClass('col-selected-last');
            }
            leafOffset += span;
        });
    }
}

// ===== Volume Region Report (Hacim — Bölge) =====
function loadVolumeRegionReport(regionCode, subTabId) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityVolumeRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            subTabId: subTabId || 0,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable, true);
            renderVolumeRegionTable(items);
        }
    });
}

function renderVolumeRegionTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    var topLevelItems = items.filter(function (item) { return item._depth === 0; });
    var lastTopLevelId = topLevelItems.length > 0 ? topLevelItems[topLevelItems.length - 1].Id : null;

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';
        var lastRowClass = (item.Id === lastTopLevelId) ? ' custom-last-row' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + lastRowClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-left">' + indent + '</td>';

        html += '<td>' + formatNumber(item.RealizationRegionLastYearValue) + '</td>';
        html += '<td>' + formatNumber(item.TargetValue) + '</td>';
        html += '<td class="' + percentColor(item.HgRate) + '">' + formatPercent(item.HgRate) + '</td>';
        html += '<td>' + formatNumber(item.RealizationRegionAverageValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverageValue) + formatDiff(item.RealizationBankAverageDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.NetGrowthRegionAverageValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.NetGrowthBankAverageValue) + formatDiff(item.NetGrowthBankAverageDiff, true) + '</td>';
        html += '<td class="' + compareColor(item.YtdRegionValue, item.YtdBankAverageValue) + '">' + formatRateValue(item.YtdRegionValue) + '</td>';
        html += '<td class="has-diff">' + formatRateValue(item.YtdBankAverageValue) + formatDiff(item.YtdBankAverageDiff) + '</td>';
        html += '<td class="' + compareColor(item.QtdRegionValue, item.QtdBankAverageValue) + '">' + formatRateValue(item.QtdRegionValue) + '</td>';
        html += '<td class="has-diff">' + formatRateValue(item.QtdBankAverageValue) + formatDiff(item.QtdBankAverageDiff) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Volume Branch Report (Hacim — Şube) =====
function loadVolumeBranchReport(branchCode, subTabId) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityVolumeBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            subTabId: subTabId || 0,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable, true);
            renderVolumeBranchTable(items);
        }
    });
}

function renderVolumeBranchTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    var topLevelItems = items.filter(function (item) { return item._depth === 0; });
    var lastTopLevelId = topLevelItems.length > 0 ? topLevelItems[topLevelItems.length - 1].Id : null;

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';
        var lastRowClass = (item.Id === lastTopLevelId) ? ' custom-last-row' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + lastRowClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-left">' + indent + '</td>';

        html += '<td>' + formatNumber(item.RealizationBranchLastYearValue) + '</td>';
        html += '<td>' + formatNumber(item.TargetValue) + '</td>';
        html += '<td class="' + percentColor(item.HgRate) + '">' + formatPercent(item.HgRate) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationRegionAverageValue) + formatDiff(item.RealizationRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverageValue) + formatDiff(item.RealizationBankAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.NetGrowthRegionAverageValue) + formatDiff(item.NetGrowthRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.NetGrowthBankAverageValue) + formatDiff(item.NetGrowthBankAverageValueDiff, true) + '</td>';
        html += '<td class="' + compareColor(item.YtdBranchValue, item.YtdRegionValue) + '">' + formatRateValue(item.YtdBranchValue) + '</td>';
        html += '<td class="has-diff ' + compareColor(item.YtdRegionValue, item.YtdBankValue) + '">' + formatRateValue(item.YtdRegionValue) + formatDiff(item.YtdRegionValueDiff) + '</td>';
        html += '<td class="has-diff">' + formatRateValue(item.YtdBankValue) + formatDiff(item.YtdBankValueDiff) + '</td>';
        html += '<td class="' + compareColor(item.QtdBranchValue, item.QtdRegionValue) + '">' + formatRateValue(item.QtdBranchValue) + '</td>';
        html += '<td class="has-diff ' + compareColor(item.QtdRegionValue, item.QtdBankValue) + '">' + formatRateValue(item.QtdRegionValue) + formatDiff(item.QtdRegionValueDiff) + '</td>';
        html += '<td class="has-diff">' + formatRateValue(item.QtdBankValue) + formatDiff(item.QtdBankValueDiff) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Customer Region Report (Adet — Bölge) =====
function loadCountCustomerRegionReport(regionCode, subTabId) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityCountCustomerRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            subTabId: subTabId || 0,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable, true);
            renderCountCustomerRegionTable(items);
        }
    });
}

function renderCountCustomerRegionTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';
        var isPercent = item.ProductName.indexOf('%') !== -1;
        var fmt = function (v) { return isPercent ? v : formatNumber(v)};

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-left">' + indent + '</td>';

        html += '<td>' + fmt(item.RealizationRegion) + '</td>';
        html += '<td class="has-diff">' + fmt(item.RealizationBankAverage) + formatDiff(item.RealizationBankAverageDiff, true) + '</td>';
        html += '<td>' + fmt(item.YtdChangeRegion) + '</td>';
        html += '<td class="has-diff">' + fmt(item.YtdChangeBankAverage) + formatDiff(item.YtdChangeBankAverageDiff, true) + '</td>';
        html += '<td>' + fmt(item.QtdChangeRegion) + '</td>';
        html += '<td class="has-diff">' + fmt(item.QtdChangeBankAverage) + formatDiff(item.QtdChangeBankAverageDiff, true) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Customer Branch Report (Adet — Müşteri — Şube) =====
function loadCountCustomerBranchReport(branchCode, subTabId) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityCountCustomerBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            subTabId: subTabId || 0,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable, true);
            renderCountCustomerBranchTable(items);
        }
    });
}

function renderCountCustomerBranchTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-left">' + indent + '</td>';

        html += '<td>' + formatNumber(item.RealizationBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationRegionAverageValue) + formatDiff(item.RealizationRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverageValue) + formatDiff(item.RealizationBankAverageValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.YtdNominalChangeBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.YtdNominalChangeRegionAverageValue) + formatDiff(item.YtdNominalChangeRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.YtdNominalChangeBankAverageValue) + formatDiff(item.YtdNominalChangeBankAverageValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.QtdNominalChangeBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.QtdNominalChangeRegionAverageValue) + formatDiff(item.QtdNominalChangeRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.QtdNominalChangeBankAverageValue) + formatDiff(item.QtdNominalChangeBankAverageValueDiff, true) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Cash Management Region Report (Adet — Nakit Yönetimi — Bölge) =====
function loadCountCashManagementRegionReport(regionCode, subTabId) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityCountCashManagementRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            subTabId: subTabId || 0,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable, true);
            renderCountCashManagementRegionTable(items);
        }
    });
}

function renderCountCashManagementRegionTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-left">' + indent + '</td>';

        html += '<td>' + formatNumber(item.RealizationRegionValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationRegionAverageValue) + formatDiff(item.RealizationRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverageValue) + formatDiff(item.RealizationBankAverageValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.YtdNominalChangeRegionValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.YtdNominalChangeRegionAverageValue) + formatDiff(item.YtdNominalChangeRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.YtdNominalChangeBankAverageValue) + formatDiff(item.YtdNominalChangeBankAverageValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.QtdNominalChangeRegionValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.QtdNominalChangeRegionAverageValue) + formatDiff(item.QtdNominalChangeRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.QtdNominalChangeBankAverageValue) + formatDiff(item.QtdNominalChangeBankAverageValueDiff, true) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Cash Management Branch Report (Adet — Nakit Yönetimi — Şube) =====
function loadCountCashManagementBranchReport(branchCode, subTabId) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityCountCashManagementBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            subTabId: subTabId || 0,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable, true);
            renderCountCashManagementBranchTable(items);
        }
    });
}

function renderCountCashManagementBranchTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-left">' + indent + '</td>';

        html += '<td>' + formatNumber(item.RealizationBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationRegionAverageValue) + formatDiff(item.RealizationRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverageValue) + formatDiff(item.RealizationBankAverageValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.YtdNominalChangeBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.YtdNominalChangeRegionAverageValue) + formatDiff(item.YtdNominalChangeRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.YtdNominalChangeBankAverageValue) + formatDiff(item.YtdNominalChangeBankAverageValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.QtdNominalChangeBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.QtdNominalChangeRegionAverageValue) + formatDiff(item.QtdNominalChangeRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.QtdNominalChangeBankAverageValue) + formatDiff(item.QtdNominalChangeBankAverageValueDiff, true) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Card/POS Branch Report (Adet — Kredi Kartı / POS — Şube) =====
function loadCountCardPosBranchReport(branchCode, tabId) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityCountCardPosBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            tabId: tabId,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable, true);
            renderCountCardPosBranchTable(items);
        }
    });
}

function renderCountCardPosBranchTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-left">' + indent + '</td>';

        html += '<td>' + formatNumber(item.CurrentPeriodBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.CurrentPeriodRegionAverageValue) + formatDiff(item.CurrentPeriodRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.CurrentPeriodBankAverageValue) + formatDiff(item.CurrentPeriodBankAverageValueDiff, true) + '</td>';
        html += '<td>' + item.ThreeMonthHgBranchValue + '</td>';
        html += '<td class="has-diff">' + item.ThreeMonthHgRegionAverageValue + formatDiff(item.ThreeMonthHgRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.ThreeMonthHgBankAverageValue + formatDiff(item.ThreeMonthHgBankAverageValueDiff) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Card/POS Region Report (Adet — Kredi Kartı / POS — Bölge) =====
function loadCountCardPosRegionReport(regionCode, tabId) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityCountCardPosRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            tabId: tabId,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable, true);
            renderCountCardPosRegionTable(items);
        }
    });
}

function renderCountCardPosRegionTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-left">' + indent + '</td>';

        html += '<td>' + formatNumber(item.CurrentMonthRegionValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.CurrentMonthBankAverage) + formatDiff(item.CurrentMonthBankAverageDiff, true) + '</td>';
        html += '<td>' + item.ThreeMonthHgRegion + '</td>';
        html += '<td class="has-diff">' + item.ThreeMonthHgBankAverage + formatDiff(item.ThreeMonthHgBankAverageDiff) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Card/POS Ratio Region Report (Adet — Oran Tablosu — Bölge) =====
// Ödeme Sistemleri tek tablo gösterir; oran tablosu ana konteynere (#dynamicTable) render edilir.
function loadCountCardPosRatioRegionReport(regionCode, tabId) {
    // Load headers first, then data
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityCountCardPosRatioRegionReportTableHeaders',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            tabId: tabId,
            reportDate: _selectedDate
        }),
        success: function (headers) {
            renderCountCardPosRatioRegionHeaders(headers);

            _yieldTableXhr = $.ajax({
                url: '/ProductivityReport/GetProductivityCountCardPosRatioRegionReport',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    sessionId: '1',
                    regionCode: regionCode,
                    tabId: tabId,
                    reportDate: _selectedDate
                }),
                success: function (response) {
                    var data = extractResponseData(response);
                    renderCountCardPosRatioRegionTable(data);
                }
            });
        }
    });
}


function setRatioCachedHeaders(names) {
    _cachedHeaders = names.map(function (name, i) {
        return { Id: i + 1, HeaderName: name, ParentId: 0, OrderNo: i + 1, Sortable: false };
    });
    _lastHeaderParams = null;
}

function renderCountCardPosRatioRegionHeaders(h) {
    var $thead = $('#dynamicTableHead');
    $thead.empty();

    setRatioCachedHeaders([
        h.RowNumberTitle,
        h.RatioNameTitle,
        h.PreviousQuarterRegionTitle,
        h.CurrentRegionTitle,
        h.CurrentBankAverageTitle
    ]);

    var row = '<tr>';
    row += '<th class="col-index">' + h.RowNumberTitle + '</th>';
    row += '<th class="col-left">' + h.RatioNameTitle + '</th>';
    row += '<th>' + h.PreviousQuarterRegionTitle + '</th>';
    row += '<th>' + h.CurrentRegionTitle + '</th>';
    row += '<th>' + h.CurrentBankAverageTitle + '</th>';
    // Detay kolonu şimdilik kapalı
    // row += '<th class="col-detail"></th>';
    row += '</tr>';

    $thead.append(row);
}

function renderCountCardPosRatioRegionTable(items) {
    var html = '';

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var isPercent = item.RatioName.indexOf('%') !== -1;
        var fmt = function (v) { return isPercent ? v : formatNumber(v)};

        html += '<tr class="table-row ' + cls + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';
        html += '<td class="col-left">' + item.RatioName + '</td>';
        html += '<td>' + fmt(item.PreviousQuarterRegionValue) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CurrentRegionValue) + formatDiff(item.CurrentRegionDiff, !isPercent) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CurrentBankAverageValue) + formatDiff(item.CurrentBankAverageDiff, !isPercent) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Card/POS Ratio Branch Report (Adet — Oran Tablosu — Şube) =====
function loadCountCardPosRatioBranchReport(branchCode, tabId) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityCountCardPosRatioBranchReportTableHeaders',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            tabId: tabId,
            reportDate: _selectedDate
        }),
        success: function (headers) {
            renderCountCardPosRatioBranchHeaders(headers);

            _yieldTableXhr = $.ajax({
                url: '/ProductivityReport/GetProductivityCountCardPosRatioBranchReport',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    sessionId: '1',
                    branchCode: branchCode,
                    tabId: tabId,
                    reportDate: _selectedDate
                }),
                success: function (response) {
                    var data = extractResponseData(response);
                    renderCountCardPosRatioBranchTable(data);
                }
            });
        }
    });
}

function renderCountCardPosRatioBranchHeaders(h) {
    var $thead = $('#dynamicTableHead');
    $thead.empty();

    setRatioCachedHeaders([
        h.RowNumberTitle,
        h.RatioNameTitle,
        h.PreviousQuarterBranchTitle,
        h.CurrentBranchTitle,
        h.CurrentRegionAverageTitle,
        h.CurrentBankAverageTitle
    ]);

    var row = '<tr>';
    row += '<th class="col-index">' + h.RowNumberTitle + '</th>';
    row += '<th class="col-left">' + h.RatioNameTitle + '</th>';
    row += '<th>' + h.PreviousQuarterBranchTitle + '</th>';
    row += '<th>' + h.CurrentBranchTitle + '</th>';
    row += '<th>' + h.CurrentRegionAverageTitle + '</th>';
    row += '<th>' + h.CurrentBankAverageTitle + '</th>';
    // Detay kolonu şimdilik kapalı
    // row += '<th class="col-detail"></th>';
    row += '</tr>';

    $thead.append(row);
}

function renderCountCardPosRatioBranchTable(items) {
    var html = '';

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var isPercent = item.RatioName.indexOf('%') !== -1;
        var fmt = function (v) { return isPercent ? v : formatNumber(v)};

        html += '<tr class="table-row ' + cls + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';
        html += '<td class="col-left">' + item.RatioName + '</td>';
        html += '<td>' + fmt(item.PreviousQuarterBranchValue) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CurrentBranchValue) + formatDiff(item.CurrentBranchValueDiff, !isPercent) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CurrentRegionAverageValue) + formatDiff(item.CurrentRegionAverageValueDiff, !isPercent) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CurrentBankAverageValue) + formatDiff(item.CurrentBankAverageValueDiff, !isPercent) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Profit Total Region Report (Karlılık — Üst Tablo — Bölge) =====
// Header'ları loadTableHeaders'dan alır, body dynamicTableBody'ye render eder
function loadProfitTotalRegionReport(regionCode) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityProfitTotalRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable, true);
            renderProfitTotalRegionTable(items);
        }
    });
}

function renderProfitTotalRegionTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    // Filter top-level items to find last one for special styling
    var topLevelItems = items.filter(function (item) { return item._depth === 0; });
    var lastTopLevelId = topLevelItems.length > 0 ? topLevelItems[topLevelItems.length - 1].Id : null;

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';
        var lastRowClass = (item.Id === lastTopLevelId) ? ' custom-last-row' : '';
        var summaryClass = (!lastRowClass && item.Description === 'Net İşletme Geliri') ? ' row-summary' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + lastRowClass + summaryClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.Description + '</span>' : item.Description;
        html += '<td class="col-left">' + indent + '</td>';

        html += '<td class="has-diff">' + formatNumber(item.RealizationRegionValue) + formatDiff(item.RealizationRegionValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.RegionAverageValue) + '</td>';
        html += '<td>' + formatNumber(item.BankAverageValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverageValue) + formatDiff(item.RealizationBankAverageValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.TargetValue) + '</td>';
        html += '<td>' + formatNumber(item.BankBudgetValue) + '</td>';
        html += '<td class="' + (item.HgRegionValue == 0 ? '' : 'has-diff ' + percentColor(item.HgRegionValue)) + '">' + (item.HgRegionValue == 0 ? '-' : item.HgRegionValue + formatDiff(item.HgRegionValueDiff)) + '</td>';
        html += '<td class="' + (item.HgBankAverageValue == 0 ? '' : 'has-diff ' + percentColor(item.HgBankAverageValue)) + '">' + (item.HgBankAverageValue == 0 ? '-' : item.HgBankAverageValue + formatDiff(item.HgBankAverageValueDiff)) + '</td>';
        html += '<td>' + formatNumber(item.RetailValue) + '</td>';
        html += '<td>' + formatNumber(item.KobiValue) + '</td>';
        html += '<td>' + formatNumber(item.AgricultureValue) + '</td>';
        html += '<td>' + (item.CommercialValue == 0 ? '-' : formatNumber(item.CommercialValue)) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Profit Ratio Region Report (Karlılık — Alt Tablo — Bölge) =====
// Header'ları statik renderProfitRatioHeaders ile, body dynamicTableBody2'ye render eder
function loadProfitRatioRegionReport(regionCode) {
    abortXhr(_yieldTable2Xhr);
    _yieldTable2Xhr = $.ajax({
        url: '/ProductivityReport/GetProductivityProfitRatioRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderProfitRatioHeaders(hasExpandable, false);
            renderProfitRatioRegionTable(items);
            _yieldHasSecondTable = true;
        }
    });
}

function renderProfitRatioHeaders(hasExpandable, isBranch) {
    var $thead = $('#dynamicTableHead2');
    $thead.empty();

    var expandTh = hasExpandable ? '<th class="col-expand"></th>' : '';

    var row = '<tr>';
    row += '<th class="col-index">#</th>';
    row += expandTh;
    row += '<th class="col-left">Oran Adı</th>';
    if (isBranch) row += '<th>Şube</th>';
    row += '<th>Bölge</th>';
    row += '<th>Banka</th>';
    row += '<th>Bireysel</th>';
    row += '<th>KOBİ</th>';
    row += '<th>Tarım</th>';
    row += '<th>Ticari</th>';
    // Detay kolonu şimdilik kapalı
    // row += '<th class="col-detail"></th>';
    row += '</tr>';

    $thead.append(row);
}

function renderProfitRatioRegionTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';
        var isPercent = item.RatioName.indexOf('%') !== -1;
        var fmt = function (v) { return isPercent ? v : formatNumber(v)};

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.RatioName + '</span>' : item.RatioName;
        html += '<td class="col-left">' + indent + '</td>';
        html += '<td class="has-diff">' + fmt(item.RegionValue) + formatDiff(item.RegionValueDiff, !isPercent, true) + '</td>';
        html += '<td class="has-diff">' + fmt(item.BankValue) + formatDiff(item.BankValueDiff, !isPercent, true) + '</td>';
        html += '<td>' + fmt(item.RetailValue) + '</td>';
        html += '<td>' + fmt(item.KobiValue) + '</td>';
        html += '<td class="has-diff">' + fmt(item.AgricultureValue) + formatDiff(item.AgricultureValueDiff, !isPercent, true) + '</td>';
        html += '<td>' + (item.CommercialValue == 0 ? '-' : fmt(item.CommercialValue)) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody2').html(html);
    updateProductivityStripes2();
}

// ===== Profit Ratio Branch Report (Karlılık — Alt Tablo — Şube) =====
function loadProfitRatioBranchReport(branchCode) {
    abortXhr(_yieldTable2Xhr);
    _yieldTable2Xhr = $.ajax({
        url: '/ProductivityReport/GetProductivityProfitRatioBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderProfitRatioHeaders(hasExpandable, true);
            renderProfitRatioBranchTable(items);
            _yieldHasSecondTable = true;
        }
    });
}

function renderProfitRatioBranchTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';
        var isPercent = item.RatioName.indexOf('%') !== -1;
        var fmt = function (v) { return isPercent ? v : formatNumber(v)};

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.RatioName + '</span>' : item.RatioName;
        html += '<td class="col-left">' + indent + '</td>';
        html += '<td class="has-diff">' + fmt(item.BranchValue) + formatDiff(item.BranchValueDiff, !isPercent, true) + '</td>';
        html += '<td class="has-diff">' + fmt(item.RegionValue) + formatDiff(item.RegionValueDiff, !isPercent, true) + '</td>';
        html += '<td class="has-diff">' + fmt(item.BankValue) + formatDiff(item.BankValueDiff, !isPercent, true) + '</td>';
        html += '<td>' + fmt(item.RetailValue) + '</td>';
        html += '<td>' + fmt(item.KobiValue) + '</td>';
        html += '<td class="has-diff">' + fmt(item.AgricultureValue) + formatDiff(item.AgricultureValueDiff, !isPercent, true) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CommercialValue) + formatDiff(item.CommercialValueDiff, !isPercent, true) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody2').html(html);
    updateProductivityStripes2();
}

// ===== Profit Total Branch Report (Karlılık — Üst Tablo — Şube) =====
function loadProfitTotalBranchReport(branchCode) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityProfitTotalBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable, true);
            renderProfitTotalBranchTable(items);
        }
    });
}

function renderProfitTotalBranchTable(items) {
    var html = '';
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    var topLevelItems = items.filter(function (item) { return item._depth === 0; });
    var lastTopLevelId = topLevelItems.length > 0 ? topLevelItems[topLevelItems.length - 1].Id : null;

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';
        var lastRowClass = (item.Id === lastTopLevelId) ? ' custom-last-row' : '';
        var summaryClass = (!lastRowClass && item.Description === 'Net İşletme Geliri') ? ' row-summary' : '';
        var isProfitBeforeTaxes = (item.Description === 'VERGİ ÖNCESİ KAR') ? true : false;

        html += '<tr class="table-row ' + cls + depthClass + expandClass + lastRowClass + summaryClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.Description + '</span>' : item.Description;
        html += '<td class="col-left">' + indent + '</td>';

        html += '<td class="has-diff">' + formatNumber(item.RealizationBranchValue) + formatDiff(item.RealizationBranchValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.RegionAverageValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationRegionAverageValue) + formatDiff(item.RealizationRegionAverageValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.BankAverageValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverageValue) + formatDiff(item.RealizationBankAverageValueDiff, true) + '</td>';
        html += '<td>' + (isProfitBeforeTaxes ? '-' : formatNumber(item.BranchBudgetValue)) + '</td>';
        html += '<td>' + (isProfitBeforeTaxes ? '-' : formatNumber(item.RegionBudgetValue)) + '</td>';
        html += '<td>' + (isProfitBeforeTaxes ? '-' : formatNumber(item.BankBudgetValue)) + '</td>';
        html += '<td class="' + (item.HgBranchValue == 0 ? '' : 'has-diff ' + percentColor(item.HgBranchValue)) + '">' + (item.HgBranchValue == 0 ? '-' : (formatPercent(item.HgBranchValue) + formatDiff(item.HgBranchValueDiff))) + '</td>';
        html += '<td class="' + (item.HgRegionAverageValue == 0 ? '' : 'has-diff ' + percentColor(item.HgRegionAverageValue)) + '">' + (item.HgRegionAverageValue == 0 ? '-' : (item.HgRegionAverageValue + formatDiff(item.HgRegionAverageValueDiff))) + '</td>';
        html += '<td class="' + (item.HgBankAverageValue == 0 ? '' : 'has-diff ' + percentColor(item.HgBankAverageValue)) + '">' + (item.HgBankAverageValue == 0 ? '-' : (item.HgBankAverageValue + formatDiff(item.HgBankAverageValueDiff))) + '</td>';
        html += '<td>' + (isProfitBeforeTaxes ? '-' : formatNumber(item.RetailValue)) + '</td>';
        html += '<td>' + (isProfitBeforeTaxes ? '-' : formatNumber(item.KobiValue)) + '</td>';
        html += '<td>' + (isProfitBeforeTaxes ? '-' : formatNumber(item.AgricultureValue)) + '</td>';
        html += '<td class="' + (isProfitBeforeTaxes ? '' : 'has-diff') + '">' + (isProfitBeforeTaxes ? '-' : (formatNumber(item.CommercialValue) + formatDiff(item.CommercialValueDiff, true))) + '</td>';
        html += buildProductivityDetailCell(item);
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Profit Spread Management Region Report (Karlılık — Spread Yönetimi — Bölge) =====
function loadProfitSpreadManagementRegionReport(regionCode) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityProfitSpreadManagementRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable, true, 2);
            renderSpreadTable(items, SPREAD_FIELDS.region);
        }
    });
}

// Kolon sırası başlık servisiyle (GetProductivityReportTableHeaders, MainTabId 4 / MidTabId 41)
// birebir aynı olmalı; aksi hâlde başlıklar hücrelerden kayar.
var SPREAD_FIELDS = {
    region: {
        amounts: ['RegionTargetAverageVolumeCumulativeCurrentYear', 'RegionAverageVolumeCumulativeCurrentYear', 'RegionAverageVolumeCumulativeLastYear',
                  'RegionTargetReturnCumulativeCurrentYear', 'RegionReturnCumulativeCurrentYear', 'RegionReturnCumulativeLastYear'],
        rates:   ['RegionSpreadTargetCurrentYear', 'RegionSpreadReturnCurrentYear', 'RegionSpreadReturnLastYear',
                  'BankSpreadTargetCurrentYear', 'BankSpreadReturnCurrentYear']
    },
    branch: {
        amounts: ['BranchTargetAverageVolumeCumulativeCurrentYear', 'BranchAverageVolumeCumulativeCurrentYear', 'BranchAverageVolumeCumulativeLastYear',
                  'BranchTargetReturnCumulativeCurrentYear', 'BranchReturnCumulativeCurrentYear', 'BranchReturnCumulativeLastYear'],
        rates:   ['BranchSpreadTargetCurrentYear', 'BranchSpreadReturnCurrentYear', 'BranchSpreadReturnLastYear',
                  'RegionSpreadTargetCurrentYear', 'RegionSpreadReturnCurrentYear',
                  'BankSpreadTargetCurrentYear', 'BankSpreadReturnCurrentYear']
    }
};

function spreadRowStart(item, i, hasExpandable) {
    var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
    var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
    var expandClass = item._hasChildren ? ' expandable' : '';

    var html = '<tr class="table-row ' + cls + depthClass + expandClass + '">';
    html += '<td class="col-index">' + (i + 1) + '</td>';
    if (hasExpandable) {
        html += item._hasChildren
            ? '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>'
            : '<td class="col-expand"></td>';
    }
    var name = item.ProductName || '';
    var indent = item._depth > 0
        ? '<span style="padding-left:' + (item._depth * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + name + '</span>'
        : name;
    return html + '<td class="col-left">' + indent + '</td>';
}

// mn TL kolonları düz sayı, spread kolonları yüzde basar.
function renderSpreadTable(items, fields) {
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    var html = items.map(function (item, i) {
        return spreadRowStart(item, i, hasExpandable) +
            fields.amounts.map(function (f) { return '<td>' + formatNumber(item[f]) + '</td>'; }).join('') +
            fields.rates.map(function (f) { return '<td>' + formatPercent(item[f]) + '</td>'; }).join('') +
            buildProductivityDetailCell(item) + '</tr>';
    }).join('');

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}


// ===== Profit Spread Management Branch Report (Karlılık — Spread Yönetimi — Şube) =====
function loadProfitSpreadManagementBranchReport(branchCode) {
    abortXhr(_yieldTableXhr);
    _yieldTableXhr = $.ajax({
        url: '/ProductivityReport/GetProductivityProfitSpreadManagementBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            reportDate: _selectedDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable, true, 2);
            renderSpreadTable(items, SPREAD_FIELDS.branch);
        }
    });
}


function applyProductivityStripes($table) {
    var stripeIndex = 0;
    var mainIndex = 0;
    var subCounters = {};
    var $lastVisible = null;
    $table.find('tbody tr').each(function () {
        var $tr = $(this);
        $tr.removeClass('stripe-odd stripe-even last-visible-row');
        if ($tr.hasClass('sub-row') && !$tr.hasClass('visible')) {
            return;
        }
        stripeIndex++;
        if (!$tr.hasClass('sub-row')) {
            mainIndex++;
            subCounters[mainIndex] = 0;
            $tr.find('td.col-index').text(mainIndex);
            $tr.find('td.col-left .sub-index').remove();
        } else {
            subCounters[mainIndex] = (subCounters[mainIndex] || 0) + 1;
            var subLabel = mainIndex + '.' + subCounters[mainIndex];
            $tr.find('td.col-index').text('');
            var $colText = $tr.find('td.col-left');
            $colText.find('.sub-index').remove();
            $colText.prepend('<span class="sub-index">' + subLabel + '</span>  ');
        }
        $tr.addClass(stripeIndex % 2 === 1 ? 'stripe-odd' : 'stripe-even');
        $lastVisible = $tr;
    });
    if ($lastVisible) $lastVisible.addClass('last-visible-row');
}

function updateProductivityStripes2() {
    applyProductivityStripes($('#dynamicTable2'));
    applyFirstGroupSelected($('#dynamicTable2'));
    reapplySortVisual($('#dynamicTable2'));
}

function updateProductivityStripes() {
    applyProductivityStripes($('#dynamicTable'));
    applyFirstGroupSelected($('#dynamicTable'));
    reapplySortVisual($('#dynamicTable'));
}
function applyFirstGroupSelected($table) {
    var $selected = $table.find('thead .col-group-header.selected');
    if (!$selected.length) return;

    var ranges = [];
    $selected.each(function () {
        var header = this;
        var startCol = 0;
        var found = false;
        $(header).closest('tr').find('th').each(function () {
            var span = parseInt($(this).attr('colspan')) || 1;
            if (this === header) { ranges.push({ start: startCol, count: span }); found = true; return false; }
            startCol += span;
        });
        return found;
    });
    if (!ranges.length) return;

    $table.find('tbody tr').each(function () {
        $(this).find('td').each(function (tdIdx) {
            var $td = $(this);
            ranges.forEach(function (r) {
                var last = r.start + r.count - 1;
                if (tdIdx === r.start) $td.addClass('col-selected-first');
                else if (tdIdx === last) $td.addClass('col-selected-last');
                else if (tdIdx > r.start && tdIdx < last) $td.addClass('col-selected-mid');
            });
        });
    });
}

function reapplySortVisual($table) {
    $table.find('.sort-icon').removeClass('asc desc');
    if (_yieldSortBy !== null) {
        $table.find('.sort-icon[data-sort-id="' + _yieldSortBy + '"]').addClass(_yieldSortAsc ? 'asc' : 'desc');
    }
}

// YTD/QTD kolonları: şube bölgeye, bölge bankaya göre kıyaslanır.
// Değerlerden biri yoksa (formatRateValue "-" basıyor) renk verilmez.
function compareColor(value, benchmark) {
    if (value == null || value === '' || benchmark == null || benchmark === '') return '';
    return Number(value) >= Number(benchmark) ? 'positive' : 'negative';
}

function formatRateValue(val) {
    if (val == null || val === '' || Number(val) === 0) return '-';
    return val;
}

// invertColors: oran tablosunda artı fark kırmızı, eksi fark yeşil gösterilir
function formatDiff(val, useFormatNumber, invertColors) {
    if (!val) return '<div class="diff-value-for-productivity">&nbsp;</div>';
    var isGood = invertColors ? val < 0 : val > 0;
    var cls = isGood ? 'positive' : 'negative';
    var prefix = val > 0 ? '+' : '';
    var display = useFormatNumber ? formatNumber(val) : val;
    return '<div class="diff-value-for-productivity ' + cls + '">' + prefix + display + '</div>';
}

// ===== Yield Search =====
$(function () {
    handleTableSearch('#yieldSearchInput');
});


// ===== Detay (kırılım) — verim raporu ↔ ortak modal (report-detail.js 'productivity' provider) =====
// Hedef raporundaki akışın aynısı: satırdaki detay ikonu, aynı endpoint'i productId (=item.Id) +
// userCode ile yeniden çağırır; dönen kırılımı ortak modal 'productivity' provider ile render eder.
// GetProductivityGeneralRegionReport hariç tüm tablolarda detay ikonu vardır. Top-10 asla gösterilmez.

// Detay (kırılım) şimdilik kapalı: satırlarda detay ikonu basılmaz, kolon da çizilmez
// (renderDynamicHeaders içindeki detailTh ve statik tablolardaki th.col-detail satırları da yorumda).
// Geri açmak için aşağıdaki gövdeyi yorumdan çıkarıp `return '';` satırını silmek yeterli.
function buildProductivityDetailCell(item) {
    return '';
    // var name = (item.ProductName || item.Description || item.RatioName || '').replace(/"/g, '&quot;');
    // return '<td class="col-detail"><img src="/images/expand.svg" alt="Detay" class="detail-icon"' +
    //     ' data-table="productivity"' +
    //     ' data-product-id="' + (item.Id != null ? item.Id : '') + '"' +
    //     ' data-product-name="' + name + '"' +
    //     ' data-top10="0" /></td>';
}

var _yieldBreakdownCtx = {};
var _bdHeaderNames = [];

$(document).ajaxSend(function (ev, jqXHR, settings) {
    var url = settings.url || '';
    if ((settings.type || '').toUpperCase() !== 'POST') return;
    if (!/\/ProductivityReport\/GetProductivity\w+Report$/.test(url)) return;
    if (/TableHeaders$/.test(url) || /GeneralRegion/.test(url) || /ScoreCard/.test(url)) return;
    var isRatio = /Ratio/.test(url);
    var payload = {};
    try { payload = JSON.parse(settings.data); } catch (e) { payload = {}; }
    _yieldBreakdownCtx[isRatio ? '#dynamicTableBody2' : '#dynamicTableBody'] = {
        url: url,
        payload: payload,
        headSelector: isRatio ? '#dynamicTableHead2' : '#dynamicTableHead'
    };
});

function _bdFirstArray(resp) {
    if (Array.isArray(resp)) return resp;
    for (var k in resp) { if (resp.hasOwnProperty(k) && Array.isArray(resp[k])) return resp[k]; }
    return [];
}

function loadProductivityBreakdown(ctx, productId) {
    if (!ctx || !ctx.url) {
        window.ReportBreakdownData = [];
        $(document).trigger('reportBreakdown:loaded', { table: 'productivity' });
        return;
    }
    var body = $.extend({}, ctx.payload, { productId: productId, userCode: window.USER_CODE });
    $.ajax({ url: ctx.url, type: 'POST', contentType: 'application/json', data: JSON.stringify(body) })
        .done(function (resp) { window.ReportBreakdownData = _bdFirstArray(resp); })
        .fail(function () { window.ReportBreakdownData = []; })
        .always(function () { $(document).trigger('reportBreakdown:loaded', { table: 'productivity' }); });
}

function _productivityBuildHead(ctx) {
    var sel = (ctx && ctx.headSelector) || '#dynamicTableHead';
    var $thead = $(sel).clone();
    $thead.find('th.col-index, th.col-expand, th.col-detail').remove();
    $thead.find('.sort-icon, .info-icon').remove();
    $thead.find('.col-group-header').removeClass('selected');
    $thead.find('th').removeClass('col-selected col-selected-first col-selected-mid col-selected-last');
    var $firstTh = $thead.find('tr').first().find('th').first();
    $firstTh.removeClass('valign-bottom valign-top').text('Bölge/Şube/Portföy');
    _bdHeaderNames = [];
    $thead.find('tr').last().find('th').each(function (i) {
        if (i === 0) return;
        _bdHeaderNames.push($.trim($(this).text()));
    });
    return { html: $thead.html() };
}

function _productivityValueCells(node) {
    return _yieldFields(node).slice(1).map(function (f) {
        return '<td>' + _yieldFmt(f, node[f]) + '</td>';
    }).join('');
}

function _productivityNameOf(node) {
    return node.ProductName || node.Description || node.RatioName || node.BranchName || '';
}

function _productivityPdf() {
    var data = window.ReportBreakdownData || [];
    var fields = _yieldFields((data && data[0]) || null);
    var columns = [{ header: 'Bölge/Şube/Portföy', key: 'c0', align: 'left' }];
    _bdHeaderNames.forEach(function (nm, i) { columns.push({ header: nm, key: 'c' + (i + 1) }); });
    return {
        title: ($('.report-detail-title').text() || 'Kırılım').trim(),
        infoLines: ['Bölge/Şube/Portföy Kırılımı'],
        columns: columns,
        rows: _yieldRows(data, fields),
        childrenKey: 'children',
        footerNote: 'Tablodaki değerler /1000 olarak verilmektedir.',
        filename: 'VerimRapor-Kirilim.pdf'
    };
}

$(function () {
    if (window.ReportDetail) {
        window.ReportDetail.registerProvider('productivity', {
            buildHead: function (tableKey, ctx) { return _productivityBuildHead(ctx); },
            valueCells: function (node, tableKey, ctx) { return _productivityValueCells(node); },
            nameOf: _productivityNameOf,
            legendHtml: function () {
                return $('#tableLegend .legend-colors').map(function () { return $(this).html(); }).get().join('');
            },
            pdf: function () { return _productivityPdf(); }
        });
    }

    $(document).on('click', '.detail-icon[data-table="productivity"]', function (e) {
        e.stopPropagation();
        var $icon = $(this);
        $('.report-detail-title').text($icon.data('product-name') || '');
        var $tbody = $icon.closest('tbody');
        var tbodySel = $tbody.attr('id') ? ('#' + $tbody.attr('id')) : '#dynamicTableBody';
        var ctx = _yieldBreakdownCtx[tbodySel] || {};
        if (window.ReportDetail) window.ReportDetail.setContext(ctx);
        loadProductivityBreakdown(ctx, $icon.data('product-id'));
        $('#reportDetailOverlay').addClass('active');
    });
});
