function formatPercent(ratio) {
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

function formatNumber(value, isPrice) {
  if (isPrice === undefined) isPrice = true;
  var num = new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 0 }).format(value);
  return isPrice ? num + ' ₺' : num;
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

function renderBranchList(listSelector, selectedCode) {
  var $list = $(listSelector);
  var isSingle = _branchFilters.length === 1;
  $list.empty();
  if (!isSingle) {
      var tumuClass = !selectedCode ? ' selected' : '';
      $list.append('<div class="dropdown-item tumu-item' + tumuClass + '" data-code="">Tümü</div>');
  }
  _branchFilters.forEach(function (b) {
      var cls = (isSingle || (selectedCode && selectedCode === b.Code)) ? ' selected' : '';
      $list.append('<div class="dropdown-item' + cls + '" data-code="' + b.Code + '" data-region="' + b.RegionCode + '">' + b.Name + '</div>');
  });
  return isSingle ? _branchFilters[0] : null;
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
      var query = $(this).val().toLowerCase();
      var $items = $(this).closest('.dropdown-panel').find('.dropdown-list .dropdown-item:not(.tumu-item)');
      $items.each(function () {
          var text = $(this).text().toLowerCase();
          $(this).toggle(text.indexOf(query) > -1);
      });
  });
});
