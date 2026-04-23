// ===== Yield Table Skeleton + Loading =====
function showYieldTableLoading() {
    // Tablo container'ları gizle, skeleton göster
    $('#dynamicTableContainer').hide();
    $('#dynamicTableContainer2').hide();
    $('#dynamicTableBody').html('');
    $('#dynamicTableBody2').html('');
    $('#pageSkeleton').show();
    $('body').loading({
        stoppable: false,
        message: '<div><img src="/assets/img/loader.gif" style="width: 60px;" /></div>'
    });
}

function hideYieldTableLoading() {
    $('#pageSkeleton').hide();
    $('#dynamicTableContainer').show();
    $('body').loading('stop');
}

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

// Response'tan asıl data array'ini çıkar (ilk array property veya direkt array)
function extractResponseData(response) {
    if (Array.isArray(response)) return response;
    for (var key in response) {
        if (response.hasOwnProperty(key) && Array.isArray(response[key])) {
            return response[key];
        }
    }
    return response;
}

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
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var flatData = flattenRows(data, 0);
            var hasExpandable = flatData.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);

            var html = '';
            flatData.forEach(function (row, i) {
                var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
                var depthClass = row._depth > 0 ? ' sub-row depth-' + row._depth : '';
                var expandClass = row._hasChildren ? ' expandable' : '';

                html += '<tr class="table-row ' + cls + depthClass + expandClass + '">';
                html += '<td class="col-index">' + (i + 1) + '</td>';

                if (hasExpandable) {
                    if (row._hasChildren) {
                        html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
                    } else {
                        html += '<td class="col-expand"></td>';
                    }
                }

                var matchBranch = null;
                if (typeof _branchFilters !== 'undefined' && _branchFilters) {
                    for (var bi = 0; bi < _branchFilters.length; bi++) {
                        if (_branchFilters[bi].Name === row.BranchName) {
                            matchBranch = _branchFilters[bi];
                            break;
                        }
                    }
                }
                var nameHtml = matchBranch
                    ? '<span class="branch-link" data-branch-code="' + matchBranch.Code + '">' + row.BranchName + '</span>'
                    : row.BranchName;
                var indent = row._depth > 0 ? '<span style="padding-left:' + (row._depth * 16) + 'px">' + nameHtml + '</span>' : nameHtml;
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

        var firstGroupFound = false;
        topHeaders.forEach(function (h, i) {
            var children = childMap[h.Id];
            if (children && children.length > 0) {
                var isFirstGroup = !firstGroupFound;
                if (isFirstGroup) firstGroupFound = true;
                var groupCls = isFirstGroup ? 'col-group-header selected' : 'col-group-header';
                row1 += '<th colspan="' + children.length + '" class="' + groupCls + '">' + h.HeaderName + '</th>';
                children.forEach(function (c, cIdx) {
                    var childCls = '';
                    if (isFirstGroup) {
                        if (cIdx === 0) childCls = ' class="col-selected-first"';
                        else if (cIdx === children.length - 1) childCls = ' class="col-selected-last"';
                        else childCls = ' class="col-selected-mid"';
                    }
                    row2 += '<th' + childCls + '>' + c.HeaderName + sortIcon(c) + '</th>';
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

        // DOM'a eklendikten sonra ilk col-group-header'ı selected yap
        var $groups = $thead.find('.col-group-header');
        $groups.removeClass('selected');
        $groups.first().addClass('selected');

        // İlk grubun altındaki child th'lere col-selected-first / col-selected-last ekle
        var $row2ths = $thead.find('tr:last th');
        var firstColCount = parseInt($groups.first().attr('colspan')) || 1;
        $row2ths.removeClass('col-selected-first col-selected-mid col-selected-last');
        $row2ths.eq(0).addClass('col-selected-first');
        for (var m = 1; m < firstColCount - 1; m++) {
            $row2ths.eq(m).addClass('col-selected-mid');
        }
        $row2ths.eq(firstColCount - 1).addClass('col-selected-last');
    }
}

// ===== Volume Region Report (Hacim — Bölge) =====
function loadVolumeRegionReport(regionCode, subTabId) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityVolumeRegionReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            regionCode: regionCode,
            subTabId: subTabId || 0,
            reportDate: _reportDate,
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderVolumeRegionTable(items);
        }
    });
}

function renderVolumeRegionTable(items) {
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
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + formatNumber(item.RealizationRegionValue) +'</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverageValue) + formatDiff(item.RealizationBankAverageDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.TargetValue) + '</td>';
        html += '<td>' + item.HgRate + '</td>';
        html += '<td>' + formatNumber(item.NetGrowthRegionValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.NetGrowthBankAverageValue) + formatDiff(item.NetGrowthBankAverageDiff, true) + '</td>';
        html += '<td>' + item.YtdRegionValue + '</td>';
        html += '<td class="has-diff">' + item.YtdBankAverageValue + formatDiff(item.YtdBankAverageDiff) + '</td>';
        html += '<td>' + item.QtdRegionValue + '</td>';
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
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderVolumeBranchTable(items);
        }
    });
}

function renderVolumeBranchTable(items) {
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
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + formatNumber(item.RealizationBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationRegionAverageValue) + formatDiff(item.RealizationRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverageValue) + formatDiff(item.RealizationBankAverageValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.TargetValue) + '</td>';
        html += '<td>' + item.HgRate + '</td>';
        html += '<td>' + formatNumber(item.NetGrowthBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.NetGrowthRegionAverageValue) + formatDiff(item.NetGrowthRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.NetGrowthBankAverageValue) + formatDiff(item.NetGrowthBankAverageValueDiff, true) + '</td>';
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
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
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
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + formatNumber(item.RealizationRegion) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverage) + formatDiff(item.RealizationBankAverageDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.YtdChangeRegion) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.YtdChangeBankAverage) + formatDiff(item.YtdChangeBankAverageDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.QtdChangeRegion) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.QtdChangeBankAverage) + formatDiff(item.QtdChangeBankAverageDiff, true) + '</td>';
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
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
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
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + formatNumber(item.RealizationBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationRegionAverageValue) + formatDiff(item.RealizationRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverageValue) + formatDiff(item.RealizationBankAverageValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.YtdNominalChangeBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.YtdNominalChangeRegionAverageValue) + formatDiff(item.YtdNominalChangeRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.YtdNominalChangeBankAverageValue) + formatDiff(item.YtdNominalChangeBankAverageValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.QtdNominalChangeBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.QtdNominalChangeRegionAverageValue) + formatDiff(item.QtdNominalChangeRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.QtdNominalChangeBankAverageValue) + formatDiff(item.QtdNominalChangeBankAverageValueDiff, true) + '</td>';
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
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
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
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + formatNumber(item.CurrentPeriodBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.CurrentPeriodRegionAverageValue) + formatDiff(item.CurrentPeriodRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.CurrentPeriodBankAverageValue) + formatDiff(item.CurrentPeriodBankAverageValueDiff, true) + '</td>';
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
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
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
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + formatNumber(item.CurrentMonthRegionValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.CurrentMonthBankAverage) + formatDiff(item.CurrentMonthBankAverageDiff, true) + '</td>';
        html += '<td>' + item.ThreeMonthHgRegion + '</td>';
        html += '<td class="has-diff">' + item.ThreeMonthHgBankAverage + formatDiff(item.ThreeMonthHgBankAverageDiff) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody').html(html);
    updateProductivityStripes();
}

// ===== Count Card/POS Ratio Region Report (Adet — Oran Tablosu — Bölge) =====
function loadCountCardPosRatioRegionReport(regionCode, tabId) {
    // Load headers first, then data
    $.ajax({
        url: '/ProductivityReport/GetProductivityCountCardPosRatioRegionReportTableHeaders',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            tabId: tabId,
            reportDate: _reportDate
        }),
        success: function (headers) {
            renderCountCardPosRatioRegionHeaders(headers);

            $.ajax({
                url: '/ProductivityReport/GetProductivityCountCardPosRatioRegionReport',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    sessionId: '1',
                    regionCode: regionCode,
                    tabId: tabId,
                    reportDate: _reportDate
                }),
                success: function (response) {
                    var data = extractResponseData(response);
                    renderCountCardPosRatioRegionTable(data);
                    $('#dynamicTableContainer2').show();
                }
            });
        }
    });
}

function renderCountCardPosRatioRegionHeaders(h) {
    var $thead = $('#dynamicTableHead2');
    $thead.empty();

    var row = '<tr>';
    row += '<th class="col-index">' + h.RowNumberTitle + '</th>';
    row += '<th class="col-text">' + h.RatioNameTitle + '</th>';
    row += '<th>' + h.PreviousQuarterRegionTitle + '</th>';
    row += '<th>' + h.CurrentRegionTitle + '</th>';
    row += '<th>' + h.CurrentBankAverageTitle + '</th>';
    row += '</tr>';

    $thead.append(row);
}

function renderCountCardPosRatioRegionTable(items) {
    var html = '';

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var isPercent = item.RatioName.indexOf('%') !== -1;
        var fmt = function (v) { return isPercent ? formatPercent(v) : formatNumber(v) };

        html += '<tr class="table-row ' + cls + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';
        html += '<td class="col-text">' + item.RatioName + '</td>';
        html += '<td>' + fmt(item.PreviousQuarterRegionValue) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CurrentRegionValue) + formatDiff(item.CurrentRegionDiff, !isPercent) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CurrentBankAverageValue) + formatDiff(item.CurrentBankAverageDiff, !isPercent) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody2').html(html);
    updateProductivityStripes2();
}

// ===== Count Card/POS Ratio Branch Report (Adet — Oran Tablosu — Şube) =====
function loadCountCardPosRatioBranchReport(branchCode, tabId) {
    $.ajax({
        url: '/ProductivityReport/GetProductivityCountCardPosRatioBranchReportTableHeaders',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            sessionId: '1',
            tabId: tabId,
            reportDate: _reportDate
        }),
        success: function (headers) {
            renderCountCardPosRatioBranchHeaders(headers);

            $.ajax({
                url: '/ProductivityReport/GetProductivityCountCardPosRatioBranchReport',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    sessionId: '1',
                    branchCode: branchCode,
                    tabId: tabId,
                    reportDate: _reportDate
                }),
                success: function (response) {
                    var data = extractResponseData(response);
                    renderCountCardPosRatioBranchTable(data);
                    $('#dynamicTableContainer2').show();
                }
            });
        }
    });
}

function renderCountCardPosRatioBranchHeaders(h) {
    var $thead = $('#dynamicTableHead2');
    $thead.empty();

    var row = '<tr>';
    row += '<th class="col-index">' + h.RowNumberTitle + '</th>';
    row += '<th class="col-text">' + h.RatioNameTitle + '</th>';
    row += '<th>' + h.PreviousQuarterBranchTitle + '</th>';
    row += '<th>' + h.CurrentBranchTitle + '</th>';
    row += '<th>' + h.CurrentRegionAverageTitle + '</th>';
    row += '<th>' + h.CurrentBankAverageTitle + '</th>';
    row += '</tr>';

    $thead.append(row);
}

function renderCountCardPosRatioBranchTable(items) {
    var html = '';

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var isPercent = item.RatioName.indexOf('%') !== -1;
        var fmt = function (v) { return isPercent ? formatPercent(v) : formatNumber(v) };

        html += '<tr class="table-row ' + cls + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';
        html += '<td class="col-text">' + item.RatioName + '</td>';
        html += '<td>' + fmt(item.PreviousQuarterBranchValue) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CurrentBranchValue) + formatDiff(item.CurrentBranchValueDiff, !isPercent) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CurrentRegionAverageValue) + formatDiff(item.CurrentRegionAverageValueDiff, !isPercent) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CurrentBankAverageValue) + formatDiff(item.CurrentBankAverageValueDiff, !isPercent) + '</td>';
        html += '</tr>';
    });

    $('#dynamicTableBody2').html(html);
    updateProductivityStripes2();
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
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
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
        var lastRowClass = (item.Id === lastTopLevelId) ? ' profit-total-last' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + lastRowClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.Description + '</span>' : item.Description;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + formatNumber(item.TargetValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationRegionValue) + formatDiff(item.RealizationRegionValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverageValue) + formatDiff(item.RealizationBankAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + item.HgRegionValue + formatDiff(item.HgRegionValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.HgBankAverageValue + formatDiff(item.HgBankAverageValueDiff) + '</td>';
        html += '<td>' + formatNumber(item.RetailValue) + '</td>';
        html += '<td>' + formatNumber(item.KobiValue) + '</td>';
        html += '<td>' + formatNumber(item.AgricultureValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.CommercialValue) + formatDiff(item.CommercialValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.PartnerValue) + '</td>';
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
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderProfitRatioRegionHeaders(hasExpandable);
            renderProfitRatioRegionTable(items);
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
    row += '<th class="col-text">Oran Adı</th>';
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
    var hasExpandable = items.some(function (item) { return item._hasChildren; });

    items.forEach(function (item, i) {
        var cls = (i % 2 === 0) ? 'stripe-odd' : 'stripe-even';
        var depthClass = item._depth > 0 ? ' sub-row depth-' + item._depth : '';
        var expandClass = item._hasChildren ? ' expandable' : '';
        var isPercent = item.RatioName.indexOf('%') !== -1;
        var fmt = function (v) { return isPercent ? formatPercent(v) : formatNumber(v) };

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
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + fmt(item.TargetValue) + '</td>';
        html += '<td class="has-diff">' + fmt(item.RegionValue) + formatDiff(item.RegionValueDiff, !isPercent) + '</td>';
        html += '<td class="has-diff">' + fmt(item.BankValue) + formatDiff(item.BankValueDiff, !isPercent) + '</td>';
        html += '<td>' + fmt(item.RetailValue) + '</td>';
        html += '<td>' + fmt(item.KobiValue) + '</td>';
        html += '<td class="has-diff">' + fmt(item.AgricultureValue) + formatDiff(item.AgricultureValueDiff, !isPercent) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CommercialValue) + formatDiff(item.CommercialValueDiff, !isPercent) + '</td>';
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
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderProfitRatioRegionHeaders(hasExpandable);
            renderProfitRatioBranchTable(items);
            $('#dynamicTableContainer2').show();
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
        var fmt = function (v) { return isPercent ? formatPercent(v) : formatNumber(v) };

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
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + fmt(item.TargetValue) + '</td>';
        html += '<td class="has-diff">' + fmt(item.RegionValue) + formatDiff(item.RegionValueDiff, !isPercent) + '</td>';
        html += '<td class="has-diff">' + fmt(item.BankValue) + formatDiff(item.BankValueDiff, !isPercent) + '</td>';
        html += '<td>' + fmt(item.RetailValue) + '</td>';
        html += '<td>' + fmt(item.KobiValue) + '</td>';
        html += '<td class="has-diff">' + fmt(item.AgricultureValue) + formatDiff(item.AgricultureValueDiff, !isPercent) + '</td>';
        html += '<td class="has-diff">' + fmt(item.CommercialValue) + formatDiff(item.CommercialValueDiff, !isPercent) + '</td>';
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
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
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
        var lastRowClass = (item.Id === lastTopLevelId) ? ' profit-total-last' : '';

        html += '<tr class="table-row ' + cls + depthClass + expandClass + lastRowClass + '">';
        html += '<td class="col-index">' + (i + 1) + '</td>';

        if (hasExpandable) {
            if (item._hasChildren) {
                html += '<td class="col-expand"><span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span></td>';
            } else {
                html += '<td class="col-expand"></td>';
            }
        }

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px">' + item.Description + '</span>' : item.Description;
        html += '<td class="col-text">' + indent + '</td>';

        html += '<td>' + formatNumber(item.TargetValue) + '</td>';
        html += '<td>' + formatNumber(item.RealizationBranchValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationRegionAverageValue) + formatDiff(item.RealizationRegionAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.RealizationBankAverageValue) + formatDiff(item.RealizationBankAverageValueDiff, true) + '</td>';
        html += '<td class="has-diff ' + percentColor(item.HgBranchValue) + '">' + formatPercent(item.HgBranchValue) + formatDiff(item.HgBranchValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.HgRegionAverageValue + formatDiff(item.HgRegionAverageValueDiff) + '</td>';
        html += '<td class="has-diff">' + item.HgBankAverageValue + formatDiff(item.HgBankAverageValueDiff) + '</td>';
        html += '<td>' + formatNumber(item.RetailValue) + '</td>';
        html += '<td>' + formatNumber(item.KobiValue) + '</td>';
        html += '<td>' + formatNumber(item.AgricultureValue) + '</td>';
        html += '<td class="has-diff">' + formatNumber(item.CommercialValue) + formatDiff(item.CommercialValueDiff, true) + '</td>';
        html += '<td>' + formatNumber(item.PartnerValue) + '</td>';
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
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderProfitSpreadManagementRegionTable(items);
        }
    });
}

function renderProfitSpreadManagementRegionTable(items) {
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

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.Description + '</span>' : item.Description;
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
            sortBy: _yieldSortBy !== null ? _yieldSortBy : 0,
            isAscending: _yieldSortBy !== null ? _yieldSortAsc : true
        }),
        success: function (response) {
            var data = extractResponseData(response);
            var items = flattenRows(data, 0);
            var hasExpandable = items.some(function (item) { return item._hasChildren; });
            renderDynamicHeaders(_cachedHeaders, hasExpandable);
            renderProfitSpreadManagementBranchTable(items);
        }
    });
}

function renderProfitSpreadManagementBranchTable(items) {
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

        var indent = item._depth > 0 ? '<span style="padding-left:' + (item._depth * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + item.Description + '</span>' : item.Description;
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
            $tr.find('td.col-text .sub-index').remove();
        } else {
            subCounters[mainIndex] = (subCounters[mainIndex] || 0) + 1;
            var subLabel = mainIndex + '.' + subCounters[mainIndex];
            $tr.find('td.col-index').text('');
            var $colText = $tr.find('td.col-text');
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
    hideYieldTableLoading();
}

function updateProductivityStripes() {
    applyProductivityStripes($('#dynamicTable'));
    applyFirstGroupSelected($('#dynamicTable'));
    reapplySortVisual($('#dynamicTable'));
    hideYieldTableLoading();
}

function applyFirstGroupSelected($table) {
    var $selectedHeader = $table.find('thead .col-group-header.selected');
    if (!$selectedHeader.length) return;

    // İlk group header'ın thead'deki kolon başlangıç indeksini bul
    var $row1 = $selectedHeader.closest('tr');
    var startCol = 0;
    var colCount = 0;
    var found = false;
    $row1.find('th').each(function () {
        var span = parseInt($(this).attr('colspan')) || 1;
        if (this === $selectedHeader[0]) {
            colCount = span;
            found = true;
            return false;
        }
        startCol += span;
    });
    if (!found) return;

    // Body satırlarındaki td'lere col-selected-first / col-selected-mid / col-selected-last ekle
    $table.find('tbody tr').each(function () {
        $(this).find('td').each(function (tdIdx) {
            if (tdIdx === startCol) $(this).addClass('col-selected-first');
            else if (tdIdx === startCol + colCount - 1) $(this).addClass('col-selected-last');
            else if (tdIdx > startCol && tdIdx < startCol + colCount - 1) $(this).addClass('col-selected-mid');
        });
    });
}

function reapplySortVisual($table) {
    $table.find('.sort-icon').removeClass('asc desc');
    if (_yieldSortBy !== null) {
        $table.find('.sort-icon[data-sort-id="' + _yieldSortBy + '"]').addClass(_yieldSortAsc ? 'asc' : 'desc');
    }
}

function formatDiff(val, useFormatNumber) {
    if (!val) return '<div class="diff-value-for-productivity">&nbsp;</div>';
    var cls = val < 0 ? 'negative' : (val > 0 ? 'positive' : '');
    var prefix = val > 0 ? '+' : '';
    var display = useFormatNumber ? formatNumber(val) : formatPercent(val);
    return '<div class="diff-value-for-productivity ' + cls + '">' + prefix + display + '</div>';
}

// ===== Yield Search =====
$(function () {
    handleTableSearch('#yieldSearchInput');
});
