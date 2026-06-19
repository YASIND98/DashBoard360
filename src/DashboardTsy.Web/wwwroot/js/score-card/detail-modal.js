// Skor Kart - Detay modalı (Hedef Detayı + Trend grafiği). Ana rapor: score-card/index.js
// Filtre bağlamı (dateNumber/registerId/regionCode/branchCode/scoreCardId) ScoreCard.getContext()'ten gelir.
$(function () {

    if (!document.getElementById('scReportBody')) return;

    // Ana rapordan (score-card/index.js) paylaşılan filtre bağlamı:
    // dateNumber/registerId/regionCode/branchCode/scoreCardId -> ScoreCard.getContext()
    function scCtx() {
        return (window.ScoreCard && window.ScoreCard.getContext) ? window.ScoreCard.getContext() : {};
    }

    // Detay modalı (satırdaki Detay ikonundan açılır)

    // Satır/ürün durumu -> hücre içi ikon (eski tablo: Gerçekleşmeyen / Hedef Dışı)
    var STATUS_ICON = {
        realized: '/images/realized.svg',
        pending: '/images/pending.svg',
        unrealized: '/images/unrealized.svg',
        offtarget: '/images/off-target-sales.svg'
    };

    // Filtre çipi ikonları: pasif (base) / seçili (active) -> *-selected.svg
    var FILTER_ICON = {
        realized:   { base: '/images/realized.svg',   active: '/images/realized-selected.svg' },
        pending:    { base: '/images/pending.svg',    active: '/images/pending-selected.svg' },
        unrealized: { base: '/images/unrealized.svg', active: '/images/unrealized-selected.svg' },
        offtarget:  { base: '/images/off-target-sales.svg', active: '/images/off-target-sales-selected.svg' }
    };

    var PAGE_SIZE = 13;

    var _currentPage = 1;
    var _statusFilter = 'realized';
    var _response = { columns: [], rows: [] }; // o anki (mock) servis cevabı

    var _detailCtx = {};

    // HTML metnini güvenli hale getir (hücre içeriği)
    function escapeHtml(s) {
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }

    // ISO tarih ("2026-05-05T00:00:00") -> "05.05.2026"
    function fmtIsoDate(v) {
        if (!v) return '';
        var datePart = String(v).split('T')[0];
        var p = datePart.split('-');
        return p.length === 3 ? (p[2] + '.' + p[1] + '.' + p[0]) : String(v);
    }

    // Servis (ScoreCardDetail) cevabını dinamik kolonlu tabloya çevirir.
    // Kolonlar SCORE_CARD_DETAIL_COLUMN_LABELS sırasından, veride var olan alanlarla kurulur.
    function buildDynamicResponse(scoreCardDetail) {
        var rows = [];
        try {
            rows = JSON.parse(scoreCardDetail) || [];
        } catch (e) {
            rows = [];
        }
        var labels = (typeof SCORE_CARD_DETAIL_COLUMN_LABELS !== 'undefined')
            ? SCORE_CARD_DETAIL_COLUMN_LABELS : [];
        var sample = rows.length ? rows[0] : {};
        var columns = labels.filter(function (c) {
            return Object.prototype.hasOwnProperty.call(sample, c.key);
        });
        return { columns: columns, rows: rows, dynamic: true };
    }

    // Dinamik tabloda tek hücreyi alan adına göre biçimlendir
    function formatDetailCell(key, value) {
        if (value == null) return '';
        if (key === 'ACILIS_TARIHI') return fmtIsoDate(value);
        if (key === 'ACCOUNT_NUMBER') return '<span class="sc-account">' + escapeHtml(String(value)) + '</span>';
        return escapeHtml(String(value).trim());
    }

    //scorecards/details: Hedef Detayı servisi — seçili ürünün sicil/işlem detayları (detay modalı)
    function requestScoreCardDetail(status, callback) {
        var statusCode = (typeof SCORE_CARD_DETAIL_STATUS !== 'undefined' && SCORE_CARD_DETAIL_STATUS[status] != null)
            ? SCORE_CARD_DETAIL_STATUS[status] : 0;

        function handle(raw) {
            if (raw && raw.ScoreCardDetail != null) {
                callback(buildDynamicResponse(raw.ScoreCardDetail));
            } else {
                callback(raw || { columns: [], rows: [] });
            }
        }

        var ctx = scCtx();
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecards/details',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                dateNumber: ctx.dateNumber,
                registerId: ctx.registerId,
                productId: _detailCtx.productId,
                status: statusCode,
                productType: _detailCtx.productType
            })
        }).done(function (res) {
            handle(res);
        }).fail(function () {
            handle((typeof getScoreCardDetailMock === 'function') ? getScoreCardDetailMock(status) : null);
        });
    }

    function totalPages() {
        return Math.max(1, Math.ceil(_response.rows.length / PAGE_SIZE));
    }

    // Kolonlar servis cevabından gelir
    function getColumns() {
        return _response.columns || [];
    }

    function renderDetailHead() {
        var html = '<tr>';
        if (_response.dynamic) {
            // Yeni servis: kolon başlıkları dinamik (constants.js eşlemesiyle)
            _response.columns.forEach(function (c) { html += '<th>' + c.label + '</th>'; });
        } else {
            // Eski tablo: kolon başlıkları düz metin
            getColumns().forEach(function (c) { html += '<th>' + c + '</th>'; });
        }
        html += '</tr>';
        $('#scTableHead').html(html);
    }

    function renderDetailBody() {
        var rows = _response.rows;
        var start = (_currentPage - 1) * PAGE_SIZE;
        var pageRows = rows.slice(start, start + PAGE_SIZE);

        // Seçili filtrede kayıt yoksa: kolon başlığı olmadan ikon + metin
        if (pageRows.length === 0) {
            $('#scTableBody').html(
                '<tr class="sc-empty-row"><td>' +
                    '<div class="sc-empty-state">' +
                        '<img src="/images/empty-state-seach.svg" alt="" />' +
                        '<span>Seçili döneme ait veri bulunmamaktadır.</span>' +
                    '</div>' +
                '</td></tr>'
            );
            return;
        }

        if (_response.dynamic) {
            renderDynamicBody(pageRows);
            return;
        }

        if (_response.expandable) {
            renderExpandableBody(pageRows, start);
            return;
        }

        // Eski düz tablo (Hedef Dışı): durum ikonu + ürün adı
        var html = '';
        var icon = STATUS_ICON[_statusFilter] || '';
        pageRows.forEach(function (r, i) {
            html += '<tr class="' + (i % 2 === 0 ? 'sc-zebra' : '') + '">';
            html += '<td>' + r.date + '</td>';
            for (var pi = 0; pi < 3; pi++) {
                var pname = r.products[pi] || '';
                html += '<td><span class="sc-prod">' +
                    (icon ? '<img class="sc-prod-icon" src="' + icon + '" alt="" />' : '') +
                    '<span>' + pname + '</span></span></td>';
            }
            html += '<td><span class="sc-account">' + r.accountNo + '</span></td>';
            html += '<td>' + r.customerName + '</td>';
            html += '<td>' + r.kazanim + '</td>';
            html += '<td>' + r.customerStatus + '</td>';
            html += '</tr>';
        });
        $('#scTableBody').html(html);
    }

    // Gerçekleşmeyen: özet satır + (expand'de) ürün/sebep detayları
    function renderExpandableBody(pageRows, start) {
        var colCount = getColumns().length;
        var html = '';
        pageRows.forEach(function (r, i) {
            var rid = start + i;
            html += '<tr class="sc-main-row ' + (i % 2 === 0 ? 'sc-zebra' : '') + '" data-row="' + rid + '">';
            html += '<td>' + r.date + '</td>';
            html += '<td><span class="sc-summary">' + (r.summary || '') + '</span></td>';
            html += '<td><span class="sc-account">' + r.accountNo + '</span></td>';
            html += '<td>' + r.customerName + '</td>';
            html += '<td>' + r.kazanim + '</td>';
            html += '<td>' + r.customerStatus + '</td>';
            html += '<td class="sc-expand-cell"><img class="sc-expand-icon" src="/images/expand.svg" alt="Detay" data-row="' + rid + '" /></td>';
            html += '</tr>';

            // Detay satırı (varsayılan gizli)
            html += '<tr class="sc-detail-row" data-row="' + rid + '" style="display:none;"><td colspan="' + colCount + '">';
            html += '<div class="sc-details">';
            (r.details || []).forEach(function (d) {
                var ok = d.status === 'realized';
                var cls = ok ? 'sc-detail-realized' : 'sc-detail-unrealized';
                var statusTxt = ok ? 'Gerçekleşti' : 'Gerçekleşmedi';
                var bar = '<div class="sc-detail ' + cls + '">' +
                    '<span class="sc-detail-label">Ürün:</span>' +
                    '<span class="sc-detail-product">' + d.product + '</span>' +
                    '<span class="sc-detail-sep"></span>' +
                    '<span class="sc-detail-status">' + statusTxt + '</span>';
                if (!ok) {
                    bar += '<span class="sc-detail-sep"></span>' +
                        '<span class="sc-detail-label">' + (d.reasonCode || 'Reason Code') + ':</span>' +
                        '<span class="sc-detail-reason">' + (d.reasonText || '') + '</span>';
                }
                bar += '</div>';
                html += bar;
            });
            html += '</div></td></tr>';
        });
        $('#scTableBody').html(html);
    }

    // Dinamik kolonlu (ScoreCardDetail) servis cevabını render et
    function renderDynamicBody(pageRows) {
        var cols = _response.columns;
        var html = '';
        pageRows.forEach(function (r, i) {
            html += '<tr class="' + (i % 2 === 0 ? 'sc-zebra' : '') + '">';
            cols.forEach(function (c) {
                html += '<td>' + formatDetailCell(c.key, r[c.key]) + '</td>';
            });
            html += '</tr>';
        });
        $('#scTableBody').html(html);
    }

    function renderPagination() {
        var pages = totalPages();
        var html = '';
        html += '<button type="button" class="sc-page-arrow" data-page="prev"' + (_currentPage === 1 ? ' disabled' : '') + '>‹</button>';
        html += '<span class="sc-page-info">' + _currentPage + '/' + pages + '</span>';
        html += '<button type="button" class="sc-page-arrow" data-page="next"' + (_currentPage === pages ? ' disabled' : '') + '>›</button>';
        $('#scPagination').html(html);
    }

    function renderTable() {
        renderDetailHead();
        renderDetailBody();
        renderPagination();
    }

    // Aktif filtre için (mock) servis isteği at; cevap gelince tabloyu render et
    function loadDetail() {
        _currentPage = 1;
        $('#scTableHead').empty();
        $('#scTableBody').html(
            '<tr class="sc-empty-row"><td colspan="' + getColumns().length + '">Yükleniyor...</td></tr>'
        );
        $('#scPagination').empty();
        requestScoreCardDetail(_statusFilter, function (res) {
            _response = res;
            renderTable();
        });
    }

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

    // 10) scorecards/trends/product-sale-realized: seçili ürünün H/G trend grafiği (seçili trendPeriod).
    function fetchScoreCardTrend(trendPeriod, callback) {
        var ctx = scCtx();
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecards/trends/product-sale-realized',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                productId: _detailCtx.productId,         // detay modalındaki ürün
                registerId: ctx.registerId,              // seçili sicil
                branchCode: ctx.branchCode,              // seçili şube
                regionCode: ctx.regionCode,              // seçili bölge
                trendPeriod: trendPeriod,
                scoreCardId: ctx.scoreCardId             // aktif skor kart sekmesi
            })
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(getScoreCardTrendMock());
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

    function formatDecimal(v) {
        return new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(v);
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
                    '<div class="sc-tt-title">%' + formatDecimal(+$t.attr('data-hg')) + '</div>' +
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

    // Sekme görünümünü değiştir:
    //   hedef -> #scTargetDetail (filtre + tablo + footer) görünür, trend gizli
    //   trend -> #scTrend görünür, #scTargetDetail gizli
    function showTab(tab) {
        if (tab === 'trend') {
            $('#scTargetDetail').hide();
            $('#scTrend').show();
            loadTrend();
        } else {
            $('#scTrend').hide();
            $('#scTargetDetail').show();
            loadDetail();
        }
    }

    // Modal aç/kapat
    function openModal() {
        // Aktif sekmeye göre doğru bölümü göster (Hedef Detayı varsayılan)
        var activeTab = $('#scTabs .sc-tab.active').data('tab') || 'hedef';
        showTab(activeTab);
        $('#scOverlay').addClass('active');
    }

    function closeModal() {
        $('#scOverlay').removeClass('active');
    }

    $('#scOpenBtn').on('click', openModal);
    // Rapor tablosundaki "Detay" ikonu modalı açar (başlık satır adından)
    $(document).on('click', '.sc-detail-icon', function () {
        var $i = $(this);
        var name = $i.data('name');
        if (name) $('#scModalTitle').text(name);
        // Tıklanan ürünün detay bağlamını al (satırdan data-* ile gelir)
        _detailCtx = {
            productId: $i.data('product-id'),
            productType: $i.data('product-type')
        };
        openModal();
    });
    $('#scClose').on('click', closeModal);

    // Overlay boşluğuna tıklayınca kapat
    $('#scOverlay').on('click', function (e) {
        if (e.target === this) closeModal();
    });

    // Modal sekmeleri (Hedef Detayı / Trend)
    $('#scTabs').on('click', '.sc-tab', function () {
        $('#scTabs .sc-tab').removeClass('active');
        $(this).addClass('active');
        showTab($(this).data('tab'));
    });

    // Trend periyot sekmeleri
    $('#scTrendTabs').on('click', '.sc-trend-tab', function () {
        $('#scTrendTabs .sc-trend-tab').removeClass('active');
        $(this).addClass('active');
        loadTrend();
    });

    // Aktif çipte seçili ikonu (-selected.svg), diğerlerinde normal ikonu göster
    function updateFilterIcons() {
        $('#scFilters .sc-filter').each(function () {
            var map = FILTER_ICON[$(this).data('filter')];
            if (!map) return;
            var src = $(this).hasClass('active') ? map.active : map.base;
            $(this).find('.sc-filter-icon').attr('src', src);
        });
    }

    // Filtre çipleri
    $('#scFilters').on('click', '.sc-filter', function () {
        $('#scFilters .sc-filter').removeClass('active');
        $(this).addClass('active');
        updateFilterIcons();
        _statusFilter = $(this).data('filter');
        // Her filtrede ayrı servis isteği (mock) at
        loadDetail();
    });

    updateFilterIcons();

    // Sayfalama
    $('#scPagination').on('click', '.sc-page-arrow', function () {
        var page = $(this).data('page');
        if (page === 'prev') {
            if (_currentPage > 1) _currentPage--;
        } else if (page === 'next') {
            if (_currentPage < totalPages()) _currentPage++;
        }
        renderDetailBody();
        renderPagination();
    });

    // Satır expand/collapse (Gerçekleşmeyen)
    $('#scTableBody').on('click', '.sc-expand-icon', function () {
        var rid = $(this).data('row');
        var $detail = $('#scTableBody .sc-detail-row[data-row="' + rid + '"]');
        var willOpen = !$detail.is(':visible');
        $detail.toggle(willOpen);
        $(this).toggleClass('open', willOpen);
        $('#scTableBody .sc-main-row[data-row="' + rid + '"]').toggleClass('expanded', willOpen);
    });
});
