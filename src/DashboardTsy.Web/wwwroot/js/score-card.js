// ===== Skor Kart - Aktif Kredi Kartı Performansı (mock) =====
$(function () {
    // Satır/ürün durumu -> hücre içi ikon
    // realized = yeşil, pending = turuncu, unrealized = kırmızı, offtarget = hedef dışı
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

    // ===== Mock servis cevapları =====
    // Her sekme/filtre kendi ayrı response'una sahip: { columns: [...], rows: [...] }
    // İleride her biri ayrı bir servis çağrısının cevabı olacak.

    // ----- Gerçekleşen -----
    var REALIZED_COLUMNS = ['Açılış Tarihi', 'Ürün 1', 'Ürün 2', 'Ürün 3', 'Hesap No', 'Müşteri Adı', 'Kazanım', 'Müşteri Durumu'];
    var REALIZED_ROWS = [
        { date: '03.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052701', customerName: 'Kerim Kumbaracı', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '03.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052702', customerName: 'Ahmet Nedim Paksoy', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '03.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052703', customerName: 'Selin Yıldırım', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '03.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052704', customerName: 'Murat Can Özdemir', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '03.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052705', customerName: 'Zeynep Ece Şahin', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '03.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052706', customerName: 'Emre Deniz Yılmaz', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '04.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052707', customerName: 'Aylin Kaya', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '04.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052708', customerName: 'Barış Tuncer', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '04.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052709', customerName: 'Elif Su Acar', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '04.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052710', customerName: 'Deniz Arslan', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '04.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052711', customerName: 'Seda Yılmaz', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '04.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052712', customerName: 'Onur Çelik', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '04.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052713', customerName: 'Melis Karaca', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '05.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052714', customerName: 'Kerim Kumbaracı', kazanim: 'Ürün', customerStatus: 'Yeni' }
    ];

    // ----- Bekleyen (Kalan Gün kolonlu) -----
    var PENDING_COLUMNS = ['Açılış Tarihi', 'Ürün 1', 'Ürün 2', 'Ürün 3', 'Kalan Gün', 'Hesap No', 'Müşteri Adı', 'Kazanım', 'Müşteri Durumu'];
    var PENDING_ROWS = [
        { date: '03.04.2026', remainingDays: 12, products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052731', customerName: 'Hande Selvi', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '03.04.2026', remainingDays: 8,  products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052732', customerName: 'Cenk Aydın', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '03.04.2026', remainingDays: 5,  products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052733', customerName: 'Pelin Korkmaz', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '04.04.2026', remainingDays: 20, products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052734', customerName: 'Tarık Demir', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '04.04.2026', remainingDays: 3,  products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052735', customerName: 'Buse Aksoy', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '04.04.2026', remainingDays: 15, products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052736', customerName: 'Kaan Eren', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '05.04.2026', remainingDays: 7,  products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052737', customerName: 'Nazlı Şen', kazanim: 'Ürün', customerStatus: 'Yeni' }
    ];

    // ----- Gerçekleşmeyen (satırlar expand edilir; detayda ürün + sebep) -----
    // summary  : satır kapalıyken görünen özet metin
    // details  : expand edilince ürün bazında durum + sebep (reason code)
    //   detail.status: 'realized' | 'unrealized'  (bir satırın içinde ikisi de olabilir)
    var UNREALIZED_COLUMNS = ['Açılış Tarihi', 'Gerçekleşme Durumu', 'Hesap No', 'Müşteri Adı', 'Kazanım', 'Müşteri Durumu', ''];
    var UNREALIZED_ROWS = [
        {
            date: '03.04.2026', accountNo: '26052751', customerName: 'Gökhan Ulus', kazanim: 'Ürün', customerStatus: 'Yeni',
            summary: 'Ürün Satışı Gerçekleşmedi / Süre Açıldı',
            details: [
                { product: 'GSM Fatura', status: 'unrealized', reasonCode: 'Reason Code', reasonText: 'Süre Açıldı' },
                { product: 'Deniz Bonus', status: 'unrealized', reasonCode: 'Reason Code', reasonText: 'Müşteri Talebi Yok' }
            ]
        },
        {
            date: '03.04.2026', accountNo: '26052752', customerName: 'İrem Doğan', kazanim: 'Ürün', customerStatus: 'Yeni',
            summary: 'Ürün Satışı Gerçekleşmedi / Süre Açıldı',
            details: [
                { product: 'GSM Fatura', status: 'realized', reasonCode: 'Reason Code', reasonText: 'Satış Tamamlandı' },
                { product: 'Nakit KMH', status: 'unrealized', reasonCode: 'Reason Code', reasonText: 'Süre Açıldı' }
            ]
        },
        {
            date: '03.04.2026', accountNo: '26052753', customerName: 'Serkan Aktaş', kazanim: 'Ürün', customerStatus: 'Yeni',
            summary: 'Ürün Satışı Gerçekleşmedi / Süre Açıldı',
            details: [
                { product: 'GSM Fatura', status: 'unrealized', reasonCode: 'Reason Code', reasonText: 'Süre Açıldı' }
            ]
        },
        {
            date: '04.04.2026', accountNo: '26052754', customerName: 'Ece Polat', kazanim: 'Ürün', customerStatus: 'Yeni',
            summary: 'Ürün Satışı Gerçekleşmedi / Süre Açıldı',
            details: [
                { product: 'Deniz Bonus', status: 'realized', reasonCode: 'Reason Code', reasonText: 'Satış Tamamlandı' },
                { product: 'Nakit KMH', status: 'unrealized', reasonCode: 'Reason Code', reasonText: 'Müşteri Talebi Yok' }
            ]
        },
        {
            date: '04.04.2026', accountNo: '26052755', customerName: 'Mert Güneş', kazanim: 'Ürün', customerStatus: 'Yeni',
            summary: 'Ürün Satışı Gerçekleşmedi / Süre Açıldı',
            details: [
                { product: 'GSM Fatura', status: 'unrealized', reasonCode: 'Reason Code', reasonText: 'Süre Açıldı' },
                { product: 'Deniz Bonus', status: 'unrealized', reasonCode: 'Reason Code', reasonText: 'Süre Açıldı' }
            ]
        },
        {
            date: '04.04.2026', accountNo: '26052756', customerName: 'Derya Çetin', kazanim: 'Ürün', customerStatus: 'Yeni',
            summary: 'Ürün Satışı Gerçekleşmedi / Süre Açıldı',
            details: [
                { product: 'Nakit KMH', status: 'unrealized', reasonCode: 'Reason Code', reasonText: 'Süre Açıldı' }
            ]
        }
    ];

    // ----- Hedef Dışı Satışlar -----
    var OFFTARGET_COLUMNS = ['Açılış Tarihi', 'Ürün 1', 'Ürün 2', 'Ürün 3', 'Hesap No', 'Müşteri Adı', 'Kazanım', 'Müşteri Durumu'];
    var OFFTARGET_ROWS = [
        { date: '05.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052771', customerName: 'Burak Yalçın', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '05.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052772', customerName: 'Sena Aydın', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '05.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052773', customerName: 'Tolga Acar', kazanim: 'Ürün', customerStatus: 'Yeni' },
        { date: '05.04.2026', products: ['GSM Fatura', 'Deniz Bonus', 'Nakit KMH'], accountNo: '26052774', customerName: 'Ceren Köse', kazanim: 'Ürün', customerStatus: 'Yeni' }
    ];

    // Sekme/filtre başına ayrı mock response
    var SC_MOCK_RESPONSES = {
        realized:   { columns: REALIZED_COLUMNS,   rows: REALIZED_ROWS },
        pending:    { columns: PENDING_COLUMNS,    rows: PENDING_ROWS },
        unrealized: { columns: UNREALIZED_COLUMNS, rows: UNREALIZED_ROWS, expandable: true },
        offtarget:  { columns: OFFTARGET_COLUMNS,  rows: OFFTARGET_ROWS }
    };

    var _currentPage = 1;
    var _statusFilter = 'realized';
    var _response = SC_MOCK_RESPONSES.realized; // o anki (mock) servis cevabı

    // ===== Mock servis isteği =====
    // Her sekme/filtre için ayrı response döner. İleride setTimeout yerine $.ajax:
    //   $.ajax({ url: '/ScoreCard/GetCardDetail', type: 'POST',
    //     contentType: 'application/json', data: JSON.stringify({ status: status }),
    //     success: callback });
    // Dönen yapı: { columns: [...], rows: [...] }
    function requestScoreCardDetail(status, callback) {
        setTimeout(function () {
            callback(SC_MOCK_RESPONSES[status] || { columns: REALIZED_COLUMNS, rows: [] });
        }, 250);
    }

    function totalPages() {
        return Math.max(1, Math.ceil(_response.rows.length / PAGE_SIZE));
    }

    // Kolonlar servis cevabından gelir
    function getColumns() {
        return _response.columns || REALIZED_COLUMNS;
    }

    function renderHead() {
        // Veri yoksa kolon başlıkları gösterilmez
        if (!_response.rows.length) {
            $('#scTableHead').empty();
            return;
        }
        var html = '<tr>';
        getColumns().forEach(function (c) { html += '<th>' + c + '</th>'; });
        html += '</tr>';
        $('#scTableHead').html(html);
    }

    function renderBody() {
        var rows = _response.rows;
        var hasDays = getColumns().indexOf('Kalan Gün') > -1;
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

        if (_response.expandable) {
            renderExpandableBody(pageRows, start);
            return;
        }

        var html = '';
        var icon = STATUS_ICON[_statusFilter] || '';
        pageRows.forEach(function (r, i) {
            html += '<tr class="' + (i % 2 === 0 ? 'sc-zebra' : '') + '">';
            html += '<td>' + r.date + '</td>';
            // Ürün 1-2-3: durum ikonu + ürün adı
            for (var pi = 0; pi < 3; pi++) {
                var pname = r.products[pi] || '';
                html += '<td><span class="sc-prod">' +
                    (icon ? '<img class="sc-prod-icon" src="' + icon + '" alt="" />' : '') +
                    '<span>' + pname + '</span></span></td>';
            }
            // Kalan Gün rozeti (kolon varsa)
            if (hasDays) {
                html += '<td><span class="sc-days">' + (r.remainingDays != null ? r.remainingDays + ' Gün' : '-') + '</span></td>';
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

    function renderPagination() {
        var pages = totalPages();
        var html = '';
        html += '<button type="button" class="sc-page-arrow" data-page="prev"' + (_currentPage === 1 ? ' disabled' : '') + '>‹</button>';
        html += '<span class="sc-page-info">' + _currentPage + '/' + pages + '</span>';
        html += '<button type="button" class="sc-page-arrow" data-page="next"' + (_currentPage === pages ? ' disabled' : '') + '>›</button>';
        $('#scPagination').html(html);
    }

    function renderTable() {
        renderHead();
        renderBody();
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

    // ===== Trend Analizi (mock response) =====
    // Gerçekte API'den gelecek ham response. Grafik realizedValue'yu (mavi alan)
    // period'a (x ekseni) göre çizer.
    var RAW_TREND = [
        { period: '2022-11', realizedValue: 1900,  pendingValue: 20000, hgRatio: 66.6762 },
        { period: '2022-2',  realizedValue: 14600, pendingValue: 3400,  hgRatio: 0 },
        { period: '2022-3',  realizedValue: 12000, pendingValue: 6700,  hgRatio: 0 },
        { period: '2022-4',  realizedValue: 4000,  pendingValue: 11000, hgRatio: 0 },
        { period: '2022-5',  realizedValue: 23000, pendingValue: 23000, hgRatio: 60.9542 },
        { period: '2022-6',  realizedValue: 5000,  pendingValue: 8700,  hgRatio: 39.3332 },
        { period: '2022-7',  realizedValue: 10000, pendingValue: 10400, hgRatio: 33.3381 },
        { period: '2023-1',  realizedValue: 4500,  pendingValue: 4500,  hgRatio: 50.4363 },
        { period: '2023-2',  realizedValue: 9800,  pendingValue: 14300, hgRatio: 51.592 },
        { period: '2023-3',  realizedValue: 8700,  pendingValue: 0,     hgRatio: 55.423 },
        { period: '2023-5',  realizedValue: 13000, pendingValue: 0,     hgRatio: 57.2878 },
        { period: '2023-6',  realizedValue: 21000, pendingValue: 0,     hgRatio: 66.6167 },
        { period: '2023-7',  realizedValue: 6500,  pendingValue: 0,     hgRatio: 65.4677 },
        { period: '2023-8',  realizedValue: 3450,  pendingValue: 0,     hgRatio: 60.2166 },
        { period: '2023-9',  realizedValue: 16720, pendingValue: 0,     hgRatio: 68.3704 },
        { period: '2023-10', realizedValue: 3000,  pendingValue: 0,     hgRatio: 58.7095 },
        { period: '2023-11', realizedValue: 5000,  pendingValue: 0,     hgRatio: 66.5489 },
        { period: '2023-12', realizedValue: 1800,  pendingValue: 0,     hgRatio: 104.4513 },
        { period: '2024-1',  realizedValue: 18000, pendingValue: 0,     hgRatio: 44.5902 }
    ];

    // "2022-11" -> kronolojik sıralama anahtarı
    function periodKey(period) {
        var p = period.split('-');
        return parseInt(p[0], 10) * 12 + (parseInt(p[1], 10) - 1);
    }

    // Ham veriyi kronolojik sıralayıp son `count` ayı seç (count yoksa tümü)
    function buildTrend(count) {
        var sorted = RAW_TREND.slice().sort(function (a, b) {
            return periodKey(a.period) - periodKey(b.period);
        });
        var sliced = count ? sorted.slice(-count) : sorted;
        return {
            labels: sliced.map(function (d) { return d.period; }),
            values: sliced.map(function (d) { return d.hgRatio; }),
            points: sliced
        };
    }

    // Periyot sekmeleri -> kaç aylık dilim gösterilecek
    var TREND_DATA = {
        'bu-ay': buildTrend(1),
        'son-3-ay': buildTrend(3),
        'son-6-ay': buildTrend(6),
        'son-12-ay': buildTrend(12)
    };

    function formatDecimal(v) {
        return new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(v);
    }

    function renderTrendChart() {
        var period = $('#scTrendTabs .sc-trend-tab.active').data('trend-period') || 'bu-ay';
        var data = TREND_DATA[period];
        if (!data) return;

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
            renderTrendChart();
        } else {
            $('#scTrend').hide();
            $('#scTargetDetail').show();
            loadDetail();
        }
    }

    // ===== Modal aç/kapat =====
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
        var name = $(this).data('name');
        if (name) $('#scModalTitle').text(name);
        openModal();
    });
    $('#scClose').on('click', closeModal);

    // Overlay boşluğuna tıklayınca kapat
    $('#scOverlay').on('click', function (e) {
        if (e.target === this) closeModal();
    });

    // ESC ile kapat
    $(document).on('keydown', function (e) {
        if (e.key === 'Escape') closeModal();
    });

    // ===== Sekmeler =====
    $('#scTabs').on('click', '.sc-tab', function () {
        $('#scTabs .sc-tab').removeClass('active');
        $(this).addClass('active');
        showTab($(this).data('tab'));
    });

    // ===== Trend periyot sekmeleri =====
    $('#scTrendTabs').on('click', '.sc-trend-tab', function () {
        $('#scTrendTabs .sc-trend-tab').removeClass('active');
        $(this).addClass('active');
        renderTrendChart();
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

    // ===== Filtre çipleri =====
    $('#scFilters').on('click', '.sc-filter', function () {
        $('#scFilters .sc-filter').removeClass('active');
        $(this).addClass('active');
        updateFilterIcons();
        _statusFilter = $(this).data('filter');
        // Her filtrede ayrı servis isteği (mock) at
        loadDetail();
    });

    updateFilterIcons();

    // ===== Sayfalama =====
    $('#scPagination').on('click', '.sc-page-arrow', function () {
        var page = $(this).data('page');
        if (page === 'prev') {
            if (_currentPage > 1) _currentPage--;
        } else if (page === 'next') {
            if (_currentPage < totalPages()) _currentPage++;
        }
        renderBody();
        renderPagination();
    });

    // ===== Satır expand/collapse (Gerçekleşmeyen) =====
    $('#scTableBody').on('click', '.sc-expand-icon', function () {
        var rid = $(this).data('row');
        var $detail = $('#scTableBody .sc-detail-row[data-row="' + rid + '"]');
        var willOpen = !$detail.is(':visible');
        $detail.toggle(willOpen);
        $(this).toggleClass('open', willOpen);
        $('#scTableBody .sc-main-row[data-row="' + rid + '"]').toggleClass('expanded', willOpen);
    });
});
