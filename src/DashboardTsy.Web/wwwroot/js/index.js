$(document).ready(function () {

  // =====================================================================
  // MOCK DATA — Servis geldiğinde bu bloğu silin ve
  // yorum satırındaki $.ajax çağrılarını aktif edin.
  // =====================================================================
  var mockMenuTexts = {
      screenTitle: "Hedef Raporları",
      tabAllTitle: "Tümü", tabCorporateTitle: "Kurumsal", tabCommercialTitle: "Ticari",
      tabSmeTitle: "KOBİ", tabAgricultureTitle: "Tarım", tabRetailTitle: "Bireysel",
      smeSubTabAllTitle: "Tümü", smeSubTabKbiTitle: "KBİ", smeSubTabObiTitle: "OBİ",
      retailSubTabAllTitle: "Tümü", retailSubTabGeneralTitle: "Genel Kitle",
      retailSubTabAffiliateTitle: "Afili", retailSubTabPrivateTitle: "Özel Bankacılık"
  };

  var mockDailyHeaders = {
      productNameTitle: "Ürün Adı",
      lastYearTitle: "Geçen Yıl", lastYearDate: "2025-12-31T00:00:00",
      lastWeekTitle: "Geçen Hafta", lastWeekDate: "2026-02-13T00:00:00",
      prevDayTitle: "Önceki Gün (T-2)", prevDayDate: "2026-02-17T00:00:00",
      yesterdayTitle: "Dün (T-1)", yesterdayDate: "2026-02-18T00:00:00",
      diffByPrevDayTitle: "T-2'ye Göre", diffByLastYearTitle: "Yıla Göre", diffByLastWeekTitle: "Haftaya Göre"
  };

  var mockMonthlyHeaders = {
      productNameTitle: "Ürün Adı",
      monthGroupTitle: "Şubat Ayı", yearGroupTitle: "Yıllık",
      monthActualTitle: "Gerçekleşen", monthTargetTitle: "Hedef", monthHGTitle: "H/G",
      yearActualTitle: "Gerçekleşen", yearTargetTitle: "Hedef", yearHGTitle: "H/G"
  };

  var mockDailyProducts = [
      {
          productId: 1, productName: "Toplam Mevduat",
          lastYearAmount: 125000000, lastWeekAmount: 132500000, prevDayAmount: 133200000, yesterdayAmount: 133800000,
          diffByPrevDayAmount: 600000, diffByLastYearAmount: 8800000, diffByLastWeekAmount: 1300000,
          subProducts: [
              { productId: 11, productName: "TL Mevduat", lastYearAmount: 75000000, lastWeekAmount: 80200000, prevDayAmount: 80800000, yesterdayAmount: 81100000, diffByPrevDayAmount: 300000, diffByLastYearAmount: 6100000, diffByLastWeekAmount: 900000, subProducts: [] },
              { productId: 12, productName: "YP Mevduat", lastYearAmount: 50000000, lastWeekAmount: 52300000, prevDayAmount: 52400000, yesterdayAmount: 52700000, diffByPrevDayAmount: 300000, diffByLastYearAmount: 2700000, diffByLastWeekAmount: 400000, subProducts: [] }
          ]
      },
      {
          productId: 2, productName: "Toplam Kredi",
          lastYearAmount: 98000000, lastWeekAmount: 101500000, prevDayAmount: 102100000, yesterdayAmount: 102400000,
          diffByPrevDayAmount: 300000, diffByLastYearAmount: 4400000, diffByLastWeekAmount: 900000,
          subProducts: [
              { productId: 21, productName: "Ticari Kredi", lastYearAmount: 45000000, lastWeekAmount: 46800000, prevDayAmount: 47000000, yesterdayAmount: 47200000, diffByPrevDayAmount: 200000, diffByLastYearAmount: 2200000, diffByLastWeekAmount: 400000, subProducts: [] },
              { productId: 22, productName: "Bireysel Kredi", lastYearAmount: 53000000, lastWeekAmount: 54700000, prevDayAmount: 55100000, yesterdayAmount: 55200000, diffByPrevDayAmount: 100000, diffByLastYearAmount: 2200000, diffByLastWeekAmount: 500000, subProducts: [] }
          ]
      },
      { productId: 3, productName: "Toplam Fon", lastYearAmount: 42000000, lastWeekAmount: 44100000, prevDayAmount: 44300000, yesterdayAmount: 44500000, diffByPrevDayAmount: 200000, diffByLastYearAmount: 2500000, diffByLastWeekAmount: 400000, subProducts: [] },
      { productId: 4, productName: "Sigorta", lastYearAmount: 18500000, lastWeekAmount: 19200000, prevDayAmount: 19350000, yesterdayAmount: 19400000, diffByPrevDayAmount: 50000, diffByLastYearAmount: 900000, diffByLastWeekAmount: 200000, subProducts: [] },
      { productId: 5, productName: "DBS", lastYearAmount: 8200000, lastWeekAmount: 8600000, prevDayAmount: 8650000, yesterdayAmount: 8700000, diffByPrevDayAmount: 50000, diffByLastYearAmount: 500000, diffByLastWeekAmount: 100000, subProducts: [] },
      {
          productId: 6, productName: "Kredi Kartı",
          lastYearAmount: 31000000, lastWeekAmount: 32500000, prevDayAmount: 32700000, yesterdayAmount: 32900000,
          diffByPrevDayAmount: 200000, diffByLastYearAmount: 1900000, diffByLastWeekAmount: 400000,
          subProducts: [
              { productId: 61, productName: "Yeni Kart", lastYearAmount: 12000000, lastWeekAmount: 12800000, prevDayAmount: 12900000, yesterdayAmount: 13000000, diffByPrevDayAmount: 100000, diffByLastYearAmount: 1000000, diffByLastWeekAmount: 200000, subProducts: [] },
              { productId: 62, productName: "Ciro", lastYearAmount: 19000000, lastWeekAmount: 19700000, prevDayAmount: 19800000, yesterdayAmount: 19900000, diffByPrevDayAmount: 100000, diffByLastYearAmount: 900000, diffByLastWeekAmount: 200000, subProducts: [] }
          ]
      },
      { productId: 7, productName: "POS", lastYearAmount: 5400000, lastWeekAmount: 5650000, prevDayAmount: 5680000, yesterdayAmount: 5710000, diffByPrevDayAmount: 30000, diffByLastYearAmount: 310000, diffByLastWeekAmount: 60000, subProducts: [] },
      { productId: 8, productName: "Müşteri Sayısı", lastYearAmount: 2450000, lastWeekAmount: 2520000, prevDayAmount: 2525000, yesterdayAmount: 2528000, diffByPrevDayAmount: 3000, diffByLastYearAmount: 78000, diffByLastWeekAmount: 8000, subProducts: [] }
  ];

  var mockMonthlyProducts = [
      {
          productId: 1, productName: "Toplam Mevduat",
          monthActualAmount: 133800000, monthTargetAmount: 140000000, monthRatio: 0.956,
          yearActualAmount: 133800000, yearTargetAmount: 150000000, yearRatio: 0.892,
          subProducts: [
              { productId: 11, productName: "TL Mevduat", monthActualAmount: 81100000, monthTargetAmount: 85000000, monthRatio: 0.954, yearActualAmount: 81100000, yearTargetAmount: 90000000, yearRatio: 0.901, subProducts: [] },
              { productId: 12, productName: "YP Mevduat", monthActualAmount: 52700000, monthTargetAmount: 55000000, monthRatio: 0.958, yearActualAmount: 52700000, yearTargetAmount: 60000000, yearRatio: 0.878, subProducts: [] }
          ]
      },
      {
          productId: 2, productName: "Toplam Kredi",
          monthActualAmount: 102400000, monthTargetAmount: 105000000, monthRatio: 0.975,
          yearActualAmount: 102400000, yearTargetAmount: 120000000, yearRatio: 0.853,
          subProducts: [
              { productId: 21, productName: "Ticari Kredi", monthActualAmount: 47200000, monthTargetAmount: 48000000, monthRatio: 0.983, yearActualAmount: 47200000, yearTargetAmount: 55000000, yearRatio: 0.858, subProducts: [] },
              { productId: 22, productName: "Bireysel Kredi", monthActualAmount: 55200000, monthTargetAmount: 57000000, monthRatio: 0.968, yearActualAmount: 55200000, yearTargetAmount: 65000000, yearRatio: 0.849, subProducts: [] }
          ]
      },
      { productId: 3, productName: "Toplam Fon", monthActualAmount: 44500000, monthTargetAmount: 45000000, monthRatio: 0.989, yearActualAmount: 44500000, yearTargetAmount: 50000000, yearRatio: 0.890, subProducts: [] },
      { productId: 4, productName: "Sigorta", monthActualAmount: 19400000, monthTargetAmount: 20000000, monthRatio: 0.970, yearActualAmount: 19400000, yearTargetAmount: 24000000, yearRatio: 0.808, subProducts: [] },
      { productId: 5, productName: "DBS", monthActualAmount: 8700000, monthTargetAmount: 9000000, monthRatio: 0.967, yearActualAmount: 8700000, yearTargetAmount: 10000000, yearRatio: 0.870, subProducts: [] },
      {
          productId: 6, productName: "Kredi Kartı",
          monthActualAmount: 32900000, monthTargetAmount: 30000000, monthRatio: 1.097,
          yearActualAmount: 32900000, yearTargetAmount: 35000000, yearRatio: 0.940,
          subProducts: [
              { productId: 61, productName: "Yeni Kart", monthActualAmount: 13000000, monthTargetAmount: 12000000, monthRatio: 1.083, yearActualAmount: 13000000, yearTargetAmount: 14000000, yearRatio: 0.929, subProducts: [] },
              { productId: 62, productName: "Ciro", monthActualAmount: 19900000, monthTargetAmount: 18000000, monthRatio: 1.106, yearActualAmount: 19900000, yearTargetAmount: 21000000, yearRatio: 0.948, subProducts: [] }
          ]
      },
      { productId: 7, productName: "POS", monthActualAmount: 5710000, monthTargetAmount: 6000000, monthRatio: 0.952, yearActualAmount: 5710000, yearTargetAmount: 7000000, yearRatio: 0.816, subProducts: [] },
      { productId: 8, productName: "Müşteri Sayısı", monthActualAmount: 2528000, monthTargetAmount: 2600000, monthRatio: 0.972, yearActualAmount: 2528000, yearTargetAmount: 2800000, yearRatio: 0.903, subProducts: [] }
  ];

  var mockFilters = {
      bolge: [
          { code: "B001", name: "İstanbul Anadolu" }, { code: "B002", name: "İstanbul Avrupa" },
          { code: "B003", name: "Ankara" }, { code: "B004", name: "İzmir" },
          { code: "B005", name: "Antalya" }, { code: "B006", name: "Bursa" },
          { code: "B007", name: "Adana" }, { code: "B008", name: "Trabzon" }
      ],
      sube: [
          { code: "S001", name: "Kadıköy Şubesi" }, { code: "S002", name: "Ataşehir Şubesi" },
          { code: "S003", name: "Maltepe Şubesi" }, { code: "S004", name: "Üsküdar Şubesi" },
          { code: "S005", name: "Beşiktaş Şubesi" }, { code: "S006", name: "Şişli Şubesi" }
      ],
      portfoy: [
          { code: "P001", name: "Portföy A" }, { code: "P002", name: "Portföy B" },
          { code: "P003", name: "Portföy C" }, { code: "P004", name: "Portföy D" }
      ]
  };

  // Mock: client-side search
  function mockFilterProducts(products, searchText) {
      if (!searchText) return products;
      var q = searchText.toLowerCase();
      return products.filter(function (p) { return p.productName.toLowerCase().indexOf(q) > -1; });
  }

  // Mock: client-side sort
  function mockSortProducts(products, sortBy, isAscending) {
      var sorted = products.slice();
      var keyFn;
      switch (sortBy) {
          case 1: keyFn = function (p) { return p.productName; }; break;
          case 2: keyFn = function (p) { return p.lastYearAmount; }; break;
          case 3: keyFn = function (p) { return p.lastWeekAmount; }; break;
          case 4: keyFn = function (p) { return p.prevDayAmount; }; break;
          case 5: keyFn = function (p) { return p.yesterdayAmount; }; break;
          default: return sorted;
      }
      sorted.sort(function (a, b) {
          var va = keyFn(a), vb = keyFn(b);
          if (va < vb) return isAscending ? -1 : 1;
          if (va > vb) return isAscending ? 1 : -1;
          return 0;
      });
      return sorted;
  }

  function mockSortMonthlyProducts(products, sortBy, isAscending) {
      var sorted = products.slice();
      var keyFn;
      switch (sortBy) {
          case 1: keyFn = function (p) { return p.productName; }; break;
          case 2: keyFn = function (p) { return p.monthActualAmount; }; break;
          case 3: keyFn = function (p) { return p.monthTargetAmount; }; break;
          case 4: keyFn = function (p) { return p.yearActualAmount; }; break;
          case 5: keyFn = function (p) { return p.yearTargetAmount; }; break;
          default: return sorted;
      }
      sorted.sort(function (a, b) {
          var va = keyFn(a), vb = keyFn(b);
          if (va < vb) return isAscending ? -1 : 1;
          if (va > vb) return isAscending ? 1 : -1;
          return 0;
      });
      return sorted;
  }
  // =====================================================================

  // MOCK: Load menu texts
  (function () {
      var data = mockMenuTexts;
      $('[data-menu]').each(function () {
          var key = $(this).data('menu');
          if (data[key]) $(this).text(data[key]);
      });
  })();
  // SERVİS: Load menu texts
  // $.ajax({
  //     url: '/Home/GetTargetReportMenuTexts',
  //     type: 'POST',
  //     contentType: 'application/json',
  //     data: JSON.stringify({ sessionId: '1' }),
  //     success: function (data) {
  //         $('[data-menu]').each(function () {
  //             var key = $(this).data('menu');
  //             if (data[key]) $(this).text(data[key]);
  //         });
  //     }
  // });

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
          var isExpandable = p.subProducts && p.subProducts.length > 0;
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
              html += '<span style="padding-left: ' + (depth * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + p.productName + '</span>';
          } else {
              html += p.productName;
          }
          html += '</td>';
          html += '<td>' + formatNumber(p.lastYearAmount) + '</td>';
          html += '<td>' + formatNumber(p.lastWeekAmount) + '</td>';
          html += '<td>' + formatNumber(p.prevDayAmount) + '</td>';
          html += '<td class="col-diff">';
          html += '<div class="diff-main">' + formatNumber(p.yesterdayAmount) + '</div>';
          html += '<div class="diff-details">';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="diffByPrevDayTitle"></span>';
          html += '<span class="diff-value ' + (p.diffByPrevDayAmount < 0 ? 'negative' : 'positive') + '">' + formatNumber(p.diffByPrevDayAmount || 0) + '</span></span>';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="diffByLastYearTitle"></span>';
          html += '<span class="diff-value ' + (p.diffByLastYearAmount < 0 ? 'negative' : 'positive') + '">' + formatNumber(p.diffByLastYearAmount || 0) + '</span></span>';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="diffByLastWeekTitle"></span>';
          html += '<span class="diff-value ' + (p.diffByLastWeekAmount < 0 ? 'negative' : 'positive') + '">' + formatNumber(p.diffByLastWeekAmount || 0) + '</span></span>';
          html += '</div></td>';
          html += '</tr>';

          if (isExpandable) {
              html += buildDailyRows(p.subProducts, depth + 1, true);
          }
      });
      return html;
  }

  function buildMonthlyRows(products, depth, isSub) {
      var html = '';
      products.forEach(function (p) {
          var isExpandable = p.subProducts && p.subProducts.length > 0;
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
              html += '<span style="padding-left: ' + (depth * 16) + 'px"><img src="/images/sub-arrow.svg" alt="" class="sub-arrow-icon" /> ' + p.productName + '</span>';
          } else {
              html += p.productName;
          }
          html += '</td>';
          html += '<td class="month-col month-col-first">' + formatNumber(p.monthActualAmount) + '</td>';
          html += '<td class="month-col">' + formatNumber(p.monthTargetAmount) + '</td>';
          html += '<td class="month-col month-col-last col-ratio ' + ratioClass(p.monthRatio) + '">' + formatPercent(p.monthRatio) + '</td>';
          html += '<td class="col-group-spacer"></td>';
          html += '<td>' + formatNumber(p.yearActualAmount) + '</td>';
          html += '<td>' + formatNumber(p.yearTargetAmount) + '</td>';
          html += '<td class="col-ratio ' + ratioClass(p.yearRatio) + '">' + formatPercent(p.yearRatio) + '</td>';
          html += '</tr>';

          if (isExpandable) {
              html += buildMonthlyRows(p.subProducts, depth + 1, true);
          }
      });
      return html;
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

  var currentSortBy = undefined;
  var currentSortState = null;

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
      var searchText = $('#searchInput').val() || null;

      // MOCK
      var products = mockDailyProducts.slice();
      products = mockFilterProducts(products, searchText);
      if (currentSortBy !== undefined && currentSortState) {
          products = mockSortProducts(products, currentSortBy, currentSortState === 'asc');
      }
      var html = buildDailyRows(products, 0, false);
      $('#dailyTableBody').html(html);
      $('#dailyTableBody [data-daily-header]').each(function () {
          var key = $(this).data('daily-header');
          if (window._dailyHeaders && window._dailyHeaders[key]) {
              $(this).text(window._dailyHeaders[key]);
          }
      });
      if (!showDiff) {
          $('#dailyTableBody .diff-details').hide();
      }
      updateStripes();

      // SERVİS
      // var requestBody = {
      //     sessionId: '1',
      //     tabId: getActiveTabId(),
      //     subTabId: getActiveSubTabId(),
      //     reportDate: new Date().toISOString(),
      //     regionId: getSelectedCodes('bolge'),
      //     branchId: getSelectedCodes('sube'),
      //     portfolioId: getSelectedCodes('portfoy'),
      //     searchText: searchText,
      //     showDifferences: showDiff,
      //     sortBy: currentSortBy,
      //     isAscending: currentSortState === 'asc' ? true : (currentSortState === 'desc' ? false : false)
      // };
      // $.ajax({
      //     url: '/Home/GetDailyTargetReport',
      //     type: 'POST',
      //     contentType: 'application/json',
      //     data: JSON.stringify(requestBody),
      //     success: function (data) {
      //         var html = buildDailyRows(data.products, 0, false);
      //         $('#dailyTableBody').html(html);
      //         $('#dailyTableBody [data-daily-header]').each(function () {
      //             var key = $(this).data('daily-header');
      //             if (window._dailyHeaders && window._dailyHeaders[key]) {
      //                 $(this).text(window._dailyHeaders[key]);
      //             }
      //         });
      //         if (!showDiff) {
      //             $('#dailyTableBody .diff-details').hide();
      //         }
      //         updateStripes();
      //     }
      // });
  }

  function loadMonthlyReport() {
      var searchText = $('#searchInput').val() || null;

      // MOCK
      var products = mockMonthlyProducts.slice();
      products = mockFilterProducts(products, searchText);
      if (currentSortBy !== undefined && currentSortState) {
          products = mockSortMonthlyProducts(products, currentSortBy, currentSortState === 'asc');
      }
      var html = buildMonthlyRows(products, 0, false);
      $('#monthlyTableBody').html(html);
      updateStripes();

      // SERVİS
      // var requestBody = {
      //     sessionId: '1',
      //     tabId: getActiveTabId(),
      //     subTabId: getActiveSubTabId(),
      //     reportDate: new Date().toISOString(),
      //     regionId: getSelectedCodes('bolge'),
      //     branchId: getSelectedCodes('sube'),
      //     portfolioId: getSelectedCodes('portfoy'),
      //     searchText: searchText,
      //     showDifferences: false,
      //     sortBy: currentSortBy,
      //     isAscending: currentSortState === 'asc' ? true : (currentSortState === 'desc' ? false : false)
      // };
      // $.ajax({
      //     url: '/Home/GetMonthlyTargetReport',
      //     type: 'POST',
      //     contentType: 'application/json',
      //     data: JSON.stringify(requestBody),
      //     success: function (data) {
      //         var html = buildMonthlyRows(data.products, 0, false);
      //         $('#monthlyTableBody').html(html);
      //         updateStripes();
      //     }
      // });
  }

  // Sayfa yüklendiğinde daily report çek
  loadDailyReport();

  // Filtre seçim durumunu kontrol et
  function updateFilterButtons() {
      var hasSelection = false;
      ['bolge', 'sube', 'portfoy'].forEach(function (panelId) {
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

  // Temizle butonu
  $('.filter-clear-btn').on('click', function () {
      clearFilterPanel('bolge');
      clearFilterPanel('sube');
      clearFilterPanel('portfoy');
      loadFilterOptions('bolge');
      updateFilterButtons();
      loadActiveReport();
  });

  // MOCK: Load daily table headers
  window._dailyHeaders = mockDailyHeaders;
  (function () {
      var data = window._dailyHeaders;
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
  })();
  // SERVİS: Load daily table headers
  // window._dailyHeaders = {};
  // $.ajax({
  //     url: '/Home/GetDailyTargetReportTableHeaders',
  //     type: 'POST',
  //     contentType: 'application/json',
  //     data: JSON.stringify({ sessionId: '1' }),
  //     success: function (data) {
  //         window._dailyHeaders = data;
  //         $('[data-daily-header]').each(function () {
  //             var key = $(this).data('daily-header');
  //             if (key.indexOf('Date') > -1 && data[key]) {
  //                 var d = new Date(data[key]);
  //                 var dd = String(d.getDate()).padStart(2, '0');
  //                 var mm = String(d.getMonth() + 1).padStart(2, '0');
  //                 var yyyy = d.getFullYear();
  //                 $(this).text('(' + dd + '.' + mm + '.' + yyyy + ')');
  //             } else if (data[key]) {
  //                 $(this).text(data[key]);
  //             }
  //         });
  //     }
  // });

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

  // Period toggle
  var monthlyHeadersLoaded = false;
  $('.toggle-btn').on('click', function () {
      $('.toggle-btn').removeClass('active');
      $(this).addClass('active');

      var period = $(this).data('period');
      if (period === 'monthly') {
          $('#dailyTable').hide();
          $('#monthlyTable').show();
          $('#diffToggle').hide();
          // MOCK: Monthly headers
          if (!monthlyHeadersLoaded) {
              monthlyHeadersLoaded = true;
              var data = mockMonthlyHeaders;
              $('[data-monthly-header]').each(function () {
                  var key = $(this).data('monthly-header');
                  if (data[key]) $(this).text(data[key]);
              });
          }
          // SERVİS: Monthly headers
          // if (!monthlyHeadersLoaded) {
          //     monthlyHeadersLoaded = true;
          //     $.ajax({
          //         url: '/Home/GetMonthlyTargetReportTableHeaders',
          //         type: 'POST',
          //         contentType: 'application/json',
          //         data: JSON.stringify({ sessionId: '1' }),
          //         success: function (data) {
          //             $('[data-monthly-header]').each(function () {
          //                 var key = $(this).data('monthly-header');
          //                 if (data[key]) $(this).text(data[key]);
          //             });
          //         }
          //     });
          // }
          loadMonthlyReport();
      } else {
          $('#monthlyTable').hide();
          $('#dailyTable').show();
          $('#diffToggle').show();
          loadDailyReport();
      }
      updateStripes();
  });

  // Recalculate row striping and row numbers
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

  // Expandable rows
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

  updateStripes();

  // Search filter
  $('#searchInput').on('keydown', function (e) {
      if (e.key === 'Enter') {
          e.preventDefault();
          var query = $(this).val().trim();
          if (query.length >= 1) {
              loadActiveReport();
          }
      }
  });

  // Fark toggle
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

  $('#diffToggle').attr('data-active', 'false');
  $('.diff-details').hide();

  // ===== Dropdown Panels =====

  var filterMap = { bolge: 0, sube: 1, portfoy: 2 };

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

  // Filtre item'larını panele render et (mock ve servis için ortak)
  function renderFilterItems($panel, data) {
      var $list = $panel.find('.dropdown-list');
      $list.find('.dropdown-item:not(.select-all)').remove();
      $panel.find('.select-all').removeClass('checked');

      data.forEach(function (item) {
          var $item = $('<label class="dropdown-item" data-code="' + item.code + '">' +
              '<img src="/images/checkbox.svg" class="cb-icon cb-off" alt="" />' +
              '<img src="/images/checkbox-selected.svg" class="cb-icon cb-on" alt="" />' +
              '<span>' + item.name + '</span></label>');
          $list.append($item);
      });

      var panelId = $panel.data('panel');
      var $dropdown = $('.filter-dropdown[data-dropdown="' + panelId + '"]');
      $dropdown.find('.filter-badge').text('0').hide();
  }

  function loadFilterOptions(panelId, filterCode) {
      var $panel = $('.dropdown-panel[data-panel="' + panelId + '"]');

      // MOCK
      renderFilterItems($panel, mockFilters[panelId] || []);

      // SERVİS
      // var requestBody = {
      //     sessionId: '1',
      //     filterId: filterMap[panelId],
      //     filterCode: filterCode || []
      // };
      // $.ajax({
      //     url: '/Home/GetTargetReportFilters',
      //     type: 'POST',
      //     contentType: 'application/json',
      //     data: JSON.stringify(requestBody),
      //     success: function (data) {
      //         renderFilterItems($panel, data);
      //     }
      // });
  }

  // Sayfa yüklendiğinde sadece bölgeleri yükle
  loadFilterOptions('bolge');
  clearFilterPanel('sube');
  clearFilterPanel('portfoy');

  // Open/close dropdown
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

  $(document).on('click', '.dropdown-panel', function (e) {
      if (!$(e.target).closest('.dropdown-item').length) {
          e.stopPropagation();
      }
  });

  $(document).on('click', function (e) {
      if (!$(e.target).closest('.dropdown-panel, .filter-dropdown').length) {
          $('.dropdown-panel').removeClass('open');
      }
  });

  function updateCheckboxIcon($item) {
      if ($item.hasClass('checked')) {
          $item.find('.cb-off').hide();
          $item.find('.cb-on').show();
      } else {
          $item.find('.cb-off').show();
          $item.find('.cb-on').hide();
      }
  }

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

  $('.dropdown-search-input').on('keyup', function () {
      var query = $(this).val().toLowerCase();
      var $items = $(this).closest('.dropdown-panel').find('.dropdown-item:not(.select-all)');
      $items.each(function () {
          var text = $(this).find('span').text().toLowerCase();
          $(this).toggle(text.indexOf(query) > -1);
      });
  });

  // Kaydet — cascade
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

      if (panelId === 'bolge') {
          var regionCodes = getSelectedCodes('bolge');
          if (regionCodes.length > 0) {
              loadFilterOptions('sube', regionCodes);
          } else {
              clearFilterPanel('sube');
          }
          clearFilterPanel('portfoy');
      } else if (panelId === 'sube') {
          var branchCodes = getSelectedCodes('sube');
          if (branchCodes.length > 0) {
              loadFilterOptions('portfoy', branchCodes);
          } else {
              clearFilterPanel('portfoy');
          }
      }

      updateFilterButtons();
  });
});
