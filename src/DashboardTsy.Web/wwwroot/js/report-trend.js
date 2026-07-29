// Hedef Raporları - Ürün Detayı modalı Trend Analizi sekmesi (Hacim > H/G ve Adet tabloları).
// H/G (monthly) -> /TargetReport/GetVolumeTrendAnalysis (RP_Hacimler_Trend_Analizi, HACIM/TL serisi).
// Adet (quantity) -> /TargetReport/GetQuantityTrendAnalysis (RP_Adetler_Trend_Analizi, ADET serisi).
$(function () {

    if (!document.getElementById('reportDetailTrendTab')) return;

    var _trMonthsShort = (typeof _trMonths !== 'undefined' ? _trMonths : []).map(function (m) { return m.substring(0, 3); });
    var _trendData = { labels: [], values: [], points: [], isVolume: false, productName: '' };

    // Y ekseni ölçek etiketleri: formatNumber 0 için "-" döndürür (tablo hücreleri için doğru),
    // ama eksen ucundaki 0 noktasının "0" olarak görünmesi gerekir.
    function axisLabel(v, isVolume, productName) {
        var num = new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 0 }).format(Math.round(v));
        if (!isVolume) return num;
        var currency = (productName && productName.indexOf('YP') !== -1) ? '$' : '₺';
        return currency + ' ' + num;
    }

    function monthLabel(iso) {
        var d = new Date(iso);
        if (isNaN(d.getTime())) return '';
        return _trMonthsShort[d.getMonth()] + ' ' + String(d.getFullYear()).slice(-2);
    }

    // H/G tablosunda Hacim (TL), Adet tablosunda Adet (Count) serisi gösterilir.
    function buildTrend(raw, isVolume) {
        var list = raw || [];
        return {
            labels: list.map(function (d) { return monthLabel(d.ReportDate); }),
            values: list.map(function (d) { return isVolume ? d.Amount : d.Count; }),
            points: list,
            isVolume: isVolume,
            productName: list.length ? list[0].ProductName : ''
        };
    }

    function filterContext() {
        return (typeof window.getTargetReportFilterContext === 'function') ? window.getTargetReportFilterContext() : {};
    }

    // tableKey 'monthly' (H/G) -> GetVolumeTrendAnalysis; 'quantity' (Adet) -> GetQuantityTrendAnalysis.
    function fetchTrend(callback) {
        var iconCtx = (window.ReportDetail && window.ReportDetail.iconCtx) || {};
        var isVolume = iconCtx.table === 'monthly';
        var filters = filterContext();
        $.ajax({
            url: isVolume ? '/TargetReport/GetVolumeTrendAnalysis' : '/TargetReport/GetQuantityTrendAnalysis',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                sessionId: '1',
                productId: iconCtx.productId,
                userCode: window.USER_CODE,
                urun: iconCtx.productName,
                bolge: filters.regionName,
                subeKodu: filters.branchCode,
                isKolu: filters.tabLabel,
                segment: filters.subTabLabel
            })
        }).done(function (res) {
            callback(res, isVolume);
        }).fail(function () {
            callback(null, isVolume);
        });
    }

    function loadTrend() {
        fetchTrend(function (res, isVolume) {
            _trendData = buildTrend(res, isVolume);
            renderTrendChart();
        });
    }

    function renderTrendChart() {
        var $chart = $('#reportDetailTrendChart');
        var data = _trendData;
        if (!data || !data.values.length) {
            $chart.html('<div class="report-detail-trend-empty">Seçili ürüne ait trend verisi bulunmamaktadır.</div>');
            return;
        }

        var W = 760, H = 320;
        var ml = 70, mr = 20, mt = 20, mb = 40;
        var plotW = W - ml - mr;
        var plotH = H - mt - mb;

        // Y ekseni dinamik: veri aralığına göre ölçeklenir (skorkart'taki sabit %0-20 ölçeğinin aksine).
        var maxVal = Math.max.apply(null, data.values);
        var minVal = Math.min.apply(null, data.values);
        var range = maxVal - minVal;
        var pad = range > 0 ? range * 0.15 : Math.max(maxVal * 0.1, 1);
        var top = maxVal + pad;
        var bottom = Math.max(0, minVal - pad);

        var TICK_COUNT = 5;
        var yTicks = [];
        for (var t = 0; t <= TICK_COUNT; t++) {
            yTicks.push(bottom + (top - bottom) * t / TICK_COUNT);
        }

        var n = data.values.length;
        var step = n > 1 ? plotW / (n - 1) : 0;

        function xAt(i) { return ml + i * step; }
        function yAt(v) { return mt + plotH - ((v - bottom) / (top - bottom || 1)) * plotH; }

        var linePath = '';
        if (n === 1) {
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

        var yLabels = '', yGrid = '';
        yTicks.forEach(function (val) {
            var y = yAt(val);
            yGrid += '<line x1="' + ml + '" y1="' + y + '" x2="' + (W - mr) + '" y2="' + y + '" class="rd-trend-grid" />';
            yLabels += '<text x="' + (ml - 12) + '" y="' + (y + 4) + '" text-anchor="end" class="rd-trend-axis">' + axisLabel(val, data.isVolume, data.productName) + '</text>';
        });

        var xLabels = '', xGrid = '', xDots = '', hovers = '';
        for (var j = 0; j < n; j++) {
            var hx = n === 1 ? (ml + plotW / 2) : xAt(j);
            var hy = yAt(data.values[j]);
            xGrid += '<line x1="' + xAt(j) + '" y1="' + mt + '" x2="' + xAt(j) + '" y2="' + (mt + plotH) + '" class="rd-trend-grid" />';
            xDots += '<circle cx="' + xAt(j) + '" cy="' + (mt + plotH) + '" r="2.5" class="rd-trend-dot" />';
            xLabels += '<text x="' + xAt(j) + '" y="' + (H - mb + 24) + '" text-anchor="middle" class="rd-trend-axis">' + data.labels[j] + '</text>';
            hovers += '<g class="rd-trend-point">' +
                '<line x1="' + hx + '" y1="' + mt + '" x2="' + hx + '" y2="' + (mt + plotH) + '" class="rd-trend-vline" />' +
                '<circle cx="' + hx + '" cy="' + hy + '" r="10" class="rd-trend-hover" data-index="' + j + '" />' +
                '<circle cx="' + hx + '" cy="' + hy + '" r="4" class="rd-trend-marker" />' +
            '</g>';
        }

        var svg =
            '<svg viewBox="0 0 ' + W + ' ' + H + '" width="100%" preserveAspectRatio="xMidYMid meet">' +
                '<rect x="' + ml + '" y="' + mt + '" width="' + plotW + '" height="' + plotH + '" class="rd-trend-plot-bg" />' +
                '<path d="' + areaPath + '" class="rd-trend-area" />' +
                yGrid + xGrid +
                '<path d="' + linePath + '" class="rd-trend-line" />' +
                xDots +
                yLabels + xLabels +
                hovers +
            '</svg>';

        $chart.html(svg + '<div class="rd-trend-tooltip" id="reportDetailTrendTooltip"></div>');
        bindTrendTooltip();
    }

    function bindTrendTooltip() {
        var $chart = $('#reportDetailTrendChart');
        var $tip = $('#reportDetailTrendTooltip');

        $chart.find('.rd-trend-hover')
            .on('mouseenter', function () {
                var idx = +$(this).attr('data-index');
                $tip.html(
                    '<div class="rd-tt-title">' + formatNumber(_trendData.values[idx], _trendData.isVolume, _trendData.productName) + '</div>' +
                    '<div class="rd-tt-row">' + _trendData.labels[idx] + '</div>'
                ).show();

                var chartRect = $chart[0].getBoundingClientRect();
                var dot = this.getBoundingClientRect();
                var pointX = dot.left + dot.width / 2 - chartRect.left;
                var pointY = dot.top + dot.height / 2 - chartRect.top;

                var tw = $tip.outerWidth();
                var th = $tip.outerHeight();
                var gap = 12;

                var flipped = pointX + gap + tw > chartRect.width;
                var left = flipped ? (pointX - gap - tw) : (pointX + gap);
                var top = pointY - th / 2;

                $tip.toggleClass('rd-tt-left', flipped);
                $tip.css({ left: left + 'px', top: top + 'px' });
            })
            .on('mouseleave', function () {
                $tip.hide();
            });
    }

    window.ReportTrend = { load: loadTrend };

    // Genel PDF motoru (download-pdf.js) bu config'i data-pdf="reportTrend" üzerinden çeker.
    window.PdfSources = window.PdfSources || {};
    window.PdfSources.reportTrend = function () {
        var isVolume = _trendData.isVolume;
        return {
            title: ($('.report-detail-title').text() || '').trim(),
            infoLines: ['Trend Analizi'],
            columns: [
                { header: 'Dönem', key: 'label', align: 'left' },
                { header: isVolume ? 'Hacim' : 'Adet', key: 'value', format: function (v) { return formatNumber(v, isVolume, _trendData.productName); } }
            ],
            rows: (_trendData.values || []).map(function (v, i) { return { value: v, label: _trendData.labels[i] }; }),
            filename: 'HedefRapor-Trend-Analizi.pdf'
        };
    };
});
