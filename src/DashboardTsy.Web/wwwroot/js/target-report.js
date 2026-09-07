$(document).ready(function () {

  // ===== Initial Loading Overlay =====
  showLoadingOverlay();

  // ===== State =====
  var selectedRegion = null;
  var selectedBranch = null;

  function persistSelection() {
      saveFilterSelection(selectedRegion, selectedBranch);
  }
  var currentSortBy = 0;
  var currentSortState = null;
  var monthlyHeadersLoaded = false;
  var _selectedReportDate = _todayDate;

  // ===== In-flight Request Tracking =====
  // Hızlı tab/şube/bölge değişiminde önceki isteğin geç gelen cevabı ekranı ezmesin diye
  // her yeni tablo isteğinden önce bir öncekini iptal ederiz.
  var _targetHeadersXhr = null;
  var _targetTableXhr = null;
  var _targetTop10Xhr = null;
  var _targetBreakdownXhr = null;

  // ===== Load Menu Texts =====
  var cachedMenu = sessionStorage.getItem('_menuTexts');
  if (cachedMenu) {
      var menuData = JSON.parse(cachedMenu);
      $('[data-menu]').each(function () {
          var key = $(this).data('menu');
          if (menuData[key]) $(this).text(menuData[key]);
      });
  } else {
      $.ajax({
          url: '/TargetReport/GetTargetReportMenuTexts',
          type: 'GET',
          data: { sessionId: '1' },
          success: function (data) {
              sessionStorage.setItem('_menuTexts', JSON.stringify(data));
              $('[data-menu]').each(function () {
                  var key = $(this).data('menu');
                  if (data[key]) $(this).text(data[key]);
              });
          }
      });
  }

  // ===== Header Helpers =====
  function loadHeaders(url, storageKey, callback) {
      var cached = sessionStorage.getItem(storageKey);
      if (cached) {
          callback(JSON.parse(cached));
          return;
      }
      abortXhr(_targetHeadersXhr);
      _targetHeadersXhr = $.ajax({
          url: url,
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify({ sessionId: '1' }),
          success: function (data) {
              sessionStorage.setItem(storageKey, JSON.stringify(data));
              callback(data);
          }
      });
  }

  // ===== Load Daily Headers =====
  window._dailyHeaders = {};
  loadHeaders('/TargetReport/GetDailyTargetReportTableHeaders', '_dailyHeaders', function (data) {
      window._dailyHeaders = data;
      $('[data-daily-header]').each(function () {
          var key = $(this).data('daily-header');
          if (key.indexOf('Date') > -1 && data[key]) {
              $(this).text('(' + fmtIsoDate(data[key]) + ')');
          } else if (data[key]) {
              $(this).text(data[key]);
          }
      });
  });

  // ===== Tab Helpers =====
  function getActiveTabId() {
      var tabMap = { tumu: 0, kurumsal: 1, ticari: 2, kobi: 3, tarim: 4, bireysel: 5 };
      return tabMap[$('.tab.active').data('tab')] || 0;
  }

  function getActiveSubTabId() {
      var $active = $('.sub-tab-bar:visible .sub-tab.active');
      if (!$active.length) return 0;
      var subTabMap = {
          'kobi-tumu': 0, 'kobi-kbi': 1, 'kobi-obi': 2,
          'bireysel-tumu': 0, 'bireysel-genel': 1, 'bireysel-afili': 2, 'bireysel-ozel': 3, 'bireysel-adet': 3
      };
      var key = $active.data('subtab') || '';
      return subTabMap[key] !== undefined ? subTabMap[key] : 0;
  }

  function getActivePeriod() {
      return $('[data-period].active').data('period') || 'daily';
  }

  // Ürün adı "oran" içeriyorsa (ör. aktiflik oranı) değerler % ile gösterilir.
  function isRatioProduct(name) {
      return (name || '').toLocaleLowerCase('tr').indexOf('oran') > -1;
  }

  // Adet tipinde: "Özel Bankacılık" gizlenir, yerine Afili yanında "AmountSubTabTitle"
  // sekmesi (subTabId=3) gösterilir. Hacim tipinde tam tersi geçerlidir.
  function updateBireyselOzelVisibility() {
      var $ozel = $('.sub-tab[data-subtab="bireysel-ozel"]');
      var $adet = $('.sub-tab[data-subtab="bireysel-adet"]');
      if (!$ozel.length && !$adet.length) return;

      function resetActiveTo(needle) {
          $(needle).closest('.sub-tab-bar')
              .find('.sub-tab[data-subtab="bireysel-tumu"]')
              .addClass('active');
      }

      if (getActiveType() === 'adet') {
          if ($ozel.hasClass('active')) {
              $ozel.removeClass('active');
              resetActiveTo($ozel);
          }
          $ozel.hide();
          $adet.show();
      } else {
          if ($adet.hasClass('active')) {
              $adet.removeClass('active');
              resetActiveTo($adet);
          }
          $adet.hide();
          $ozel.show();
      }
  }

  function getActiveType() {
      return $('.segment[data-type].active').data('type') || 'hacim';
  }

  function loadActiveReport() {
      if (getActiveType() === 'adet') {
          loadQuantityReport();
      } else if (getActivePeriod() === 'monthly') {
          loadMonthlyReport();
      } else {
          loadDailyReport();
      }
  }

  // ===== Build Report Request =====
  function buildRequest() {
      return {
          sessionId: '1',
          tabId: getActiveTabId(),
          subTabId: getActiveSubTabId(),
          reportDate: _selectedReportDate,
          regionId: selectedRegion ? [selectedRegion.code] : [],
          branchId: selectedBranch ? [selectedBranch.code] : [],
          showDifferences: true,
          sortBy: currentSortState ? currentSortBy : 0,
          isAscending: currentSortState ? currentSortState === 'asc' : true
      };
  }

  // Trend Analizi (report-trend.js) sayfadaki aktif bölge/şube/tab filtrelerini buradan okur.
  window.getTargetReportFilterContext = function () {
      var $subTab = $('.sub-tab-bar:visible .sub-tab.active');
      return {
          regionName: selectedRegion ? selectedRegion.name : '',
          branchCode: selectedBranch ? selectedBranch.code : '',
          tabLabel: ($('.tab.active').text() || '').trim(),
          subTabLabel: $subTab.length ? ($subTab.text() || '').trim() : ''
      };
  };

  // ===== Row Builders =====
  function buildRowStart(product, depth, isSub, indexLabel) {
      var isExpandable = product.SubProducts && product.SubProducts.length > 0;
      var rowClass = isSub ? 'table-row sub-row depth-' + depth : 'table-row';
      if (isExpandable) rowClass += ' expandable';

      var html = '<tr class="' + rowClass + '">';
      if (isSub) {
          html += '<td class="col-index"></td>';
          html += '<td class="col-expand">';
          if (isExpandable) {
              html += '<span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span>';
          }
          html += '</td>';
          html += '<td class="col-left"><span class="sub-index" style="padding-left: ' + (depth * 16) + 'px">' + indexLabel + '</span>  ' + product.ProductName;
      } else {
          html += '<td class="col-index">' + indexLabel + '</td>';
          html += '<td class="col-expand">';
          if (isExpandable) {
              html += '<span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span>';
          }
          html += '</td>';
          html += '<td class="col-left">' + product.ProductName;
      }
      html += '</td>';
      return html;
  }

  // Tüm tablolardaki "Detay" hücresi: detay modalını açan ikon. tableKey = daily|quantity|monthly.
  // Top-10 sekmesi yalnızca hacim/bakiye (daily) + TOP10 ürününde açılır (data-top10).
  // Trend Analizi sekmesi yalnızca Adet (quantity) ve Aylık H/G (monthly) tablolarında vardır.
  // Bölge/Şube/Portföy Kırılımı her satırda açıldığı için detay ikonu her satıra basılır.
  function buildDetailCell(p, tableKey) {
      var isTop10 = tableKey === 'daily' && TOP10_PRODUCT_NAMES.includes((p.ProductName || '').replace(/\s*\(.*/, ''));
      return '<td class="col-detail"><img src="/images/expand.svg" alt="Detay" class="detail-icon"' +
          ' data-table="' + tableKey + '"' +
          ' data-product-id="' + p.ProductId + '"' +
          ' data-product-name="' + (p.ProductName || '').replace(/"/g, '&quot;') + '"' +
          ' data-top10="' + (isTop10 ? '1' : '0') + '" /></td>';
  }

  function buildDailyRows(products, depth, isSub, parentIndex) {
      var html = '';
      products.forEach(function (p, i) {
          var indexLabel = parentIndex ? parentIndex + '.' + (i + 1) : String(i + 1);
          html += buildRowStart(p, depth, isSub, indexLabel);
          html += '<td class="col-diff">';
          html += '<div>' + formatNumber(p.YesterdayAmount) + '</div>';
          html += '<div class="diff-details">';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="DiffByPrevDayTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByPrevDayAmount < 0 ? 'negative' : (p.DiffByPrevDayAmount > 0 ? 'positive' : '')) + '">' + formatNumber(p.DiffByPrevDayAmount || 0) + '</span></span>';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="DiffByLastWeekTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByLastWeekAmount < 0 ? 'negative' : (p.DiffByLastWeekAmount > 0 ? 'positive' : '')) + '">' + formatNumber(p.DiffByLastWeekAmount || 0) + '</span></span>';
          html += '<span class="diff-detail"><span class="diff-label" data-daily-header="DiffByLastYearTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByLastYearAmount < 0 ? 'negative' : (p.DiffByLastYearAmount > 0 ? 'positive' : '')) + '">' + formatNumber(p.DiffByLastYearAmount || 0) + '</span></span>';
          html += '</div>';
          html += '</td>';
          html += '<td>' + formatNumber(p.PrevDayAmount) + '</td>';
          html += '<td>' + formatNumber(p.LastWeekAmount) + '</td>';
          html += '<td>' + formatNumber(p.LastYearAmount) + '</td>';
          html += buildDetailCell(p, 'daily');
          html += '</tr>';

          if (p.SubProducts && p.SubProducts.length > 0) {
              html += buildDailyRows(p.SubProducts, depth + 1, true, indexLabel);
          }
      });
      return html;
  }

  function buildQuantityRows(products, depth, isSub, parentIndex) {
      var html = '';
      // "%" yalnızca gerçek sayılarda eklenir ("-" değerinde eklenmez)
      function qVal(v, pct) { return formatNumber(v) + (v ? pct : ''); }
      products.forEach(function (p, i) {
          var indexLabel = parentIndex ? parentIndex + '.' + (i + 1) : String(i + 1);
          // Ürün adı "oran" içeriyorsa bu satırın değerleri % ile gösterilir
          var pct = isRatioProduct(p.ProductName) ? '%' : '';
          html += buildRowStart(p, depth, isSub, indexLabel);
          html += '<td class="col-diff">';
          html += '<div>' + qVal(p.LastMonthAmount, pct) + '</div>';
          html += '<div class="diff-details">';
          html += '<span class="diff-detail"><span class="diff-label" data-quantity-header="DiffByLastTwoMonthEarlierTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByLastTwoMonthEarlierAmount < 0 ? 'negative' : (p.DiffByLastTwoMonthEarlierAmount > 0 ? 'positive' : '')) + '">' + qVal(p.DiffByLastTwoMonthEarlierAmount || 0, pct) + '</span></span>';
          html += '<span class="diff-detail"><span class="diff-label" data-quantity-header="DiffByLastYearTitle"></span>';
          html += '<span class="diff-value ' + (p.DiffByLastYearAmount < 0 ? 'negative' : (p.DiffByLastYearAmount > 0 ? 'positive' : '')) + '">' + qVal(p.DiffByLastYearAmount || 0, pct) + '</span></span>';
          html += '</div>';
          html += '</td>';
          html += '<td>' + qVal(p.LastTwoMonthEarlierAmount, pct) + '</td>';
          html += '<td>' + qVal(p.LastYearAmount, pct) + '</td>';
          html += buildDetailCell(p, 'quantity');
          html += '</tr>';

          if (p.SubProducts && p.SubProducts.length > 0) {
              html += buildQuantityRows(p.SubProducts, depth + 1, true, indexLabel);
          }
      });
      return html;
  }

  function buildMonthlyRows(products, depth, isSub, parentIndex) {
      var html = '';
      products.forEach(function (p, i) {
          var indexLabel = parentIndex ? parentIndex + '.' + (i + 1) : String(i + 1);
          html += buildRowStart(p, depth, isSub, indexLabel);
          html += '<td class="col-selected col-selected-first">' + formatNumber(p.MonthActualAmount) + '</td>';
          html += '<td class="col-selected col-selected-mid">' + formatNumber(p.MonthTargetAmount) + '</td>';
          html += '<td class="col-selected col-selected-last ' + percentColor(p.MonthRatio) + '">' + formatPercent(p.MonthRatio) + '</td>';
          html += '<td>' + formatNumber(p.YearActualAmount) + '</td>';
          html += '<td>' + formatNumber(p.YearTargetAmount) + '</td>';
          html += '<td class="' + percentColor(p.YearRatio) + '">' + formatPercent(p.YearRatio) + '</td>';
          html += buildDetailCell(p, 'monthly');
          html += '</tr>';

          if (p.SubProducts && p.SubProducts.length > 0) {
              html += buildMonthlyRows(p.SubProducts, depth + 1, true, indexLabel);
          }
      });
      return html;
  }

  // ===== PDF verisi (window.PdfReport) — servis cevabından kurulur, DOM'dan okunmaz =====
  function _pdfInfoLines() {
    // Tarih: seçilen ayın rapor günü (#dpLabel yalnızca ayı gösterir); .date-text fallback.
    var date = formatReportDateTr(_selectedReportDate) || ($('#dpLabel').text() || $('.date-text').text() || '').trim();
    var region = (selectedRegion && selectedRegion.name) ? selectedRegion.name : 'Tüm Bölgeler';
    var branch = (selectedBranch && selectedBranch.name) ? selectedBranch.name : 'Tüm Şubeler';
    var type = ($('.segment[data-type].active').text() || '').trim();                                       // Hacim / Adet
    var period = $('.period-toggle:visible').length ? ($('[data-period].active').text() || '').trim() : '';   // Bakiye / H / G (Adet'te yok)
    var tab = ($('.tab.active').text() || '').trim();
    var subtab = ($('.sub-tab-bar:visible .sub-tab.active').text() || '').trim();

    var lines = [];
    lines.push((date ? date + ' tarihine ait ' : '') + region + ' / ' + branch);
    var reportType = [type, period].filter(Boolean).join(' - ');
    if (reportType) lines.push('Rapor Türü: ' + reportType);
    var segment = [tab, subtab].filter(Boolean).join(' - ');
    if (segment) lines.push('Segment: ' + segment);
    return lines;
  }

  function setHomePdfReport(kind, products) {
    // Kırılım modalı (report-detail.js) aktif tab'ın GERÇEK servis cevabını (data.Products)
    // buradan okur; ayrı mock üretmez -> ekranda görünen veri kırılımda da görünür.
    // kind: daily / quantity / monthly (detay ikonundaki data-table ile aynı anahtar).
    window.TargetReportData = window.TargetReportData || {};
    window.TargetReportData[kind] = products || [];
    // Başlık altı tarih (varsa) -> "(gg.aa.yyyy)" — paylaşımlı fmtIsoDate kullanılır
    var _fmtDateHeader = function (v) { return v ? '(' + fmtIsoDate(v) + ')' : ''; };                                 // düz tutar (para birimsiz)
    var _amount = function (v) { return formatNumber(v); };
    var _qty = function (v, p) { return formatNumber(v) + (isRatioProduct(p.ProductName) && v ? '%' : ''); };

    // Fark detayları (ekrandaki .diff-details karşılığı): değerin altına label + value (renksiz, siyah).
    // Toggle ("Farkları Göster") kapalıyken boş döner -> PDF ekranla aynı kalır.
    var _diffBlock = function (items, fmtVal) {
      var inner = items.map(function (it, i) {
        var v = it.value || 0;
        var sep = i ? 'border-left:1px solid #e6eaf0;' : '';
        return '<div style="display:inline-block; text-align:center; padding:0 12px; vertical-align:top;' + sep + '">' +
                 '<div style="font-size:11px; font-weight:300; color:#9aa3b2; margin-bottom:3px; white-space:nowrap;">' + (it.label || '') + '</div>' +
                 '<div style="font-size:12px; font-weight:600; color:#1a1a1a;">' + fmtVal(v) + '</div>' +
               '</div>';
      }).join('');
      return '<div style="margin-top:8px; white-space:nowrap;">' + inner + '</div>';
    };

    var h, cols;
    if (kind === 'monthly') {
      h = window._monthlyHeaders || {};
      var mg = h.MonthGroupTitle;
      var yg = h.YearGroupTitle;
      cols = [
        { header: h.ProductNameTitle, key: 'ProductName', align: 'left' },
        { group: mg, header: h.MonthActualTitle, key: 'MonthActualAmount', format: formatNumber },
        { group: mg, header: h.MonthTargetTitle, key: 'MonthTargetAmount', format: formatNumber },
        { group: mg, header: h.MonthHGTitle, key: 'MonthRatio', format: formatPercent },
        { group: yg, header: h.YearActualTitle, key: 'YearActualAmount', format: formatNumber },
        { group: yg, header: h.YearTargetTitle, key: 'YearTargetAmount', format: formatNumber },
        { group: yg, header: h.YearHGTitle, key: 'YearRatio', format: formatPercent }
      ];
    } else if (kind === 'quantity') {
      h = window._quantityHeaders || {};
      var _qtyDiff = function (p) {
        if ($('#quantityDiffToggle').attr('data-active') !== 'true') return '';
        var pct = isRatioProduct(p.ProductName) ? '%' : '';
        return _diffBlock([
          { label: h.DiffByLastTwoMonthEarlierTitle, value: p.DiffByLastTwoMonthEarlierAmount },
          { label: h.DiffByLastYearTitle, value: p.DiffByLastYearAmount }
        ], function (v) { return formatNumber(v || 0) + ((v || 0) ? pct : ''); });
      };
      cols = [
        { header: h.ProductNameTitle, key: 'ProductName', align: 'left' },
        { header: h.LastMonthTitle, subHeader: _fmtDateHeader(h.LastMonthDate), key: 'LastMonthAmount', format: _qty, extra: _qtyDiff },
        { header: h.LastTwoMonthEarlierTitle, subHeader: _fmtDateHeader(h.LastTwoMonthEarlierDate), key: 'LastTwoMonthEarlierAmount', format: _qty },
        { header: h.LastYearTitle, subHeader: _fmtDateHeader(h.LastYearDate), key: 'LastYearAmount', format: _qty }
      ];
    } else {
      h = window._dailyHeaders || {};
      var _dailyDiff = function (p) {
        if ($('#diffToggle').attr('data-active') !== 'true') return '';
        return _diffBlock([
          { label: h.DiffByPrevDayTitle, value: p.DiffByPrevDayAmount },
          { label: h.DiffByLastWeekTitle, value: p.DiffByLastWeekAmount },
          { label: h.DiffByLastYearTitle, value: p.DiffByLastYearAmount }
        ], function (v) { return formatNumber(v || 0); });
      };
      cols = [
        { header: h.ProductNameTitle, key: 'ProductName', align: 'left' },
        { header: h.YesterdayTitle, subHeader: _fmtDateHeader(h.YesterdayDate), key: 'YesterdayAmount', format: _amount, extra: _dailyDiff },
        { header: h.PrevDayTitle, subHeader: _fmtDateHeader(h.PrevDayDate), key: 'PrevDayAmount', format: _amount },
        { header: h.LastWeekTitle, subHeader: _fmtDateHeader(h.LastWeekDate), key: 'LastWeekAmount', format: _amount },
        { header: h.LastYearTitle, subHeader: _fmtDateHeader(h.LastYearDate), key: 'LastYearAmount', format: _amount }
      ];
    }
    var title = ($('.page-title').text() || 'Rapor').trim();
    window.PdfReport = {
      title: title,
      infoLines: _pdfInfoLines(),
      columns: cols,
      rows: products || [],
      childrenKey: 'SubProducts',
      footerNote: 'Tablodaki değerler /1000 olarak verilmektedir.',
      filename: (title.replace(/[\\/:*?"<>|]+/g, '').trim() || 'Rapor') + '.pdf'
    };
  }

  // ===== Skeleton =====
  function hideSkeleton() {
    hideLoadingOverlay();
    $('#pageSkeleton').fadeOut(200, function () {
        $('#pageContent').fadeIn(200, function () {
            updateStripes();
        });
    });
  }

  // ===== Report Loaders =====
  function loadDailyReport() {
      abortXhr(_targetTableXhr);
      _targetTableXhr = $.ajax({
          url: '/TargetReport/GetDailyTargetReport',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify(buildRequest()),
          success: function (data) {
              $('#dailyTableBody').html(buildDailyRows(data.Products, 0, false));
              resetTableScroll('#dailyTableBody');
              setHomePdfReport('daily', data.Products);

              $('#dailyTableBody [data-daily-header]').each(function () {
                  var key = $(this).data('daily-header');
                  if (window._dailyHeaders && window._dailyHeaders[key]) {
                      $(this).text(window._dailyHeaders[key]);
                  }
              });

              var showDiff = $('#diffToggle').attr('data-active') === 'true';
              if (!showDiff) {
                  $('#dailyTableBody .diff-details').hide();
              }
              updateStripes();
              hideSkeleton();
          }
      });
  }

  var monthlyFirstLoad = true;

  function loadMonthlyReport() {
      if (monthlyFirstLoad) {
          $('#monthlyTable').hide();
          $('#monthlyTableSkeleton').show();
          showLoadingOverlay();
      }

      abortXhr(_targetTableXhr);
      _targetTableXhr = $.ajax({
          url: '/TargetReport/GetMonthlyTargetReport',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify(buildRequest()),
          success: function (data) {
              $('#monthlyTableBody').html(buildMonthlyRows(data.Products, 0, false));
              resetTableScroll('#monthlyTableBody');
              setHomePdfReport('monthly', data.Products);
              if (monthlyFirstLoad) {
                  monthlyFirstLoad = false;
                  $('#monthlyTableSkeleton').hide();
                  $('#monthlyTable').show();
                  hideLoadingOverlay();
              }
              updateStripes();
              hideSkeleton();
          }
      });
  }

  // ===== Quantity Report =====
  var quantityHeadersLoaded = false;

  function loadQuantityReport() {
      if (!quantityHeadersLoaded) {
          quantityHeadersLoaded = true;
          loadHeaders('/TargetReport/GetDailyQuantityTargetReportTableHeaders', '_quantityHeaders', function (data) {
              window._quantityHeaders = data;
              $('[data-quantity-header]').each(function () {
                  var key = $(this).data('quantity-header');
                  if (key.indexOf('Date') > -1 && data[key]) {
                      $(this).text('(' + fmtIsoDate(data[key]) + ')');
                  } else if (data[key]) {
                      $(this).text(data[key]);
                  }
              });
          });
      }

      abortXhr(_targetTableXhr);
      _targetTableXhr = $.ajax({
          url: '/TargetReport/GetDailyQuantityTargetReport',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify(buildRequest()),
          success: function (data) {
              $('#quantityTableBody').html(buildQuantityRows(data.Products, 0, false));
              resetTableScroll('#quantityTableBody');
              setHomePdfReport('quantity', data.Products);

              $('#quantityTableBody [data-quantity-header]').each(function () {
                  var key = $(this).data('quantity-header');
                  if (window._quantityHeaders && window._quantityHeaders[key]) {
                      $(this).text(window._quantityHeaders[key]);
                  }
              });

              var showDiff = $('#quantityDiffToggle').attr('data-active') === 'true';
              if (!showDiff) {
                  $('#quantityTableBody .diff-details').hide();
              }
              updateStripes();
              hideSkeleton();
          }
      });
  }

  $('#quantityDiffToggle').on('click', function () {
      var isActive = $(this).attr('data-active') === 'true';
      $(this).attr('data-active', isActive ? 'false' : 'true');
      if (isActive) {
          $('#quantityTableBody .diff-details').hide();
      } else {
          $('#quantityTableBody .diff-details').show();
      }
      updateStripes();
  });
  $('#quantityDiffToggle').attr('data-active', 'true');

  // ===== Sorting =====
  $(document).on('click', '#dailyTable thead th, #monthlyTable thead th, #quantityTable thead th', function () {
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
      updateBireyselOzelVisibility();
      showLoadingOverlay();
      loadActiveReport();
  });

  $('.sub-tab').on('click', function () {
      $(this).closest('.sub-tab-bar').find('.sub-tab').removeClass('active');
      $(this).addClass('active');
      showLoadingOverlay();
      loadActiveReport();
  });

  // ===== Hacim / Adet Toggle =====
  $('.segment[data-type]').on('click', function () {
      $('.segment[data-type]').removeClass('active');
      $(this).addClass('active');

      var type = $(this).data('type');
      var now = new Date();
      var trMonths = ['Ocak','Şubat','Mart','Nisan','Mayıs','Haziran','Temmuz','Ağustos','Eylül','Ekim','Kasım','Aralık'];
      updateBireyselOzelVisibility();
      showLoadingOverlay();
      if (type === 'adet') {
          $('.date-text').text(trMonths[now.getMonth()] + ' ' + now.getFullYear());
          $('.date-badge').text('Bu Ay');
          $('#dailyTable').hide();
          $('#monthlyTable').hide();
          $('[data-period]').closest('.period-toggle').hide();
          $('#quantityTable').show();
          loadQuantityReport();
      } else {
          var dd = String(now.getDate()).padStart(2, '0');
          $('.date-text').text(dd + ' ' + trMonths[now.getMonth()] + ' ' + now.getFullYear());
          $('.date-badge').text('Bugün');
          $('#quantityTable').hide();
          $('[data-period]').closest('.period-toggle').show();
          $('[data-period]').removeClass('active');
          $('[data-period="daily"]').addClass('active');
          $('#monthlyTable').hide();
          $('#dailyTable').show();
          $('#diffToggle').show();
          loadDailyReport();
      }
      updateStripes();
  });

  // ===== Period Toggle (Günlük / Aylık) =====
  $('[data-period]').on('click', function () {
      var period = $(this).data('period');
      $('[data-period]').removeClass('active');
      $('[data-period="' + period + '"]').addClass('active');

      var now = new Date();
      var trMonths = ['Ocak','Şubat','Mart','Nisan','Mayıs','Haziran','Temmuz','Ağustos','Eylül','Ekim','Kasım','Aralık'];

      // Adet seçiliyken tablo gösterme
      var activeType = $('.segment[data-type].active').data('type');
      if (activeType === 'adet') return;

      showLoadingOverlay();
      if (period === 'monthly') {
          $('.date-text').text(trMonths[now.getMonth()] + ' ' + now.getFullYear());
          $('.date-badge').text('Bu Ay');
          $('#dailyTable').hide();
          $('#quantityTable').hide();
          $('#monthlyTable').show();
          $('#diffToggle').hide();
          if (!monthlyHeadersLoaded) {
              monthlyHeadersLoaded = true;
              loadHeaders('/TargetReport/GetMonthlyTargetReportTableHeaders', '_monthlyHeaders', function (data) {
                  window._monthlyHeaders = data;
                  $('[data-monthly-header]').each(function () {
                      var key = $(this).data('monthly-header');
                      if (data[key]) $(this).text(data[key]);
                  });
                  loadMonthlyReport();   // başlıklar geldikten sonra (PDF gerçek başlıkları kullansın)
              });
          } else {
              loadMonthlyReport();
          }
      } else {
          var dd = String(now.getDate()).padStart(2, '0');
          $('.date-text').text(dd + ' ' + trMonths[now.getMonth()] + ' ' + now.getFullYear());
          $('.date-badge').text('Bugün');
          $('#monthlyTable').hide();
          $('#quantityTable').hide();
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
      if (isActive) {
          $('#dailyTableBody .diff-details').hide();
      } else {
          $('#dailyTableBody .diff-details').show();
      }
      updateStripes();
  });

  $('#diffToggle').attr('data-active', 'true');

  // ===== Striping =====
  function updateStripes() {
      $('.table-container:visible .data-table').each(function () {
          var $table = $(this);
          var stripeIndex = 0;
          var mainIndex = 0;
          var $lastVisible = null;
          $table.find('tbody tr').each(function () {
              var $tr = $(this);
              $tr.removeClass('stripe-odd stripe-even last-visible-row');
              if ($tr.hasClass('sub-row') && !$tr.hasClass('visible')) return;
              stripeIndex++;
              if (!$tr.hasClass('sub-row')) {
                  mainIndex++;
                  $tr.find('td:first').text(mainIndex);
              }
              $tr.addClass(stripeIndex % 2 === 1 ? 'stripe-odd' : 'stripe-even');
              $lastVisible = $tr;
          });
          if ($lastVisible) $lastVisible.addClass('last-visible-row');
      });
      var _q = $('#searchInput').val();
      if (_q && _q.trim()) $('#searchInput').trigger('input');
  }

  // ===== Expandable Rows =====
  $(document).on('click', '.expandable', function () {
      var $row = $(this);
      var parentDepth = getDepth($row);

      $row.toggleClass('expanded');

      if ($row.hasClass('expanded')) {
          var childDepth = parentDepth + 1;
          $row.nextAll('tr').each(function () {
              var d = getDepth($(this));
              if (d <= parentDepth) return false;
              if (d === childDepth) $(this).addClass('visible');
          });
      } else {
          $row.nextAll('tr').each(function () {
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
  handleTableSearch('#searchInput');

  // ===== Region/Branch Filters =====
  function renderRegionDropdown() {
      return renderRegionList('#indexRegionList', selectedRegion ? selectedRegion.code : null);
  }

  function renderBranchDropdown() {
      return renderBranchList('#indexBranchList', selectedBranch ? selectedBranch.code : null, selectedRegion ? selectedRegion.code : null);
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

      renderBranchDropdown();
      persistSelection();
      showLoadingOverlay();
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

      persistSelection();
      showLoadingOverlay();
      loadActiveReport();
  });

  // ===== Init =====
  loadTodayDate(function () {
      // İstekte kullanılacak rapor tarihi başlangıçta _todayDate (session); ay seçilince güncellenir.
      _selectedReportDate = _todayDate;
      // Date picker ay modunda (Skor Kart'taki gibi): kapanan aylarda ayın son takvim günü,
      // içinde bulunulan ayda son veri günü (_todayDate) rapor tarihi olarak gider.
      if (window.DatePicker) {
          DatePicker.init({
              mode: 'month',
              initial: _todayDate,
              max: _todayDate,
              onChange: function (iso) {
                  _selectedReportDate = iso;
                  showLoadingOverlay();
                  loadActiveReport();
              }
          });
      }
      var savedSelection = initFilterSelection();

      loadRegionFilters(function () {
          if (savedSelection.region && findRegion(savedSelection.region.code)) {
              selectedRegion = savedSelection.region;
              $('#indexRegionLabel').text(selectedRegion.name);
          }
          var single = renderRegionDropdown();
          if (single) {
              selectedRegion = { code: single.Code, name: single.Name };
          }
          loadBranchFilters(function () {
              if (savedSelection.branch && findBranch(savedSelection.branch.code, selectedRegion ? selectedRegion.code : null)) {
                  selectedBranch = savedSelection.branch;
                  $('#indexBranchLabel').text(selectedBranch.name);
              }
              var singleBranch = renderBranchDropdown();
              if (singleBranch) {
                  selectedBranch = { code: singleBranch.Code, name: singleBranch.Name };
              }
              persistSelection();
              loadDailyReport();
          });
      });
  });

  updateStripes();

  // ===== Top 10 Modal =====
  var currentTop10ProductId = null;
  var currentTop10FilterType = 0; // 0: Günlük, 1: Haftalık

  function formatTop10Value(val) {
    var prefix = val >= 0 ? '+' : '';
    return prefix + new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(val);
  }

  function showLoadingOverlay() {
    $('body').loading({
      stoppable: false,
      message: '<div><div class="brand-spinner"></div><p class="loading-text">Yükleniyor<span class="loading-dots"><span>.</span><span>.</span><span>.</span></span></p></div>'
    });
  }

  function hideLoadingOverlay() {
    $('body').loading('stop');
  }

  function loadTop10Data(productId, filterType, openModal) {
    $('#top10First').html('');
    $('#top10Last').html('');
    showLoadingOverlay();
    abortXhr(_targetTop10Xhr);
    _targetTop10Xhr = $.ajax({
      url: '/TargetReport/GetProductTop10DailyAndWeeklyDifferences',
      type: 'POST',
      contentType: 'application/json',
      data: JSON.stringify({
        productId,
        filterType,
        regionId: selectedRegion ? [selectedRegion.code] : [],
        branchId: selectedBranch ? [selectedBranch.code] : [],
        subTabId: getActiveSubTabId(),
        tabId: getActiveTabId()
      }),
      success: function (data) {
        var firstHtml = '';
        var lastHtml = '';
        if (data.First10 && data.First10.length > 0) {
          for (var i = 0; i < data.First10.length; i++) {
            var f = data.First10[i];
            firstHtml += '<div class="top10-row"><span class="top10-id">' + (f.CompanyId || '') + '</span><span class="top10-name">' + (f.CompanyName || '') + '</span><span class="top10-value positive">' + formatTop10Value(f.Value) + '</span></div>';
          }
        }
        if (data.Last10 && data.Last10.length > 0) {
          for (var j = 0; j < data.Last10.length; j++) {
            var l = data.Last10[j];
            lastHtml += '<div class="top10-row"><span class="top10-id">' + (l.CompanyId || '') + '</span><span class="top10-name">' + (l.CompanyName || '') + '</span><span class="top10-value negative">' + formatTop10Value(l.Value) + '</span></div>';
          }
        }
        $('#top10First').html(firstHtml);
        $('#top10Last').html(lastHtml);
        hideLoadingOverlay();
        if (openModal) $('#reportDetailOverlay').addClass('active');
      },
      error: function () {
        hideLoadingOverlay();
      }
    });
  }

  // Kırılım verisi: bulunulan tablonun (daily/quantity/monthly) servisine ana request + ek
  // productId + userCode ile istek at; gelen Products'ı global'e koyup detail.js'e event ile bildir.
  var BREAKDOWN_URLS = {
      daily: '/TargetReport/GetDailyTargetReport',
      quantity: '/TargetReport/GetDailyQuantityTargetReport',
      monthly: '/TargetReport/GetMonthlyTargetReport'
  };
  function loadBreakdown(tableKey, productId) {
      var url = BREAKDOWN_URLS[tableKey] || BREAKDOWN_URLS.daily;
      var body = $.extend({}, buildRequest(), {
          productId: productId,
          userCode: window.USER_CODE   // session User.DomainName (Index.cshtml -> window.USER_CODE)
      });
      abortXhr(_targetBreakdownXhr);
      _targetBreakdownXhr = $.ajax({ url: url, type: 'POST', contentType: 'application/json', data: JSON.stringify(body) })
          .done(function (data) { window.ReportBreakdownData = (data && data.Products) || []; })
          .fail(function () { window.ReportBreakdownData = []; })
          .always(function () { $(document).trigger('reportBreakdown:loaded', { table: tableKey }); });
  }

  // Detay ikonu (tüm tablolarda): kırılım modalını açar. Kırılımı report-detail.js render eder.
  // Top-10 verisi/sekmesi yalnızca hacim/bakiye (daily) + TOP10 ürününde (data-top10="1") yüklenir.
  $(document).on('click', '.detail-icon', function (e) {
    e.stopPropagation();
    var productName = $(this).data('product-name');
    var isTop10 = String($(this).data('top10')) === '1';
    $('.report-detail-title').text(productName);

    // Bölge/Şube/Portföy kırılımı: modal her satırda bu sekmeyle açılır, veriyi burada yükleriz.
    loadBreakdown($(this).data('table') || 'daily', $(this).data('product-id'));

    if (isTop10) {
      currentTop10ProductId = $(this).data('product-id');
      currentTop10FilterType = 0;
      $('[data-top10-period]').removeClass('active');
      $('[data-top10-period="daily"]').addClass('active');
      loadTop10Data(currentTop10ProductId, 0, true);   // başarınca modalı açar
    } else {
      $('#reportDetailOverlay').addClass('active');
    }
  });

  // Modalı kapatma (X + overlay tıklama) artık ortak report-detail.js'te — her iki sayfada çalışır.

  $('[data-top10-period]').on('click', function () {
    $('[data-top10-period]').removeClass('active');
    $(this).addClass('active');
    currentTop10FilterType = $(this).data('top10-period') === 'weekly' ? 1 : 0;
    loadTop10Data(currentTop10ProductId, currentTop10FilterType);
  });

  // Rapor tarihini paylaşımlı usd-kur-modal.js'e ilet (closure üzerinden güncel değer döner)
  window._getUsdKurDate = function () { return _selectedReportDate; };
});
