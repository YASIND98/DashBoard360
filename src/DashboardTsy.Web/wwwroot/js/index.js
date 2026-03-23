$(document).ready(function () {

  // ===== State =====
  var selectedRegion = null;
  var selectedBranch = null;
  var currentSortBy = undefined;
  var currentSortState = null;
  var monthlyHeadersLoaded = false;

  // ===== Load Menu Texts =====
  $.ajax({
      url: '/TargetReport/GetTargetReportMenuTexts',
      type: 'GET',
      data: { sessionId: '1' },
      success: function (data) {
          $('[data-menu]').each(function () {
              var key = $(this).data('menu');
              if (data[key]) $(this).text(data[key]);
          });
      }
  });

  // ===== Load Daily Headers =====
  window._dailyHeaders = {};
  $.ajax({
      url: '/TargetReport/GetDailyTargetReportTableHeaders',
      type: 'POST',
      contentType: 'application/json',
      data: JSON.stringify({ sessionId: '1' }),
      success: function (data) {
          window._dailyHeaders = data;
          $('[data-daily-header]').each(function () {
              var key = $(this).data('daily-header');
              if (key.indexOf('Date') > -1 && data[key]) {
                  var d = new Date(data[key]);
                  var dd = String(d.getDate()).padStart(2, '0');
                  var mm = String(d.getMonth() + 1).padStart(2, '0');
                  $(this).text('(' + dd + '.' + mm + '.' + d.getFullYear() + ')');
              } else if (data[key]) {
                  $(this).text(data[key]);
              }
          });
      }
  });

  // ===== Tab Helpers =====
  function getActiveTabId() {
      var tabMap = { tumu: 0, kurumsal: 1, ticari: 2, kobi: 3, tarim: 4, bireysel: 5 };
      return tabMap[$('.tab.active').data('tab')] || 0;
  }

  function getActiveSubTabId() {
      var $active = $('.sub-tab-bar:visible .sub-tab.active');
      if (!$active.length) return null;
      var subTabMap = {
          'kobi-tumu': 0, 'kobi-kbi': 1, 'kobi-obi': 2,
          'bireysel-tumu': 0, 'bireysel-genel': 1, 'bireysel-afili': 2, 'bireysel-ozel': 3
      };
      var key = $active.data('subtab') || '';
      return subTabMap[key] !== undefined ? subTabMap[key] : null;
  }

  function getActivePeriod() {
      return $('.toggle-btn.active').data('period') || 'daily';
  }

  function loadActiveReport() {
      if (getActivePeriod() === 'monthly') {
          loadMonthlyReport();
      } else {
          loadDailyReport();
      }
  }

  // ===== Build Report Request =====
  function buildRequest(showDiff) {
      return {
          sessionId: '1',
          tabId: getActiveTabId(),
          subTabId: getActiveSubTabId(),
          reportDate: new Date().toISOString(),
          regionId: selectedRegion ? [selectedRegion.code] : [],
          branchId: selectedBranch ? [selectedBranch.code] : [],
          searchText: $('#searchInput').val() || null,
          showDifferences: showDiff || false,
          sortBy: currentSortBy,
          isAscending: currentSortState === 'asc'
      };
  }

  // ===== Row Builders =====
  function buildRowStart(product, depth, isSub) {
      var isExpandable = product.SubProducts && product.SubProducts.length > 0;
      var rowClass = isSub ? 'table-row sub-row depth-' + depth : 'table-row';
      if (isExpandable) rowClass += ' expandable';

      var html = '<tr class="' + rowClass + '">';
      html += '<td class="col-index"></td>';
      html += '<td class="col-expand">';
      if (isExpandable) {
          html += '<span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span>';
      }
      html += '</td>';
      html += '<td class="col-text">';
      if (isSub) {
          html += '<span style="padding-left: ' + (depth * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + product.ProductName + '</span>';
      } else {
          html += product.ProductName;
      }
      html += '</td>';
      return html;
  }

  function buildDailyRows(products, depth, isSub) {
      var html = '';
      products.forEach(function (p) {
          html += buildRowStart(p, depth, isSub);
          html += '<td class="text-center">' + formatNumber(p.LastYearAmount) + '</td>';
          html += '<td class="text-center">' + formatNumber(p.LastWeekAmount) + '</td>';
          html += '<td class="text-center">' + formatNumber(p.PrevDayAmount) + '</td>';
          html += '<td class="col-diff text-center">';
          html += '<div class="diff-main">' + formatNumber(p.YesterdayAmount) + '</div>';
          html += '<div class="diff-details">';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="DiffByPrevDayTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByPrevDayAmount < 0 ? 'negative' : 'positive') + '">' + formatNumber(p.DiffByPrevDayAmount || 0) + '</span></span>';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="DiffByLastYearTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByLastYearAmount < 0 ? 'negative' : 'positive') + '">' + formatNumber(p.DiffByLastYearAmount || 0) + '</span></span>';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="DiffByLastWeekTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByLastWeekAmount < 0 ? 'negative' : 'positive') + '">' + formatNumber(p.DiffByLastWeekAmount || 0) + '</span></span>';
          html += '</div></td></tr>';

          if (p.SubProducts && p.SubProducts.length > 0) {
              html += buildDailyRows(p.SubProducts, depth + 1, true);
          }
      });
      return html;
  }

  function buildMonthlyRows(products, depth, isSub) {
      var html = '';
      products.forEach(function (p) {
          html += buildRowStart(p, depth, isSub);
          html += '<td class="text-center month-col month-col-first">' + formatNumber(p.MonthActualAmount) + '</td>';
          html += '<td class="text-center month-col">' + formatNumber(p.MonthTargetAmount) + '</td>';
          html += '<td class="text-center month-col month-col-last col-ratio ' + percentColor(p.MonthRatio) + '">' + formatPercent(p.MonthRatio) + '%</td>';
          html += '<td class="text-center">' + formatNumber(p.YearActualAmount) + '</td>';
          html += '<td>' + formatNumber(p.YearTargetAmount) + '</td>';
          html += '<td class="text-center col-ratio ' + percentColor(p.YearRatio) + '">' + formatPercent(p.YearRatio) + '%</td>';
          html += '</tr>';

          if (p.SubProducts && p.SubProducts.length > 0) {
              html += buildMonthlyRows(p.SubProducts, depth + 1, true);
          }
      });
      return html;
  }

  // ===== Report Loaders =====
  function loadDailyReport() {
      var showDiff = $('#diffToggle').attr('data-active') === 'true';

      $.ajax({
          url: '/TargetReport/GetDailyTargetReport',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify(buildRequest(showDiff)),
          success: function (data) {
              $('#dailyTableBody').html(buildDailyRows(data.Products, 0, false));

              $('#dailyTableBody [data-daily-header]').each(function () {
                  var key = $(this).data('daily-header');
                  if (window._dailyHeaders && window._dailyHeaders[key]) {
                      $(this).text(window._dailyHeaders[key]);
                  }
              });

              if (!showDiff) $('#dailyTableBody .diff-details').hide();
              updateStripes();
          }
      });
  }

  function loadMonthlyReport() {
      $.ajax({
          url: '/TargetReport/GetMonthlyTargetReport',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify(buildRequest(false)),
          success: function (data) {
              $('#monthlyTableBody').html(buildMonthlyRows(data.Products, 0, false));
              updateStripes();
          }
      });
  }

  // ===== Sorting =====
  $(document).on('click', '#dailyTable thead th, #monthlyTable thead th', function () {
      var $icon = $(this).find('.sort-icon[data-sort-by]');
      if (!$icon.length) return;

      var sortBy = parseInt($icon.data('sort-by'));

      if (currentSortBy === sortBy) {
          if (currentSortState === 'asc') {
              currentSortState = 'desc';
          } else if (currentSortState === 'desc') {
              currentSortState = null;
              currentSortBy = undefined;
          }
      } else {
          currentSortBy = sortBy;
          currentSortState = 'asc';
      }

      $('.data-table .sort-icon').removeClass('asc desc');
      if (currentSortState) $icon.addClass(currentSortState);

      loadActiveReport();
  });

  // ===== Tab Switching =====
  $('.tab').on('click', function () {
      $('.tab').removeClass('active');
      $(this).addClass('active');

      var tabId = $(this).data('tab');
      $('.sub-tab-bar').hide();
      $('.sub-tab-bar[data-parent-tab="' + tabId + '"]')
          .show()
          .find('.sub-tab').removeClass('active')
          .first().addClass('active');
      $('#searchInput').val('');
      loadActiveReport();
  });

  $('.sub-tab').on('click', function () {
      $(this).closest('.sub-tab-bar').find('.sub-tab').removeClass('active');
      $(this).addClass('active');
      $('#searchInput').val('');
      loadActiveReport();
  });

  // ===== Period Toggle =====
  $('.toggle-btn').on('click', function () {
      $('.toggle-btn').removeClass('active');
      $(this).addClass('active');

      var period = $(this).data('period');
      if (period === 'monthly') {
          $('#dailyTable').hide();
          $('#monthlyTable').show();
          $('#diffToggle').hide();
          if (!monthlyHeadersLoaded) {
              monthlyHeadersLoaded = true;
              $.ajax({
                  url: '/TargetReport/GetMonthlyTargetReportTableHeaders',
                  type: 'POST',
                  contentType: 'application/json',
                  data: JSON.stringify({ sessionId: '1' }),
                  success: function (data) {
                      $('[data-monthly-header]').each(function () {
                          var key = $(this).data('monthly-header');
                          if (data[key]) $(this).text(data[key]);
                      });
                  }
              });
          }
          loadMonthlyReport();
      } else {
          $('#monthlyTable').hide();
          $('#dailyTable').show();
          $('#diffToggle').show();
          loadDailyReport();
      }
      updateStripes();
  });

  // ===== Diff Toggle =====
  $('#diffToggle').on('click', function () {
      var isActive = $(this).attr('data-active') === 'true';
      $(this).attr('data-active', isActive ? 'false' : 'true');
      loadActiveReport();
  });

  $('#diffToggle').attr('data-active', 'false');
  $('.diff-details').hide();

  // ===== Striping =====
  function updateStripes() {
      $('.table-container:visible .data-table').each(function () {
          var $table = $(this);
          var visibleIndex = 0;
          var $lastVisible = null;
          $table.find('tbody tr').each(function () {
              var $tr = $(this);
              $tr.removeClass('stripe-odd stripe-even last-visible-row');
              if ($tr.hasClass('sub-row') && !$tr.hasClass('visible')) return;
              visibleIndex++;
              $tr.addClass(visibleIndex % 2 === 1 ? 'stripe-odd' : 'stripe-even');
              $tr.find('td:first').text(visibleIndex);
              $lastVisible = $tr;
          });
          if ($lastVisible) $lastVisible.addClass('last-visible-row');
      });
  }

  // ===== Expandable Rows =====
  $(document).on('click', '.expandable', function () {
      var $row = $(this);
      var parentDepth = getDepth($row);

      $row.toggleClass('expanded');

      if ($row.hasClass('expanded')) {
          var childDepth = parentDepth + 1;
          $row.nextAll('tr.sub-row').each(function () {
              var d = getDepth($(this));
              if (d <= parentDepth) return false;
              if (d === childDepth) $(this).addClass('visible');
          });
      } else {
          $row.nextAll('tr.sub-row').each(function () {
              var d = getDepth($(this));
              if (d <= parentDepth) return false;
              $(this).removeClass('visible expanded');
          });
      }
      updateStripes();
  });

  function getDepth($tr) {
      var match = $tr.attr('class').match(/depth-(\d+)/);
      return match ? parseInt(match[1]) : 0;
  }

  // ===== Search =====
  $('#searchInput').on('keydown', function (e) {
      if (e.key === 'Enter') {
          e.preventDefault();
          if ($(this).val().trim().length >= 1) loadActiveReport();
      }
  });

  // ===== Region/Branch Filters =====
  function renderRegionDropdown() {
      return renderRegionList('#indexRegionList', selectedRegion ? selectedRegion.code : null);
  }

  function renderBranchDropdown() {
      return renderBranchList('#indexBranchList', selectedBranch ? selectedBranch.code : null);
  }

  $(document).on('click', '#indexRegionList .dropdown-item', function () {
      var code = $(this).attr('data-code');
      var name = $(this).text();

      selectedRegion = code ? { code: code, name: name } : null;
      $('#indexRegionLabel').text(code ? name : 'Bölge');

      selectedBranch = null;
      $('#indexBranchLabel').text('Şube');

      $('#indexRegionList .dropdown-item').removeClass('selected');
      $(this).addClass('selected');
      $('#indexRegionPanel').removeClass('open');
      $('#indexBranchPanel').removeClass('open');
      $('#indexRegionSearch').val('');

      loadActiveReport();
  });

  $(document).on('click', '#indexBranchList .dropdown-item', function () {
      var code = $(this).attr('data-code');
      var name = $(this).text();

      if (!code) {
          selectedBranch = null;
          $('#indexBranchLabel').text('Şube');
      } else {
          var regionCode = $(this).attr('data-region');
          selectedBranch = { code: code, name: name };
          $('#indexBranchLabel').text(name);

          var region = findRegion(regionCode);
          if (region) {
              selectedRegion = { code: region.Code, name: region.Name };
              $('#indexRegionLabel').text(region.Name);
              $('#indexRegionList .dropdown-item').removeClass('selected');
              $('#indexRegionList .dropdown-item[data-code="' + region.Code + '"]').addClass('selected');
          }
      }

      $('#indexBranchList .dropdown-item').removeClass('selected');
      $(this).addClass('selected');
      $('#indexRegionPanel').removeClass('open');
      $('#indexBranchPanel').removeClass('open');
      $('#indexBranchSearch').val('');

      loadActiveReport();
  });

  // ===== Init =====
  loadRegionFilters(function () {
      var single = renderRegionDropdown();
      if (single) {
          selectedRegion = { code: single.Code, name: single.Name };
      }
      loadBranchFilters(function () {
          var singleBranch = renderBranchDropdown();
          if (singleBranch) {
              selectedBranch = { code: singleBranch.Code, name: singleBranch.Name };
          }
          loadDailyReport();
      });
  });

  updateStripes();
});
