$(function () {
    if (!document.getElementById('nplChart')) return;

    var NPL = window.NplReport = window.NplReport || {};

    var _rawData = [];      // servis cevabı (Bakiye + Oran birlikte)
    var _chartData = [];    // aktif metriğe göre türetilmiş
    var H = 520;
    var ML = 78, MR = 24, MT = 44, MB = 52;
    var BAR_W = 44;                                 // sabit sütun genişliği
    var BAND = 78;                                  // sütun + boşluk (etiketler sığsın diye geniş)
    var PLOT_H = H - MT - MB;
    var MIN_LABEL_H = 16;                            // etiket parçaya sığan en küçük yükseklik
    var RATIO_TICKS = [0, 25, 50, 75, 100];          // Oran ekseni sabit %
    var TL_PER_BN = 1e9;                             // eksen etiketi bn'ye
    var TL_PER_MN = 1e6;                             // değer etiketi Mn'ye

    function isRatio() { return _metric === 'ratio'; }

    function num(v) { return Number(v) || 0; }

    // ISO tarih -> "2025.06" (x ekseni)
    function fmtPeriod(iso) {
        var m = /^(\d{4})-(\d{2})/.exec(String(iso || ''));
        return m ? (m[1] + '.' + m[2]) : String(iso || '');
    }

    // formatNumber (formatter.js); 0'ı "-" yerine "0" göster
    function fmtNum(v) {
        return Number(v) ? formatNumber(v) : '0';
    }

    // Sabit basamaklı TR sayı: fmtTr(20) -> "20,0", fmtTr(20,2) -> "20,00"
    function fmtTr(v, digits) {
        var d = (digits == null) ? 1 : digits;
        return new Intl.NumberFormat('tr-TR', { minimumFractionDigits: d, maximumFractionDigits: d })
            .format(Number(v) || 0);
    }

    // Değeri Mn olarak göster; ondalık nokta ile, yuvarlamadan kırparak (122,97... -> "122.9Mn").
    function fmtMn(v) {
        var mn = Math.trunc(Number(v) / TL_PER_MN * 10 + 1e-6) / 10;
        return fmtTr(mn, 1).replace(',', '.') + 'Mn';
    }

    // Tablo hücresi + bar etiketi: Bakiye'de Mn TL, Oran'da yüzde
    function fmtValue(v) {
        return isRatio() ? v ? formatPercent(v) : 0 : fmtMn(v);
    }

    // Adımı tam gösteren en az ondalık (40 -> 0; 2,5 -> 1; 0,025 -> 3), etiket yanıltmasın.
    function stepDecimals(v) {
        for (var d = 0; d <= 3; d++) {
            var scaled = v * Math.pow(10, d);
            if (Math.abs(scaled - Math.round(scaled)) < 1e-9) return d;
        }
        return 3;
    }
    // Y ekseni etiketi: Bakiye'de birim (Mn/bn) tavana göre, Oran'da "%25"
    function fmtTick(v, unit, suffix, decimals) {
        return isRatio() ? ('%' + fmtNum(v)) : (fmtTr(Number(v) / unit, decimals) + suffix);
    }

    // ISO tarih -> "Aralık 2025" (tablo dönem kolonu)
    function fmtPeriodLong(iso) {
        var m = /^(\d{4})-(\d{2})/.exec(String(iso || ''));
        return m ? (_trMonths[+m[2] - 1] + ' ' + m[1]) : String(iso || '');
    }

    // En yüksek değeri 2 anlamlı haneye yukarı yuvarlar: 122,9M -> 130M, 8,35bn -> 8,4bn.
    function roundMax(x) {
        if (x <= 0) return 0;
        var unit = Math.pow(10, Math.floor(Math.log10(x)) - 1); // magnitude / 10
        return Math.ceil(x / unit - 1e-9) * unit;
    }

    // Bakiye ekseni: en yüksek toplamı yuvarla, 4 eşit parçaya böl -> 0 dahil 5 çizgi (123M -> tavan 130M).
    function balanceTicks() {
        var max = 0;
        _chartData.forEach(function (d) {
            var t = num(d.totalRaw);
            if (t > max) max = t;
        });
        if (max <= 0) return [0, 1e8, 2e8, 3e8, 4e8]; // veri yoksa varsayılan eksen
        var top = roundMax(max);                      // en yüksek değeri yuvarla
        var step = top / 4;                           // 4'e böl
        return [0, step, 2 * step, 3 * step, top];
    }

    // Oran ekseni sabit (%0-%100); Bakiye ekseni dinamik.
    function yTicks() {
        return isRatio() ? RATIO_TICKS : balanceTicks();
    }
    function buildChartData(raw) {
        return (raw || []).map(function (d) {
            var ratio = isRatio();

            var principal = ratio ? num(d.RatioAnapara) * 100 : num(d.BalanceAnapara);
            var kof = ratio ? num(d.RatioKof) * 100 : num(d.BalanceKof);
            var total = ratio ? (principal + kof) : num(d.BalanceToplam);

            return {
                date: d.ReportDate,
                period: fmtPeriod(d.ReportDate),
                periodLong: fmtPeriodLong(d.ReportDate),

                // Gösterilecek değerler (formatlanır)
                principal: principal,
                kof: kof,
                total: total,

                // Ham sayılar (bar yüksekliği + tooltip)
                principalRaw: principal,
                kofRaw: kof,
                totalRaw: total
            };
        });
    }

    // Panel filtre seçimlerini SP parametrelerine eşler (karşılığı olmayan gönderilmez).
    var FILTER_CODE_TO_FIELD = {
        BUSINESS: 'isKolu',
        ALLOCATION: 'tahsisKolu',
        AUTHORITY: 'yetkiKodu',
        PERIOD: 'katDonem'
    };

    function toInt(v) {
        var n = parseInt(v, 10);
        return isNaN(n) ? null : n;
    }

    function buildRequest() {
        var f = (typeof NPL.getFilters === 'function') ? NPL.getFilters() : {};
        var tab = $('#nplTabList .tab.active').attr('data-npltab') || '';

        var request = {
            bolgeKodu: toInt(f.regionCode),
            subeKodu: toInt(f.branchCode),
            yil: toInt(f.period),
            urun: f.productCode || null,
            // Segment tabı iş kolunu belirler; "Tümü" filtre uygulamaz
            isKolu: (tab && tab !== 'tumu') ? tab.toUpperCase() : null,
            tahsisKolu: null,
            yetkiKodu: null,
            katDonem: null
        };

        // Panel seçimi tab varsayılanını ezer
        Object.keys(FILTER_CODE_TO_FIELD).forEach(function (code) {
            if (f[code]) request[FILTER_CODE_TO_FIELD[code]] = f[code];
        });

        return request;
    }

    function fetchChart(callback) {
        $.ajax({
            url: '/NplReport/GetNplBalanceRatio',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(buildRequest()),
            success: function (data) { callback(data || []); },
            error: function () { callback([]); }
        });
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
        var PLOT_W = n * band;                       // bar alanı (band × bar sayısı)

        // Az veri: SVG kart enine uzar (eksen boydan boya). Çok veri: taşar, yatay scroll.
        var available = $('#nplChart')[0].clientWidth || 0;
        var W = Math.max(ML + PLOT_W + MR, available);
        var baseY = MT + PLOT_H;

        var ticks = yTicks();
        var yMax = ticks[ticks.length - 1] || 1;
        var step = ticks[1] || yMax;                 // kademe büyüklüğü
        var useMn = yMax < TL_PER_BN;                // tavan <1bn -> Mn, değilse bn
        var tickUnit = useMn ? TL_PER_MN : TL_PER_BN;
        var tickSuffix = useMn ? 'Mn' : 'bn';
        var tickDec = stepDecimals(step / tickUnit); // eksen ondalığı (Bakiye)

        function yAt(v) { return baseY - (v / yMax) * PLOT_H; }

        // Y ekseni: kesikli yatay çizgiler + değer etiketleri
        var yAxis = '';
        ticks.forEach(function (t) {
            var y = yAt(t);
            yAxis += '<line x1="' + ML + '" y1="' + y + '" x2="' + (W - MR) + '" y2="' + y + '" class="npl-chart-grid" />';
            yAxis += '<text x="' + (ML - 14) + '" y="' + (y + 4) + '" text-anchor="end" class="npl-chart-axis">' + fmtTick(t, tickUnit, tickSuffix, tickDec) + '</text>';
        });

        var bars = '';
        data.forEach(function (d, i) {
            var cx = ML + i * band + band / 2;
            var x = cx - barW / 2;

            // Yükseklik ham sayıdan; tavanı aşan taşmaz
            var hPrincipal = Math.min((num(d.principalRaw) / yMax) * PLOT_H, PLOT_H);
            var hInterest = Math.min((num(d.kofRaw) / yMax) * PLOT_H, PLOT_H - hPrincipal);
            var yPrincipal = baseY - hPrincipal;
            var yInterest = yPrincipal - hInterest;

            // Tooltip: BalanceToplam'ın ham hâli, fmtNum ile
            bars += '<g class="npl-chart-bar" data-total="' + fmtNum(d.totalRaw) + '">';

            // Alt parça: NPL Anapara, üst parça: NPL Kat Öncesi Faiz
            bars += '<rect x="' + x + '" y="' + yPrincipal + '" width="' + barW + '" height="' + hPrincipal + '" class="npl-bar-principal" />';
            bars += '<rect x="' + x + '" y="' + yInterest + '" width="' + barW + '" height="' + hInterest + '" class="npl-bar-interest" />';

            // Toplam bar üstünde (Oran'da %100 olduğu için yazılmaz).
            // Parça değeri, yeterince yüksekse parça içinde gösterilir.
            if (!isRatio()) {
                bars += '<text x="' + cx + '" y="' + (yInterest - 8) + '" text-anchor="middle" class="npl-chart-total">' + fmtValue(d.total) + '</text>';
            }
            if (hInterest >= MIN_LABEL_H) {
                bars += '<text x="' + cx + '" y="' + (yInterest + hInterest / 2 + 4) + '" text-anchor="middle" class="npl-chart-value">' + fmtValue(d.kof) + '</text>';
            }
            if (hPrincipal >= MIN_LABEL_H) {
                bars += '<text x="' + cx + '" y="' + (yPrincipal + hPrincipal / 2 + 4) + '" text-anchor="middle" class="npl-chart-value">' + fmtValue(d.principal) + '</text>';
            }

            // Şeffaf hedef: tooltip kolonun her yerinde açılsın (Bakiye)
            if (!isRatio()) {
                bars += '<rect x="' + x + '" y="' + MT + '" width="' + barW + '" height="' + PLOT_H + '" class="npl-bar-hover" />';
            }
            bars += '</g>';

            bars += '<text x="' + cx + '" y="' + (baseY + 24) + '" text-anchor="middle" class="npl-chart-axis">' + d.period + '</text>';
        });

        // width/height px (1:1); genişse .npl-chart yatay kaydırır
        var svg =
            '<svg viewBox="0 0 ' + W + ' ' + H + '" width="' + W + '" height="' + H + '">' +
                yAxis + bars +
            '</svg>';

        // Oran görünümünde tooltip yok (toplam sabit %100)
        if (isRatio()) {
            $('#nplChart').html(svg);
            return;
        }

        $('#nplChart').html(svg + '<div class="npl-chart-tooltip" id="nplChartTooltip"></div>');
        bindChartTooltip();
    }

    // Bar hover'da tam (küsüratlı) toplamı gösteren tooltip
    function bindChartTooltip() {
        var $chart = $('#nplChart');
        var $tip = $('#nplChartTooltip');

        $chart.find('.npl-chart-bar')
            .on('mouseenter', function () {
                var $g = $(this);
                $tip.text($g.attr('data-total'));

                // Tooltip'i bar'a hizala (sağı, dikey ortası); konum scrollLeft dahil içerik koordinatı.
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

                // Sağa sığmazsa sola geç, oku çevir
                var visibleRight = topRect.right - chartRect.left;
                var flipped = visibleRight + gap + tw > chartRect.width;
                var left = flipped ? (barLeft - gap - tw) : (barRight + gap);
                // Dikeyde taşmasın; ok yine bar ortasını gösterir
                var top = Math.min(Math.max(0, barMidY - th / 2), Math.max(0, chartRect.height - th));

                // Önce konumlandır sonra göster (ilk hover köşede çıkmasın)
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

    // ===== Tablo görünümü =====
    // Grafikle aynı cevap; kolonlar key ile eşleşir.
    function tableColumns() {
        // Oran'da birim başlıkta (%), alt başlık yok
        if (isRatio()) {
            return [
                { key: 'periodLong', header: 'Dönem',                    align: 'left', sortKey: 'date' },
                { key: 'principal',  header: 'NPL Anapara (%)',          format: fmtValue },
                { key: 'kof',        header: 'NPL Kat Öncesi Faiz (%)',  format: fmtValue },
                { key: 'total',      header: 'Toplam NPL Oranı (%)',     format: fmtValue }
            ];
        }

        // Servis değerleri birebir: Anapara, KOF, Toplam
        return [
            { key: 'periodLong', header: 'Dönem',               align: 'left',  sortKey: 'date' },
            { key: 'principal',  header: 'NPL Anapara',         sub: 'Milyon TL', format: fmtValue },
            { key: 'kof',        header: 'NPL Kat Öncesi Faiz', sub: 'Milyon TL', format: fmtValue },
            { key: 'total',      header: 'Toplam NPL',          sub: 'Milyon TL', format: fmtValue }
        ];
    }

    function legendNote() {
        return isRatio()
            ? 'Değerler yüzde olarak gösterilmektedir.'
            : 'Değerler milyon TL olarak gösterilmektedir.';
    }

    // Varsayılan sıralama yok; başlığa tıklanınca o kolona göre sıralanır.
    var _sort = { key: null, dir: 'desc' };

    function sortedRows() {
        if (!_sort.key) return _chartData.slice();   // servis sırası
        return _chartData.slice().sort(function (a, b) {
            var x = a[_sort.key], y = b[_sort.key];
            var cmp = (typeof x === 'string') ? x.localeCompare(y, 'tr') : (x - y);
            return _sort.dir === 'asc' ? cmp : -cmp;
        });
    }

    function renderTable() {
        var columns = tableColumns();

        var head = '<tr>';
        columns.forEach(function (c) {
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
                '<tr class="no-result-row"><td colspan="' + columns.length + '" style="text-align:center;padding:48px 16px;">' +
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
            columns.forEach(function (c) {
                html += '<td' + (c.align === 'left' ? ' class="col-left"' : '') + '>' +
                            (c.format ? c.format(r[c.key]) : r[c.key]) +
                        '</td>';
            });
            html += '</tr>';
        });
        var $body = $('#nplTableBody').html(html);
        if (typeof reStripeTable === 'function') reStripeTable($body);
    }

    // Başlığa tıkla: aynı kolonsa yön değiş, değilse o kolona geç
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

    // Grafik <-> Tablo geçişi. Toggle masaüstü+mobil kopyalı, [data-view] ile senkron.
    var _view = 'chart';
    function applyView() {
        var isChart = _view === 'chart';
        $('#nplChartCard').toggle(isChart);
        $('#nplChartLegend').toggle(isChart);
        $('#nplTableContainer').toggle(!isChart);
        // Legend görünen kartın içinde kalır
        $('#nplLegend').appendTo(isChart ? '#nplChartCard' : '#nplTableContainer');
        $('.npl-view-toggle [data-view]').removeClass('active');
        $('.npl-view-toggle [data-view="' + _view + '"]').addClass('active');
        $('.table-legend .legend-note').text(legendNote());
        if (isChart) renderChart(); else renderTable();
    }

    $(document).on('click', '.npl-view-toggle [data-view]', function () {
        var view = $(this).data('view');
        if (view === _view) return;
        _view = view;
        applyView();
    });

    // Pencere boyutu değişince yeniden çiz (SVG genişliği karta bağlı)
    var _resizeTimer = null;
    $(window).on('resize', function () {
        if (_view !== 'chart' || !_chartData.length) return;
        clearTimeout(_resizeTimer);
        _resizeTimer = setTimeout(renderChart, 150);
    });

    // Metriğe göre veriyi türet + çiz (servis çağrısız)
    function applyData() {
        _chartData = buildChartData(_rawData);
        applyView();
    }

    // Filtre/sekme değişince servisi çağır -> çiz
    function loadChart() {
        fetchChart(function (res) {
            _rawData = res;
            applyData();
        });
    }

    // filter.js her filtre değişiminde çağırır
    NPL.reload = loadChart;

    // ===== Sekmeler =====
    $('#nplTabList').on('click', '.tab', function () {
        $('#nplTabList .tab').removeClass('active');
        $(this).addClass('active');
        loadChart();
    });

    // ===== Bakiye / Oran (metrik) =====
    // İkisi de aynı cevapta; metrik değişince servis çağrılmaz, sadece yeniden çizilir.
    // Toggle masaüstü+mobil kopyalı, [data-metric] ile senkron.
    var _metric = 'balance';
    $(document).on('click', '.npl-metric-toggle [data-metric]', function () {
        var metric = $(this).data('metric');
        if (metric === _metric) return;
        _metric = metric;
        $('.npl-metric-toggle [data-metric]').removeClass('active');
        $('.npl-metric-toggle [data-metric="' + metric + '"]').addClass('active');
        applyData();
    });

    // PDF motoru (download-pdf.js) data-pdf="nplReport" ile bu config'i çeker.
    window.PdfSources = window.PdfSources || {};
    window.PdfSources.nplReport = function () {
        return {
            title: 'NPL Girişleri',
            infoLines: [
                $('#nplTabList .tab.active').text().trim(),
                $('#nplBreadcrumb').text().replace(/\s+/g, ' ').trim()
            ],
            columns: tableColumns().map(function (c) {
                return { header: c.header, subHeader: c.sub, key: c.key, align: c.align || 'center', format: c.format };
            }),
            rows: sortedRows(),
            footerNote: legendNote(),
            filename: 'NPL-Gecikmeli-Krediler-Raporu.pdf'
        };
    };

    // ===== İlk render =====
    loadChart();
});
