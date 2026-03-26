// ===== Tab State =====
var _productivityTabs = [];
var _activeToggleId = null;
var _activeTabId = null;
var _activeSubTabId = null;

// ===== Sort State =====
var _yieldSortBy = null;
var _yieldSortAsc = true;

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
function renderMainTabs() {
    var mainTabs = _productivityTabs.filter(function (t) { return t.TabLevel === 1; });
    var $toggle = $('#yieldToggle');
    $toggle.empty();

    mainTabs.forEach(function (tab, i) {
        if (i > 0) {
            $toggle.append('<div class="divider"></div>');
        }
        var activeClass = (i === 0) ? ' active' : '';
        $toggle.append('<button class="toggle-btn' + activeClass + '" data-tab-id="' + tab.TabId + '">' + tab.TabName + '</button>');
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
$(document).on('click', '#yieldToggle .toggle-btn', function () {
    $('#yieldToggle .toggle-btn').removeClass('active');
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

// ===== General Region Report =====
function loadGeneralRegionReport(regionCode) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityGeneralRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode || null,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            data.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);

            var hasChildren = {};
            if (hasExpandable) {
                data.forEach(function (item) {
                    if (item.ParentId) hasChildren[item.ParentId] = true;
                });
            }

            var html = '';
            data.forEach(function (row, i) {
                var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
                var depthClass = row.LevelNo > 0 ? ' sub-row depth-' + row.LevelNo : '';
                var expandClass = hasChildren[row.Id] ? ' expandable' : '';

                html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
                html += '<td class="col-index">' + (i + 1) + '</td>';

                if (hasExpandable) {
                    if (hasChildren[row.Id]) {
                        html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
                    } else {
                        html += '<td class="col-expand"></td>';
                    }
                }

                var indent = row.LevelNo > 0 ? '<span style="padding-left:' + (row.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + row.BranchName + '</span>' : row.BranchName;
                html += '<td class="col-text">' + indent + '</td>';
                html += '<td class="' + percentColor(row.FirstMonthRealizationRate) + '">' + formatPercent(row.FirstMonthRealizationRate) + '</td>';
                html += '<td class="' + percentColor(row.SecondMonthRealizationRate) + '">' + formatPercent(row.SecondMonthRealizationRate) + '</td>';
                html += '<td class="' + percentColor(row.ThirdMonthRealizationRate) + '">' + formatPercent(row.ThirdMonthRealizationRate) + '</td>';
                html += '<td class="' + percentColor(row.CorporateRate) + '">' + formatPercent(row.CorporateRate) + '</td>';
                html += '<td class="' + percentColor(row.CommercialRate) + '">' + formatPercent(row.CommercialRate) + '</td>';
                html += '<td class="' + percentColor(row.KbiRate) + '">' + formatPercent(row.KbiRate) + '</td>';
                html += '<td class="' + percentColor(row.ObiRate) + '">' + formatPercent(row.ObiRate) + '</td>';
                html += '<td class="' + percentColor(row.AgricultureRate) + '">' + formatPercent(row.AgricultureRate) + '</td>';
                html += '<td class="' + percentColor(row.MassRate) + '">' + formatPercent(row.MassRate) + '</td>';
                html += '<td class="' + percentColor(row.AffluentRate) + '">' + formatPercent(row.AffluentRate) + '</td>';
                html += '<td class="' + percentColor(row.PrivateBankingRate) + '">' + formatPercent(row.PrivateBankingRate) + '</td>';
                html += '</tr>';
            });
            $('#dynamicTableBody').html(html);
            updateProductivityStripes();
        }
    });
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

    $.ajax({
        url: '/ProductivityReport/GetProductivityReportTableHeaders',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            mainTabId: toggleId,
            midTabId: tabId || null,
            subTabId: subTabId || null,
            filterType: filterType,
            reportDate: _reportDate
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
    return ' <i class="sort-icon" data-sort-id="' + h.Id + '"><span class="sort-up">▲</span><span class="sort-down">▼</span></i>';
}

function renderDynamicHeaders(headers, hasExpandable) {
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

    if (!hasGroupHeaders) {
        var row = '<tr>';
        topHeaders.forEach(function (h, i) {
            row += '<th' + getClasses(h, false) + '>' + h.HeaderName + sortIcon(h) + '</th>';
            if (i === 0) row += expandTh;
        });
        row += '</tr>';
        $thead.append(row);
    } else {
        var row1 = '<tr>';
        var row2 = '<tr>';

        topHeaders.forEach(function (h, i) {
            var children = childMap[h.Id];
            if (children && children.length > 0) {
                row1 += '<th colspan="' + children.length + '" class="col-group-header">' + h.HeaderName + '</th>';
                children.forEach(function (c) {
                    row2 += '<th>' + c.HeaderName + sortIcon(c) + '</th>';
                });
            } else {
                row1 += '<th rowspan="2"' + getClasses(h, true) + '>' + h.HeaderName + sortIcon(h) + '</th>';
            }
            if (i === 0) row1 += expandTh;
        });

        row1 += '</tr>';
        row2 += '</tr>';
        $thead.append(row1);
        $thead.append(row2);
    }
}

// ===== Volume Region Report (Hacim — Bölge) =====
function loadVolumeRegionReport(regionCode, subTabId) {
    $.ajax({
        url: '/Home/GetProductivityVolumeRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            subTabId: subTabId || 0,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            // Data'da parentId olan satır var mı?
            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            // Header'ları expandable bilgisiyle tekrar render et
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderVolumeRegionTable(data);
        }
    });
}

function renderVolumeRegionTable(items) {
    var html = '';
    items.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

    var hasExpandable = items.some(function (item) { return item.ParentId !== null; });

    var hasChildren = {};
    if (hasExpandable) {
        items.forEach(function (item) {
            if (item.ParentId) hasChildren[item.ParentId] = true;
        });
    }

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item.LevelNo > 0 ? ' sub-row depth-' + item.LevelNo : '';
        var expandClass = hasChildren[item.Id] ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (hasChildren[item.Id]) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        // Product name with indent
        var indent = item.LevelNo > 0 ? '<span style="padding-left:' + (item.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td class="has-diff">' + item.RealizationRegionValue + formatDiff(item.RealizationRegionDiff) + '</td>';
        html += '<td class="has-diff">' + item.RealizationBankAverageValue + formatDiff(item.RealizationBankAverageDiff) + '</td>';
        html += '<td>' + item.TargetValue + '</td>';
        html += '<td>' + item.HgRate + '</td>';
        html += '<td class="has-diff">' + item.NetGrowthRegionValue + formatDiff(item.NetGrowthRegionDiff) + '</td>';
        html += '<td class="has-diff">' + item.NetGrowthBankAverageValue + formatDiff(item.NetGrowthBankAverageDiff) + '</td>';
        html += '<td class="has-diff">' + item.YtdRegionValue + formatDiff(item.YtdRegionDiff) + '</td>';
        html += '<td class="has-diff">' + item.YtdBankAverageValue + formatDiff(item.YtdBankAverageDiff) + '</td>';
        html += '<td class="has-diff">' + item.QtdRegionValue + formatDiff(item.QtdRegionDiff) + '</td>';
        html += '<td class="has-diff">' + item.QtdBankAverageValue + formatDiff(item.QtdBankAverageDiff) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Volume Branch Report (Hacim — Şube) =====
function loadVolumeBranchReport(branchCode, subTabId) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityVolumeBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            subTabId: subTabId || 0,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderVolumeBranchTable(data);
        }
    });
}

function renderVolumeBranchTable(items) {
    var html = '';
    items.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

    var hasExpandable = items.some(function (item) { return item.ParentId !== null; });

    var hasChildren = {};
    if (hasExpandable) {
        items.forEach(function (item) {
            if (item.ParentId) hasChildren[item.ParentId] = true;
        });
    }

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item.LevelNo > 0 ? ' sub-row depth-' + item.LevelNo : '';
        var expandClass = hasChildren[item.Id] ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (hasChildren[item.Id]) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item.LevelNo > 0 ? '<span style="padding-left:' + (item.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + item.RealizationBranchValue + '</td>';
        html += '<td class="has-diff">' + item.RealizationRegionAverageValue + formatDiff(item.RealizationRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.RealizationBankAverageValue + formatDiff(item.RealizationBankAverageValueDiff) + '</td>';
        html += '<td>' + item.TargetValue + '</td>';
        html += '<td>' + item.HgRate + '</td>';
        html += '<td>' + item.NetGrowthBranchValue + '</td>';
        html += '<td class="has-diff">' + item.NetGrowthRegionAverageValue + formatDiff(item.NetGrowthRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.NetGrowthBankAverageValue + formatDiff(item.NetGrowthBankAverageValueDiff) + '</td>';
        html += '<td>' + item.YtdBranchValue + '</td>';
        html += '<td class="has-diff">' + item.YtdRegionValue + formatDiff(item.YtdRegionValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.YtdBankValue + formatDiff(item.YtdBankValueDiff) + '</td>';
        html += '<td>' + item.QtdBranchValue + '</td>';
        html += '<td class="has-diff">' + item.QtdRegionValue + formatDiff(item.QtdRegionValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.QtdBankValue + formatDiff(item.QtdBankValueDiff) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Customer Region Report (Adet — Bölge) =====
function loadCountCustomerRegionReport(regionCode, subTabId) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityCountCustomerRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            subTabId: subTabId || 0,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderCountCustomerRegionTable(data);
        }
    });
}

function renderCountCustomerRegionTable(items) {
    var html = '';
    items.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

    var hasExpandable = items.some(function (item) { return item.ParentId !== null; });

    var hasChildren = {};
    if (hasExpandable) {
        items.forEach(function (item) {
            if (item.ParentId) hasChildren[item.ParentId] = true;
        });
    }

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item.LevelNo > 0 ? ' sub-row depth-' + item.LevelNo : '';
        var expandClass = hasChildren[item.Id] ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (hasChildren[item.Id]) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item.LevelNo > 0 ? '<span style="padding-left:' + (item.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td class="has-diff">' + item.RealizationRegion + formatDiff(item.RealizationRegionDiff) + '</td>';
        html += '<td class="has-diff">' + item.RealizationBankAverage + formatDiff(item.RealizationBankAverageDiff) + '</td>';
        html += '<td class="has-diff">' + item.YtdChangeRegion + formatDiff(item.YtdChangeRegionDiff) + '</td>';
        html += '<td class="has-diff">' + item.YtdChangeBankAverage + formatDiff(item.YtdChangeBankAverageDiff) + '</td>';
        html += '<td class="has-diff">' + item.QtdChangeRegion + formatDiff(item.QtdChangeRegionDiff) + '</td>';
        html += '<td class="has-diff">' + item.QtdChangeBankAverage + formatDiff(item.QtdChangeBankAverageDiff) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Customer Branch Report (Adet — Müşteri — Şube) =====
function loadCountCustomerBranchReport(branchCode, subTabId) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityCountCustomerBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            subTabId: subTabId || 0,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderCountCustomerBranchTable(data);
        }
    });
}

function renderCountCustomerBranchTable(items) {
    var html = '';
    items.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

    var hasExpandable = items.some(function (item) { return item.ParentId !== null; });

    var hasChildren = {};
    if (hasExpandable) {
        items.forEach(function (item) {
            if (item.ParentId) hasChildren[item.ParentId] = true;
        });
    }

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item.LevelNo > 0 ? ' sub-row depth-' + item.LevelNo : '';
        var expandClass = hasChildren[item.Id] ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (hasChildren[item.Id]) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item.LevelNo > 0 ? '<span style="padding-left:' + (item.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + item.RealizationBranchValue + '</td>';
        html += '<td class="has-diff">' + item.RealizationRegionAverageValue + formatDiff(item.RealizationRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.RealizationBankAverageValue + formatDiff(item.RealizationBankAverageValueDiff) + '</td>';
        html += '<td>' + item.YtdNominalChangeBranchValue + '</td>';
        html += '<td class="has-diff">' + item.YtdNominalChangeRegionAverageValue + formatDiff(item.YtdNominalChangeRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.YtdNominalChangeBankAverageValue + formatDiff(item.YtdNominalChangeBankAverageValueDiff) + '</td>';
        html += '<td>' + item.QtdNominalChangeBranchValue + '</td>';
        html += '<td class="has-diff">' + item.QtdNominalChangeRegionAverageValue + formatDiff(item.QtdNominalChangeRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.QtdNominalChangeBankAverageValue + formatDiff(item.QtdNominalChangeBankAverageValueDiff) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Card/POS Branch Report (Adet — Kredi Kartı / POS — Şube) =====
function loadCountCardPosBranchReport(branchCode, tabId) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityCountCardPosBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            tabId: tabId,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderCountCardPosBranchTable(data);
        }
    });
}

function renderCountCardPosBranchTable(items) {
    var html = '';
    items.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

    var hasExpandable = items.some(function (item) { return item.ParentId !== null; });

    var hasChildren = {};
    if (hasExpandable) {
        items.forEach(function (item) {
            if (item.ParentId) hasChildren[item.ParentId] = true;
        });
    }

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item.LevelNo > 0 ? ' sub-row depth-' + item.LevelNo : '';
        var expandClass = hasChildren[item.Id] ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (hasChildren[item.Id]) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item.LevelNo > 0 ? '<span style="padding-left:' + (item.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + item.CurrentPeriodBranchValue + '</td>';
        html += '<td class="has-diff">' + item.CurrentPeriodRegionAverageValue + formatDiff(item.CurrentPeriodRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.CurrentPeriodBankAverageValue + formatDiff(item.CurrentPeriodBankAverageValueDiff) + '</td>';
        html += '<td>' + item.ThreeMonthHgBranchValue + '</td>';
        html += '<td class="has-diff">' + item.ThreeMonthHgRegionAverageValue + formatDiff(item.ThreeMonthHgRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.ThreeMonthHgBankAverageValue + formatDiff(item.ThreeMonthHgBankAverageValueDiff) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Card/POS Region Report (Adet — Kredi Kartı / POS — Bölge) =====
function loadCountCardPosRegionReport(regionCode, tabId) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityCountCardPosRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            tabId: tabId,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderCountCardPosRegionTable(data);
        }
    });
}

function renderCountCardPosRegionTable(items) {
    var html = '';
    items.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

    var hasExpandable = items.some(function (item) { return item.ParentId !== null; });

    var hasChildren = {};
    if (hasExpandable) {
        items.forEach(function (item) {
            if (item.ParentId) hasChildren[item.ParentId] = true;
        });
    }

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item.LevelNo > 0 ? ' sub-row depth-' + item.LevelNo : '';
        var expandClass = hasChildren[item.Id] ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (hasChildren[item.Id]) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item.LevelNo > 0 ? '<span style="padding-left:' + (item.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.ProductName + '</span>' : item.ProductName;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + item.CurrentMonthRegionValue + '</td>';
        html += '<td class="has-diff">' + item.CurrentMonthBankAverage + formatDiff(item.CurrentMonthBankAverageDiff) + '</td>';
        html += '<td>' + item.ThreeMonthHgRegion + '</td>';
        html += '<td class="has-diff">' + item.ThreeMonthHgBankAverage + formatDiff(item.ThreeMonthHgBankAverageDiff) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Profit Total Region Report (Karlılık — Üst Tablo — Bölge) =====
// Header'ları loadTableHeaders'dan alır, body dynamicTableBody'ye render eder
function loadProfitTotalRegionReport(regionCode) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityProfitTotalRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderProfitTotalRegionTable(data);
        }
    });
}

function renderProfitTotalRegionTable(items) {
    var html = '';
    items.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

    var hasExpandable = items.some(function (item) { return item.ParentId !== null; });

    var hasChildren = {};
    if (hasExpandable) {
        items.forEach(function (item) {
            if (item.ParentId) hasChildren[item.ParentId] = true;
        });
    }

    // Filter top-level items to find last one for special styling
    var topLevelItems = items.filter(function (item) { return item.LevelNo === 0; });
    var lastTopLevelId = topLevelItems.length > 0 ? topLevelItems[topLevelItems.length - 1].Id : null;

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item.LevelNo > 0 ? ' sub-row depth-' + item.LevelNo : '';
        var expandClass = hasChildren[item.Id] ? ' expandable' : '';
        var lastRowClass = (item.Id === lastTopLevelId) ? ' profit-total-last' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + lastRowClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (hasChildren[item.Id]) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item.LevelNo > 0 ? '<span style="padding-left:' + (item.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.Description + '</span>' : item.Description;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + item.TargetValue + '</td>';
        html += '<td class="has-diff">' + item.RealizationRegionValue + formatDiff(item.RealizationRegionValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.RealizationBankAverageValue + formatDiff(item.RealizationBankAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.HgRegionValue + formatDiff(item.HgRegionValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.HgBankAverageValue + formatDiff(item.HgBankAverageValueDiff) + '</td>';
        html += '<td>' + item.RetailValue + '</td>';
        html += '<td>' + item.KobiValue + '</td>';
        html += '<td>' + item.AgricultureValue + '</td>';
        html += '<td class="has-diff">' + item.CommercialValue + formatDiff(item.CommercialValueDiff) + '</td>';
        html += '<td>' + item.PartnerValue + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Profit Ratio Region Report (Karlılık — Alt Tablo — Bölge) =====
// Header'ları statik renderProfitRatioRegionHeaders ile, body dynamicTableBody2'ye render eder
function loadProfitRatioRegionReport(regionCode) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityProfitRatioRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            renderProfitRatioRegionHeaders(hasExpandable);
            renderProfitRatioRegionTable(data);
            $('#dynamicTableContainer2').show();
        }
    });
}

function renderProfitRatioRegionHeaders(hasExpandable) {
    var $thead = $('#dynamicTableHead2');
    $thead.empty();

    var expandTh = hasExpandable ? '<th class="col-expand"></th>' : '';

    var row = '<tr>';
    row += '<th class="col-index">#</th>';
    row += expandTh;
    row += '<th>Oran Adı</th>';
    row += '<th>Hedef</th>';
    row += '<th>Bölge</th>';
    row += '<th>Banka</th>';
    row += '<th>Bireysel</th>';
    row += '<th>KOBİ</th>';
    row += '<th>Tarım</th>';
    row += '<th>Ticari</th>';
    row += '</tr>';

    $thead.append(row);
}

function renderProfitRatioRegionTable(items) {
    var html = '';
    items.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

    var hasExpandable = items.some(function (item) { return item.ParentId !== null; });

    var hasChildren = {};
    if (hasExpandable) {
        items.forEach(function (item) {
            if (item.ParentId) hasChildren[item.ParentId] = true;
        });
    }

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item.LevelNo > 0 ? ' sub-row depth-' + item.LevelNo : '';
        var expandClass = hasChildren[item.Id] ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (hasChildren[item.Id]) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item.LevelNo > 0 ? '<span style="padding-left:' + (item.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.RatioName + '</span>' : item.RatioName;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + item.TargetValue + '</td>';
        html += '<td class="has-diff">' + item.RegionValue + formatDiff(item.RegionValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.BankValue + formatDiff(item.BankValueDiff) + '</td>';
        html += '<td>' + item.RetailValue + '</td>';
        html += '<td>' + item.KobiValue + '</td>';
        html += '<td class="has-diff">' + item.AgricultureValue + formatDiff(item.AgricultureValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.CommercialValue + formatDiff(item.CommercialValueDiff) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody2').html(html);
    updateProductivityStripes2();
}

// ===== Profit Ratio Branch Report (Karlılık — Alt Tablo — Şube) =====
function loadProfitRatioBranchReport(branchCode) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityProfitRatioBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            renderProfitRatioRegionHeaders(hasExpandable);
            renderProfitRatioBranchTable(data);
            $('#dynamicTableContainer2').show();
        }
    });
}

function renderProfitRatioBranchTable(items) {
    var html = '';
    items.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

    var hasExpandable = items.some(function (item) { return item.ParentId !== null; });

    var hasChildren = {};
    if (hasExpandable) {
        items.forEach(function (item) {
            if (item.ParentId) hasChildren[item.ParentId] = true;
        });
    }

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item.LevelNo > 0 ? ' sub-row depth-' + item.LevelNo : '';
        var expandClass = hasChildren[item.Id] ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (hasChildren[item.Id]) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item.LevelNo > 0 ? '<span style="padding-left:' + (item.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.RatioName + '</span>' : item.RatioName;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + item.TargetValue + '</td>';
        html += '<td class="has-diff">' + item.RegionValue + formatDiff(item.RegionValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.BankValue + formatDiff(item.BankValueDiff) + '</td>';
        html += '<td>' + item.RetailValue + '</td>';
        html += '<td>' + item.KobiValue + '</td>';
        html += '<td class="has-diff">' + item.AgricultureValue + formatDiff(item.AgricultureValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.CommercialValue + formatDiff(item.CommercialValueDiff) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody2').html(html);
    updateProductivityStripes2();
}

// ===== Profit Total Branch Report (Karlılık — Üst Tablo — Şube) =====
function loadProfitTotalBranchReport(branchCode) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityProfitTotalBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderProfitTotalBranchTable(data);
        }
    });
}

function renderProfitTotalBranchTable(items) {
    var html = '';
    items.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

    var hasExpandable = items.some(function (item) { return item.ParentId !== null; });

    var hasChildren = {};
    if (hasExpandable) {
        items.forEach(function (item) {
            if (item.ParentId) hasChildren[item.ParentId] = true;
        });
    }

    var topLevelItems = items.filter(function (item) { return item.LevelNo === 0; });
    var lastTopLevelId = topLevelItems.length > 0 ? topLevelItems[topLevelItems.length - 1].Id : null;

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item.LevelNo > 0 ? ' sub-row depth-' + item.LevelNo : '';
        var expandClass = hasChildren[item.Id] ? ' expandable' : '';
        var lastRowClass = (item.Id === lastTopLevelId) ? ' profit-total-last' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + lastRowClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (hasChildren[item.Id]) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item.LevelNo > 0 ? '<span style="padding-left:' + (item.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.Description + '</span>' : item.Description;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + item.TargetValue + '</td>';
        html += '<td>' + item.RealizationBranchValue + '</td>';
        html += '<td class="has-diff">' + item.RealizationRegionAverageValue + formatDiff(item.RealizationRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.RealizationBankAverageValue + formatDiff(item.RealizationBankAverageValueDiff) + '</td>';
        html += '<td class="has-diff ' + percentColor(item.HgBranchValue) + '">' + formatPercent(item.HgBranchValue) + formatDiff(item.HgBranchValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.HgRegionAverageValue + formatDiff(item.HgRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.HgBankAverageValue + formatDiff(item.HgBankAverageValueDiff) + '</td>';
        html += '<td>' + item.RetailValue + '</td>';
        html += '<td>' + item.KobiValue + '</td>';
        html += '<td>' + item.AgricultureValue + '</td>';
        html += '<td class="has-diff">' + item.CommercialValue + formatDiff(item.CommercialValueDiff) + '</td>';
        html += '<td>' + item.PartnerValue + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Profit Spread Management Region Report (Karlılık — Spread Yönetimi — Bölge) =====
function loadProfitSpreadManagementRegionReport(regionCode) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityProfitSpreadManagementRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderProfitSpreadManagementRegionTable(data);
        }
    });
}

function renderProfitSpreadManagementRegionTable(items) {
    var html = '';
    items.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

    var hasExpandable = items.some(function (item) { return item.ParentId !== null; });

    var hasChildren = {};
    if (hasExpandable) {
        items.forEach(function (item) {
            if (item.ParentId) hasChildren[item.ParentId] = true;
        });
    }

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item.LevelNo > 0 ? ' sub-row depth-' + item.LevelNo : '';
        var expandClass = hasChildren[item.Id] ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (hasChildren[item.Id]) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item.LevelNo > 0 ? '<span style="padding-left:' + (item.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.Description + '</span>' : item.Description;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + item.SpreadValue + '</td>';
        html += '<td>' + item.RatioRegionValue + '</td>';
        html += '<td>' + item.RatioBankAverageValue + '</td>';
        html += '<td>' + item.NetReturnRegionValue + '</td>';
        html += '<td>' + item.NetReturnBankAverageValue + '</td>';
        html += '<td>' + item.NetReturnHgRegionValue + '</td>';
        html += '<td>' + item.NetReturnHgBankAverageValue + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Profit Spread Management Branch Report (Karlılık — Spread Yönetimi — Şube) =====
function loadProfitSpreadManagementBranchReport(branchCode) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityProfitSpreadManagementBranchReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            branchCode: branchCode,
            reportDate: _reportDate,
            sortBy: _yieldSortBy,
            isAscending: _yieldSortAsc
        }),
        success: function (data) {
            var hasExpandable = data.some(function (item) { return item.ParentId !== null; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderProfitSpreadManagementBranchTable(data);
        }
    });
}

function renderProfitSpreadManagementBranchTable(items) {
    var html = '';
    items.sort(function (a, b) { return a.SortOrder - b.SortOrder; });

    var hasExpandable = items.some(function (item) { return item.ParentId !== null; });

    var hasChildren = {};
    if (hasExpandable) {
        items.forEach(function (item) {
            if (item.ParentId) hasChildren[item.ParentId] = true;
        });
    }

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item.LevelNo > 0 ? ' sub-row depth-' + item.LevelNo : '';
        var expandClass = hasChildren[item.Id] ? ' expandable' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (hasChildren[item.Id]) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item.LevelNo > 0 ? '<span style="padding-left:' + (item.LevelNo * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.Description + '</span>' : item.Description;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + item.SpreadValue + '</td>';
        html += '<td>' + item.RatioBranchValue + '</td>';
        html += '<td class="has-diff">' + item.RatioRegionAverageValue + formatDiff(item.RatioRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.RatioBankAverageValue + formatDiff(item.RatioBankAverageValueDiff) + '</td>';
        html += '<td>' + item.NetReturnBranchValue + '</td>';
        html += '<td class="has-diff">' + item.NetReturnRegionAverageValue + formatDiff(item.NetReturnRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.NetReturnBankAverageValue + formatDiff(item.NetReturnBankAverageValueDiff) + '</td>';
        html += '<td>' + item.NetReturnHgBranchValue + '</td>';
        html += '<td class="has-diff">' + item.NetReturnHgRegionAverageValue + formatDiff(item.NetReturnHgRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.NetReturnHgBankAverageValue + formatDiff(item.NetReturnHgBankAverageValueDiff) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

function updateProductivityStripes2() {
    var visibleIndex = 0;
    var $lastVisible = null;
    $('#dynamicTable2 tbody tr').each(function () {
        var $tr = $(this);
        $tr.removeClass('stripe-odd stripe-even last-visible-row');
        if ($tr.hasClass('sub-row') && !$tr.hasClass('visible')) {
            return;
        }
        visibleIndex++;
        $tr.addClass(visibleIndex % 2 === 1 ? 'stripe-odd' : 'stripe-even');
        $tr.find('td.col-index').text(visibleIndex);
        $lastVisible = $tr;
    });
    if ($lastVisible) $lastVisible.addClass('last-visible-row');
    reapplySortVisual($('#dynamicTable2'));
}

function updateProductivityStripes() {
    var visibleIndex = 0;
    var $lastVisible = null;
    $('#dynamicTable tbody tr').each(function () {
        var $tr = $(this);
        $tr.removeClass('stripe-odd stripe-even last-visible-row');
        if ($tr.hasClass('sub-row') && !$tr.hasClass('visible')) {
            return;
        }
        visibleIndex++;
        $tr.addClass(visibleIndex % 2 === 1 ? 'stripe-odd' : 'stripe-even');
        $tr.find('td.col-index').text(visibleIndex);
        $lastVisible = $tr;
    });
    if ($lastVisible) $lastVisible.addClass('last-visible-row');
    reapplySortVisual($('#dynamicTable'));
}

function reapplySortVisual($table) {
    $table.find('.sort-icon').removeClass('asc desc');
    if (_yieldSortBy !== null) {
        $table.find('.sort-icon[data-sort-id="' + _yieldSortBy + '"]').addClass(_yieldSortAsc ? 'asc' : 'desc');
    }
}

function formatDiff(val) {
    if (val == null) return '<div class="diff-value">&nbsp;</div>';
    var cls = val < 0 ? 'negative' : (val > 0 ? 'positive' : '');
    var prefix = val > 0 ? '+' : '';
    return '<div class="diff-value ' + cls + '">' + prefix + val + '</div>';
}
