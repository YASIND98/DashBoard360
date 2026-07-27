window.MOCK = window.MOCK || {};
window.MOCK.nplChart = [
    { reportDate: '2024-01-31T00:00:00', nplPrincipalAmount: 1204.7, nplPreLegalInterest: 318.4,  totalAmount: 1523.1 },
    { reportDate: '2024-02-29T00:00:00', nplPrincipalAmount: 1876.2, nplPreLegalInterest: 402.9,  totalAmount: 2279.1 },
    { reportDate: '2024-03-31T00:00:00', nplPrincipalAmount: 3985.5, nplPreLegalInterest: 1428.6, totalAmount: 5414.1 },
    { reportDate: '2024-04-30T00:00:00', nplPrincipalAmount: 968.3,  nplPreLegalInterest: 241.7,  totalAmount: 1210.0 },
    { reportDate: '2024-05-31T00:00:00', nplPrincipalAmount: 1342.8, nplPreLegalInterest: 486.2,  totalAmount: 1829.0 },
    { reportDate: '2024-06-30T00:00:00', nplPrincipalAmount: 2914.6, nplPreLegalInterest: 705.3,  totalAmount: 3619.9 },
    { reportDate: '2024-07-31T00:00:00', nplPrincipalAmount: 3210.4, nplPreLegalInterest: 2203.7, totalAmount: 5414.1 },
    { reportDate: '2024-08-31T00:00:00', nplPrincipalAmount: 2688.9, nplPreLegalInterest: 597.4,  totalAmount: 3286.3 },
    { reportDate: '2024-09-30T00:00:00', nplPrincipalAmount: 3105.2, nplPreLegalInterest: 542.8,  totalAmount: 3648.0 },
    { reportDate: '2024-10-31T00:00:00', nplPrincipalAmount: 4872.5, nplPreLegalInterest: 2015.3, totalAmount: 6887.8 },
    { reportDate: '2024-11-30T00:00:00', nplPrincipalAmount: 1798.4, nplPreLegalInterest: 486.9,  totalAmount: 2285.3 },
    { reportDate: '2024-12-31T00:00:00', nplPrincipalAmount: 3024.1, nplPreLegalInterest: 690.5,  totalAmount: 3714.6 },
    { reportDate: '2025-01-31T00:00:00', nplPrincipalAmount: 5966.7, nplPreLegalInterest: 1428.2, totalAmount: 7394.9 },
    { reportDate: '2025-02-28T00:00:00', nplPrincipalAmount: 1015.3, nplPreLegalInterest: 268.7,  totalAmount: 1284.0 },
    { reportDate: '2025-03-31T00:00:00', nplPrincipalAmount: 2452.8, nplPreLegalInterest: 830.6,  totalAmount: 3283.4 },
    { reportDate: '2025-04-30T00:00:00', nplPrincipalAmount: 3186.5, nplPreLegalInterest: 726.4,  totalAmount: 3912.9 },
    { reportDate: '2025-05-31T00:00:00', nplPrincipalAmount: 2270.9, nplPreLegalInterest: 583.1,  totalAmount: 2854.0 }
];

window.getNplChartMock = function () {
    return window.MOCK.nplChart;
};

// ===== NPL Girişleri: yığılmış bar grafik + tablo görünümü =====
// Filtre alanı ayrı dosyadadır (filter.js); seçili filtreler NplReport.getFilters() ile okunur.
$(function () {
    if (!document.getElementById('nplChart')) return;

    var NPL = window.NplReport = window.NplReport || {};

    var _chartData = [];

    // Grafik ölçüleri: barlar SABİT 36px; içerik dar ekrana sığmazsa küçülmez, yatay scroll olur.
    // Bu yüzden SVG genişliği (W) bar sayısına göre dinamik hesaplanır (viewBox ile ölçeklenmez).
    var H = 520;
    var ML = 78, MR = 24, MT = 44, MB = 52;
    var BAR_W = 36;                                 // sabit sütun genişliği
    var BAND = 54;                                  // sütun + iki yanı boşluk (36 + ~18)
    var PLOT_H = H - MT - MB;
    var Y_TICKS = [0, 2000, 4000, 6000, 8000];      // milyon TL - sabit ölçek
    var Y_MAX = Y_TICKS[Y_TICKS.length - 1];
    var MIN_LABEL_H = 16;                            // etiketin parçanın içine sığdığı en küçük yükseklik

    // ISO tarih ("2025-06-30T00:00:00") -> "2025.06" (x ekseni etiketi)
    function fmtPeriod(iso) {
        var m = /^(\d{4})-(\d{2})/.exec(String(iso || ''));
        return m ? (m[1] + '.' + m[2]) : String(iso || '');
    }

    // Paylaşılan formatNumber (formatter.js); yalnız 0'ı "-" yerine "0" göster (eksen için)
    function fmtNum(v) {
        return Number(v) ? formatNumber(v) : '0';
    }

    // Bar üstü / parça içi kısa etiket: 1.234,5 -> "1,2B"
    function fmtShort(v) {
        var num = Number(v) || 0;
        if (num >= 1000) return fmtNum(Math.round(num / 100) / 10) + 'B';
        return fmtNum(Math.round(num * 10) / 10);
    }

    // Tablo tutarı: "5.908,5 Mn" (tasarımdaki gibi her zaman tek basamak küsürat)
    function fmtMn(v) {
        return new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 1, maximumFractionDigits: 1 })
            .format(Number(v) || 0) + ' Mn';
    }

    // ISO tarih -> "Aralık 2025" (tablodaki dönem kolonu)
    function fmtPeriodLong(iso) {
        var m = /^(\d{4})-(\d{2})/.exec(String(iso || ''));
        return m ? (_trMonths[+m[2] - 1] + ' ' + m[1]) : String(iso || '');
    }

    // Servisten gelen dönem tarihini ekseni bozmayacak şekilde sıraya koy
    function buildChartData(raw) {
        return (raw || []).map(function (d) {
            return {
                date: d.reportDate,
                period: fmtPeriod(d.reportDate),
                periodLong: fmtPeriodLong(d.reportDate),
                principal: Number(d.nplPrincipalAmount) || 0,
                interest: Number(d.nplPreLegalInterest) || 0,
                total: Number(d.totalAmount) || 0
            };
        });
    }
    
    function fetchChart(callback) {
        var request = $.extend({
            tab: $('#nplTabList .tab.active').data('npltab') || 'tumu',
            metric: _metric   // 'balance' (Bakiye) | 'ratio' (Oran)
        }, (typeof NPL.getFilters === 'function') ? NPL.getFilters() : {});
        callback(getNplChartMock(request));
    }

    function renderChart() {
        var data = _chartData;

        if (!data.length) {
            $('#nplChart').html(
                '<div class="table-empty-state npl-chart-empty">' +
                    '<img src="/images/empty-state-seach.svg" alt="" />' +
                    '<span>Seçili döneme ait veri bulunmamaktadır.</span>' +
                '</div>'
            );
            return;
        }

        var n = data.length;
        var band = BAND;
        var barW = BAR_W;
        var PLOT_W = n * band;                       // bar alanı (sabit band × bar sayısı)
        var W = ML + PLOT_W + MR;                    // dinamik SVG genişliği
        var baseY = MT + PLOT_H;

        function yAt(v) { return baseY - (v / Y_MAX) * PLOT_H; }

        // Y ekseni: kesikli yatay çizgiler + sabit değer etiketleri
        var yAxis = '';
        Y_TICKS.forEach(function (t) {
            var y = yAt(t);
            yAxis += '<line x1="' + ML + '" y1="' + y + '" x2="' + (W - MR) + '" y2="' + y + '" class="npl-chart-grid" />';
            yAxis += '<text x="' + (ML - 14) + '" y="' + (y + 4) + '" text-anchor="end" class="npl-chart-axis">' + fmtNum(t) + '</text>';
        });

        var bars = '';
        data.forEach(function (d, i) {
            var cx = ML + i * band + band / 2;
            var x = cx - barW / 2;

            var hPrincipal = (d.principal / Y_MAX) * PLOT_H;
            var hInterest = (d.interest / Y_MAX) * PLOT_H;
            var yPrincipal = baseY - hPrincipal;
            var yInterest = yPrincipal - hInterest;

            bars += '<g class="npl-chart-bar" data-total="' + fmtNum(d.total) + '">';

            // Alt parça: NPL Anapara, üst parça: NPL Kat Öncesi Faiz
            bars += '<rect x="' + x + '" y="' + yPrincipal + '" width="' + barW + '" height="' + hPrincipal + '" class="npl-bar-principal" />';
            bars += '<rect x="' + x + '" y="' + yInterest + '" width="' + barW + '" height="' + hInterest + '" class="npl-bar-interest" />';

            // Toplam bar'ın üstünde; parça değerleri sığdığı sürece parçanın içinde
            bars += '<text x="' + cx + '" y="' + (yInterest - 8) + '" text-anchor="middle" class="npl-chart-total">' + fmtShort(d.total) + '</text>';
            if (hInterest >= MIN_LABEL_H) {
                bars += '<text x="' + cx + '" y="' + (yInterest + hInterest / 2 + 4) + '" text-anchor="middle" class="npl-chart-value">' + fmtShort(d.interest) + '</text>';
            }
            if (hPrincipal >= MIN_LABEL_H) {
                bars += '<text x="' + cx + '" y="' + (yPrincipal + hPrincipal / 2 + 4) + '" text-anchor="middle" class="npl-chart-value">' + fmtShort(d.principal) + '</text>';
            }

            // Bar'ın tamamını kaplayan şeffaf hedef: tooltip kolonun her yerinde açılsın
            bars += '<rect x="' + x + '" y="' + MT + '" width="' + barW + '" height="' + PLOT_H + '" class="npl-bar-hover" />';
            bars += '</g>';

            bars += '<text x="' + cx + '" y="' + (baseY + 24) + '" text-anchor="middle" class="npl-chart-axis">' + d.period + '</text>';
        });

        // width/height px olarak verilir (1:1); container'dan genişse .npl-chart yatay kaydırır
        var svg =
            '<svg viewBox="0 0 ' + W + ' ' + H + '" width="' + W + '" height="' + H + '">' +
                yAxis + bars +
            '</svg>';

        $('#nplChart').html(svg + '<div class="npl-chart-tooltip" id="nplChartTooltip"></div>');
        bindChartTooltip();
    }

    // Bar'a gelince toplamı tam (küsüratlı) haliyle gösteren tooltip
    function bindChartTooltip() {
        var $chart = $('#nplChart');
        var $tip = $('#nplChartTooltip');

        $chart.find('.npl-chart-bar')
            .on('mouseenter', function () {
                var $g = $(this);
                $tip.text($g.attr('data-total'));

                // Tooltip'i imlece değil bar'a hizala: bar'ın sağında, bar'ın dikey ortasında.
                // .npl-chart yatay kaydırılabildiği için konum içerik koordinatında (scrollLeft dahil).
                var chartRect = $chart[0].getBoundingClientRect();
                var scrollLeft = $chart[0].scrollLeft || 0;
                var topRect = $g.find('.npl-bar-interest')[0].getBoundingClientRect();
                var bottomRect = $g.find('.npl-bar-principal')[0].getBoundingClientRect();
                var barLeft = topRect.left - chartRect.left + scrollLeft;
                var barRight = topRect.right - chartRect.left + scrollLeft;
                var barMidY = (topRect.top + bottomRect.bottom) / 2 - chartRect.top;

                var tw = $tip.outerWidth();
                var th = $tip.outerHeight();
                var gap = 10;   // ok payı + küçük boşluk

                // Sağa sığmıyorsa (görünür alan) bar'ın soluna geç ve oku çevir
                var visibleRight = topRect.right - chartRect.left;
                var flipped = visibleRight + gap + tw > chartRect.width;
                var left = flipped ? (barLeft - gap - tw) : (barRight + gap);
                // Dikeyde grafiğin dışına taşmasın; ok yine bar'ın ortasını gösterir
                var top = Math.min(Math.max(0, barMidY - th / 2), Math.max(0, chartRect.height - th));

                // Önce konumlandır, sonra göster: ilk hover'da sol üst köşede görünmesin
                $tip.toggleClass('npl-tt-left', flipped).css({
                    left: Math.max(0, left) + 'px',
                    top: top + 'px',
                    '--npl-tt-arrow': (barMidY - top) + 'px'
                }).show();
            })
            .on('mouseleave', function () {
                $tip.hide();
            });
    }

    // ===== Tablo görünümü (Grafik/Tablo toggle) =====
    // Grafikle aynı servis cevabı; kolonlar dizideki key'lerle birebir eşleşir.
    var TABLE_COLUMNS = [
        { key: 'periodLong', header: 'Dönem',               align: 'left',  sortKey: 'date' },
        { key: 'principal',  header: 'NPL Anapara',         sub: 'Milyon TL', format: fmtMn },
        { key: 'interest',   header: 'NPL Kat Öncesi Faiz', sub: 'Milyon TL', format: fmtMn },
        { key: 'total',      header: 'Toplam NPL',          sub: 'Milyon TL', format: fmtMn }
    ];

    // Varsayılan sıralama YOK: veriler servis/mock sırasında gelir, kolonlarda nötr yukarı/aşağı ok.
    // Kullanıcı bir başlığa tıklarsa o kolona göre sıralanır.
    var _sort = { key: null, dir: 'desc' };

    function sortedRows() {
        if (!_sort.key) return _chartData.slice();   // servisten döndüğü sıra
        return _chartData.slice().sort(function (a, b) {
            var x = a[_sort.key], y = b[_sort.key];
            var cmp = (typeof x === 'string') ? x.localeCompare(y, 'tr') : (x - y);
            return _sort.dir === 'asc' ? cmp : -cmp;
        });
    }

    function renderTable() {
        var head = '<tr>';
        TABLE_COLUMNS.forEach(function (c) {
            var sortKey = c.sortKey || c.key;
            var dirCls = (_sort.key === sortKey) ? (' ' + _sort.dir) : '';
            head += '<th' + (c.align === 'left' ? ' class="col-left"' : '') + ' data-sort-key="' + sortKey + '">' +
                        '<span>' + c.header + '</span>' +
                        '<i class="sort-icon' + dirCls + '">' +
                            '<img class="sort-up" src="/images/sort-asc.svg" alt="" />' +
                            '<img class="sort-down" src="/images/sort-dec.svg" alt="" />' +
                        '</i>' +
                        (c.sub ? '<small>' + c.sub + '</small>' : '') +
                    '</th>';
        });
        head += '</tr>';
        $('#nplTableHead').html(head);

        var rows = sortedRows();
        if (!rows.length) {
            $('#nplTableBody').html(
                '<tr class="no-result-row"><td colspan="' + TABLE_COLUMNS.length + '" style="text-align:center;padding:48px 16px;">' +
                    '<div class="table-empty-state">' +
                        '<img src="/images/empty-state-seach.svg" alt="" />' +
                        '<span>Seçili döneme ait veri bulunmamaktadır.</span>' +
                    '</div>' +
                '</td></tr>'
            );
            return;
        }

        var html = '';
        rows.forEach(function (r) {
            html += '<tr class="table-row">';
            TABLE_COLUMNS.forEach(function (c) {
                html += '<td' + (c.align === 'left' ? ' class="col-left"' : '') + '>' +
                            (c.format ? c.format(r[c.key]) : r[c.key]) +
                        '</td>';
            });
            html += '</tr>';
        });
        var $body = $('#nplTableBody').html(html);
        if (typeof reStripeTable === 'function') reStripeTable($body);
    }

    // Kolon başlığına tıklanınca yön değiştir (aynı kolon) / o kolona geç
    $(document).on('click', '#nplTableHead th', function () {
        var key = $(this).data('sort-key');
        if (!key) return;
        if (_sort.key === key) {
            _sort.dir = (_sort.dir === 'asc') ? 'desc' : 'asc';
        } else {
            _sort.key = key;
            _sort.dir = 'desc';
        }
        renderTable();
    });

    // Grafik <-> Tablo geçişi (legend yalnızca grafikte anlamlı).
    // Toggle iki kopyadadır (masaüstü + mobil); ikisi de [data-view] değerine göre senkronlanır.
    var _view = 'chart';
    function applyView() {
        var isChart = _view === 'chart';
        $('#nplChartCard').toggle(isChart);
        $('#nplChartLegend').toggle(isChart);
        $('#nplTableContainer').toggle(!isChart);
        $('.npl-view-toggle [data-view]').removeClass('active');
        $('.npl-view-toggle [data-view="' + _view + '"]').addClass('active');
        if (isChart) renderChart(); else renderTable();
    }

    $(document).on('click', '.npl-view-toggle [data-view]', function () {
        var view = $(this).data('view');
        if (view === _view) return;
        _view = view;
        applyView();
    });

    // Filtre/sekme değişiminde servisi yeniden çağır -> aktif görünümü çiz
    function loadChart() {
        fetchChart(function (res) {
            _chartData = buildChartData(res);
            applyView();
        });
    }

    // filter.js her filtre değişiminde bunu çağırır
    NPL.reload = loadChart;

    // ===== Sekmeler =====
    $('#nplTabList').on('click', '.tab', function () {
        $('#nplTabList .tab').removeClass('active');
        $(this).addClass('active');
        loadChart();
    });

    // ===== Bakiye / Oran (metrik) =====
    // Bakiye = tutar (milyon TL); Oran = yüzde. Seçim request'e metric olarak gider.
    // Toggle iki kopyadadır (masaüstü + mobil); ikisi de [data-metric] değerine göre senkronlanır.
    var _metric = 'balance';
    $(document).on('click', '.npl-metric-toggle [data-metric]', function () {
        var metric = $(this).data('metric');
        if (metric === _metric) return;
        _metric = metric;
        $('.npl-metric-toggle [data-metric]').removeClass('active');
        $('.npl-metric-toggle [data-metric="' + metric + '"]').addClass('active');
        loadChart();
    });

    // PDF motoru (download-pdf.js) bu config'i data-pdf="nplReport" üzerinden çeker.
    window.PdfSources = window.PdfSources || {};
    window.PdfSources.nplReport = function () {
        return {
            title: 'NPL Girişleri',
            infoLines: [
                $('#nplTabList .tab.active').text().trim(),
                $('#nplBreadcrumb').text().replace(/\s+/g, ' ').trim()
            ],
            columns: TABLE_COLUMNS.map(function (c) {
                return { header: c.header, subHeader: c.sub, key: c.key, align: c.align || 'center', format: c.format };
            }),
            rows: sortedRows(),
            footerNote: 'Değerler milyon TL olarak gösterilmektedir.',
            filename: 'NPL-Gecikmeli-Krediler-Raporu.pdf'
        };
    };

    // ===== İlk render =====
    loadChart();
});
