// POS Raporları — /PosReport/GetPosReport'tan gelen düz (metrik x tarih) listeyi
// ekrandaki tabloya çevirir: satırlar metrik, kolonlar ay (yeniden eskiye).
$(document).ready(function () {

  if (!document.getElementById('posDataTable')) return;

  var METRIC_COLUMN_NAME = 'POS Müşterileri';
  var INVERTED_DIFF_METRIC = 'İptal Adedi';

  var selectedRegion = null;
  var selectedBranch = null;

  var _dates = [];
  var _rows = [];

  function getActiveTabId() {
      return parseInt($('#posTabList .tab.active').data('tab-id'), 10) || 0;
  }

  function pivot(items) {
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
          cells[metric][dateKey] = { value: it.Value, diff: it.DiffValue };
      });

      dateKeys.sort(function (a, b) { return a < b ? 1 : (a > b ? -1 : 0); });

      _dates = dateKeys.map(function (key) {
          var parts = key.split('-');
          return {
              key: key,
              month: _trMonths[parseInt(parts[1], 10) - 1] || '',
              date: fmtIsoDate(key)
          };
      });

      _rows = metrics.map(function (metric) {
          var row = { metric: metric };
          _dates.forEach(function (d, i) {
              row['d' + i] = (cells[metric] || {})[d.key] || { value: null, diff: null };
          });
          return row;
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

  function cellHtml(cell, metric) {
      cell = cell || {};
      if (cell.diff == null) {
          return formatNumber(cell.value) + '<div class="diff-value">&nbsp;</div>';
      }
      return formatNumber(cell.value) +
          '<div class="diff-value ' + diffClass(cell.diff, metric) + '">' + formatDiffNumber(cell.diff) + '</div>';
  }

  // ===== Render =====
  function renderTable() {
      var head = '<tr><th class="col-left">' + METRIC_COLUMN_NAME + '</th>';
      _dates.forEach(function (d) {
          head += '<th><span>' + d.month + '</span><small>' + d.date + '</small></th>';
      });
      head += '</tr>';
      $('#posTableHead').html(head);

      // Boş durum satırı .no-result-row olmamalı: handleTableSearch her tuş vuruşunda
      // o sınıftaki satırları siliyor, aramada boş durum kaybolurdu.
      if (!_rows.length) {
          $('#posTableBody').html(
              '<tr class="pos-empty-row"><td colspan="' + (_dates.length + 1) + '" style="text-align:center;padding:48px 16px;">' +
                  '<div class="table-empty-state">' +
                      '<img src="/images/empty-state-seach.svg" alt="" />' +
                      '<span>Seçili kırılıma ait veri bulunmamaktadır.</span>' +
                  '</div>' +
              '</td></tr>'
          );
          return;
      }

      var html = '';
      _rows.forEach(function (row) {
          html += '<tr class="table-row">';
          html += '<td class="col-left">' + row.metric + '</td>';
          _dates.forEach(function (_d, i) {
              html += '<td class="has-diff">' + cellHtml(row['d' + i], row.metric) + '</td>';
          });
          html += '</tr>';
      });

      var $body = $('#posTableBody').html(html);
      reStripeTable($body);
      applyDiffVisibility();
  }

  // Fark satırları client'ta gizlenir; veri her iki durumda da aynı geldiği için servise gidilmez.
  function applyDiffVisibility() {
      $('#posTableBody .diff-value').toggle($('#posDiffToggle').attr('data-active') === 'true');
  }

  $(document).on('click', '#posDiffToggle', function () {
      $(this).attr('data-active', $(this).attr('data-active') === 'true' ? 'false' : 'true');
      applyDiffVisibility();
  });

  // ===== PDF (window.PdfReport) — ekranda görünenle aynı veriden kurulur =====
  function pdfInfoLines() {
      var date = ($('.date-text').text() || '').trim();
      var region = selectedRegion ? selectedRegion.name : 'Tüm Bölgeler';
      var branch = selectedBranch ? selectedBranch.name : 'Tüm Şubeler';
      return [(date ? date + ' tarihine ait ' : '') + region + ' / ' + branch];
  }

  function setPdfReport() {
      var columns = [{ header: METRIC_COLUMN_NAME, key: 'metric', align: 'left' }];
      _dates.forEach(function (d, i) {
          columns.push({
              header: d.month,
              subHeader: d.date,
              key: 'd' + i,
              format: function (cell, row) {
                  cell = cell || {};
                  var html = '<div style="white-space:nowrap;">' + formatNumber(cell.value) + '</div>';
                  if (cell.diff != null) {
                      var cls = diffClass(cell.diff, row ? row.metric : '');
                      var color = cls === 'negative' ? '#f12831' : (cls === 'positive' ? '#27b857' : '#5a6275');
                      html += '<div style="font-size:11px; color:' + color + '; white-space:nowrap;">' +
                          formatDiffNumber(cell.diff) + '</div>';
                  }
                  return html;
              }
          });
      });

      window.PdfReport = {
          title: 'POS Raporları',
          infoLines: pdfInfoLines(),
          columns: columns,
          rows: _rows,
          filename: 'POS-Raporlari.pdf'
      };
  }

  // ===== Data =====
  function loadPosReport() {
      showLoadingOverlay();
      $.ajax({
          url: '/PosReport/GetPosReport',
          type: 'POST',
          contentType: 'application/json',
          data: JSON.stringify({
              regionCode: selectedRegion ? selectedRegion.code : null,
              branchCode: selectedBranch ? selectedBranch.code : null,
              tabId: getActiveTabId()
          }),
          success: function (data) {
              pivot(data);
              renderTable();
              setPdfReport();
              // Filtre değişince ekrandaki arama yeni satırlara da uygulansın
              $('#posSearchInput').trigger('input');
          },
          error: function () {
              pivot([]);
              renderTable();
              setPdfReport();
          },
          complete: hideLoadingOverlay
      });
  }

  $(document).on('click', '#posTabList .tab', function () {
      if ($(this).hasClass('active')) return;
      $('#posTabList .tab').removeClass('active');
      $(this).addClass('active');
      loadPosReport();
  });

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

  $(document).on('click', '#posRegionList .dropdown-item', function () {
      var code = $(this).data('code');
      var name = $(this).text();

      selectedRegion = code ? { code: code, name: name } : null;
      $('#posRegionLabel').text(code ? name : 'Bölge');

      selectedBranch = null;
      $('#posBranchLabel').text('Şube');

      $('#posRegionList .dropdown-item').removeClass('selected');
      $(this).addClass('selected');
      $('#posRegionPanel').removeClass('open');
      $('#posBranchPanel').removeClass('open');
      $('#posRegionSearch').val('');

      renderBranchDropdown();
      persistSelection();
      loadPosReport();
  });

  $(document).on('click', '#posBranchList .dropdown-item', function () {
      var code = $(this).data('code');
      var name = $(this).text();
      var regionCode = $(this).data('region');

      if (!code) {
          selectedBranch = null;
          $('#posBranchLabel').text('Şube');
      } else {
          selectedBranch = { code: code, name: name };
          $('#posBranchLabel').text(name);

          // Şube bölgesiz seçilirse bağlı olduğu bölge de işaretlenir
          var region = findRegion(regionCode);
          if (region && (!selectedRegion || selectedRegion.code !== region.Code)) {
              selectedRegion = { code: region.Code, name: region.Name };
              $('#posRegionLabel').text(region.Name);
              $('#posRegionList .dropdown-item').removeClass('selected');
              $('#posRegionList .dropdown-item[data-code="' + region.Code + '"]').addClass('selected');
          }
      }

      $('#posBranchList .dropdown-item').removeClass('selected');
      $(this).addClass('selected');
      $('#posBranchPanel').removeClass('open');
      $('#posRegionPanel').removeClass('open');
      $('#posBranchSearch').val('');

      persistSelection();
      loadPosReport();
  });

  handleTableSearch('#posSearchInput');

  function showLoadingOverlay() {
      $('body').loading({
          stoppable: false,
          message: '<div><div class="brand-spinner"></div><p class="loading-text">Yükleniyor<span class="loading-dots"><span>.</span><span>.</span><span>.</span></span></p></div>'
      });
  }

  function hideLoadingOverlay() {
      resetTableScroll('#posTableBody');
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
