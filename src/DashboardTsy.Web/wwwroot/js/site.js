function formatPercent(ratio) {
  var pct = ratio * 100;
  var rounded = Math.round(pct * 10) / 10;
  if (rounded % 1 === 0) return rounded;
  return rounded.toFixed(1).replace('.', '.<small>') + '</small>';
}

function percentColor(ratio) {
  var pct = ratio * 100;
  if (pct < 75) return 'ratio-red';
  if (pct < 100) return 'ratio-orange';
  if (pct < 120) return 'ratio-green';
  return 'ratio-blue';
}

function formatNumber(value) {
  return new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 2 }).format(value);
}

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
          console.log("gel buraya");
          
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
