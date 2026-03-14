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

// ===== Filter Data (shared across pages) =====
var _regionFilters = [];
var _branchFilters = [];

function loadRegionFilters(callback) {
  if (_regionFilters.length > 0) {
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
          if (callback) callback(data);
      }
  });
}

function loadBranchFilters(callback) {
  if (_branchFilters.length > 0) {
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
          if (callback) callback(data);
      }
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
  $list.empty();
  var tumuClass = !selectedCode ? ' selected' : '';
  $list.append('<div class="dropdown-item tumu-item' + tumuClass + '" data-code="">Tümü</div>');
  _regionFilters.forEach(function (r) {
      var cls = (selectedCode && selectedCode === r.Code) ? ' selected' : '';
      $list.append('<div class="dropdown-item' + cls + '" data-code="' + r.Code + '">' + r.Name + '</div>');
  });
}

function renderBranchList(listSelector, selectedCode) {
  var $list = $(listSelector);
  $list.empty();
  var tumuClass = !selectedCode ? ' selected' : '';
  $list.append('<div class="dropdown-item tumu-item' + tumuClass + '" data-code="">Tümü</div>');
  _branchFilters.forEach(function (b) {
      var cls = (selectedCode && selectedCode === b.Code) ? ' selected' : '';
      $list.append('<div class="dropdown-item' + cls + '" data-code="' + b.Code + '" data-region="' + b.RegionCode + '">' + b.Name + '</div>');
  });
}

// ===== Dropdown Panel =====

$(document).ready(function () {

  // Dropdown open/close
  $(document).on('click', '.filter-dropdown', function (e) {
      e.stopPropagation();
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
