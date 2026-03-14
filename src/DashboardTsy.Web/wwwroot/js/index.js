$(document).ready(function () {

  // ===== Filter State =====
  var selectedBolge = null;
  var selectedSube = null;

  // Load menu texts from API
  $.ajax({
      url: '/TargetReport/GetTargetReportMenuTexts',
      type: 'GET',
      data: { sessionId: '1' },
      success: function (data) {
          $('[data-menu]').each(function () {
              var key = $(this).data('menu');
              if (data[key]) {
                  $(this).text(data[key]);
              }
          });
      }
  });

  function getActiveSubTabId() {
      var $activeSub = $('.sub-tab-bar:visible .sub-tab.active');
      if (!$activeSub.length) return null;
      var subtab = $activeSub.data('subtab') || '';
      var subTabMap = {
          'kobi-tumu': 0, 'kobi-kbi': 1, 'kobi-obi': 2,
          'bireysel-tumu': 0, 'bireysel-genel': 1, 'bireysel-afili': 2, 'bireysel-ozel': 3
      };
      return subTabMap[subtab] !== undefined ? subTabMap[subtab] : null;
  }

  function buildDailyRows(products, depth, isSub) {
      var html = '';
      products.forEach(function (p) {
          var isExpandable = p.SubProducts && p.SubProducts.length > 0;
          var rowClass = isSub ? 'table-row sub-row depth-' + depth : 'table-row';
          if (isExpandable) rowClass += ' expandable';

          html += '<tr class="' + rowClass + '">';
          html += '<td class="col-index"></td>';
          html += '<td class="col-expand">';
          if (isExpandable) {
              html += '<span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span>';
          }
          html += '</td>';
          html += '<td class="col-text">';
          if (isSub) {
              html += '<span style="padding-left: ' + (depth * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + p.ProductName + '</span>';
          } else {
              html += p.ProductName;
          }
          html += '</td>';
          html += '<td class="text-center">' + formatNumber(p.LastYearAmount) + '</td>';
          html += '<td class="text-center">' + formatNumber(p.LastWeekAmount) + '</td>';
          html += '<td class="text-center">' + formatNumber(p.PrevDayAmount) + '</td>';
          html += '<td class="col-diff text-center">';
          html += '<div class="diff-main">' + formatNumber(p.YesterdayAmount) + '</div>';
          html += '<div class="diff-details">';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="diffByPrevDayTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByPrevDayAmount < 0 ? 'negative' : 'positive') + '">' + formatNumber(p.DiffByPrevDayAmount || 0) + '</span></span>';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="diffByLastYearTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByLastYearAmount < 0 ? 'negative' : 'positive') + '">' + formatNumber(p.DiffByLastYearAmount || 0) + '</span></span>';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="diffByLastWeekTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByLastWeekAmount < 0 ? 'negative' : 'positive') + '">' + formatNumber(p.DiffByLastWeekAmount || 0) + '</span></span>';
          html += '</div></td>';
          html += '</tr>';

          if (isExpandable) {
              html += buildDailyRows(p.SubProducts, depth + 1, true);
          }
      });
      return html;
  }

  function buildMonthlyRows(products, depth, isSub) {
      var html = '';
      products.forEach(function (p) {
          var isExpandable = p.SubProducts && p.SubProducts.length > 0;
          var rowClass = isSub ? 'table-row sub-row depth-' + depth : 'table-row';
          if (isExpandable) rowClass += ' expandable';

          html += '<tr class="' + rowClass + '">';
          html += '<td class="col-index"></td>';
          html += '<td class="col-expand">';
          if (isExpandable) {
              html += '<span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span>';
          }
          html += '</td>';
          html += '<td class="col-text">';
          if (isSub) {
              html += '<span style="padding-left: ' + (depth * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + p.ProductName + '</span>';
          } else {
              html += p.ProductName;
          }
          html += '</td>';
          html += '<td class="text-center month-col month-col-first">' + formatNumber(p.MonthActualAmount) + '</td>';
          html += '<td class="text-center month-col">' + formatNumber(p.MonthTargetAmount) + '</td>';
          html += '<td class="text-center month-col month-col-last col-ratio ' + percentColor(p.MonthRatio) + '">' + formatPercent(p.MonthRatio) + "%" + '</td>';
          html += '<td class="text-center">' + formatNumber(p.YearActualAmount) + '</td>';
          html += '<td>' + formatNumber(p.YearTargetAmount) + '</td>';
          html += '<td class="text-center col-ratio ' + percentColor(p.YearRatio) + '">' + formatPercent(p.YearRatio) + '</td>';
          html += '</tr>';

          if (isExpandable) {
              html += buildMonthlyRows(p.SubProducts, depth + 1, true);
          }
      });
      return html;
  }

  function getActivePeriod() {
      return $('.toggle-btn.active').data('period') || 'daily';
  }

  // Aktif period'a göre doğru servisi çağır
  function loadActiveReport() {
      if (getActivePeriod() === 'monthly') {
          loadMonthlyReport();
      } else {
          loadDailyReport();
      }
  }

  var currentSortBy = undefined;
  var currentSortState = null; // null=yok, 'asc', 'desc'

  // Sort — th'ye tıklanınca çalışır
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
      if (currentSortState) {
          $icon.addClass(currentSortState);
      }

      loadActiveReport();
  });

  function loadDailyReport() {
      var showDiff = $('#diffToggle').attr('data-active') === 'true';

      var requestBody = {
          sessionId: '1',
          tabId: getActiveTabId(),
          subTabId: getActiveSubTabId(),
          reportDate: new Date().toISOString(),
          regionId: selectedBolge ? [selectedBolge.code] : [],
          branchId: selectedSube ? [selectedSube.code] : [],
          searchText: $('#searchInput').val() || null,
          showDifferences: showDiff,
          sortBy: currentSortBy,
          isAscending: currentSortState === 'asc' ? true : (currentSortState === 'desc' ? false : false)
      };

      $.ajax({
          url: '/TargetReport/GetDailyTargetReport',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify(requestBody),
          success: function (data) {
              var html = buildDailyRows(data.Products, 0, false);
              $('#dailyTableBody').html(html);

              // Diff label'ları header bilgisiyle doldur
              $('#dailyTableBody [data-daily-header]').each(function () {
                  var key = $(this).data('daily-header');
                  if (window._dailyHeaders && window._dailyHeaders[key]) {
                      $(this).text(window._dailyHeaders[key]);
                  }
              });

              // Diff toggle durumuna göre göster/gizle
              if (!showDiff) {
                  $('#dailyTableBody .diff-details').hide();
              }

              updateStripes();
          }
      });
  }

  function loadMonthlyReport() {
      var requestBody = {
          sessionId: '1',
          tabId: getActiveTabId(),
          subTabId: getActiveSubTabId(),
          reportDate: new Date().toISOString(),
          regionId: selectedBolge ? [selectedBolge.code] : [],
          branchId: selectedSube ? [selectedSube.code] : [],
          searchText: $('#searchInput').val() || null,
          showDifferences: false,
          sortBy: currentSortBy,
          isAscending: currentSortState === 'asc' ? true : (currentSortState === 'desc' ? false : false)
      };

      $.ajax({
          url: '/TargetReport/GetMonthlyTargetReport',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify(requestBody),
          success: function (data) {
              var html = buildMonthlyRows(data.Products, 0, false);
              $('#monthlyTableBody').html(html);
              updateStripes();
          }
      });
  }

  // Load daily table headers from API
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
                  var yyyy = d.getFullYear();
                  $(this).text('(' + dd + '.' + mm + '.' + yyyy + ')');
              } else if (data[key]) {
                  $(this).text(data[key]);
              }
          });
      }
  });

  // Tab switching
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

  // Sub-tab switching
  $('.sub-tab').on('click', function () {
      $(this).closest('.sub-tab-bar').find('.sub-tab').removeClass('active');
      $(this).addClass('active');
      $('#searchInput').val('');
      loadActiveReport();
  });

  // Period toggle - switch between daily/monthly tables
  var monthlyHeadersLoaded = false;
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
                          if (data[key]) {
                              $(this).text(data[key]);
                          }
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

  // Recalculate row striping and row numbers based on visible rows
  function updateStripes() {
      $('.table-container:visible .data-table').each(function () {
          var visibleIndex = 0;
          var $lastVisible = null;
          $(this).find('tbody tr').each(function () {
              var $tr = $(this);
              $tr.removeClass('stripe-odd stripe-even last-visible-row');
              if ($tr.hasClass('sub-row') && !$tr.hasClass('visible')) {
                  return;
              }
              visibleIndex++;
              $tr.addClass(visibleIndex % 2 === 1 ? 'stripe-odd' : 'stripe-even');
              $tr.find('td:first').text(visibleIndex);
              $lastVisible = $tr;
          });
          if ($lastVisible) $lastVisible.addClass('last-visible-row');
      });
  }

  // Expandable rows - click on expand icon or row (delegated for dynamic rows)
  $(document).on('click', '.expandable', function (e) {
      var $row = $(this);
      var parentDepth = getDepth($row);
      var isExpanded = $row.hasClass('expanded');

      if (isExpanded) {
          $row.removeClass('expanded');
          $row.nextAll('tr').each(function () {
              var $tr = $(this);
              if (!$tr.hasClass('sub-row')) return false;
              var d = getDepth($tr);
              if (d <= parentDepth) return false;
              $tr.removeClass('visible expanded');
          });
      } else {
          $row.addClass('expanded');
          var childDepth = parentDepth + 1;
          $row.nextAll('tr').each(function () {
              var $tr = $(this);
              if (!$tr.hasClass('sub-row')) return false;
              var d = getDepth($tr);
              if (d <= parentDepth) return false;
              if (d === childDepth) $tr.addClass('visible');
          });
      }
      updateStripes();
  });

  function getDepth($tr) {
      var match = $tr.attr('class').match(/depth-(\d+)/);
      return match ? parseInt(match[1]) : 0;
  }

  // Initial stripe calculation
  updateStripes();

  // Search filter — enter'a basıldığında min 1 karakter varsa servise istek at
  $('#searchInput').on('keydown', function (e) {
      if (e.key === 'Enter') {
          e.preventDefault();
          var query = $(this).val().trim();
          if (query.length >= 1) {
              loadActiveReport();
          }
      }
  });

  // Fark toggle with SVG switch (data-active attribute drives CSS)
  $('#diffToggle').on('click', function () {
      var $toggle = $(this);
      var isActive = $toggle.attr('data-active') === 'true';

      if (isActive) {
          $toggle.attr('data-active', 'false');
      } else {
          $toggle.attr('data-active', 'true');
      }
      loadActiveReport();
  });

  // Initially inactive - hide diff-details
  $('#diffToggle').attr('data-active', 'false');
  $('.diff-details').hide();

  // ===== Single-Select Dropdown Filters =====

  function getActiveTabId() {
      var tabMap = { tumu: 0, kurumsal: 1, ticari: 2, kobi: 3, tarim: 4, bireysel: 5 };
      return tabMap[$('.tab.active').data('tab')] || 0;
  }

  function renderBolgeList() {
      renderRegionList('#indexBolgeList', selectedBolge ? selectedBolge.code : null);
  }

  function renderSubeList() {
      renderBranchList('#indexSubeList', selectedSube ? selectedSube.code : null);
  }

  // Bölge seçimi
  $(document).on('click', '#indexBolgeList .dropdown-item', function () {
      var code = $(this).attr('data-code');
      var name = $(this).text();

      if (!code) {
          // Tümü seçildi
          selectedBolge = null;
          $('#indexBolgeLabel').text('Bölge');
      } else {
          selectedBolge = { code: code, name: name };
          $('#indexBolgeLabel').text(name);
      }

      // Şube seçimini temizle
      selectedSube = null;
      $('#indexSubeLabel').text('Şube');

      $('#indexBolgeList .dropdown-item').removeClass('selected');
      $(this).addClass('selected');
      $('#indexBolgePanel').removeClass('open');
      $('#indexSubePanel').removeClass('open');
      $('#indexBolgeSearch').val('');

      loadActiveReport();
  });

  // Şube seçimi
  $(document).on('click', '#indexSubeList .dropdown-item', function () {
      var code = $(this).attr('data-code');
      var name = $(this).text();

      if (!code) {
          // Tümü seçildi
          selectedSube = null;
          $('#indexSubeLabel').text('Şube');
      } else {
          var regionCode = $(this).attr('data-region');
          selectedSube = { code: code, name: name };
          $('#indexSubeLabel').text(name);

          // Bölgeyi otomatik seçili yap
          var region = findRegion(regionCode);
          if (region) {
              selectedBolge = { code: region.Code, name: region.Name };
              $('#indexBolgeLabel').text(region.Name);
              $('#indexBolgeList .dropdown-item').removeClass('selected');
              $('#indexBolgeList .dropdown-item[data-code="' + region.Code + '"]').addClass('selected');
          }
      }

      $('#indexSubeList .dropdown-item').removeClass('selected');
      $(this).addClass('selected');
      $('#indexBolgePanel').removeClass('open');
      $('#indexSubePanel').removeClass('open');
      $('#indexSubeSearch').val('');

      loadActiveReport();
  });


  // ===== Init =====
  loadRegionFilters(function () { renderBolgeList(); });
  loadBranchFilters(function () { renderSubeList(); });
  loadDailyReport();
});
