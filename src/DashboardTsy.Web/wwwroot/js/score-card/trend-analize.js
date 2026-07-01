// Skor Kart - Detay modalı trend Analizi sekmesi.
$(function () {

    if (!document.getElementById('scReportBody')) return;

    function scCtx() { return (window.ScoreCard && window.ScoreCard.getContext) ? window.ScoreCard.getContext() : {}; }
    function detailCtx() { return (window.ScoreCardDetail && window.ScoreCardDetail.ctx) || {}; }

    // Trend Analizi (scorecards/trends/product-sale-realized)
    var _trendData = { labels: [], values: [], points: [] };

    // Servis cevabı kronolojik sırada gelir; doğrudan grafik verisine çevir
    function buildTrend(raw) {
        var list = raw || [];
        return {
            labels: list.map(function (d) { return d.period; }),
            values: list.map(function (d) { return d.hgRatio; }),
            points: list
        };
    }

    //scorecards/trends/product-sale-realized: seçili ürünün H/G trend grafiği (seçili trendPeriod).
    function fetchScoreCardTrend(trendPeriod, callback) {
        var ctx = scCtx();
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecards/trends/product-sale-realized',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                productId: detailCtx().productId,
                registerId: ctx.registerId,
                branchCode: ctx.branchCode,
                regionCode: ctx.regionCode,
                trendPeriod: trendPeriod,
                scoreCardId: ctx.scoreCardId
            })
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(null);
        });
    }

    // Aktif trend sekmesine göre servisi tetikle -> grafiği çiz
    function loadTrend() {
        var tabKey = $('#scTrendTabs .sc-trend-tab.active').data('trend-period') || 'bu-ay';
        var trendPeriod = (typeof SCORE_CARD_TREND_PERIOD !== 'undefined' && SCORE_CARD_TREND_PERIOD[tabKey]) || 1;
        fetchScoreCardTrend(trendPeriod, function (res) {
            _trendData = buildTrend(res);
            renderTrendChart();
        });
    }

    function renderTrendChart() {
        var data = _trendData;
        if (!data || !data.values.length) return;

        var W = 700, H = 320;
        var ml = 60, mr = 20, mt = 20, mb = 40;
        var plotW = W - ml - mr;
        var plotH = H - mt - mb;

        // Y ekseni statik: HG oranı (%) — sabit ölçek 0..200
        var top = 200;
        var Y_TICKS = [0, 50, 75, 100, 150, 200];
        var n = data.values.length;
        var step = n > 1 ? plotW / (n - 1) : 0;

        function xAt(i) { return ml + i * step; }
        function yAt(v) { return mt + plotH - (v / top) * plotH; }

        var linePath = '';
        if (n === 1) {
            // Tek nokta: değeri tüm genişlik boyunca düz çizgi olarak göster
            var ySingle = yAt(data.values[0]);
            linePath = 'M' + ml + ',' + ySingle + 'L' + (W - mr) + ',' + ySingle;
        } else {
            for (var i = 0; i < n; i++) {
                linePath += (i === 0 ? 'M' : 'L') + xAt(i) + ',' + yAt(data.values[i]);
            }
        }
        var areaLeft = ml;
        var areaRight = n === 1 ? (W - mr) : xAt(n - 1);
        var areaPath = linePath +
            ' L' + areaRight + ',' + (mt + plotH) +
            ' L' + areaLeft + ',' + (mt + plotH) + ' Z';

        var yLabels = '', yGrid = '', yDots = '';
        Y_TICKS.forEach(function (val) {
            var y = yAt(val);
            // %75 eşik çizgisi vurgulu (yeşil)
            var hl = val === 75 ? ' sc-trend-hl' : '';
            yGrid += '<line x1="' + ml + '" y1="' + y + '" x2="' + (W - mr) + '" y2="' + y + '" class="sc-trend-grid' + hl + '" />';
            yDots += '<circle cx="' + ml + '" cy="' + y + '" r="2.5" class="sc-trend-dot' + hl + '" />';
            yLabels += '<text x="' + (ml - 12) + '" y="' + (y + 4) + '" text-anchor="end" class="sc-trend-axis' + hl + '">%' + val + '</text>';
        });
        var xLabels = '', xGrid = '', xDots = '', hovers = '';
        for (var j = 0; j < n; j++) {
            var hx = n === 1 ? (ml + plotW / 2) : xAt(j);
            var hy = yAt(data.values[j]);
            var pt = data.points[j];
            xGrid += '<line x1="' + xAt(j) + '" y1="' + mt + '" x2="' + xAt(j) + '" y2="' + (mt + plotH) + '" class="sc-trend-grid" />';
            xDots += '<circle cx="' + xAt(j) + '" cy="' + (mt + plotH) + '" r="2.5" class="sc-trend-dot" />';
            xLabels += '<text x="' + xAt(j) + '" y="' + (H - mb + 24) + '" text-anchor="middle" class="sc-trend-axis">' + data.labels[j] + '</text>';
            // Üzerine gelince: tam boy dikey çizgi + nokta işareti + tooltip hedefi
            hovers += '<g class="sc-trend-point">' +
                '<line x1="' + hx + '" y1="' + mt + '" x2="' + hx + '" y2="' + (mt + plotH) + '" class="sc-trend-vline" />' +
                '<circle cx="' + hx + '" cy="' + hy + '" r="10" class="sc-trend-hover" ' +
                'data-period="' + pt.period + '" ' +
                'data-hg="' + pt.hgRatio + '" />' +
                '<circle cx="' + hx + '" cy="' + hy + '" r="4" class="sc-trend-marker" />' +
            '</g>';
        }

        var svg =
            '<svg viewBox="0 0 ' + W + ' ' + H + '" width="100%" preserveAspectRatio="xMidYMid meet">' +
                '<rect x="' + ml + '" y="' + mt + '" width="' + plotW + '" height="' + plotH + '" class="sc-trend-plot-bg" />' +
                '<path d="' + areaPath + '" class="sc-trend-area" />' +
                yGrid + xGrid +
                '<path d="' + linePath + '" class="sc-trend-line" />' +
                xDots + yDots +
                yLabels + xLabels +
                hovers +
            '</svg>';

        $('#scTrendChart').html(svg + '<div class="sc-trend-tooltip" id="scTrendTooltip"></div>');
        bindTrendTooltip();
    }

    // Hover hedeflerine tooltip davranışını bağla
    function bindTrendTooltip() {
        var $chart = $('#scTrendChart');
        var $tip = $('#scTrendTooltip');

        $chart.find('.sc-trend-hover')
            .on('mouseenter', function () {
                var $t = $(this);
                $tip.html(
                    '<div class="sc-tt-title">%' + formatPercent(+$t.attr('data-hg')) + '</div>' +
                    '<div class="sc-tt-row">' + $t.attr('data-period') + '</div>'
                ).show();

                // Tooltip'i imlece değil, veri noktasına hizala: ok ucu noktanın hemen yanında
                var chartRect = $chart[0].getBoundingClientRect();
                var dot = this.getBoundingClientRect();
                var pointX = dot.left + dot.width / 2 - chartRect.left;
                var pointY = dot.top + dot.height / 2 - chartRect.top;

                var tw = $tip.outerWidth();
                var th = $tip.outerHeight();
                var gap = 12; // ok payı + küçük boşluk

                // Varsayılan: nokta sağında; taşarsa soluna al ve oku çevir
                var flipped = pointX + gap + tw > chartRect.width;
                var left = flipped ? (pointX - gap - tw) : (pointX + gap);
                var top = pointY - th / 2; // ok dikeyde (top:50%) nokta hizasında

                $tip.toggleClass('sc-tt-left', flipped);
                $tip.css({ left: left + 'px', top: top + 'px' });
            })
            .on('mouseleave', function () {
                $tip.hide();
            });
    }

    // Trend periyot sekmeleri
    $('#scTrendTabs').on('click', '.sc-trend-tab', function () {
        $('#scTrendTabs .sc-trend-tab').removeClass('active');
        $(this).addClass('active');
        loadTrend();
    });

    window.ScoreCardDetail = window.ScoreCardDetail || {};
    window.ScoreCardDetail.loadTrend = loadTrend;

    // "PDF İndir" butonu yalnızca Trend Analizi sekmesi açıkken görünür
    function syncTrendPdfBtn() { $('#scTrendPdfBtn').toggle($('#scTrend').is(':visible')); }
    $(document).on('click', '#scTabs .sc-tab, .sc-detail-icon', function () { setTimeout(syncTrendPdfBtn, 0); });

    // Genel PDF motoru (download-pdf.js) bu config'i data-pdf="scTrend" üzerinden çeker:
    // kolon başlıkları + her kolonun key'i + satır verisi (trend servis/mock cevabı) + başlık/info.
    window.PdfSources = window.PdfSources || {};
    window.PdfSources.scTrend = function () {
        return {
            title: $('#scModalTitle').text().trim(),
            infoLines: ['Trend Analizi', 'Periyot: ' + ($('#scTrendTabs .sc-trend-tab.active').text().trim() || '-')],
            columns: [
                { header: 'Dönem', key: 'period', align: 'left' },
                { header: 'H/G %', key: 'hgRatio', format: formatPercent }   // diğer tablolarla aynı; % başlıkta var
            ],
            rows: (_trendData && _trendData.points) || [],
            filename: 'SkorKart-Trend-Analizi.pdf'
        };
    };
});
