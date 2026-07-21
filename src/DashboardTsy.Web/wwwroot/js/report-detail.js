$(function () {
  if (!document.getElementById('reportDetailOverlay')) return;

  var TABLE_IDS = { daily: 'dailyTable', quantity: 'quantityTable', monthly: 'monthlyTable' };
  var TABLE_LABELS = { daily: 'Hacim / Bakiye', quantity: 'Adet', monthly: 'Aylık H/G' };

  var DIFF_LEGEND = '<span class="legend-value">-</span><span class="legend-bar ratio-red-bg"></span><span class="legend-value">0</span><span class="legend-bar ratio-green-bg"></span><span class="legend-value">+</span>';
  var RATIO_LEGEND = '<span class="legend-value">0</span><span class="legend-bar ratio-red-bg"></span><span class="legend-value">75</span><span class="legend-bar ratio-orange-bg"></span><span class="legend-value">100</span><span class="legend-bar ratio-green-bg"></span><span class="legend-value">120</span><span class="legend-bar ratio-blue-bg"></span>';

  var _open = {};
  var _currentTable = 'daily';
  var _providers = {};
  var _ctx = {};

  function activeProvider() { return _providers[_currentTable] || _defaultProvider; }

  function nameOf(node) {
      var p = activeProvider();
      return (p && p.nameOf) ? p.nameOf(node) : (node.ProductName || '');
  }

  function hasSub(n) { return !!(n.SubProducts && n.SubProducts.length); }

  function isRatioProduct(name) {
      return (name || '').toLocaleLowerCase('tr').indexOf('oran') > -1;
  }

  function activeProducts() {
      return window.ReportBreakdownData || [];
  }

  function buildHead(tableKey) {
      var $thead = $('#' + (TABLE_IDS[tableKey] || 'dailyTable') + ' thead').clone();
      $thead.find('th.col-index, th.col-expand, th.col-detail').remove();
      $thead.find('.sort-icon, .info-icon').remove();
      $thead.find('.col-group-header').removeClass('selected');
      $thead.find('th').removeClass('col-selected col-selected-first col-selected-mid col-selected-last');
      $thead.find('th.col-left').removeClass('valign-top valign-bottom').text('Bölge/Şube/Portföy');
      $thead.find('th').each(function () {
          var $th = $(this);
          if (!$th.hasClass('col-left') && !$th.attr('colspan') && !$.trim($th.text())) $th.remove();
      });
      var headers = [];
      $thead.find('tr').last().find('th').each(function () {
          var $th = $(this);
          if ($th.hasClass('col-left')) return;
          var $title = $th.find('[data-daily-header],[data-quantity-header],[data-monthly-header]').first();
          headers.push($.trim($title.length ? $title.text() : $th.text()));
      });
      return { html: $thead.html(), headers: headers };
  }

  function diffItem(label, val) {
      var v = val || 0;
      var cls = v < 0 ? 'negative' : (v > 0 ? 'positive' : '');
      return '<span class="diff-detail"><span class="diff-label">' + (label || '') + '</span>' +
             '<span class="diff-value ' + cls + '">' + formatNumber(v, false) + '</span></span>';
  }

  function diffDetails(items) {
      return '<div class="diff-details">' + items.map(function (it) {
          return diffItem(it.label, it.value);
      }).join('') + '</div>';
  }

  function diffDetailsPdf(items) {
      var inner = items.map(function (it, i) {
          var sep = i ? 'border-left:1px solid #e6eaf0;' : '';
          return '<div style="display:inline-block;text-align:center;padding:0 8px;vertical-align:top;' + sep + '">' +
                     '<div style="font-size:11px;color:#9aa3b2;margin-bottom:3px;white-space:nowrap;">' + (it.label || '') + '</div>' +
                     '<div style="font-size:12px;font-weight:600;color:#1a1a1a;">' + formatNumber(it.value || 0, false) + '</div>' +
                 '</div>';
      }).join('');
      return '<div style="margin-top:8px;white-space:nowrap;">' + inner + '</div>';
  }

  function diffItemsFor(node) {
      if (_currentTable === 'quantity') {
          var q = window._quantityHeaders || {};
          return [
              { label: q.DiffByLastYearTitle, value: node.DiffByLastYearAmount },
              { label: q.DiffByLastTwoMonthEarlierTitle, value: node.DiffByLastTwoMonthEarlierAmount }
          ];
      }
      var d = window._dailyHeaders || {};
      return [
          { label: d.DiffByLastYearTitle, value: node.DiffByLastYearAmount },
          { label: d.DiffByLastWeekTitle, value: node.DiffByLastWeekAmount },
          { label: d.DiffByPrevDayTitle, value: node.DiffByPrevDayAmount }
      ];
  }

  function nodeDiffHtml(node) { return diffDetails(diffItemsFor(node)); }

  function diffToggleActive() {
      return $('#reportDetailDiffToggle').attr('data-active') === 'true';
  }

  function leafDescriptors(tableKey) {
      if (tableKey === 'monthly') {
          return [
              { key: 'MonthActualAmount', fmt: function (n) { return formatNumber(n.MonthActualAmount); } },
              { key: 'MonthTargetAmount', fmt: function (n) { return formatNumber(n.MonthTargetAmount); } },
              { key: 'MonthRatio', fmt: function (n) { return formatPercent(n.MonthRatio); }, color: function (n) { return percentColor(n.MonthRatio); } },
              { key: 'YearActualAmount', fmt: function (n) { return formatNumber(n.YearActualAmount); } },
              { key: 'YearTargetAmount', fmt: function (n) { return formatNumber(n.YearTargetAmount); } },
              { key: 'YearRatio', fmt: function (n) { return formatPercent(n.YearRatio); }, color: function (n) { return percentColor(n.YearRatio); } }
          ];
      }
      if (tableKey === 'quantity') {
          var qv = function (v, n) { return formatNumber(v, false) + (v && isRatioProduct(n.ProductName) ? '%' : ''); };
          return [
              { key: 'LastYearAmount', fmt: function (n) { return qv(n.LastYearAmount, n); } },
              { key: 'LastTwoMonthEarlierAmount', fmt: function (n) { return qv(n.LastTwoMonthEarlierAmount, n); } },
              { key: 'LastMonthAmount', fmt: function (n) { return qv(n.LastMonthAmount, n); }, tdClass: 'col-diff', extra: nodeDiffHtml }
          ];
      }
      var price = function (n, k) { return formatNumber(n[k], true, n.ProductName); };
      return [
          { key: 'LastYearAmount', fmt: function (n) { return price(n, 'LastYearAmount'); } },
          { key: 'LastWeekAmount', fmt: function (n) { return price(n, 'LastWeekAmount'); } },
          { key: 'PrevDayAmount', fmt: function (n) { return price(n, 'PrevDayAmount'); } },
          { key: 'YesterdayAmount', fmt: function (n) { return price(n, 'YesterdayAmount'); }, tdClass: 'col-diff', extra: nodeDiffHtml }
      ];
  }

  function valueCells(node, tableKey) {
      return leafDescriptors(tableKey).map(function (d) {
          var cls = [d.color && d.color(node), d.tdClass].filter(Boolean).join(' ');
          var inner = d.extra ? ('<div>' + d.fmt(node) + '</div>' + d.extra(node)) : d.fmt(node);
          return '<td' + (cls ? ' class="' + cls + '"' : '') + '>' + inner + '</td>';
      }).join('');
  }

  function buildVisible(products) {
      var out = [];
      (function walk(nodes, depth, path, trunks) {
          nodes.forEach(function (node, idx) {
              var isLast = idx === nodes.length - 1;
              var kids = hasSub(node);
              var p = path + idx;
              out.push({ node: node, depth: depth, isLast: isLast, hasKids: kids, open: !!_open[p], path: p, trunks: trunks });
              if (kids && _open[p]) walk(node.SubProducts, depth + 1, p + '.', trunks.concat(!isLast));
          });
      })(products, 0, '', []);
      return out;
  }

  function nameCell(v) {
      var guides = '';
      for (var j = 0; j < v.depth; j++) {
          if (j < v.depth - 1) {
              guides += '<span class="bd-guide' + (v.trunks[j] ? ' bd-trunk' : '') + '"></span>';
          } else {
              guides += '<span class="bd-guide bd-connector' + (!v.isLast ? ' bd-continue' : '') + '"></span>';
          }
      }
      var chevron;
      if (v.hasKids) {
          chevron = '<span class="bd-chevron' + (v.open ? ' bd-open' : '') + '">' +
                        '<span class="expand-icon"><img src="/images/expand.svg" alt="expand" /></span>' +
                    '</span>';
      } else if (v.depth > 0) {
          chevron = '<span class="bd-chevron"><img class="bd-noexpand" src="/images/non-expandable.svg" alt="" /></span>';
      } else {
          chevron = '<span class="bd-chevron"></span>';
      }
      return '<td class="col-left"><span class="bd-tree">' + guides + chevron +
          '<span class="bd-name">' + nameOf(v.node) + '</span></span></td>';
  }

  function applyDiffVisibility() {
      $('#reportDetailBdBody .diff-details').toggle(diffToggleActive());
  }

  function syncDiffMenuLabel() {
      var active = $('#reportDetailDiffToggle').attr('data-active') === 'true';
      $('#reportDetailDiffText').text(active ? 'Farkları Gizle' : 'Farkları Göster');
  }

  function renderBreakdown(tableKey) {
      var prov = activeProvider();
      var head = prov.buildHead(tableKey, _ctx);
      var visible = buildVisible(activeProducts(tableKey));
      var body = visible.map(function (v, k) {
          var stripe = (k % 2 === 0 ? 'stripe-odd' : 'stripe-even') + (k === visible.length - 1 ? ' last-visible-row' : '');
          var cls = 'table-row ' + stripe + (v.hasKids ? ' hr-expandable' : '');
          var cursor = v.hasKids ? ' style="cursor:pointer"' : '';
          return '<tr class="' + cls + '"' + cursor + ' data-path="' + v.path + '">' +
              nameCell(v) + prov.valueCells(v.node, tableKey, _ctx) + '</tr>';
      }).join('');
      $('#reportDetailBdHead').html(head.html);
      $('#reportDetailBdBody').html(body);
      if (prov.legendHtml) $('#reportDetailBreakdownTab .legend-colors').html(prov.legendHtml(tableKey, _ctx));
      var hasDiff = tableKey === 'daily' || tableKey === 'quantity';
      $('#reportDetailDiffToggle').toggle(hasDiff);
      $('#reportDetailDiffBtn').toggle(hasDiff);
      syncDiffMenuLabel();
      applyDiffVisibility();
  }

  function showTab(tab) {
      $('#reportDetailTabs .report-detail-tab').removeClass('active');
      $('#reportDetailTabs .report-detail-tab[data-report-detail-tab="' + tab + '"]').addClass('active');
      $('#reportDetailBreakdownTab').toggleClass('report-detail-hidden', tab !== 'breakdown');
      $('#reportDetailTop10Tab').toggleClass('report-detail-hidden', tab !== 'top10');
  }

  function setTabsBar(showTop10) {
      $('#reportDetailTabs').css('display', showTop10 ? '' : 'none');
  }

  function defaultBreakdownPdf() {
      var head = buildHead(_currentTable);
      var descriptors = leafDescriptors(_currentTable);
      var showDiff = diffToggleActive();
      var columns = [{ header: 'Bölge/Şube/Portföy', key: 'ProductName', align: 'left' }];
      head.headers.forEach(function (h, i) { columns.push({ header: h, key: 'c' + i }); });
      // İç içe satırlar: download-pdf.js childrenKey ile alt kırılımları hiyerarşik index + girinti ile çizer.
      function build(nodes) {
          return (nodes || []).map(function (nd) {
              var row = { ProductName: nd.ProductName };
              descriptors.forEach(function (d, i) {
                  var val = d.fmt(nd);
                  if (d.extra && showDiff) val += diffDetailsPdf(diffItemsFor(nd));
                  row['c' + i] = val;
              });
              if (nd.SubProducts && nd.SubProducts.length) row.children = build(nd.SubProducts);
              return row;
          });
      }
      return {
          title: ($('.report-detail-title').text() || 'Kırılım').trim(),
          infoLines: ['Bölge/Şube/Portföy Kırılımı', 'Rapor: ' + (TABLE_LABELS[_currentTable] || '')],
          columns: columns,
          rows: build(activeProducts(_currentTable)),
          childrenKey: 'children',
          footerNote: 'Tabloda yer alan tutarlar /1000 olarak verilmektedir.',
          filename: 'HedefRapor-Kirilim.pdf'
      };
  }

  var _defaultProvider = {
      buildHead: function (tableKey) { return buildHead(tableKey); },
      valueCells: function (node, tableKey) { return valueCells(node, tableKey); },
      nameOf: function (node) { return node.ProductName || ''; },
      legendHtml: function (tableKey) { return tableKey === 'monthly' ? RATIO_LEGEND : DIFF_LEGEND; },
      pdf: function () { return defaultBreakdownPdf(); }
  };

  $('#reportDetailBdBody').on('click', '.hr-expandable', function () {
      var p = $(this).attr('data-path');
      _open[p] = !_open[p];
      renderBreakdown(_currentTable);
  });

  $(document).on('click', '#reportDetailDiffToggle', function () {
      var isActive = $(this).attr('data-active') === 'true';
      $(this).attr('data-active', isActive ? 'false' : 'true');
      applyDiffVisibility();
  });

  $(document).on('click', '#reportDetailDiffBtn', function () {
      $('#reportDetailDiffToggle').trigger('click');
      syncDiffMenuLabel();
      $(this).closest('details').removeAttr('open');
  });
  $(document).on('click', '#reportDetailPdfBtn', function () {
      $(this).closest('details').removeAttr('open');
  });

  $('#reportDetailTabs').on('click', '.report-detail-tab', function () { showTab($(this).data('report-detail-tab')); });

  $('#reportDetailClose').on('click', function () {
      $('#reportDetailOverlay').removeClass('active');
  });
  $('#reportDetailOverlay').on('click', function (e) {
      if ($(e.target).is('#reportDetailOverlay')) {
          $('#reportDetailOverlay').removeClass('active');
      }
  });

  $(document).on('click', '.detail-icon', function () {
      _currentTable = $(this).data('table') || 'daily';
      _open = {};
      setTabsBar(String($(this).data('top10')) === '1');
      showTab('breakdown');
  });

  $(document).on('reportBreakdown:loaded', function (e, info) {
      if (info && info.table) _currentTable = info.table;
      $('#reportDetailDiffToggle').attr('data-active', 'true');
      renderBreakdown(_currentTable);
  });

  window.PdfSources = window.PdfSources || {};
  window.PdfSources.reportBreakdown = function () { return activeProvider().pdf(_currentTable, _ctx); };

  window.ReportDetail = {
      registerProvider: function (key, provider) { _providers[key] = provider; },
      setContext: function (ctx) { _ctx = ctx || {}; }
  };
});
