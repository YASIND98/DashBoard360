// Dinamik tablo PDF motoru. Veri sayfanın servis cevabından gelir; sayfaya özel kod yok.
// Çıktı her yerde aynı düz tablo (renk/legend yok); expand satırları index + girinti ile gösterilir.
//
// Config: { title, infoLines[], columns[{header,key,format?,extra?,align?}], rows[], childrenKey?, footerNote?, filename }
//   col.align: 'left' | 'center' (varsayılan center) — th + td hizası; metin (col-text) kolonları 'left'.
//   1) Sayfa render ederken window.PdfReport'u set eder (varsayılan).
//   2) Buton data-pdf="<kaynak>" taşırsa window.PdfSources[<kaynak>]() çağrılır (ör. grafik ekranları).
$(function () {

  // Başlık + alt info satırları (beyaz zemin, siyah metin)
  function buildTitleHeader(title, lines) {
    var $h = $('<div></div>').css({
      'background-color': '#ffffff', padding: '24px 20px 16px', 'font-family': 'Inter, sans-serif'
    });
    $h.append($('<div></div>').text(title || '').css({
      color: '#000000', 'font-size': '22px', 'font-weight': '700', 'margin-bottom': '10px'
    }));
    (lines || []).forEach(function (l) {
      if (!l) return;
      $h.append($('<div></div>').attr('style', 'color:#454b54;font-size:14px;margin-bottom:4px;').text(l));
    });
    return $h;
  }

  // columns sırayla; rows: obje dizisi (col.key ile eşlenir). col.extra(row): değerin altına ek HTML.
  // childrenKey: alt kayıtları (her derinlik) ayrı satır çizer; ilk kolonda index (1, 1.1, 1.2.1) + girinti.
  function buildDataTable(columns, rows, opts) {
    opts = opts || {};
    columns = columns || [];
    rows = rows || [];
    var childrenKey = opts.childrenKey;
    var numbered = !!childrenKey;   // expandable raporlarda hiyerarşik index göster

    var $table = $('<table></table>').css({
      'border-collapse': 'collapse', width: '100%',
      'font-family': 'Inter, sans-serif', 'font-size': '13px'
    });

    function thBase() {
      return $('<th></th>').css({
        padding: '12px 14px', background: '#eef2f7', color: '#5a6275',
        'font-weight': '500', 'border-bottom': '1px solid #dfe4ea', 'white-space': 'nowrap', 'vertical-align': 'bottom'
      });
    }
    // Leaf başlık: ana başlık + varsa alt başlık (ör. tarih). Hizalama col.align (varsayılan center).
    function leafTh(col) {
      var align = col.align || 'center';
      var $th = thBase().css('text-align', align);
      $th.append($('<div></div>').text(col.header || ''));
      if (col.subHeader) {
        $th.append($('<div></div>').text(col.subHeader).css({
          'font-weight': '400', 'font-size': '11px', 'color': '#9aa3b2', 'margin-top': '3px', 'text-align': align
        }));
      }
      return $th;
    }

    var $thead = $('<thead></thead>');
    var hasGroups = columns.some(function (c) { return c.group; });
    if (!hasGroups) {
      var $htr = $('<tr></tr>');
      columns.forEach(function (col) { $htr.append(leafTh(col)); });
      $thead.append($htr);
    } else {
      // 2 satırlı başlık: üstte grup (colspan), altta leaf'ler; gruba girmeyen kolonlar rowspan 2.
      var $row1 = $('<tr></tr>'), $row2 = $('<tr></tr>');
      var gi = 0;
      while (gi < columns.length) {
        var col = columns[gi];
        if (col.group) {
          var span = 1;
          while (gi + span < columns.length && columns[gi + span].group === col.group) span++;
          var $gth = thBase().attr('colspan', span).css({ 'text-align': 'center', 'border-bottom': 'none' });
          $gth.append($('<div></div>').text(col.group));
          // Grup alt başlığı (ör. tarih) — grubun ilk kolonundan
          if (col.groupSub) {
            $gth.append($('<div></div>').text(col.groupSub).css({
              'font-weight': '400', 'font-size': '11px', 'color': '#9aa3b2', 'margin-top': '3px', 'text-align': 'center'
            }));
          }
          $row1.append($gth);
          for (var gj = 0; gj < span; gj++) $row2.append(leafTh(columns[gi + gj]));
          gi += span;
        } else {
          $row1.append(leafTh(col).attr('rowspan', 2));
          gi++;
        }
      }
      $thead.append($row1).append($row2);
    }
    $table.append($thead);

    var $tbody = $('<tbody></tbody>');

    function cellVal(col, row) {
      var v = row ? row[col.key] : null;
      if (typeof col.format === 'function') return col.format(v, row);
      return (v == null ? '' : v);
    }

    var stripe = 0;
    // Tüm derinlikleri sırayla çiz; prefix hiyerarşik index'i, depth girintiyi belirler.
    function renderRows(list, prefix, depth) {
      list.forEach(function (row, i) {
        var idx = numbered ? (prefix ? prefix + '.' + (i + 1) : String(i + 1)) : null;
        var $tr = $('<tr></tr>').css('background', (stripe++ % 2 === 1) ? '#f5f5f5' : '#ffffff');
        columns.forEach(function (col, ci) {
          var css = {
            'text-align': col.align || 'center', padding: '11px 14px', color: '#1a1a1a',
            'border-bottom': '1px solid #eef1f5', 'white-space': 'nowrap'
          };
          var $td = $('<td></td>');
          if (ci === 0) {
            // İlk kolon: index + ad (düz metin)
            var text = String(cellVal(col, row));
            if (idx != null) text = idx + '   ' + text;          // 1 / 1.1 / 1.2.1
            if (depth > 0) css['padding-left'] = (14 + depth * 18) + 'px';  // derinlik girintisi
            $td.text(text);
          } else {
            // Değer kolonları: formatlayıcı çıktısı (HTML olabilir) + opsiyonel col.extra(row) ek bilgisi.
            var cellHtml = String(cellVal(col, row));
            if (typeof col.extra === 'function') {
              var ex = col.extra(row);
              if (ex) cellHtml += ex;
            }
            $td.html(cellHtml);
          }
          $tr.append($td.css(css));
        });
        $tbody.append($tr);

        var kids = childrenKey ? row[childrenKey] : null;
        if (Array.isArray(kids) && kids.length) renderRows(kids, idx, depth + 1);
      });
    }
    renderRows(rows, '', 0);
    $table.append($tbody);

    return $('<div></div>').css({
      margin: '0 20px 20px', 'border-radius': '16px', overflow: 'hidden',
      border: '1px solid #e6eaf0', 'background-color': '#ffffff'
    }).append($table);
  }

  // Config'ten beyaz temalı sayfa kurup PDF olarak indirir.
  function exportTablePdf(config) {
    config = config || {};
    var $btn = config.$btn;
    var originalHtml = $btn ? $btn.html() : null;
    if ($btn) {
      $btn.css('pointer-events', 'none');
      $btn.html('<div class="pdf-spinner"></div> <span>PDF Yükleniyor...</span>');
    }

    var $wrapper = $('<div></div>').css({
      'position': 'absolute', 'left': '-9999px', 'top': '0',
      'min-width': '700px', 'background-color': '#ffffff', 'color': '#000000', 'font-family': 'Inter, sans-serif'
    });
    $wrapper.append(buildTitleHeader(config.title, config.infoLines));
    $wrapper.append(buildDataTable(config.columns, config.rows, config));

    // Tablo altı not (ör. "tutarlar /1000"): ekrandaki legend-note karşılığı
    if (config.footerNote) {
      $wrapper.append($('<div></div>').text(config.footerNote).css({
        margin: '0 20px 24px', 'font-size': '12px', 'font-style': 'italic', color: '#8a93a3', 'font-family': 'Inter, sans-serif'
      }));
    }

    renderWrapperToPdf($wrapper, config.filename || 'rapor.pdf', $btn, originalHtml);
  }

  // Hazır $wrapper'ı görüntüye çevirip PDF olarak indirir.
  function renderWrapperToPdf($wrapper, filename, $btn, originalHtml) {
    $('body').append($wrapper);

    var realWidth = $wrapper[0].scrollWidth;
    $wrapper.css('width', realWidth + 'px');

    var minHeight = Math.round(realWidth * (210 / 297));   // A4 landscape oranı
    if ($wrapper[0].scrollHeight < minHeight) {
      $wrapper.css('height', minHeight + 'px');
    }

    function restore() {
      if ($btn) { $btn.html(originalHtml); $btn.css('pointer-events', ''); }
    }

    html2canvas($wrapper[0], {
      scale: 2, useCORS: true, backgroundColor: '#ffffff', scrollX: 0, scrollY: 0,
      width: $wrapper[0].scrollWidth, height: $wrapper[0].scrollHeight
    }).then(function (canvas) {
      $wrapper.remove();
      var imgData = canvas.toDataURL('image/png');
      var pdfWidth = canvas.width * 0.264583;
      var pdfHeight = canvas.height * 0.264583;
      var orientation = pdfWidth > pdfHeight ? 'l' : 'p';
      var jsPDF = (window.jspdf && window.jspdf.jsPDF) || window.jsPDF;
      var pdf = new jsPDF(orientation, 'mm', [pdfWidth, pdfHeight]);
      pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, pdfHeight);
      pdf.save(filename);
      restore();
    }).catch(function (err) {
      console.error('PDF oluşturma hatası:', err);
      $wrapper.remove();
      restore();
    });
  }

  // Buton tıklanınca: data-pdf="<kaynak>" varsa PdfSources[<kaynak>](), yoksa window.PdfReport.
  $(document).on('click', '.download-pdf-btn', function () {
    var $btn = $(this);
    var pdfSource = $btn.data('pdf');
    var hasProvider = pdfSource && window.PdfSources && typeof window.PdfSources[pdfSource] === 'function';
    var cfg = hasProvider ? (window.PdfSources[pdfSource]() || {}) : (window.PdfReport || {});
    cfg.$btn = $btn;
    exportTablePdf(cfg);
  });
});
