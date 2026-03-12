$(document).ready(function () {

  // Load menu texts from API
  $.ajax({
      url: 'http://localhost:5024/TargetReport/GetTargetReportMenuTexts',
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
          html += '<td>' + formatNumber(p.LastYearAmount) + '</td>';
          html += '<td>' + formatNumber(p.LastWeekAmount) + '</td>';
          html += '<td>' + formatNumber(p.PrevDayAmount) + '</td>';
          html += '<td class="col-diff">';
          html += '<div class="diff-main">' + formatNumber(p.YesterdayAmount) + '</div>';
          html += '<div class="diff-details">';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="DiffByPrevDayTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByPrevDayAmount < 0 ? 'negative' : 'positive') + '">' + formatNumber(p.DiffByPrevDayAmount || 0) + '</span></span>';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="DiffByLastYearTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByLastYearAmount < 0 ? 'negative' : 'positive') + '">' + formatNumber(p.DiffByLastYearAmount || 0) + '</span></span>';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="DiffByLastWeekTitle"></span>';
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
          html += '<td class="month-col month-col-first">' + formatNumber(p.MonthActualAmount) + '</td>';
          html += '<td class="month-col">' + formatNumber(p.MonthTargetAmount) + '</td>';
          html += '<td class="month-col month-col-last col-ratio ' + ratioClass(p.MonthRatio) + '">' + formatPercent(p.MonthRatio) + '</td>';
          html += '<td class="col-group-spacer"></td>';
          html += '<td>' + formatNumber(p.YearActualAmount) + '</td>';
          html += '<td>' + formatNumber(p.YearTargetAmount) + '</td>';
          html += '<td class="col-ratio ' + ratioClass(p.YearRatio) + '">' + formatPercent(p.YearRatio) + '</td>';
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
          regionId: getSelectedCodes('bolge'),
          branchId: getSelectedCodes('sube'),
          searchText: $('#searchInput').val() || null,
          showDifferences: showDiff,
          sortBy: currentSortBy,
          isAscending: currentSortState === 'asc' ? true : (currentSortState === 'desc' ? false : false)
      };

      $.ajax({
          url: 'http://localhost:5024/TargetReport/GetDailyTargetReport',
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
          regionId: getSelectedCodes('bolge'),
          branchId: getSelectedCodes('sube'),
          searchText: $('#searchInput').val() || null,
          showDifferences: false,
          sortBy: currentSortBy,
          isAscending: currentSortState === 'asc' ? true : (currentSortState === 'desc' ? false : false)
      };

      $.ajax({
          url: 'http://localhost:5024/TargetReport/GetMonthlyTargetReport',
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

  // Sayfa yüklendiğinde daily report çek
  loadDailyReport();

  // Filtre seçim durumunu kontrol et — Uygula/Temizle butonlarını güncelle
  function updateFilterButtons() {
      var hasSelection = false;
      ['bolge', 'sube'].forEach(function (panelId) {
          if (getSelectedCodes(panelId).length > 0) {
              hasSelection = true;
          }
      });
      if (hasSelection) {
          $('.filter-apply-btn').prop('disabled', false);
          $('.filter-clear-btn').show();
      } else {
          $('.filter-apply-btn').prop('disabled', true);
          $('.filter-clear-btn').hide();
      }
  }

  // Uygula butonu
  $('.filter-apply-btn').on('click', function () {
      if (!$(this).prop('disabled')) {
          loadActiveReport();
      }
  });

  // Temizle butonu — tüm filtreleri sıfırla
  $('.filter-clear-btn').on('click', function () {
      clearFilterPanel('bolge');
      clearFilterPanel('sube');
      loadFilterOptions('bolge');
      updateFilterButtons();
      loadActiveReport();
  });

  // Load daily table headers from API
  window._dailyHeaders = {};
  $.ajax({
      url: 'http://localhost:5024/TargetReport/GetDailyTargetReportTableHeaders',
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
                  url: 'http://localhost:5024/TargetReport/GetMonthlyTargetReportTableHeaders',
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
          $(this).find('tbody tr').each(function () {
              var $tr = $(this);
              $tr.removeClass('stripe-odd stripe-even');
              if ($tr.hasClass('sub-row') && !$tr.hasClass('visible')) {
                  return;
              }
              visibleIndex++;
              $tr.addClass(visibleIndex % 2 === 1 ? 'stripe-odd' : 'stripe-even');
              $tr.find('td:first').text(visibleIndex);
          });
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

  // ===== Dropdown Panels =====

  var filterMap = { bolge: 0, sube: 1 };

  function getActiveTabId() {
      var tabMap = { tumu: 0, kurumsal: 1, ticari: 2, kobi: 3, tarim: 4, bireysel: 5 };
      return tabMap[$('.tab.active').data('tab')] || 0;
  }

  function getSelectedCodes(panelId) {
      var codes = [];
      $('.dropdown-panel[data-panel="' + panelId + '"] .dropdown-item:not(.select-all).checked').each(function () {
          codes.push($(this).data('code'));
      });
      return codes;
  }

  function clearFilterPanel(panelId) {
      var $panel = $('.dropdown-panel[data-panel="' + panelId + '"]');
      $panel.find('.dropdown-list .dropdown-item:not(.select-all)').remove();
      $panel.find('.select-all').removeClass('checked');
      var $dropdown = $('.filter-dropdown[data-dropdown="' + panelId + '"]');
      $dropdown.find('.filter-badge').text('0').hide();
  }

  function loadFilterOptions(panelId, filterCode) {
      var $panel = $('.dropdown-panel[data-panel="' + panelId + '"]');
      var $list = $panel.find('.dropdown-list');

      var requestBody = {
          sessionId: '1',
          filterId: filterMap[panelId],
          filterCode: filterCode || []
      };

      $.ajax({
          url: 'http://localhost:5024/TargetReport/GetTargetReportFilters',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify(requestBody),
          success: function (data) {
              $list.find('.dropdown-item:not(.select-all)').remove();
              $panel.find('.select-all').removeClass('checked');

              data.forEach(function (item) {
                  var $item = $('<label class="dropdown-item" data-code="' + item.Code + '">' +
                      '<img src="/images/checkbox.svg" class="cb-icon cb-off" alt="" />' +
                      '<img src="/images/checkbox-selected.svg" class="cb-icon cb-on" alt="" />' +
                      '<span>' + item.Name + '</span></label>');
                  $list.append($item);
              });

              var $dropdown = $('.filter-dropdown[data-dropdown="' + panelId + '"]');
              $dropdown.find('.filter-badge').text('0').hide();
          }
      });
  }

  // Sayfa yüklendiğinde sadece bölgeleri yükle
  loadFilterOptions('bolge');
  clearFilterPanel('sube');

  // Open/close dropdown on filter-dropdown click
  $('.filter-dropdown').on('click', function (e) {
      e.stopPropagation();
      var panelId = $(this).data('dropdown');
      var $panel = $('.dropdown-panel[data-panel="' + panelId + '"]');
      var wasOpen = $panel.hasClass('open');

      $('.dropdown-panel').removeClass('open');

      if (!wasOpen) {
          $panel.addClass('open');
      }
  });

  // Prevent panel click from closing (but not dropdown-item clicks)
  $(document).on('click', '.dropdown-panel', function (e) {
      if (!$(e.target).closest('.dropdown-item').length) {
          e.stopPropagation();
      }
  });

  // Close panels on outside click
  $(document).on('click', function (e) {
      if (!$(e.target).closest('.dropdown-panel, .filter-dropdown').length) {
          $('.dropdown-panel').removeClass('open');
      }
  });

  // Checkbox toggle helper — img'leri swap eder
  function updateCheckboxIcon($item) {
      if ($item.hasClass('checked')) {
          $item.find('.cb-off').hide();
          $item.find('.cb-on').show();
      } else {
          $item.find('.cb-off').show();
          $item.find('.cb-on').hide();
      }
  }

  // Checkbox toggle (delegated — dinamik item'lar için gerekli)
  $(document).on('click', '.dropdown-item:not(.select-all)', function (e) {
      e.preventDefault();
      $(this).toggleClass('checked');
      updateCheckboxIcon($(this));
      var $panel = $(this).closest('.dropdown-panel');
      var $items = $panel.find('.dropdown-item:not(.select-all)');
      var $selectAll = $panel.find('.select-all');
      if ($items.length === $items.filter('.checked').length) {
          $selectAll.addClass('checked');
          updateCheckboxIcon($selectAll);
      } else {
          $selectAll.removeClass('checked');
          updateCheckboxIcon($selectAll);
      }
  });

  // Tümünü Seç toggle (delegated)
  $(document).on('click', '.dropdown-item.select-all', function (e) {
      e.preventDefault();
      var $this = $(this);
      var $panel = $this.closest('.dropdown-panel');
      var $items = $panel.find('.dropdown-item:not(.select-all)');

      if ($this.hasClass('checked')) {
          $this.removeClass('checked');
          $items.removeClass('checked');
      } else {
          $this.addClass('checked');
          $items.addClass('checked');
      }
      updateCheckboxIcon($this);
      $items.each(function () { updateCheckboxIcon($(this)); });
  });

  // Search inside dropdown
  $('.dropdown-search-input').on('keyup', function () {
      var query = $(this).val().toLowerCase();
      var $items = $(this).closest('.dropdown-panel').find('.dropdown-item:not(.select-all)');
      $items.each(function () {
          var text = $(this).find('span').text().toLowerCase();
          $(this).toggle(text.indexOf(query) > -1);
      });
  });

  // Kaydet — cascade: bolge → sube
  $('.dropdown-save').on('click', function () {
      var panelId = $(this).data('panel');
      var $panel = $('.dropdown-panel[data-panel="' + panelId + '"]');
      var $dropdown = $('.filter-dropdown[data-dropdown="' + panelId + '"]');
      var count = $panel.find('.dropdown-item:not(.select-all).checked').length;
      var $badge = $dropdown.find('.filter-badge');

      if (count > 0) {
          $badge.text(count).show();
      } else {
          $badge.hide();
      }

      $panel.removeClass('open');

      // Cascade dependent filters
      if (panelId === 'bolge') {
          var regionCodes = getSelectedCodes('bolge');
          if (regionCodes.length > 0) {
              loadFilterOptions('sube', regionCodes);
          } else {
              clearFilterPanel('sube');
          }
      }

      updateFilterButtons();
  });
});
