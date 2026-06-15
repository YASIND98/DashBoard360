// ===== Skor Kart sayfası (rapor tablosu - mock) =====
$(function () {
    if (!document.getElementById('scReportBody')) return;

    var COLUMNS = ['', 'Ürün / Hedef Adı', 'Ürün Tipi', 'Hedef', 'Gerçekleşen', 'H/G %', 'Ağırlık %', 'Ağırlıklı H/G %', 'Bekleyen', 'Detay'];

    // ===== Ürün Tipi sütun filtresi =====
    // value, veri içindeki productType ile eşleşir ('' = Tümü)
    var TYPE_OPTIONS = [
        { label: 'Tümü', value: '' },
        { label: 'Hacim', value: 'HACİM' },
        { label: 'Kart Satış', value: 'KART SATIŞ' },
        { label: 'Müşteri Adedi', value: 'MÜŞTERİ ADEDİ' },
        { label: 'Satış', value: 'SATIŞ' }
    ];
    var selectedType = '';

    // ===== Sıralama (client-side) - "Ürün / Hedef Adı" =====
    var sortKey = null;   // null = sırasız
    var sortAsc = true;

    // ===== Mock servis cevabı (örnek response yapısı) =====
    // İleride: $.ajax(...).success -> SC_RESPONSE; alanlar aynı isimlerde gelir.
    var SC_RESPONSE = {
        mainTableData: [
            { productName: 'Müşteri Kazanımı (Bireysel + Mikro + Emekli Maaş Transferi)', productType: 'SATIŞ', targetValue: 20042, realizedValue: 5198, targetRealizationPercentage: 25.9355, productWeight: 20, weightedPercentage: 5.19, pending: 13984, productInfo: 'Bireysel, mikro ve emekli maaş transferi müşteri kazanımlarını kapsar.' },
            { productName: 'Aktif Kredi Kartı Performansı + Aktif Bonus Business Performansı', productType: 'KART SATIŞ', targetValue: 18976, realizedValue: 5472, targetRealizationPercentage: 28.8364, productWeight: 15, weightedPercentage: 4.33, pending: 7437, productInfo: 'Aktif kredi kartı ve aktif Bonus Business performansını kapsar.' },
            { productName: 'Tüketici Kredileri', productType: 'HACİM', targetValue: 554555778444, realizedValue: 554555778999, targetRealizationPercentage: 101.0023, productWeight: 12, weightedPercentage: 12.12, pending: 0, productInfo: 'İhtiyaç Kredisinde 10.000 TL ve üzeri bakiye kullandırımlarında skorkarta yansır.Konut ve taşıtta sınır bulunmamaktadır.(Taksitli İhtiyaç Kredinsman Kredisi,İhtiyaç Kredisi Yapılandırma,İhtiyaç,Kredisi Bayii,Bayi Kazanımı,İhtiyaç Kredisi Modifikasyon,Konut Kredilerı,Taşıt Kredileri)' },
            { productName: 'Vadeli Mevduat', productType: 'HACİM', targetValue: 2038, realizedValue: 1029, targetRealizationPercentage: 50.4906, productWeight: 10, weightedPercentage: 5.05, pending: 1009 },
            { productName: 'Otomatik Ödeme', productType: 'SATIŞ', targetValue: 8500, realizedValue: 7480, targetRealizationPercentage: 88.0, productWeight: 8, weightedPercentage: 7.04, pending: 1020 },
            { productName: 'Nakit KMH', productType: 'KART SATIŞ', targetValue: 6400, realizedValue: 7808, targetRealizationPercentage: 122.0, productWeight: 10, weightedPercentage: 12.2, pending: 0 },
            { productName: 'Sigorta + BES', productType: 'SATIŞ', targetValue: 9200, realizedValue: 5888, targetRealizationPercentage: 64.0, productWeight: 9, weightedPercentage: 5.76, pending: 3312 },
            { productName: 'Konut Kredisi', productType: 'HACİM', targetValue: 12500, realizedValue: 9125, targetRealizationPercentage: 73.0, productWeight: 6, weightedPercentage: 4.38, pending: 3375 }
        ],
        ladatedDate: null,
        isVisible: 0,
        multiParenthesisFlag: false
    };
    var ROWS = SC_RESPONSE.mainTableData;

    // HTML attribute içine güvenli yerleştirme (tooltip metni)
    function escapeAttr(s) {
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }

    // ===== Biçimlendiriciler =====
    function fmtNumber(v) {
        return new Intl.NumberFormat('tr-TR').format(v);
    }
    function fmtPercent(v) {
        return '%' + new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 2 }).format(v);
    }

    // H/G oranına göre renk sınıfı (site.js'teki percentColor ile aynı eşikler)
    function ratioClass(v) {
        if (v < 75) return 'ratio-red';
        if (v < 100) return 'ratio-orange';
        if (v < 120) return 'ratio-green';
        return 'ratio-blue';
    }

    // "Ürün Tipi" başlığı: tıklanınca açılan özel filtre menüsü (tasarıma özel .sc-type-* stilleri)
    function typeFilterHtml() {
        var items = TYPE_OPTIONS.map(function (o) {
            var sel = (o.value === selectedType) ? ' selected' : '';
            return '<div class="sc-type-option' + sel + '" data-type="' + o.value + '">' + o.label + '</div>';
        }).join('');
        return '' +
            '<div class="sc-type-filter">' +
                '<button type="button" class="sc-type-trigger">' +
                    '<span>Ürün Tipi</span>' +
                    '<img src="/images/down.svg" alt="" />' +
                '</button>' +
                '<div class="sc-type-menu">' + items + '</div>' +
            '</div>';
    }

    // Sort ikonu (mevcut .sort-icon deseni); aktif sütunda asc/desc yansıtılır
    function sortIconHtml(key) {
        var cls = (sortKey === key) ? (sortAsc ? ' asc' : ' desc') : '';
        return ' <i class="sort-icon' + cls + '"><span class="sort-up">▲</span><span class="sort-down">▼</span></i>';
    }

    function renderHead() {
        var html = '<tr>';
        COLUMNS.forEach(function (c) {
            if (c === 'Ürün Tipi') {
                html += '<th class="sc-type-th">' + typeFilterHtml() + '</th>';
            } else if (c === 'Ürün / Hedef Adı') {
                html += '<th class="col-text" data-sort-key="productName">' + c + sortIconHtml('productName') + '</th>';
            } else {
                html += '<th>' + c + '</th>';
            }
        });
        html += '</tr>';
        $('#scReportHead').html(html);
    }

    function renderBody() {
        var query = ($('#scSearchInput').val() || '').trim().toLowerCase();
        var rows = ROWS.filter(function (r) {
            var matchesQuery = !query || (r.productName || '').toLowerCase().indexOf(query) > -1;
            var matchesType = !selectedType || (r.productType || '').toUpperCase() === selectedType;
            return matchesQuery && matchesType;
        });

        // Client-side sıralama (aktifse). filter zaten yeni dizi döndürür.
        if (sortKey) {
            rows.sort(function (a, b) {
                var cmp = String(a[sortKey] || '').localeCompare(String(b[sortKey] || ''), 'tr', { sensitivity: 'base' });
                return sortAsc ? cmp : -cmp;
            });
        }

        // Başlık (ve Ürün Tipi filtresi) sonuç boş olsa da görünür kalsın
        renderHead();

        if (!rows.length) {
            $('#scReportBody').html(
                '<tr class="no-result-row"><td colspan="' + COLUMNS.length + '" style="text-align:center;padding:48px 16px;">' +
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
            // ProductInfo varsa info ikonu + hover tooltip; yoksa hücre boş
            var infoCell = r.productInfo
                ? '<img class="info-icon" tabindex="0" data-tooltip="' + escapeAttr(r.productInfo) + '" src="/images/table-info.svg" alt="Bilgi" />'
                : '';
            html += '<td class="col-info">' + infoCell + '</td>';
            html += '<td class="col-text sc-product-name">' + r.productName + '</td>';
            html += '<td>' + r.productType + '</td>';
            html += '<td>' + fmtNumber(r.targetValue) + '</td>';
            html += '<td>' + fmtNumber(r.realizedValue) + '</td>';
            html += '<td class="' + ratioClass(r.targetRealizationPercentage) + '">' + fmtPercent(r.targetRealizationPercentage) + '</td>';
            html += '<td>' + fmtPercent(r.productWeight) + '</td>';
            html += '<td>' + fmtPercent(r.weightedPercentage) + '</td>';
            html += '<td>' + fmtNumber(r.pending) + '</td>';
            html += '<td><img class="sc-detail-icon" src="/images/detail.svg" alt="Detay" data-name="' + r.productName + '" /></td>';
            html += '</tr>';
        });
        var $body = $('#scReportBody').html(html);
        if (typeof reStripeTable === 'function') reStripeTable($body);
    }

    function renderLegend() {
        if (typeof renderTableLegend === 'function') {
            renderTableLegend('#scReportLegend', {
                note: 'Tabloda yer alan tutarlar /1000 olarak verilmektedir.',
                ratio: true
            });
        }
    }

    // ===== Sekmeler (Genel Bakış / Kurumsal / ...) =====
    $('#scTabList').on('click', '.tab', function () {
        $('#scTabList .tab').removeClass('active');
        $(this).addClass('active');
        renderBody();
    });

    // ===== Kanal (Pazarlama / Operasyon / SDK / Özel Bankacılık) =====
    $('#scChannels').on('click', '.segment', function () {
        $('#scChannels .segment').removeClass('active');
        $(this).addClass('active');
        renderBody();
    });

    // ===== Periyot (Aylık / Çeyreklik / Yıllık) =====
    $('#scPeriod').on('click', '.period-btn', function () {
        $('#scPeriod .period-btn').removeClass('active');
        $(this).addClass('active');
        renderBody();
    });

    // ===== Arama =====
    $('#scSearchInput').on('input', renderBody);

    // ===== Sıralama (başlığa tıkla: asc -> desc -> sırasız) =====
    $(document).on('click', '#scReportHead th[data-sort-key]', function () {
        var key = $(this).data('sort-key');
        if (sortKey === key) {
            if (sortAsc) { sortAsc = false; }
            else { sortKey = null; sortAsc = true; }
        } else {
            sortKey = key;
            sortAsc = true;
        }
        renderBody();
    });

    // ===== Ürün Tipi sütun filtresi (başlıktaki özel menü) =====
    // Aç/kapa
    $(document).on('click', '.sc-type-trigger', function (e) {
        e.stopPropagation();
        $(this).closest('.sc-type-filter').toggleClass('open');
    });
    // Seçim (renderBody başlığı yeniden kurar -> menü kapanır)
    $(document).on('click', '.sc-type-option', function () {
        selectedType = $(this).attr('data-type') || '';
        renderBody();
    });
    // Dışarı tıklayınca kapat
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.sc-type-filter').length) {
            $('.sc-type-filter').removeClass('open');
        }
    });

    // ===== Bölge / Şube filtreleri (paylaşılan veri) =====
    if (typeof loadRegionFilters === 'function') {
        loadRegionFilters(function () { renderRegionList('#scRegionList', null); });
    }
    if (typeof loadBranchFilters === 'function') {
        loadBranchFilters(function () { renderBranchList('#scBranchList', null, null); });
    }
    $(document).on('click', '#scRegionList .dropdown-item', function () {
        $('#scRegionLabel').text($(this).text());
        $('#scRegionPanel').removeClass('open');
    });
    $(document).on('click', '#scBranchList .dropdown-item', function () {
        $('#scBranchLabel').text($(this).text());
        $('#scBranchPanel').removeClass('open');
    });

    // ===== İlk render =====
    renderLegend();
    renderBody();
});
