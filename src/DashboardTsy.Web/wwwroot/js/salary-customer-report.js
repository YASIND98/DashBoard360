$(document).ready(function () {

  showLoadingOverlay();

  var selectedRegion = null;
  var selectedBranch = null;
  var currentSortBy;
  var currentSortState = null;
  var bankShareCustomerType = 1; // 1: Maaş Müşterileri, 2: Emekli Müşterileri

  // ===== mock Data (servisler hazır olana kadar ekranı beslemek için) =====
  var MOCK_TABS = [
      { TabId: 1, TabName: 'Hacim', ParentId: 0, TabLevel: 1 },
      { TabId: 2, TabName: 'Çapraz Satış Gelişimi', ParentId: 0, TabLevel: 1 },
      { TabId: 3, TabName: 'Banka Payı', ParentId: 0, TabLevel: 1 },

      { TabId: 10, TabName: 'Tümü', ParentId: 1, TabLevel: 2 },
      { TabId: 11, TabName: 'Kurumsal', ParentId: 1, TabLevel: 2 },
      { TabId: 12, TabName: 'Ticari', ParentId: 1, TabLevel: 2 },
      { TabId: 13, TabName: 'KOBİ', ParentId: 1, TabLevel: 2 },
      { TabId: 14, TabName: 'Tarım', ParentId: 1, TabLevel: 2 },
      { TabId: 15, TabName: 'Bireysel', ParentId: 1, TabLevel: 2 },

      { TabId: 20, TabName: 'Tümü', ParentId: 2, TabLevel: 2 },
      { TabId: 21, TabName: 'Kurumsal', ParentId: 2, TabLevel: 2 },
      { TabId: 22, TabName: 'Ticari', ParentId: 2, TabLevel: 2 },
      { TabId: 23, TabName: 'KOBİ', ParentId: 2, TabLevel: 2 },
      { TabId: 24, TabName: 'Tarım', ParentId: 2, TabLevel: 2 },
      { TabId: 25, TabName: 'Bireysel', ParentId: 2, TabLevel: 2 },

      { TabId: 30, TabName: 'Tümü', ParentId: 3, TabLevel: 2 },
      { TabId: 31, TabName: 'Kurumsal', ParentId: 3, TabLevel: 2 },
      { TabId: 32, TabName: 'Ticari', ParentId: 3, TabLevel: 2 },
      { TabId: 33, TabName: 'KOBİ', ParentId: 3, TabLevel: 2 },
      { TabId: 34, TabName: 'Tarım', ParentId: 3, TabLevel: 2 },
      { TabId: 35, TabName: 'Bireysel', ParentId: 3, TabLevel: 2 }
  ];

  var MOCK_VOLUME_HEADERS = {
      ProductColumnName: 'Ürün',
      LastYearColumnName: 'Geçen Yıl', LastYearColumnDate: '2025-07-13T00:00:00',
      LastYearDifferenceColumnName: 'Geçen Yıla Göre Fark',
      LastWeekColumnName: 'Geçen Hafta', LastWeekColumnDate: '2026-07-06T00:00:00',
      LastWeekDifferenceColumnName: 'Geçen Haftaya Göre Fark',
      PreviousDayColumnName: 'Önceki Gün', PreviousDayColumnDate: '2026-07-11T00:00:00',
      PreviousDayDifferenceColumnName: 'Önceki Güne Göre Fark',
      YesterdayColumnName: 'Dün', YesterdayColumnDate: '2026-07-12T00:00:00',
      SalaryLabel: 'Maaş', RetiredLabel: 'Emekli'
  };

  var MOCK_CROSSSELL_HEADERS = {
      ProductColumnName: 'Ürün',
      LastYearColumnName: 'Geçen Yıl', LastYearColumnDate: '2025-07-31T00:00:00',
      LastYearDifferenceColumnName: 'Geçen Yıla Göre Fark',
      TwoMonthsAgoColumnName: 'İki Ay Önce', TwoMonthsAgoColumnDate: '2026-05-31T00:00:00',
      TwoMonthsAgoDifferenceColumnName: 'İki Ay Önceye Göre Fark',
      LastMonthColumnName: 'Geçen Ay', LastMonthColumnDate: '2026-06-30T00:00:00',
      SalaryLabel: 'Maaş', RetiredLabel: 'Emekli'
  };

  var MOCK_BANKSHARE_HEADERS = {
      ProductColumnName: 'Ürün',
      DenizbankCreditGroupName: 'Denizbank Kredisi Olan',
      OtherBanksCreditGroupName: 'Kredisi Diğer Bankalarda Olan',
      WalletShareGroupName: 'Cüzdan Payı',
      FirstMonthName: "Haziran'26",
      SecondMonthName: "Temmuz'26",
      SalaryCustomersLabel: 'Maaş Müşterileri',
      RetiredCustomersLabel: 'Emekli Müşterileri'
  };

  function pct(part, total) {
      if (!total) return 0;
      return Math.round((part / total) * 1000) / 10;
  }

  function mkPeriod(prefix, total, salaryShare, diff, withDiff) {
      var salary = Math.round(total * salaryShare * 100) / 100;
      var retired = Math.round((total - salary) * 100) / 100;
      var o = {};
      o[prefix + 'TotalAmount'] = total;
      o[prefix + 'SalaryAmount'] = salary;
      o[prefix + 'SalaryRate'] = pct(salary, total);
      o[prefix + 'RetiredAmount'] = retired;
      o[prefix + 'RetiredRate'] = pct(retired, total);
      if (withDiff) {
          o[prefix + 'TotalDifference'] = diff.t;
          o[prefix + 'TotalDifferenceStatus'] = diff.t >= 0 ? 1 : 2;
          o[prefix + 'SalaryDifference'] = diff.s;
          o[prefix + 'SalaryDifferenceStatus'] = diff.s >= 0 ? 1 : 2;
          o[prefix + 'RetiredDifference'] = diff.r;
          o[prefix + 'RetiredDifferenceStatus'] = diff.r >= 0 ? 1 : 2;
      }
      return o;
  }

  var VOLUME_ROW_DEFS = [
      { name: 'Çalışma Büyüklüğü', share: 0.62, ly: 32609591452, lw: 810, pd: 828, y: 834, dLy: { t: 54, s: 36, r: 18 }, dLw: { t: 24, s: 16, r: 8 }, dPd: { t: 6, s: 4, r: 2 } },
      { name: 'Aktif Büyüklük', share: 0.58, ly: 610, lw: 632, pd: 645, y: 651, dLy: { t: 41, s: 26, r: 15 }, dLw: { t: 19, s: 12, r: 7 }, dPd: { t: 6, s: 4, r: 2 } },
      { name: 'Vadesiz TL', share: 0.71, ly: 298, lw: 305, pd: 311, y: 309, dLy: { t: 11, s: 9, r: 2 }, dLw: { t: 6, s: 5, r: 1 }, dPd: { t: -2, s: -1, r: -1 } },
      { name: 'Vadeli TL', share: 0.55, ly: 462, lw: 470, pd: 481, y: 484, dLy: { t: 22, s: 13, r: 9 }, dLw: { t: 14, s: 8, r: 6 }, dPd: { t: 3, s: 2, r: 1 } },
      { name: 'Vadesiz YP', share: 0.40, ly: 88, lw: 91, pd: 90, y: 87, dLy: { t: -1, s: -1, r: 0 }, dLw: { t: -4, s: -2, r: -2 }, dPd: { t: -3, s: -2, r: -1 } },
      { name: 'Vadeli YP', share: 0.47, ly: 132, lw: 137, pd: 141, y: 143, dLy: { t: 11, s: 6, r: 5 }, dLw: { t: 6, s: 3, r: 3 }, dPd: { t: 2, s: 1, r: 1 } }
  ];

  var MOCK_VOLUME_ROWS = VOLUME_ROW_DEFS.map(function (d, i) {
      return $.extend({ Id: i + 1, SortOrder: i + 1, ProductName: d.name },
          mkPeriod('LastYear', d.ly, d.share, d.dLy, true),
          mkPeriod('LastWeek', d.lw, d.share, d.dLw, true),
          mkPeriod('PreviousDay', d.pd, d.share, d.dPd, true),
          mkPeriod('Yesterday', d.y, d.share, null, false));
  });

  var CROSSSELL_ROW_DEFS = [
      { name: 'Toplam Müşteri', share: 0.65, ly: 920, tma: 946, lm: 955, dLy: { t: 35, s: 24, r: 11 }, dTma: { t: 9, s: 6, r: 3 } },
      { name: 'Aktif Büyüklük', share: 0.60, ly: 615, tma: 628, lm: 634, dLy: { t: 19, s: 12, r: 7 }, dTma: { t: 6, s: 4, r: 2 } },
      { name: 'Vadesiz TL', share: 0.72, ly: 301, tma: 306, lm: 304, dLy: { t: 3, s: 3, r: 0 }, dTma: { t: -2, s: -1, r: -1 } },
      { name: 'Vadeli TL', share: 0.56, ly: 470, tma: 479, lm: 486, dLy: { t: 16, s: 9, r: 7 }, dTma: { t: 7, s: 4, r: 3 } },
      { name: 'Vadesiz YP', share: 0.41, ly: 89, tma: 90, lm: 86, dLy: { t: -3, s: -2, r: -1 }, dTma: { t: -4, s: -2, r: -2 } },
      { name: 'Vadeli YP', share: 0.48, ly: 134, tma: 139, lm: 141, dLy: { t: 7, s: 4, r: 3 }, dTma: { t: 2, s: 1, r: 1 } }
  ];

  var MOCK_CROSSSELL_ROWS = CROSSSELL_ROW_DEFS.map(function (d, i) {
      return $.extend({ Id: i + 1, SortOrder: i + 1, ProductName: d.name },
          mkPeriod('LastYear', d.ly, d.share, d.dLy, true),
          mkPeriod('TwoMonthsAgo', d.tma, d.share, d.dTma, true),
          mkPeriod('LastMonth', d.lm, d.share, null, false));
  });

  var BANKSHARE_ROW_DEFS = [
      { name: 'Tüketici Kredisi Müşteri (Adet)', valueType: 1, dFirst: 12500, dSecond: 12840, oFirst: 3200, oSecond: 3050, wFirst: 79.6, wSecond: 80.8, wStatus: 1 },
      { name: 'Tüketici Kredisi Müşteri (Hacim)', valueType: 2, dFirst: 845000, dSecond: 872300, oFirst: 210500, oSecond: 198700, wFirst: 80.1, wSecond: 81.4, wStatus: 1 },
      { name: 'Kredi Kartı (Adet)', valueType: 1, dFirst: 28400, dSecond: 28950, oFirst: 9600, oSecond: 9820, wFirst: 74.7, wSecond: 74.6, wStatus: 2 },
      { name: 'Kredi Kartı (Hacim)', valueType: 2, dFirst: 512000, dSecond: 528600, oFirst: 168400, oSecond: 171200, wFirst: 75.3, wSecond: 75.5, wStatus: 1 },
      { name: 'KMH (Adet)', valueType: 1, dFirst: 6100, dSecond: 6080, oFirst: 2450, oSecond: 2510, wFirst: 71.3, wSecond: 70.8, wStatus: 2 },
      { name: 'KMH (Hacim)', valueType: 2, dFirst: 98400, dSecond: 99650, oFirst: 41200, oSecond: 42800, wFirst: 70.5, wSecond: 69.9, wStatus: 2 }
  ];

  function buildBankShareRows(mult) {
      return BANKSHARE_ROW_DEFS.map(function (d, i) {
          return {
              Id: i + 1,
              SortOrder: i + 1,
              ProductName: d.name,
              ValueType: d.valueType,
              DenizbankFirstMonthValue: Math.round(d.dFirst * mult),
              DenizbankSecondMonthValue: Math.round(d.dSecond * mult),
              OtherBanksFirstMonthValue: Math.round(d.oFirst * mult),
              OtherBanksSecondMonthValue: Math.round(d.oSecond * mult),
              WalletShareFirstMonthRate: d.wFirst,
              WalletShareSecondMonthRate: d.wSecond,
              WalletShareSecondMonthRateStatus: d.wStatus
          };
      });
  }

  var MOCK_BANKSHARE_ROWS_SALARY = buildBankShareRows(1);
  var MOCK_BANKSHARE_ROWS_RETIRED = buildBankShareRows(0.35);

  // ===== Tab Helpers =====
  function tabKindFromName(name) {
      name = (name || '').toLocaleLowerCase('tr');
      if (name.indexOf('banka') > -1) return 'bankshare';
      if (name.indexOf('çapraz') > -1) return 'crosssell';
      return 'volume';
  }

  function getActiveMainTabId() {
      return parseInt($('.salary-main-tabs .segment.active').first().data('tab-id')) || 0;
  }

  function getActiveMainTabKind() {
      return $('.salary-main-tabs .segment.active').first().data('tab-kind') || 'volume';
  }

  function getActiveSubTabId() {
      return parseInt($('#salarySubTabList .tab.active').data('subtab-id')) || 0;
  }

  function renderMainTabs(tabs) {
      var mains = tabs.filter(function (t) { return t.TabLevel === 1; })
          .sort(function (a, b) { return (a.TabId || 0) - (b.TabId || 0); });
      var html = '';
      mains.forEach(function (t, i) {
          if (i > 0) html += '<div class="divider"></div>';
          html += '<button class="segment' + (i === 0 ? ' active' : '') + '" data-tab-id="' + t.TabId + '" data-tab-kind="' + tabKindFromName(t.TabName) + '">' + t.TabName + '</button>';
      });
      $('.salary-main-tabs').html(html);
  }

  function renderSubTabs(tabs, mainTabId) {
      var subs = tabs.filter(function (t) { return t.TabLevel === 2 && t.ParentId === mainTabId; })
          .sort(function (a, b) { return (a.TabId || 0) - (b.TabId || 0); });
      var html = '';
      subs.forEach(function (t, i) {
          html += '<button class="tab' + (i === 0 ? ' active' : '') + '" data-subtab-id="' + t.TabId + '">' + t.TabName + '</button>';
      });
      $('#salarySubTabList').html(html);
  }

  function showActiveTableContainer(kind) {
      $('#volumeTableContainer, #crossSellTableContainer, #bankShareTableContainer').hide();
      $('#bankShareCustomerToggleWrap').toggle(kind === 'bankshare');
      if (kind === 'bankshare') $('#bankShareTableContainer').show();
      else if (kind === 'crosssell') $('#crossSellTableContainer').show();
      else $('#volumeTableContainer').show();
  }

  // ===== Request Builders =====
  function buildCommonRequest() {
      return {
          sessionId: '1',
          regionCode: selectedRegion ? selectedRegion.code : null,
          branchCode: selectedBranch ? selectedBranch.code : null,
          subTabId: getActiveSubTabId(),
          reportDate: _todayDate
      };
  }

  function buildDiffSortRequest(diffToggleId) {
      var req = buildCommonRequest();
      req.showDifferences = $(diffToggleId).attr('data-active') === 'true';
      req.sortBy = currentSortBy || 0;
      req.isAscending = currentSortState === 'asc';
      return req;
  }

  // ===== Header Renderers =====
  function renderVolumeHeaders(data) {
      window._volHeaders = data;
      $('[data-vol-header]').each(function () {
          var key = $(this).data('vol-header');
          if (!data[key]) return;
          $(this).text(key.indexOf('Date') > -1 ? fmtIsoDate(data[key]) : data[key]);
      });
  }

  function renderCrossSellHeaders(data) {
      window._csHeaders = data;
      $('[data-cs-header]').each(function () {
          var key = $(this).data('cs-header');
          if (!data[key]) return;
          $(this).text(key.indexOf('Date') > -1 ? fmtIsoDate(data[key]) : data[key]);
      });
  }

  function renderBankShareHeaders(data) {
      window._bsHeaders = data;
      $('[data-bs-header]').each(function () {
          var key = $(this).data('bs-header');
          if (data[key]) $(this).text(data[key]);
      });
  }

  // ===== Row Builders =====
  function formatDiffNumber(value) {
      var formatted = formatNumber(value, false);
      return value > 0 ? '+' + formatted : formatted;
  }

  function buildMetricCell(row, prefix, salaryLabel, retiredLabel, withDiff, extraClass) {
      var name = row.ProductName;
      var html = '<td class="col-metric' + (extraClass ? ' ' + extraClass : '') + '">';
      html += '<div class="sc-metric-flex">';
      html += '<div class="sc-metric-main">';
      html += '<div class="sc-total">' + formatNumber(row[prefix + 'TotalAmount'], true, name) + '</div>';
      html += '<div class="sc-divider"></div>';
      html += '<div class="sc-sub-group">';
      html += '<div class="sc-sub"><span class="sc-sub-amount">' + formatNumber(row[prefix + 'SalaryAmount'], true, name) + '</span><span class="sc-sub-rate">%' + formatPercent(row[prefix + 'SalaryRate']) + '</span></div>';
      html += '<div class="sc-sub"><span class="sc-sub-amount">' + formatNumber(row[prefix + 'RetiredAmount'], true, name) + '</span><span class="sc-sub-rate">%' + formatPercent(row[prefix + 'RetiredRate']) + '</span></div>';
      html += '</div>';
      html += '</div>';

      if (withDiff) {
          var td = row[prefix + 'TotalDifference'] || 0;
          var sd = row[prefix + 'SalaryDifference'] || 0;
          var rd = row[prefix + 'RetiredDifference'] || 0;
          html += '<div class="sc-metric-diff diff-details">';
          html += '<div class="sc-total"><span class="diff-value ' + (td < 0 ? 'negative' : (td > 0 ? 'positive' : '')) + '">' + formatDiffNumber(td) + '</span></div>';
          html += '<div class="sc-divider"></div>';
          html += '<div class="sc-sub-group">';
          html += '<div class="sc-sub"><span class="diff-value ' + (sd < 0 ? 'negative' : (sd > 0 ? 'positive' : '')) + '">' + formatDiffNumber(sd) + '</span></div>';
          html += '<div class="sc-sub"><span class="diff-value ' + (rd < 0 ? 'negative' : (rd > 0 ? 'positive' : '')) + '">' + formatDiffNumber(rd) + '</span></div>';
          html += '</div>';
          html += '</div>';
      }
      html += '</div>';
      html += '</td>';
      return html;
  }

  function buildVolumeRows(products, headers) {
      var html = '';
      products.forEach(function (p, i) {
          html += '<tr class="table-row">';
          html += '<td class="col-index">' + (i + 1) + '</td>';
          html += '<td class="col-left col-product-header">' + p.ProductName +
              '<div class="sc-divider"></div>' +
              '<div class="sc-sub-group">' +
              '<div class="sc-sub"><span class="sc-tag">' + (headers.SalaryLabel || 'Maaş') + '</span></div>' +
              '<div class="sc-sub"><span class="sc-tag">' + (headers.RetiredLabel || 'Emekli') + '</span></div>' +
              '</div>' +
              '</td>';
          html += buildMetricCell(p, 'LastYear', headers.SalaryLabel, headers.RetiredLabel, true, 'col-cmp col-cmp-first');
          html += buildMetricCell(p, 'LastWeek', headers.SalaryLabel, headers.RetiredLabel, true, 'col-cmp');
          html += buildMetricCell(p, 'PreviousDay', headers.SalaryLabel, headers.RetiredLabel, true, 'col-cmp');
          html += buildMetricCell(p, 'Yesterday', headers.SalaryLabel, headers.RetiredLabel, false, 'col-cmp col-cmp-last');
          html += '</tr>';
      });
      return html;
  }

  function buildCrossSellRows(products, headers) {
      var html = '';
      products.forEach(function (p, i) {
          html += '<tr class="table-row">';
          html += '<td class="col-index">' + (i + 1) + '</td>';
          html += '<td class="col-left col-product-header">' + p.ProductName +
              '<div class="sc-divider"></div>' +
              '<div class="sc-sub-group">' +
              '<div class="sc-sub"><span class="sc-tag">' + (headers.SalaryLabel || 'Maaş') + '</span></div>' +
              '<div class="sc-sub"><span class="sc-tag">' + (headers.RetiredLabel || 'Emekli') + '</span></div>' +
              '</div>' +
              '</td>';
          html += buildMetricCell(p, 'LastYear', headers.SalaryLabel, headers.RetiredLabel, true, 'col-cmp col-cmp-first');
          html += buildMetricCell(p, 'TwoMonthsAgo', headers.SalaryLabel, headers.RetiredLabel, true, 'col-cmp');
          html += buildMetricCell(p, 'LastMonth', headers.SalaryLabel, headers.RetiredLabel, false, 'col-cmp col-cmp-last');
          html += '</tr>';
      });
      return html;
  }

  function formatBankShareValue(v, valueType, name) {
      return valueType === 2 ? formatNumber(v, true, name) : formatNumber(v, false);
  }

  function buildBankShareRowsHtml(products) {
      var html = '';
      products.forEach(function (p) {
          var statusClass = p.WalletShareSecondMonthRateStatus === 2 ? 'negative' : 'positive';
          html += '<tr class="table-row">';
          html += '<td class="col-left">' + p.ProductName + '</td>';
          html += '<td>' + formatBankShareValue(p.DenizbankFirstMonthValue, p.ValueType, p.ProductName) + '</td>';
          html += '<td>' + formatBankShareValue(p.DenizbankSecondMonthValue, p.ValueType, p.ProductName) + '</td>';
          html += '<td>' + formatBankShareValue(p.OtherBanksFirstMonthValue, p.ValueType, p.ProductName) + '</td>';
          html += '<td>' + formatBankShareValue(p.OtherBanksSecondMonthValue, p.ValueType, p.ProductName) + '</td>';
          html += '<td>%' + formatPercent(p.WalletShareFirstMonthRate) + '</td>';
          html += '<td class="' + statusClass + '">%' + formatPercent(p.WalletShareSecondMonthRate) + '</td>';
          html += '</tr>';
      });
      return html;
  }

  // ===== PDF verisi (window.PdfReport) — ekranda görünenle aynı veriden kurulur =====
  var lastVolumeRows = [];
  var lastCrossSellRows = [];
  var lastBankShareRows = [];

  function pdfMetricCellHtml(row, prefix) {
      var name = row.ProductName;
      return '<div style="font-weight:600;">' + formatNumber(row[prefix + 'TotalAmount'], true, name) + '</div>' +
          '<div style="margin-top:4px; font-size:11px; color:#5a6275; white-space:nowrap;">' +
              formatNumber(row[prefix + 'SalaryAmount'], true, name) + ' <span style="color:#9aa3b2;">(%' + formatPercent(row[prefix + 'SalaryRate']) + ')</span>' +
          '</div>' +
          '<div style="font-size:11px; color:#5a6275; white-space:nowrap;">' +
              formatNumber(row[prefix + 'RetiredAmount'], true, name) + ' <span style="color:#9aa3b2;">(%' + formatPercent(row[prefix + 'RetiredRate']) + ')</span>' +
          '</div>';
  }

  function pdfDiffExtraHtml(row, prefix, showDiff) {
      if (!showDiff) return '';
      var td = row[prefix + 'TotalDifference'] || 0;
      var sd = row[prefix + 'SalaryDifference'] || 0;
      var rd = row[prefix + 'RetiredDifference'] || 0;
      return '<div style="margin-top:6px; padding-top:6px; border-top:1px solid #eef1f5; font-size:11px; color:#1a1a1a; white-space:nowrap;">' +
          'Fark: ' + formatDiffNumber(td) + ' &nbsp;|&nbsp; Maaş: ' + formatDiffNumber(sd) + ' &nbsp;|&nbsp; Emekli: ' + formatDiffNumber(rd) +
      '</div>';
  }

  function pdfPeriodColumn(header, dateStr, prefix, withDiff, diffOn) {
      var col = {
          header: header,
          subHeader: dateStr ? fmtIsoDate(dateStr) : '',
          key: prefix + 'TotalAmount',
          format: function (_v, row) { return pdfMetricCellHtml(row, prefix); }
      };
      if (withDiff) col.extra = function (row) { return pdfDiffExtraHtml(row, prefix, diffOn); };
      return col;
  }

  function salaryPdfInfoLines(kind) {
      var date = ($('.date-text').text() || '').trim();
      var region = selectedRegion ? selectedRegion.name : 'Tüm Bölgeler';
      var branch = selectedBranch ? selectedBranch.name : 'Tüm Şubeler';
      var mainTab = ($('.salary-main-tabs .segment.active').text() || '').trim();
      var subTab = ($('#salarySubTabList .tab.active').text() || '').trim();

      var lines = [];
      lines.push((date ? date + ' tarihine ait ' : '') + region + ' / ' + branch);
      var segment = [mainTab, subTab].filter(Boolean).join(' - ');
      if (segment) lines.push('Segment: ' + segment);
      if (kind === 'bankshare') {
          lines.push('Müşteri Tipi: ' + (bankShareCustomerType === 2 ? 'Emekli Müşterileri' : 'Maaş Müşterileri'));
      }
      return lines;
  }

  function setSalaryPdfReport(kind, rows) {
      var title = ($('.page-title').first().text() || 'Rapor').trim();
      var safeTitle = title.replace(/[\\/:*?"<>|]+/g, '').trim() || 'Rapor';
      var cols;

      if (kind === 'bankshare') {
          var bh = window._bsHeaders || MOCK_BANKSHARE_HEADERS;
          var valCol = function (key) {
              return { header: '', key: key, format: function (v, row) { return formatBankShareValue(v, row.ValueType, row.ProductName); } };
          };
          cols = [
              { header: bh.ProductColumnName, key: 'ProductName', align: 'left' },
              $.extend(valCol('DenizbankFirstMonthValue'), { group: bh.DenizbankCreditGroupName, header: bh.FirstMonthName }),
              $.extend(valCol('DenizbankSecondMonthValue'), { group: bh.DenizbankCreditGroupName, header: bh.SecondMonthName }),
              $.extend(valCol('OtherBanksFirstMonthValue'), { group: bh.OtherBanksCreditGroupName, header: bh.FirstMonthName }),
              $.extend(valCol('OtherBanksSecondMonthValue'), { group: bh.OtherBanksCreditGroupName, header: bh.SecondMonthName }),
              { group: bh.WalletShareGroupName, header: bh.FirstMonthName, key: 'WalletShareFirstMonthRate', format: function (v) { return '%' + formatPercent(v); } },
              { group: bh.WalletShareGroupName, header: bh.SecondMonthName, key: 'WalletShareSecondMonthRate', format: function (v) { return '%' + formatPercent(v); } }
          ];
          window.PdfReport = {
              title: title, infoLines: salaryPdfInfoLines(kind), columns: cols, rows: rows || [],
              filename: safeTitle + '-BankaPayi.pdf'
          };
      } else if (kind === 'crosssell') {
          var ch = window._csHeaders || MOCK_CROSSSELL_HEADERS;
          var csDiffOn = $('#crossSellDiffToggle').attr('data-active') === 'true';
          cols = [
              { header: ch.ProductColumnName, key: 'ProductName', align: 'left' },
              pdfPeriodColumn(ch.LastYearColumnName, ch.LastYearColumnDate, 'LastYear', true, csDiffOn),
              pdfPeriodColumn(ch.TwoMonthsAgoColumnName, ch.TwoMonthsAgoColumnDate, 'TwoMonthsAgo', true, csDiffOn),
              pdfPeriodColumn(ch.LastMonthColumnName, ch.LastMonthColumnDate, 'LastMonth', false, csDiffOn)
          ];
          window.PdfReport = {
              title: title, infoLines: salaryPdfInfoLines(kind), columns: cols, rows: rows || [],
              filename: safeTitle + '-CaprazSatisGelisimi.pdf'
          };
      } else {
          var vh = window._volHeaders || MOCK_VOLUME_HEADERS;
          var volDiffOn = $('#volumeDiffToggle').attr('data-active') === 'true';
          cols = [
              { header: vh.ProductColumnName, key: 'ProductName', align: 'left' },
              pdfPeriodColumn(vh.LastYearColumnName, vh.LastYearColumnDate, 'LastYear', true, volDiffOn),
              pdfPeriodColumn(vh.LastWeekColumnName, vh.LastWeekColumnDate, 'LastWeek', true, volDiffOn),
              pdfPeriodColumn(vh.PreviousDayColumnName, vh.PreviousDayColumnDate, 'PreviousDay', true, volDiffOn),
              pdfPeriodColumn(vh.YesterdayColumnName, vh.YesterdayColumnDate, 'Yesterday', false, volDiffOn)
          ];
          window.PdfReport = {
              title: title, infoLines: salaryPdfInfoLines(kind), columns: cols, rows: rows || [],
              filename: safeTitle + '-Hacim.pdf'
          };
      }
  }

  // ===== Sorting (client-side, mock veriler üzerinde) =====
  function sortRows(list, kind) {
      if (!currentSortBy) return list;
      var fieldMap = kind === 'volume'
          ? { 1: 'ProductName', 2: 'LastYearTotalAmount', 3: 'LastWeekTotalAmount', 4: 'PreviousDayTotalAmount', 5: 'YesterdayTotalAmount' }
          : { 1: 'ProductName', 2: 'LastYearTotalAmount', 3: 'TwoMonthsAgoTotalAmount', 4: 'LastMonthTotalAmount' };
      var field = fieldMap[currentSortBy];
      if (!field) return list;
      var dir = currentSortState === 'asc' ? 1 : -1;
      list.sort(function (a, b) {
          if (field === 'ProductName') return dir * String(a[field]).localeCompare(String(b[field]), 'tr');
          return dir * ((a[field] || 0) - (b[field] || 0));
      });
      return list;
  }

  // ===== Loaders (servisler yazılana kadar ajax istekleri yorum satırında, mock veriler kullanılıyor) =====
  function loadSalaryTabs(callback) {
      // $.ajax({
      //     url: '/SalaryCustomersReport/GetSalaryCustomerReportTabs',
      //     type: 'GET',
      //     data: { sessionId: '1' },
      //     success: function (data) { callback(data); }
      // });
      callback(MOCK_TABS);
  }

  function loadVolumeHeaders(callback) {
      // $.ajax({
      //     url: '/SalaryCustomersReport/GetSalaryCustomerVolumeReportHeaders',
      //     type: 'POST',
      //     contentType: 'application/json',
      //     data: JSON.stringify({ sessionId: '1', reportDate: _reportDate }),
      //     success: function (data) { callback(data); }
      // });
      callback(MOCK_VOLUME_HEADERS);
  }

  function loadVolumeReport(callback) {
      // $.ajax({
      //     url: '/SalaryCustomersReport/GetSalaryCustomerVolumeReport',
      //     type: 'POST',
      //     contentType: 'application/json',
      //     data: JSON.stringify(buildDiffSortRequest('#volumeDiffToggle')),
      //     success: function (data) { callback(data); }
      // });
      callback(sortRows(MOCK_VOLUME_ROWS.slice(), 'volume'));
  }

  function loadCrossSellHeaders(callback) {
      // $.ajax({
      //     url: '/SalaryCustomersReport/GetSalaryCustomerCrossSellReportHeaders',
      //     type: 'POST',
      //     contentType: 'application/json',
      //     data: JSON.stringify({ sessionId: '1', reportDate: _reportDate }),
      //     success: function (data) { callback(data); }
      // });
      callback(MOCK_CROSSSELL_HEADERS);
  }

  function loadCrossSellReport(callback) {
      // $.ajax({
      //     url: '/SalaryCustomersReport/GetSalaryCustomerCrossSellReport',
      //     type: 'POST',
      //     contentType: 'application/json',
      //     data: JSON.stringify(buildDiffSortRequest('#crossSellDiffToggle')),
      //     success: function (data) { callback(data); }
      // });
      callback(sortRows(MOCK_CROSSSELL_ROWS.slice(), 'crosssell'));
  }

  function loadBankShareHeaders(callback) {
      // $.ajax({
      //     url: '/SalaryCustomersReport/GetSalaryCustomerBankShareReportHeaders',
      //     type: 'POST',
      //     contentType: 'application/json',
      //     data: JSON.stringify({ sessionId: '1', reportDate: _reportDate }),
      //     success: function (data) { callback(data); }
      // });
      callback(MOCK_BANKSHARE_HEADERS);
  }

  function loadBankShareReport(callback) {
      var req = buildCommonRequest();
      req.customerType = bankShareCustomerType;
      // $.ajax({
      //     url: '/SalaryCustomersReport/GetSalaryCustomerBankShareReport',
      //     type: 'POST',
      //     contentType: 'application/json',
      //     data: JSON.stringify(req),
      //     success: function (data) { callback(data); }
      // });
      callback(bankShareCustomerType === 2 ? MOCK_BANKSHARE_ROWS_RETIRED : MOCK_BANKSHARE_ROWS_SALARY);
  }

  // ===== Active Table Loading =====
  function loadActiveTable() {
      showLoadingOverlay();
      var kind = getActiveMainTabKind();

      if (kind === 'bankshare') {
          loadBankShareReport(function (data) {
              $('#bankShareTableBody').html(buildBankShareRowsHtml(data));
              lastBankShareRows = data;
              setSalaryPdfReport('bankshare', data);
              updateStripes();
              hideLoadingOverlay();
          });
      } else if (kind === 'crosssell') {
          loadCrossSellReport(function (data) {
              $('#crossSellTableBody').html(buildCrossSellRows(data, window._csHeaders || MOCK_CROSSSELL_HEADERS));
              var showDiff = $('#crossSellDiffToggle').attr('data-active') === 'true';
              if (!showDiff) $('#crossSellTableBody .diff-details').hide();
              if (showDiff) $('#crossSellTableBody .sc-sub-rate').hide();
              lastCrossSellRows = data;
              setSalaryPdfReport('crosssell', data);
              updateStripes();
              hideLoadingOverlay();
          });
      } else {
          loadVolumeReport(function (data) {
              $('#volumeTableBody').html(buildVolumeRows(data, window._volHeaders || MOCK_VOLUME_HEADERS));
              var showDiff = $('#volumeDiffToggle').attr('data-active') === 'true';
              if (!showDiff) $('#volumeTableBody .diff-details').hide();
              if (showDiff) $('#volumeTableBody .sc-sub-rate').hide();
              lastVolumeRows = data;
              setSalaryPdfReport('volume', data);
              updateStripes();
              hideLoadingOverlay();
          });
      }
  }

  // ===== Main Tab (segmented control) / Sub Tab (tab-bar) Switching =====
  $(document).on('click', '.salary-main-tabs .segment', function () {
      var tabId = $(this).data('tab-id');
      $('.salary-main-tabs .segment').removeClass('active');
      $('.salary-main-tabs .segment[data-tab-id="' + tabId + '"]').addClass('active');

      currentSortBy = undefined;
      currentSortState = null;
      $('.data-table .sort-icon').removeClass('asc desc');
      $('#salarySearchInput').val('');

      showActiveTableContainer($(this).data('tab-kind'));
      renderSubTabs(window._salaryTabs || MOCK_TABS, getActiveMainTabId());
      loadActiveTable();
  });

  $(document).on('click', '#salarySubTabList .tab', function () {
      $('#salarySubTabList .tab').removeClass('active');
      $(this).addClass('active');
      $('#salarySearchInput').val('');
      loadActiveTable();
  });

  // ===== Sorting =====
  $(document).on('click', '#volumeDataTable thead th, #crossSellDataTable thead th', function () {
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

      $(this).closest('table').find('.sort-icon').removeClass('asc desc');
      if (currentSortState) $icon.addClass(currentSortState);

      loadActiveTable();
  });

  // ===== Farkları Göster Toggles =====
  $(document).on('click', '#volumeDiffToggle', function () {
      var willShowDiff = $(this).attr('data-active') !== 'true';
      $(this).attr('data-active', willShowDiff ? 'true' : 'false');
      $('#volumeTableBody .diff-details').toggle(willShowDiff);
      $('#volumeTableBody .sc-sub-rate').toggle(!willShowDiff);
      setSalaryPdfReport('volume', lastVolumeRows);
  });
  $('#volumeDiffToggle').attr('data-active', 'false');

  $(document).on('click', '#crossSellDiffToggle', function () {
      var willShowDiff = $(this).attr('data-active') !== 'true';
      $(this).attr('data-active', willShowDiff ? 'true' : 'false');
      $('#crossSellDataTable .diff-details').toggle(willShowDiff);
      $('#crossSellTableBody .sc-sub-rate').toggle(!willShowDiff);
      setSalaryPdfReport('crosssell', lastCrossSellRows);
  });
  $('#crossSellDiffToggle').attr('data-active', 'false');

  // ===== Banka Payı - Maaş / Emekli Müşteri Toggle =====
  $(document).on('click', '#bankShareCustomerToggle', function () {
      var showRetired = $(this).attr('data-active') !== 'true';
      $(this).attr('data-active', showRetired ? 'true' : 'false');
      bankShareCustomerType = showRetired ? 2 : 1;
      loadActiveTable();
  });

  // ===== Striping =====
  function updateStripes() {
      $('.table-container:visible .data-table').each(function () {
          var stripeIndex = 0;
          var $lastVisible = null;
          $(this).find('tbody tr.table-row:visible').each(function () {
              stripeIndex++;
              $(this).removeClass('stripe-odd stripe-even last-visible-row');
              $(this).addClass(stripeIndex % 2 === 1 ? 'stripe-odd' : 'stripe-even');
              $(this).find('td.col-index').text(stripeIndex);
              $lastVisible = $(this);
          });
          if ($lastVisible) $lastVisible.addClass('last-visible-row');
      });
  }

  // ===== Search =====
  handleTableSearch('#salarySearchInput');

  // ===== Region/Branch Filters =====
  function renderRegionDropdown() {
      return renderRegionList('#salaryRegionList', selectedRegion ? selectedRegion.code : null);
  }

  function renderBranchDropdown() {
      return renderBranchList('#salaryBranchList', selectedBranch ? selectedBranch.code : null, selectedRegion ? selectedRegion.code : null);
  }

  $(document).on('click', '#salaryRegionList .dropdown-item', function () {
      var code = $(this).attr('data-code');
      var name = $(this).text();

      selectedRegion = code ? { code: code, name: name } : null;
      $('#salaryRegionLabel').text(code ? name : 'Bölge');

      selectedBranch = null;
      $('#salaryBranchLabel').text('Şube');

      $('#salaryRegionList .dropdown-item').removeClass('selected');
      $(this).addClass('selected');
      $('#salaryRegionPanel').removeClass('open');
      $('#salaryBranchPanel').removeClass('open');
      $('#salaryRegionSearch').val('');

      renderBranchDropdown();
      loadActiveTable();
  });

  $(document).on('click', '#salaryBranchList .dropdown-item', function () {
      var code = $(this).attr('data-code');
      var name = $(this).text();

      if (!code) {
          selectedBranch = null;
          $('#salaryBranchLabel').text('Şube');
      } else {
          var regionCode = $(this).attr('data-region');
          selectedBranch = { code: code, name: name };
          $('#salaryBranchLabel').text(name);

          var region = findRegion(regionCode);
          if (region) {
              selectedRegion = { code: region.Code, name: region.Name };
              $('#salaryRegionLabel').text(region.Name);
              $('#salaryRegionList .dropdown-item').removeClass('selected');
              $('#salaryRegionList .dropdown-item[data-code="' + region.Code + '"]').addClass('selected');
          }
      }

      $('#salaryBranchList .dropdown-item').removeClass('selected');
      $(this).addClass('selected');
      $('#salaryRegionPanel').removeClass('open');
      $('#salaryBranchPanel').removeClass('open');
      $('#salaryBranchSearch').val('');

      loadActiveTable();
  });

  function showLoadingOverlay() {
      $('body').loading({
          stoppable: false,
          message: '<div><div class="brand-spinner"></div><p class="loading-text">Yükleniyor<span class="loading-dots"><span>.</span><span>.</span><span>.</span></span></p></div>'
      });
  }

  function hideLoadingOverlay() {
      $('body').loading('stop');
  }

  loadTodayDate(function () {
      loadRegionFilters(function () {
          var single = renderRegionDropdown();
          if (single) selectedRegion = { code: single.Code, name: single.Name };
          loadBranchFilters(function () {
              var singleBranch = renderBranchDropdown();
              if (singleBranch) selectedBranch = { code: singleBranch.Code, name: singleBranch.Name };

              loadSalaryTabs(function (tabs) {
                  window._salaryTabs = tabs;
                  renderMainTabs(tabs);
                  renderSubTabs(tabs, getActiveMainTabId());

                  loadVolumeHeaders(function (headers) {
                      renderVolumeHeaders(headers);
                      loadCrossSellHeaders(function (csHeaders) {
                          renderCrossSellHeaders(csHeaders);
                          loadBankShareHeaders(function (bsHeaders) {
                              renderBankShareHeaders(bsHeaders);
                              loadActiveTable();
                          });
                      });
                  });
              });
          });
      });
  });
});
