// POS Raporları — /PosReport/GetPosReport'tan gelen düz (metrik x tarih) listeyi
// ekrandaki tabloya çevirir: satırlar metrik, kolonlar ay (yeniden eskiye).
$(document).ready(function () {

  if (!document.getElementById('posDataTable')) return;

  var METRIC_COLUMN_NAME = 'POS Müşterileri';
  var INVERTED_DIFF_METRIC = 'İptal Adedi';
  var SCORECARD_COLUMN_NAME = 'Skorkart';
  var SCORECARD_ROWS = [
      { key: 'Achievement', label: 'Gerçekleşen', diffKey: 'DiffValue' },
      { key: 'Target', label: 'Hedef' },
      { key: 'TaRate', label: 'H/G %' }
  ];

  var selectedRegion = null;
  var selectedBranch = null;

  var _dates = [];
  var _rows = [];

  var _scDates = [];
  var _scRows = [];

  var _products = [];
  var _selectedProduct = null;
  var _productsLoaded = false;

  var _view = 'count';
  var _countStale = false;
  var _scorecardStale = true;

  function isCountView() {
      return _view === 'count';
  }

  function getActiveTabId() {
      return parseInt($('#posTabList .tab.active').data('tab-id'), 10) || 0;
  }

  function getActiveTabName() {
      return ($('#posTabList .tab.active').text() || '').trim();
  }

  // ===== Pivot =====
  function dateColumns(dateKeys) {
      return dateKeys
          .sort(function (a, b) { return a < b ? 1 : (a > b ? -1 : 0); })
          .map(function (key) {
              var parts = key.split('-');
              return {
                  key: key,
                  month: _trMonths[parseInt(parts[1], 10) - 1] || '',
                  date: fmtIsoDate(key)
              };
          });
  }

  function groupByMetricAndDate(items) {
      var dateKeys = [];
      var metrics = [];
      var cells = {};

      (items || []).forEach(function (it) {
          var metric = it.Metrics || '';
          var dateKey = String(it.Date || '').substring(0, 10);
          if (!metric || !dateKey) return;

          if (dateKeys.indexOf(dateKey) === -1) dateKeys.push(dateKey);
          if (metrics.indexOf(metric) === -1) metrics.push(metric);

          cells[metric] = cells[metric] || {};
          cells[metric][dateKey] = it;
      });

      return { dateKeys: dateKeys, metrics: metrics, cells: cells };
  }

  function pivot(items) {
      var grouped = groupByMetricAndDate(items);
      _dates = dateColumns(grouped.dateKeys);

      _rows = grouped.metrics.map(function (metric) {
          var row = { metric: metric };
          _dates.forEach(function (d, i) {
              var item = (grouped.cells[metric] || {})[d.key];
              row['d' + i] = { value: item ? item.Value : null, diff: item ? item.DiffValue : null };
          });
          return row;
      });
  }

  function pivotScorecard(items) {
      var grouped = groupByMetricAndDate(items);
      var products = grouped.metrics;
      _scDates = dateColumns(grouped.dateKeys);

      _scRows = [];
      products.forEach(function (product) {
          SCORECARD_ROWS.forEach(function (measure) {
              var row = {
                  label: products.length > 1 ? product + ' - ' + measure.label : measure.label,
                  key: measure.key,
                  diffKey: measure.diffKey
              };
              _scDates.forEach(function (d, i) {
                  var item = (grouped.cells[product] || {})[d.key];
                  row['d' + i] = item ? item[measure.key] : null;
                  if (measure.diffKey) row['diff' + i] = item ? item[measure.diffKey] : null;
              });
              _scRows.push(row);
          });
      });
  }

  // ===== Formatters =====
  function formatDiffNumber(value) {
      var formatted = formatNumber(value);
      return value > 0 ? '+' + formatted : formatted;
  }

  function diffClass(value, metric) {
      if (!value) return '';
      var isGood = metric === INVERTED_DIFF_METRIC ? value < 0 : value > 0;
      return isGood ? 'positive' : 'negative';
  }

  function scorecardValueText(key, value) {
      if (value == null) return '-';
      if (key === 'TaRate') return '%' + formatPercent(value * 100);
      return formatNumber(value);
  }

  function diffCellHtml(text, diff, metric) {
      var diffHtml = diff == null
          ? '<div class="diff-value">&nbsp;</div>'
          : '<div class="diff-value ' + diffClass(diff, metric) + '">' + formatDiffNumber(diff) + '</div>';

      return '<td class="has-diff">' + text + diffHtml + '</td>';
  }

  function emptyRowHtml(colspan) {
      return '<tr class="pos-empty-row"><td colspan="' + colspan + '" style="text-align:center;padding:48px 16px;">' +
                 '<div class="table-empty-state">' +
                     '<img src="/images/empty-state-seach.svg" alt="" />' +
                     '<span>Seçili kırılıma ait veri bulunmamaktadır.</span>' +
                 '</div>' +
             '</td></tr>';
  }

  // ===== Render =====
  function renderReportTable(table) {
      var head = '<tr><th class="col-left">' + table.title + '</th>';
      table.dates.forEach(function (d) {
          head += '<th><span>' + d.month + '</span><small>' + d.date + '</small></th>';
      });
      $(table.head).html(head + '</tr>');

      if (!table.rows.length) {
          $(table.body).html(emptyRowHtml(table.dates.length + 1));
          return;
      }

      var html = '';
      table.rows.forEach(function (row) {
          html += '<tr class="table-row"><td class="col-left">' + table.label(row) + '</td>';
          table.dates.forEach(function (_d, i) {
              html += table.cell(row, i);
          });
          html += '</tr>';
      });

      reStripeTable($(table.body).html(html));
      applyDiffVisibility();
  }

  function renderTable() {
      renderReportTable({
          head: '#posTableHead',
          body: '#posTableBody',
          title: METRIC_COLUMN_NAME,
          dates: _dates,
          rows: _rows,
          label: function (row) { return row.metric; },
          cell: function (row, i) {
              var cell = row['d' + i] || {};
              return diffCellHtml(formatNumber(cell.value), cell.diff, row.metric);
          }
      });
  }

  function renderScorecardTable() {
      renderReportTable({
          head: '#posScoreCardHead',
          body: '#posScoreCardBody',
          title: SCORECARD_COLUMN_NAME,
          dates: _scDates,
          rows: _scRows,
          label: function (row) { return row.label; },
          cell: function (row, i) {
              return diffCellHtml(scorecardValueText(row.key, row['d' + i]), row.diffKey ? row['diff' + i] : null);
          }
      });
  }

  function applyDiffVisibility() {
      $('#posTableBody .diff-value').toggle($('#posDiffToggle').attr('data-active') === 'true');
      $('#posScoreCardBody .diff-value').toggle($('#posScoreCardDiffToggle').attr('data-active') === 'true');
  }

  $(document).on('click', '#posDiffToggle, #posScoreCardDiffToggle', function () {
      $(this).attr('data-active', $(this).attr('data-active') === 'true' ? 'false' : 'true');
      applyDiffVisibility();
  });

  // ===== PDF (window.PdfReport) =====
  function pdfInfoLines(reportType) {
      var date = ($('.date-text').text() || '').trim();
      var region = selectedRegion ? selectedRegion.name : 'Tüm Bölgeler';
      var branch = selectedBranch ? selectedBranch.name : 'Tüm Şubeler';
      var segment = getActiveTabName();

      var lines = [(date ? date + ' tarihine ait ' : '') + region + ' / ' + branch];
      lines.push('Rapor Türü: ' + reportType);
      if (segment) lines.push('Segment: ' + segment);
      return lines;
  }

  function pdfCellHtml(text, diff, metric) {
      var html = '<div style="white-space:nowrap;">' + text + '</div>';
      if (diff == null) return html;

      var cls = diffClass(diff, metric);
      var color = cls === 'negative' ? '#f12831' : (cls === 'positive' ? '#27b857' : '#5a6275');
      return html + '<div style="font-size:11px; color:' + color + '; white-space:nowrap;">' +
          formatDiffNumber(diff) + '</div>';
  }

  function pdfDateColumns(dates, cell) {
      return dates.map(function (d, i) {
          return {
              header: d.month,
              subHeader: d.date,
              key: 'd' + i,
              format: function (value, row) { return cell(value, row, i); }
          };
      });
  }

  function setPdfReport(pdf) {
      window.PdfReport = {
          title: 'POS Raporları',
          infoLines: pdfInfoLines(pdf.reportType),
          columns: [pdf.firstColumn].concat(pdfDateColumns(pdf.dates, pdf.cell)),
          rows: pdf.rows,
          filename: 'POS-Raporlari.pdf'
      };
  }

  function setCountPdf() {
      setPdfReport({
          reportType: 'Adet Raporu',
          firstColumn: { header: METRIC_COLUMN_NAME, key: 'metric', align: 'left' },
          dates: _dates,
          rows: _rows,
          cell: function (value, row) {
              var cell = value || {};
              return pdfCellHtml(formatNumber(cell.value), cell.diff, row ? row.metric : '');
          }
      });
  }

  function setScorecardPdf() {
      setPdfReport({
          reportType: 'Skorkart',
          firstColumn: { header: SCORECARD_COLUMN_NAME, key: 'label', align: 'left' },
          dates: _scDates,
          rows: _scRows,
          cell: function (value, row, i) {
              return pdfCellHtml(scorecardValueText(row ? row.key : '', value), row && row.diffKey ? row['diff' + i] : null);
          }
      });
  }

  // ===== Skorkart ürün filtresi (GetPosScorecardFilters) =====
  var _productFilterMq = window.matchMedia('(max-width: 1199px)');

  function relocateProductFilter() {
      var $filter = $('#posProductFilter');
      if (!$filter.length) return;

      if (_productFilterMq.matches) $filter.insertAfter($('#posBranchSelect').closest('.filter-dropdown-wrapper'));
      else $filter.appendTo('#posTabBar');
  }

  relocateProductFilter();
  if (_productFilterMq.addEventListener) _productFilterMq.addEventListener('change', relocateProductFilter);
  else if (_productFilterMq.addListener) _productFilterMq.addListener(relocateProductFilter);

  function renderProductDropdown() {
      var html = '';
      _products.forEach(function (product) {
          var isSelected = _selectedProduct && _selectedProduct.code === product.ProductCode;
          html += '<div class="dropdown-item' + (isSelected ? ' selected' : '') + '" data-code="' + product.ProductCode + '">' +
                      product.ProductName +
                  '</div>';
      });
      $('#posProductList').html(html);
      $('#posProductLabel').text(_selectedProduct ? _selectedProduct.name : 'Ürün');
  }

  function loadScorecardFilters(callback) {
      _productsLoaded = true;
      $.ajax({
          url: '/PosReport/GetPosScorecardFilters',
          type: 'POST',
          success: function (data) {
              _products = data || [];
              if (!_selectedProduct && _products.length) {
                  _selectedProduct = { code: _products[0].ProductCode, name: _products[0].ProductName };
              }
              renderProductDropdown();
          },
          complete: function () { if (callback) callback(); }
      });
  }

  $(document).on('click', '#posProductList .dropdown-item', function () {
      var code = parseInt($(this).attr('data-code'), 10);
      $('#posProductPanel').removeClass('open');
      if (_selectedProduct && _selectedProduct.code === code) return;

      _selectedProduct = { code: code, name: $(this).text() };
      renderProductDropdown();
      loadPosScorecard();
  });

  // ===== Data =====
  function reportPayload() {
      return {
          regionCode: selectedRegion ? selectedRegion.code : null,
          branchCode: selectedBranch ? selectedBranch.code : null,
          tabId: getActiveTabId()
      };
  }

  function scorecardPayload() {
      return {
          regionCode: selectedBranch ? null : (selectedRegion ? selectedRegion.code : null),
          branchCode: selectedBranch ? selectedBranch.code : null,
          productCode: _selectedProduct ? _selectedProduct.code : null
      };
  }

  function loadReport(url, payload, apply) {
      showLoadingOverlay();
      $.ajax({
          url: url,
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify(payload),
          success: function (data) {
              apply(data || [], true);
              $('#posSearchInput').trigger('input');
          },
          error: function () {
              apply([], false);
          },
          complete: hideLoadingOverlay
      });
  }

  function loadPosReport() {
      loadReport('/PosReport/GetPosReport', reportPayload(), function (items, loaded) {
          if (loaded) _countStale = false;
          pivot(items);
          renderTable();
          setCountPdf();
      });
  }

  function loadPosScorecard() {
      loadReport('/PosReport/GetPosScorecard', scorecardPayload(), function (items, loaded) {
          if (loaded) _scorecardStale = false;
          pivotScorecard(items);
          renderScorecardTable();
          setScorecardPdf();
      });
  }

  // ===== Adet Raporu / Skorkart =====
  $(document).on('click', '#posTabList .tab', function () {
      if ($(this).hasClass('active')) return;
      $('#posTabList .tab').removeClass('active');
      $(this).addClass('active');
      if (isCountView()) loadPosReport();
  });

  function applyPosView() {
      var isCount = isCountView();
      $('#posTableContainer').toggle(isCount);
      $('#posScoreCardContainer').toggle(!isCount);
      $('#posProductFilter').toggle(!isCount);
      $('.pos-view-toggle [data-view]').removeClass('active');
      $('.pos-view-toggle [data-view="' + _view + '"]').addClass('active');

      if (isCount) {
          if (_countStale) loadPosReport(); else setCountPdf();
      } else if (!_productsLoaded) {
          loadScorecardFilters(loadPosScorecard);
      } else if (_scorecardStale) {
          loadPosScorecard();
      } else {
          setScorecardPdf();
      }
      $('#posSearchInput').trigger('input');
  }

  $(document).on('click', '.pos-view-toggle [data-view]', function () {
      var view = $(this).data('view');
      if (view === _view) return;
      _view = view;
      applyPosView();
  });

  function reloadActiveView() {
      if (isCountView()) {
          _scorecardStale = true;
          loadPosReport();
      } else {
          _countStale = true;
          loadPosScorecard();
      }
  }

  // ===== Region/Branch Filters =====
  function persistSelection() {
      saveFilterSelection(selectedRegion, selectedBranch);
  }

  function renderRegionDropdown() {
      return renderRegionList('#posRegionList', selectedRegion ? selectedRegion.code : null);
  }

  function renderBranchDropdown() {
      return renderBranchList('#posBranchList', selectedBranch ? selectedBranch.code : null, selectedRegion ? selectedRegion.code : null);
  }

  function markSelected($item) {
      $item.siblings().removeClass('selected');
      $item.addClass('selected');
  }

  function closeFilterPanels() {
      $('#posRegionPanel, #posBranchPanel').removeClass('open');
  }

  $(document).on('click', '#posRegionList .dropdown-item', function () {
      var code = $(this).attr('data-code');
      var name = $(this).text();

      selectedRegion = code ? { code: code, name: name } : null;
      $('#posRegionLabel').text(code ? name : 'Bölge');

      selectedBranch = null;
      $('#posBranchLabel').text('Şube');

      markSelected($(this));
      closeFilterPanels();
      $('#posRegionSearch').val('');

      renderBranchDropdown();
      persistSelection();
      reloadActiveView();
  });

  $(document).on('click', '#posBranchList .dropdown-item', function () {
      var code = $(this).attr('data-code');
      var name = $(this).text();

      if (!code) {
          selectedBranch = null;
          $('#posBranchLabel').text('Şube');
      } else {
          selectedBranch = { code: code, name: name };
          $('#posBranchLabel').text(name);

          var region = findRegion($(this).attr('data-region'));
          if (region && (!selectedRegion || selectedRegion.code !== region.Code)) {
              selectedRegion = { code: region.Code, name: region.Name };
              $('#posRegionLabel').text(region.Name);
              markSelected($('#posRegionList .dropdown-item[data-code="' + region.Code + '"]'));
          }
      }

      markSelected($(this));
      closeFilterPanels();
      $('#posBranchSearch').val('');

      persistSelection();
      reloadActiveView();
  });

  handleTableSearch('#posSearchInput');

  function showLoadingOverlay() {
      $('body').loading({
          stoppable: false,
          message: '<div><div class="brand-spinner"></div><p class="loading-text">Yükleniyor<span class="loading-dots"><span>.</span><span>.</span><span>.</span></span></p></div>'
      });
  }

  function hideLoadingOverlay() {
      resetTableScroll(isCountView() ? '#posTableBody' : '#posScoreCardBody');
      $('body').loading('stop');
  }

  loadTodayDate(function () {
      var savedSelection = initFilterSelection();

      loadRegionFilters(function () {
          if (savedSelection.region && findRegion(savedSelection.region.code)) {
              selectedRegion = savedSelection.region;
              $('#posRegionLabel').text(selectedRegion.name);
          }
          var single = renderRegionDropdown();
          if (single) selectedRegion = { code: single.Code, name: single.Name };

          loadBranchFilters(function () {
              if (savedSelection.branch && findBranch(savedSelection.branch.code, selectedRegion ? selectedRegion.code : null)) {
                  selectedBranch = savedSelection.branch;
                  $('#posBranchLabel').text(selectedBranch.name);
              }
              var singleBranch = renderBranchDropdown();
              if (singleBranch) selectedBranch = { code: singleBranch.Code, name: singleBranch.Name };
              persistSelection();

              loadPosReport();
          });
      });
  });
});
