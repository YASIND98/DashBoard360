// ===== Dynamic Sidebar =====
function loadSidebarItems() {
    var cached = sessionStorage.getItem('_sidebarItems');
    if (cached) {
        renderSidebar(JSON.parse(cached));
        return;
    }
    $.ajax({
        url: '/ProductivityReport/GetReportSidebarItems',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ sessionId: '1' }),
        success: function (data) {
            if (data && data.length > 0) {
                sessionStorage.setItem('_sidebarItems', JSON.stringify(data));
                renderSidebar(data);
            }
        }
    });
}

var _sidebarIcons = {
    'TargetReport': '/images/homepage.svg',
    'ProductivityReport': '/images/magic-star.svg'
};

function renderSidebar(items) {
    var $container = $('#sidebar-nav-items');
    var currentPath = window.location.pathname.replace(/\/$/, '') || '/';
    var html = '';
    items.sort(function (a, b) { return (a.OrderNo || 0) - (b.OrderNo || 0); });
    for (var i = 0; i < items.length; i++) {
        var item = items[i];
        if (!item.IsVisible) continue;
        var itemPath = (item.Url || '/').replace(/\/$/, '') || '/';
        var isActive = currentPath === itemPath ? ' active' : '';
        var icon = _sidebarIcons[item.Code] || '/images/homepage.svg';
        html += '<a href="' + item.Url + '" class="sidebar-nav' + isActive + '">';
        html += '<img src="' + icon + '" alt="' + item.Name + '" />';
        html += '<span class="sidebar-label">' + item.Name + '</span>';
        html += '</a>';
    }
    $container.html(html);
}

$(document).ready(function () {
    loadSidebarItems();
});

function formatPercent(ratio) {
  if(!ratio) return "-"
  var rounded = Math.round(ratio * 10) / 10;
  if (rounded % 1 === 0) return rounded;
  return rounded.toFixed(1).replace('.', '.<small>') + '</small>';
}

function percentColor(ratio) {
  if (ratio < 75) return 'ratio-red';
  if (ratio < 100) return 'ratio-orange';
  if (ratio < 120) return 'ratio-green';
  return 'ratio-blue';
}

function formatNumber(value, isPrice, productName) {
  if(!value) return "-";
  var num = new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 0 }).format(value);
  if (!isPrice) return num;
  var currency = (productName && productName.indexOf('YP') !== -1) ? '$' : '₺';
  return num + ' ' + currency;
}

// ===== Table Legend Helpers =====
var LEGEND_RATIO = '<span class="legend-value">0</span><span class="legend-bar ratio-red-bg"></span><span class="legend-value">75</span><span class="legend-bar ratio-orange-bg"></span><span class="legend-value">100</span><span class="legend-bar ratio-green-bg"></span><span class="legend-value">120</span><span class="legend-bar ratio-blue-bg"></span>';

function buildDiffLegend(label) {
  return '<span class="legend-label">' + label + ':&nbsp; -</span><span class="legend-bar ratio-red-bg"></span><span class="legend-value">0</span><span class="legend-bar ratio-green-bg"></span><span class="legend-label">+</span>';
}

function renderTableLegend(containerId, options) {
  var $el = $(containerId);
  var note = options.note || '';
  var html = '<span class="legend-note">' + note + '</span>';
  if (options.diff) html += '<div class="legend-colors">' + buildDiffLegend(options.diff) + '</div>';
  if (options.ratio) html += '<div class="legend-colors">' + LEGEND_RATIO + '</div>';
  html += '<div class="download-pdf-btn" style="cursor:pointer;"><img src="/images/download.svg" alt="PDF İndir" /><span>PDF İndir</span></div>';
  $el.html(html);
}


// ===== Shared Report Date =====
var _reportDate = new Date().toISOString();

// ===== Filter Data (shared across pages, cached in sessionStorage) =====
var _regionFilters = JSON.parse(sessionStorage.getItem('_regionFilters') || '[]');
var _branchFilters = JSON.parse(sessionStorage.getItem('_branchFilters') || '[]');

function loadRegionFilters(callback) {
  if (_regionFilters.length > 0) {
      if (_regionFilters.length === 1) {
          applySingleFilter('.filter-dropdown-wrapper', 'region', _regionFilters[0]);
      }
      if (callback) callback(_regionFilters);
      return;
  }
  $.ajax({
      url: '/ProductivityReport/GetReportRegionFilters',
      type: 'POST',
      contentType: 'application/json',
      data: JSON.stringify({ sessionId: '1' }),
      success: function (data) {
          _regionFilters = data;
          sessionStorage.setItem('_regionFilters', JSON.stringify(data));
          if (data.length === 1) {
              applySingleFilter('.filter-dropdown-wrapper', 'region', data[0]);
          }
          if (callback) callback(data);
      }
  });
}

function loadBranchFilters(callback) {
  if (_branchFilters.length > 0) {
      if (_branchFilters.length === 1) {
          applySingleFilter('.filter-dropdown-wrapper', 'branch', _branchFilters[0]);
      }
      if (callback) callback(_branchFilters);
      return;
  }
  $.ajax({
      url: '/ProductivityReport/GetReportBranchFilters',
      type: 'POST',
      contentType: 'application/json',
      data: JSON.stringify({ sessionId: '1' }),
      success: function (data) {
          _branchFilters = data;
          sessionStorage.setItem('_branchFilters', JSON.stringify(data));
          if (data.length === 1) {
              applySingleFilter('.filter-dropdown-wrapper', 'branch', data[0]);
          }
          if (callback) callback(data);
      }
  });
}

function applySingleFilter(wrapperSelector, type, item) {
  $(wrapperSelector).each(function () {
      var $wrapper = $(this);
      var $list = $wrapper.find('.dropdown-list');
      var listId = $list.attr('id') || '';
      var isRegion = type === 'region' && (listId.indexOf('Bolge') > -1 || listId.indexOf('Region') > -1);
      var isBranch = type === 'branch' && (listId.indexOf('Sube') > -1 || listId.indexOf('Branch') > -1);

      if (!isRegion && !isBranch) return;

      $wrapper.find('.filter-label').text(item.Name);
      $wrapper.find('.filter-dropdown').addClass('disabled');
  });
}

function findRegion(code) {
  for (var i = 0; i < _regionFilters.length; i++) {
      if (_regionFilters[i].Code === code) return _regionFilters[i];
  }
  return null;
}

function renderRegionList(listSelector, selectedCode) {
  var $list = $(listSelector);
  var isSingle = _regionFilters.length === 1;
  $list.empty();
  if (!isSingle) {
      var tumuClass = !selectedCode ? ' selected' : '';
      $list.append('<div class="dropdown-item tumu-item' + tumuClass + '" data-code="">Tümü</div>');
  }
  _regionFilters.forEach(function (r) {
      var cls = (isSingle || (selectedCode && selectedCode === r.Code)) ? ' selected' : '';
      $list.append('<div class="dropdown-item' + cls + '" data-code="' + r.Code + '">' + r.Name + '</div>');
  });
  return isSingle ? _regionFilters[0] : null;
}

function renderBranchList(listSelector, selectedCode, regionCode) {
  var $list = $(listSelector);
  var filtered = regionCode
      ? _branchFilters.filter(function (b) { return b.RegionCode === regionCode; })
      : _branchFilters;
  var isSingle = filtered.length === 1;
  $list.empty();
  if (!isSingle) {
      var tumuClass = !selectedCode ? ' selected' : '';
      $list.append('<div class="dropdown-item tumu-item' + tumuClass + '" data-code="">Tümü</div>');
  }
  filtered.forEach(function (b) {
      var cls = (isSingle || (selectedCode && selectedCode === b.Code)) ? ' selected' : '';
      $list.append('<div class="dropdown-item' + cls + '" data-code="' + b.Code + '" data-region="' + b.RegionCode + '">' + b.Name + '</div>');
  });
  return isSingle ? filtered[0] : null;
}

// ===== Scorecard Headers (cached per filterType in sessionStorage) =====
function loadScoreCardHeaders(filterType, callback) {
  var key = '_scoreCardHeaders_' + filterType;
  var cached = sessionStorage.getItem(key);
  if (cached) {
      if (callback) callback(JSON.parse(cached));
      return;
  }
  $.ajax({
      url: '/ProductivityReport/GetProductivityScoreCardReportHeaders',
      type: 'POST',
      contentType: 'application/json',
      data: JSON.stringify({ sessionId: '1', filterType: filterType, reportDate: _reportDate }),
      success: function (data) {
          sessionStorage.setItem(key, JSON.stringify(data));
          if (callback) callback(data);
      }
  });
}

// ===== Table Search Helpers =====
function normalizeTurkish(str) {
    return str
        .replace(/İ/g, 'i').replace(/I/g, 'i')
        .toLowerCase()
        .replace(/i̇/g, 'i')
        .replace(/ç/g, 'c').replace(/ğ/g, 'g').replace(/ı/g, 'i')
        .replace(/ö/g, 'o').replace(/ş/g, 's').replace(/ü/g, 'u');
}

function reStripeTable($tbody) {
    var stripeIndex = 0;
    var $lastVisible = null;
    $tbody.find('tr.table-row').removeClass('last-visible-row');
    $tbody.find('tr.table-row:visible').each(function () {
        var $tr = $(this);
        $tr.removeClass('stripe-odd stripe-even');
        stripeIndex++;
        $tr.addClass(stripeIndex % 2 === 1 ? 'stripe-odd' : 'stripe-even');
        $lastVisible = $tr;
    });
    if ($lastVisible) $lastVisible.addClass('last-visible-row');
}

function handleTableSearch(inputSelector) {
    $(inputSelector).on('input', function () {
        var query = $(this).val().trim();
        var $table = $('.table-container:visible .data-table tbody');
        if (!$table.length) return;

        $table.find('.no-result-row').remove();
        var $legend = $table.closest('.table-container').find('.table-legend');

        if (!query) {
            $table.find('tr.table-row').css('display', '');
            $legend.show();
            reStripeTable($table);
            return;
        }

        var normalizedQuery = normalizeTurkish(query);
        $table.find('tr.table-row').hide();
        $table.find('tr.table-row').not('.sub-row').each(function () {
            var $mainRow = $(this);
            var mainName = normalizeTurkish($mainRow.find('.col-text').text().trim());
            var mainMatch = mainName.indexOf(normalizedQuery) !== -1;
            var $subRows = $mainRow.nextUntil('tr.table-row:not(.sub-row)').filter('.sub-row');

            if (mainMatch) {
                $mainRow.show();
                $subRows.show();
            } else {
                var anySubMatch = false;
                $subRows.each(function () {
                    var subName = normalizeTurkish($(this).find('.col-text').text().trim());
                    if (subName.indexOf(normalizedQuery) !== -1) {
                        $(this).show();
                        anySubMatch = true;
                    }
                });
                if (anySubMatch) {
                    $mainRow.show();
                }
            }
        });

        if ($table.find('tr.table-row:visible').length === 0) {
            $legend.hide();
            var colCount = $table.closest('table').find('thead th').length || 1;
            $table.append(
                '<tr class="no-result-row"><td colspan="' + colCount + '" style="text-align:center;padding:48px 16px;">' +
                '<div style="color:#8b95b8;font-size:14px;">' +
                '<svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#8b95b8" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" style="margin-bottom:12px;display:block;margin-left:auto;margin-right:auto;"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>' +
                '"<strong>' + $('<span>').text(query).html() + '</strong>" ile eşleşen sonuç bulunamadı.' +
                '</div></td></tr>'
            );
        } else {
            $legend.show();
        }

        reStripeTable($table);
    });
}

// ===== Dropdown Panel =====

$(document).ready(function () {

  // Dropdown open/close
  $(document).on('click', '.filter-dropdown', function (e) {
      e.stopPropagation();
      if ($(this).hasClass('disabled')) return;
      var $panel = $(this).siblings('.dropdown-panel');
      var wasOpen = $panel.hasClass('open');
      $('.dropdown-panel').removeClass('open');
      if (!wasOpen) {
          $panel.addClass('open');
          $panel.find('.dropdown-search-input').val('').focus();
      }
  });

  // Close panels on outside click
  $(document).on('click', function (e) {
      if (!$(e.target).closest('.dropdown-panel, .filter-dropdown').length) {
          $('.dropdown-panel').removeClass('open');
      }
  });

  // Prevent panel click from closing
  $(document).on('click', '.dropdown-panel', function (e) {
      if (!$(e.target).closest('.dropdown-item').length) {
          e.stopPropagation();
      }
  });

  // Prevent search input from triggering dropdown close
  $(document).on('click', '.dropdown-search-input', function (e) {
      e.stopPropagation();
  });

  // Search inside dropdown
  $(document).on('keyup', '.dropdown-search-input', function (e) {
      e.stopPropagation();
      var query = normalizeTurkish($(this).val());
      var $items = $(this).closest('.dropdown-panel').find('.dropdown-list .dropdown-item:not(.tumu-item)');
      $items.each(function () {
          var text = normalizeTurkish($(this).text());
          $(this).toggle(text.indexOf(query) > -1);
      });
  });
});
