$(document).ready(function () {

  showLoadingOverlay();

  var selectedRegion = null;
  var selectedBranch = null;
  var currentSortBy;
  var currentSortState = null;
  var bankShareCustomerType = 1; // 1: Maaş Müşterileri, 2: Emekli Müşterileri

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
      var formatted = formatNumber(value);
      return value > 0 ? '+' + formatted : formatted;
  }

  function buildMetricCell(row, prefix, salaryLabel, retiredLabel, withDiff, extraClass) {
      var html = '<td class="col-metric' + (extraClass ? ' ' + extraClass : '') + '">';
      html += '<div class="sc-metric-flex">';
      html += '<div class="sc-metric-main">';
      html += '<div class="sc-total">' + formatNumber(row[prefix + 'TotalAmount']) + '</div>';
      html += '<div class="sc-divider"></div>';
      html += '<div class="sc-sub-group">';
      html += '<div class="sc-sub"><span class="sc-sub-amount">' + formatNumber(row[prefix + 'SalaryAmount']) + '</span><span class="sc-sub-rate">%' + formatPercent(row[prefix + 'SalaryRate']) + '</span></div>';
      html += '<div class="sc-sub"><span class="sc-sub-amount">' + formatNumber(row[prefix + 'RetiredAmount']) + '</span><span class="sc-sub-rate">%' + formatPercent(row[prefix + 'RetiredRate']) + '</span></div>';
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
          html += buildMetricCell(p, 'Yesterday', headers.SalaryLabel, headers.RetiredLabel, false, 'col-cmp col-cmp-first');
          html += buildMetricCell(p, 'PreviousDay', headers.SalaryLabel, headers.RetiredLabel, true, 'col-cmp');
          html += buildMetricCell(p, 'LastWeek', headers.SalaryLabel, headers.RetiredLabel, true, 'col-cmp');
          html += buildMetricCell(p, 'LastYear', headers.SalaryLabel, headers.RetiredLabel, true, 'col-cmp col-cmp-last');
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
          html += buildMetricCell(p, 'LastMonth', headers.SalaryLabel, headers.RetiredLabel, false, 'col-cmp col-cmp-first');
          html += buildMetricCell(p, 'TwoMonthsAgo', headers.SalaryLabel, headers.RetiredLabel, true, 'col-cmp');
          html += buildMetricCell(p, 'LastYear', headers.SalaryLabel, headers.RetiredLabel, true, 'col-cmp col-cmp-last');
          html += '</tr>';
      });
      return html;
  }

  function buildBankShareRowsHtml(products) {
      var html = '';
      products.forEach(function (p) {
          var statusClass = p.WalletShareSecondMonthRateStatus === 2 ? 'negative' : 'positive';
          html += '<tr class="table-row">';
          html += '<td class="col-left">' + p.ProductName + '</td>';
          html += '<td>' + formatNumber(p.DenizbankFirstMonthValue) + '</td>';
          html += '<td>' + formatNumber(p.DenizbankSecondMonthValue) + '</td>';
          html += '<td>' + formatNumber(p.OtherBanksFirstMonthValue) + '</td>';
          html += '<td>' + formatNumber(p.OtherBanksSecondMonthValue) + '</td>';
          html += '<td>%' + formatNumber(p.WalletShareFirstMonthRate) + '</td>';
          html += '<td class="' + statusClass + '">%' + formatNumber(p.WalletShareSecondMonthRate) + '</td>';
          html += '</tr>';
      });
      return html;
  }

  // ===== PDF verisi (window.PdfReport) — ekranda görünenle aynı veriden kurulur =====
  function pdfMetricCellHtml(row, prefix) {
      return '<div style="font-weight:600;">' + formatNumber(row[prefix + 'TotalAmount']) + '</div>' +
          '<div style="margin-top:4px; font-size:11px; color:#5a6275; white-space:nowrap;">' +
              formatNumber(row[prefix + 'SalaryAmount']) + ' <span style="color:#9aa3b2;">(%' + formatPercent(row[prefix + 'SalaryRate']) + ')</span>' +
          '</div>' +
          '<div style="font-size:11px; color:#5a6275; white-space:nowrap;">' +
              formatNumber(row[prefix + 'RetiredAmount']) + ' <span style="color:#9aa3b2;">(%' + formatPercent(row[prefix + 'RetiredRate']) + ')</span>' +
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
          var bh = window._bsHeaders;
          var valCol = function (key) {
              return { header: '', key: key, format: function (v) { return formatNumber(v); } };
          };
          cols = [
              { header: bh.ProductColumnName, key: 'ProductName', align: 'left' },
              $.extend(valCol('DenizbankFirstMonthValue'), { group: bh.DenizbankCreditGroupName, header: bh.FirstMonthName }),
              $.extend(valCol('DenizbankSecondMonthValue'), { group: bh.DenizbankCreditGroupName, header: bh.SecondMonthName }),
              $.extend(valCol('OtherBanksFirstMonthValue'), { group: bh.OtherBanksCreditGroupName, header: bh.FirstMonthName }),
              $.extend(valCol('OtherBanksSecondMonthValue'), { group: bh.OtherBanksCreditGroupName, header: bh.SecondMonthName }),
              { group: bh.WalletShareGroupName, header: bh.FirstMonthName, key: 'WalletShareFirstMonthRate', format: function (v) { return '%' + formatNumber(v); } },
              { group: bh.WalletShareGroupName, header: bh.SecondMonthName, key: 'WalletShareSecondMonthRate', format: function (v) { return '%' + formatNumber(v); } }
          ];
          window.PdfReport = {
              title: title, infoLines: salaryPdfInfoLines(kind), columns: cols, rows: rows || [],
              filename: safeTitle + '-BankaPayi.pdf'
          };
      } else if (kind === 'crosssell') {
          var ch = window._csHeaders;
          var csDiffOn = $('#crossSellDiffToggle').attr('data-active') === 'true';
          cols = [
              { header: ch.ProductColumnName, key: 'ProductName', align: 'left' },
              pdfPeriodColumn(ch.LastMonthColumnName, ch.LastMonthColumnDate, 'LastMonth', false, csDiffOn),
              pdfPeriodColumn(ch.TwoMonthsAgoColumnName, ch.TwoMonthsAgoColumnDate, 'TwoMonthsAgo', true, csDiffOn),
              pdfPeriodColumn(ch.LastYearColumnName, ch.LastYearColumnDate, 'LastYear', true, csDiffOn)
          ];
          window.PdfReport = {
              title: title, infoLines: salaryPdfInfoLines(kind), columns: cols, rows: rows || [],
              filename: safeTitle + '-CaprazSatisGelisimi.pdf'
          };
      } else {
          var vh = window._volHeaders;
          var volDiffOn = $('#volumeDiffToggle').attr('data-active') === 'true';
          cols = [
              { header: vh.ProductColumnName, key: 'ProductName', align: 'left' },
              pdfPeriodColumn(vh.YesterdayColumnName, vh.YesterdayColumnDate, 'Yesterday', false, volDiffOn),
              pdfPeriodColumn(vh.PreviousDayColumnName, vh.PreviousDayColumnDate, 'PreviousDay', true, volDiffOn),
              pdfPeriodColumn(vh.LastWeekColumnName, vh.LastWeekColumnDate, 'LastWeek', true, volDiffOn),
              pdfPeriodColumn(vh.LastYearColumnName, vh.LastYearColumnDate, 'LastYear', true, volDiffOn)
          ];
          window.PdfReport = {
              title: title, infoLines: salaryPdfInfoLines(kind), columns: cols, rows: rows || [],
              filename: safeTitle + '-Hacim.pdf'
          };
      }
  }

  // ===== Loaders =====
  function loadSalaryTabs(callback) {
      $.ajax({
          url: '/SalaryCustomerReport/GetSalaryCustomerReportTabs',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify({ sessionId: '1' }),
          success: function (data) { callback(data); }
      });
  }

  function loadVolumeHeaders(callback) {
      $.ajax({
          url: '/SalaryCustomerReport/GetSalaryCustomerVolumeReportHeaders',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify({ sessionId: '1', reportDate: _todayDate }),
          success: function (data) { callback(data); }
      });
  }

  function loadVolumeReport(callback) {
      $.ajax({
          url: '/SalaryCustomerReport/GetSalaryCustomerVolumeReport',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify(buildDiffSortRequest('#volumeDiffToggle')),
          success: function (data) { callback(data); }
      });
  }

  function loadCrossSellHeaders(callback) {
      $.ajax({
          url: '/SalaryCustomerReport/GetSalaryCustomerCrossSellReportHeaders',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify({ sessionId: '1', reportDate: _todayDate }),
          success: function (data) { callback(data); }
      });
  }

  function loadCrossSellReport(callback) {
      $.ajax({
          url: '/SalaryCustomerReport/GetSalaryCustomerCrossSellReport',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify(buildDiffSortRequest('#crossSellDiffToggle')),
          success: function (data) { callback(data); }
      });
  }

  function loadBankShareHeaders(callback) {
      $.ajax({
          url: '/SalaryCustomerReport/GetSalaryCustomerBankShareReportHeaders',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify({ sessionId: '1', reportDate: _todayDate }),
          success: function (data) { callback(data); }
      });
  }

  function loadBankShareReport(callback) {
      var req = buildCommonRequest();
      req.customerType = bankShareCustomerType;
      $.ajax({
          url: '/SalaryCustomerReport/GetSalaryCustomerBankShareReport',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify(req),
          success: function (data) { callback(data); }
      });
  }

  // ===== Active Table Loading =====
  function loadActiveTable() {
      showLoadingOverlay();
      var kind = getActiveMainTabKind();
      showActiveTableContainer(kind);

      if (kind === 'bankshare') {
          loadBankShareReport(function (data) {
              $('#bankShareTableBody').html(buildBankShareRowsHtml(data));
              setSalaryPdfReport('bankshare', data);
              updateStripes();
              hideLoadingOverlay();
          });
      } else if (kind === 'crosssell') {
          loadCrossSellReport(function (data) {
              $('#crossSellTableBody').html(buildCrossSellRows(data, window._csHeaders));
              var showDiff = $('#crossSellDiffToggle').attr('data-active') === 'true';
              if (!showDiff) $('#crossSellTableBody .diff-details').hide();
              if (showDiff) $('#crossSellTableBody .sc-sub-rate').hide();
              setSalaryPdfReport('crosssell', data);
              updateStripes();
              hideLoadingOverlay();
          });
      } else {
          loadVolumeReport(function (data) {
              $('#volumeTableBody').html(buildVolumeRows(data, window._volHeaders));
              var showDiff = $('#volumeDiffToggle').attr('data-active') === 'true';
              if (!showDiff) {
                  $('#volumeTableBody .diff-details').hide();
                  $('#volumeDataTable thead .diff-details').hide();
              } else {
                  $('#volumeDataTable thead .diff-details').show();
              }
              if (showDiff) $('#volumeTableBody .sc-sub-rate').hide();
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

      renderSubTabs(window._salaryTabs, getActiveMainTabId());
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

  // ===== Farkları Göster Toggles (sunucudan showDifferences'a göre farklı veri geldiği için yeniden istek atılır) =====
  $(document).on('click', '#volumeDiffToggle', function () {
      var willShowDiff = $(this).attr('data-active') !== 'true';
      $(this).attr('data-active', willShowDiff ? 'true' : 'false');
      loadActiveTable();
  });
  $('#volumeDiffToggle').attr('data-active', 'false');

  $(document).on('click', '#crossSellDiffToggle', function () {
      var willShowDiff = $(this).attr('data-active') !== 'true';
      $(this).attr('data-active', willShowDiff ? 'true' : 'false');
      loadActiveTable();
  });
  $('#crossSellDiffToggle').attr('data-active', 'false');

  // Tablet/mobilde legend gizli; fark toggle'ı 3-nokta menüsünden yönetilir (aktif tabloya göre).
  function activeDiffToggle() {
      var kind = getActiveMainTabKind();
      if (kind === 'volume') return '#volumeDiffToggle';
      if (kind === 'crosssell') return '#crossSellDiffToggle';
      return null;   // Banka Payı'nda fark toggle'ı yok
  }

  function syncMobileDiffBtn() {
      var toggle = activeDiffToggle();
      $('#mobileDiffBtn').toggle(!!toggle);
      if (!toggle) return;
      var active = $(toggle).attr('data-active') === 'true';
      $('#mobileDiffBtnText').text(active ? 'Farkları Gizle' : 'Farkları Göster');
  }

  $(document).on('click', '#mobileDiffBtn', function () {
      var toggle = activeDiffToggle();
      if (toggle) $(toggle).trigger('click');
      syncMobileDiffBtn();
      $(this).closest('details').removeAttr('open');
  });

  $(document).on('click', '.salary-main-tabs .segment', function () { setTimeout(syncMobileDiffBtn, 0); });
  syncMobileDiffBtn();

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
  function persistSelection() {
      saveFilterSelection(selectedRegion, selectedBranch);
  }

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
      persistSelection();
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

      persistSelection();
      loadActiveTable();
  });

  function showLoadingOverlay() {
      $('body').loading({
          stoppable: false,
          message: '<div><div class="brand-spinner"></div><p class="loading-text">Yükleniyor<span class="loading-dots"><span>.</span><span>.</span><span>.</span></span></p></div>'
      });
  }

  function hideLoadingOverlay() {
      resetTableScroll('#bankShareTableBody, #crossSellTableBody, #volumeTableBody');
      $('body').loading('stop');
  }

  loadTodayDate(function () {
      var savedSelection = initFilterSelection();

      loadRegionFilters(function () {
          if (savedSelection.region && findRegion(savedSelection.region.code)) {
              selectedRegion = savedSelection.region;
              $('#salaryRegionLabel').text(selectedRegion.name);
          }
          var single = renderRegionDropdown();
          if (single) selectedRegion = { code: single.Code, name: single.Name };
          loadBranchFilters(function () {
              if (savedSelection.branch && findBranch(savedSelection.branch.code, selectedRegion ? selectedRegion.code : null)) {
                  selectedBranch = savedSelection.branch;
                  $('#salaryBranchLabel').text(selectedBranch.name);
              }
              var singleBranch = renderBranchDropdown();
              if (singleBranch) selectedBranch = { code: singleBranch.Code, name: singleBranch.Name };
              persistSelection();

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
