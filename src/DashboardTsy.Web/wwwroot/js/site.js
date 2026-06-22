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
    'TargetReport': '/images/target-report.svg',
    'ProductivityReport': '/images/productivity.svg',
    'ScoreCard': '/images/score-card.svg',
    'NplReport': '/images/npl.svg',
    'FinancialMap': '/images/financial-map.svg',
};

function renderSidebar(items) {
    var $container = $('#sidebar-nav-items');
    var currentPath = window.location.pathname.replace(/\/$/, '') || '/';
    var html = '';
    items.sort(function (a, b) { return (a.OrderNo || 0) - (b.OrderNo || 0); });
    for (var i = 0; i < items.length; i++) {
        var item = items[i];
        if (!item.IsVisible) continue;
        var isFinancialMap = item.Code === 'FinancialMap';
        var itemPath = (item.Url || '/').replace(/\/$/, '') || '/';
        var isActive = (!isFinancialMap && currentPath === itemPath) ? ' active' : '';
        var icon = _sidebarIcons[item.Code] || '/images/homepage.svg';
        var sideCode = String(item.Code || '').replace(/"/g, '');
        var targetAttr = isFinancialMap ? ' target="_blank" rel="noopener noreferrer"' : '';
        html += '<a href="' + item.Url + '" class="sidebar-nav' + isActive + '" data-sidebar-code="' + sideCode + '"' + targetAttr + '>';
        html += '<img src="' + icon + '" alt="' + item.Name + '" />';
        html += '<span class="sidebar-label">' + item.Name + '</span>';
        if (isFinancialMap) {
            html += '<img class="sidebar-export" src="/images/export.svg" alt="" />';
        }
        html += '</a>';
    }
    $container.html(html);
}

$(document).ready(function () {
    loadSidebarItems();
});


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
var _reportDateTtlMs = 4 * 60 * 60 * 1000; // 4 saat
var _reportDate = sessionStorage.getItem('_reportDate') || new Date().toISOString();

function applyReportDate() {
    var formatted = formatReportDateTr(_reportDate);
    if (formatted) $('.date-text').text(formatted);
}

function _isReportDateCacheFresh() {
    if (!sessionStorage.getItem('_reportDate')) return false;
    var fetchedAt = parseInt(sessionStorage.getItem('_reportDateFetchedAt') || '0', 10);
    if (!fetchedAt) return false;
    return (Date.now() - fetchedAt) < _reportDateTtlMs;
}

function loadReportDates(callback) {
    if (_isReportDateCacheFresh()) {
        applyReportDate();
        if (callback) callback();
        return;
    }
    $.ajax({
        url: '/ProductivityReport/GetReportDates',
        type: 'POST',
        contentType: 'application/json',
        data: '{}',
        success: function (data) {
            if (data && data.length > 0) {
                var picked = data.find(function (d) { return d.IsDefault; }) || data[0];
                _reportDate = new Date(picked.ReportDate).toISOString();
                sessionStorage.setItem('_reportDate', _reportDate);
                sessionStorage.setItem('_reportDateFetchedAt', Date.now().toString());
                applyReportDate();
            }
            if (callback) callback();
        },
        error: function () { if (callback) callback(); }
    });
}

$(document).ready(function () { applyReportDate(); });

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
        var $thead = $table.closest('table').find('thead');

        if (!query) {
            $table.find('tr.table-row').css('display', '');
            $thead.show();
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
            var colCount = $thead.find('th').length || 1;
            // Boş durumda kolon başlıklarını gizle
            $thead.hide();
            $legend.hide();
            $table.append(
                '<tr class="no-result-row"><td colspan="' + colCount + '" style="text-align:center;padding:48px 16px;">' +
                '<div class="table-empty-state">' +
                '<img src="/images/empty-state-seach.svg" alt="" />' +
                '<span>"<strong>' + $('<span>').text(query).html() + '</strong>" ile eşleşen sonuç bulunamadı.</span>' +
                '</div></td></tr>'
            );
        } else {
            $thead.show();
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

  // ===== Info-icon tooltip (rendered into body to escape overflow ancestors) =====
  var $infoTooltip = null;

  function showInfoTooltip($icon) {
      var text = $icon.attr('data-tooltip');
      if (!text) return;
      if (!$infoTooltip) {
          $infoTooltip = $('<div class="info-tooltip" role="tooltip"></div>').appendTo('body');
      }
      $infoTooltip.text(text).addClass('below');
      var rect = $icon[0].getBoundingClientRect();
      $infoTooltip.css({ visibility: 'hidden', left: 0, top: 0 }).addClass('visible');
      var tw = $infoTooltip.outerWidth();
      var th = $infoTooltip.outerHeight();
      var pad = 8;
      var iconCenter = rect.left + rect.width / 2;
      var left = iconCenter - tw / 2;
      if (left < pad) left = pad;
      if (left + tw > window.innerWidth - pad) left = window.innerWidth - tw - pad;
      // Tasarımdaki gibi varsayılan altta; altta yer yoksa üste dön
      var top = rect.bottom + 10;
      if (top + th > window.innerHeight - pad) {
          top = rect.top - th - 10;
          $infoTooltip.removeClass('below');
      }
      // Ok, kutu ortasına değil ikonun gerçek merkezine hizalansın (kenara clamp olunca kaymayı önler)
      var arrowLeft = Math.max(14, Math.min(tw - 14, iconCenter - left));
      $infoTooltip[0].style.setProperty('--arrow-left', arrowLeft + 'px');
      $infoTooltip.css({ left: left + 'px', top: top + 'px', visibility: 'visible' });
  }

  function hideInfoTooltip() {
      if ($infoTooltip) $infoTooltip.removeClass('visible');
  }

  $(document).on('mouseenter focusin', '[data-tooltip]', function () {
      showInfoTooltip($(this));
  });
  $(document).on('mouseleave focusout', '[data-tooltip]', hideInfoTooltip);
  $(window).on('scroll resize', hideInfoTooltip);
  $(document).on('scroll', '.table-wrapper, .table-container', hideInfoTooltip);
});

// ===== Client activity (sayfa görüntüleme, sidebar, data-activity butonları) =====
(function () {
    if (typeof $ === 'undefined') return;

    function postClientActivity(payload) {
        if (!window._activityLogEnabled || !window._activityLogUrl) return;
        try {
            $.ajax({
                url: window._activityLogUrl,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    EventType: payload.eventType,
                    ActionName: payload.actionName || null,
                    PageUrl: payload.pageUrl || (window.location.pathname + window.location.search),
                    Details: payload.details || null
                })
            });
        } catch (e) { /* ignore */ }
    }

    $(document).ready(function () {
        if (!window._activityLogEnabled) return;

        postClientActivity({
            eventType: 'PageView',
            actionName: (document.title || '').trim() || null,
            pageUrl: window.location.pathname + window.location.search
        });

        $(document).on('click', '[data-activity]', function () {
            var code = ($(this).attr('data-activity') || '').trim() || $(this).text().trim().slice(0, 200);
            postClientActivity({ eventType: 'ButtonClick', actionName: code });
        });

        $(document).on('click', 'a.sidebar-nav', function () {
            var label = $(this).find('.sidebar-label').first().text().trim();
            var code = ($(this).attr('data-sidebar-code') || '').trim();
            var href = ($(this).attr('href') || '').trim();
            var name = label || code || 'Sidebar';
            postClientActivity({
                eventType: 'Navigation',
                actionName: 'Sidebar:' + name,
                details: href ? 'href:' + href : null
            });
        });
    });
})();
