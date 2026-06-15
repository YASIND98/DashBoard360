// ===== NPL ve Gecikmeli Krediler Raporu (rapor tablosu - mock) =====
$(function () {
    if (!document.getElementById('nplReportBody')) return;

    // Aylık gruplar
    var MONTHS = ['Aralık 2025', 'Ocak 2026', 'Şubat 2026'];
    var SUB_COLUMNS = ['Değer', 'Gelişim Oranı', 'Bölge Gelişimi', 'Banka Gelişimi'];

    // ===== Mock servis cevapları (sekme bazında ayrı response) =====
    // İleride: $.ajax(...).success -> NPL_RESPONSES[tab]; alanlar aynı isimde gelir.
    // cells[ayIndex] = [deger, gelisimOrani, bolgeGelisimi, bankaGelisimi]
    function R(metric, c0, c1, c2) {
        return { metric: metric, cells: [c0, c1, c2] };
    }
    var NPL_RESPONSES = {
        tumu: [
            R('NPL %',           [999999999999, 5.0, 0, 0],     [999999999999, 5.0, -100, 0],   [999999999999, 5.9, 0, 0]),
            R('Kredi/Mevduat %', [999999999999, 0, 0, 0],       [999999999999, 5.9, 100, 100],  [999999999999, 5.9, 0, 0]),
            R('Gecikmeli Kredi %',[999999999999, 5.0, 0, 0],    [999999999999, -5.4, 100, 0],   [999999999999, 5.9, 0, 0]),
            R('NPL Inflow',      [999999999999, 5.0, 0, 0],     [999999999999, 5.5, 0, 0],      [999999999999, 5.9, 0, 0])
        ],
        kurumsal: [
            R('NPL %',           [482551200000, 3.2, 12, -4],   [488120450000, 1.1, 8, -2],     [501230990000, 2.7, 5, 1]),
            R('Kredi/Mevduat %', [128400000000, -1.4, -6, 3],   [131200000000, 2.2, 4, 6],      [134900000000, 2.8, 9, 4]),
            R('Gecikmeli Kredi %',[39880000000, 4.6, 18, 7],    [41200000000, 3.3, 12, 5],      [43050000000, 4.5, 14, 8]),
            R('NPL Inflow',      [9120000000, 6.1, 22, 11],     [8740000000, -4.2, -9, -6],     [9330000000, 6.7, 17, 9])
        ],
        ticari: [
            R('NPL %',           [312770000000, 2.1, 6, -3],    [318440000000, 1.8, 4, 0],      [325110000000, 2.1, 7, 2]),
            R('Kredi/Mevduat %', [98200000000, 0.9, 2, 1],      [99800000000, 1.6, 5, 3],       [102400000000, 2.5, 8, 5]),
            R('Gecikmeli Kredi %',[27640000000, 5.3, 15, 9],    [28330000000, 2.5, 10, 4],      [29870000000, 5.4, 13, 7]),
            R('NPL Inflow',      [6210000000, 4.8, 14, 6],      [5980000000, -3.7, -8, -5],     [6440000000, 7.7, 19, 10])
        ],
        kobi: [
            R('NPL %',           [221360000000, 4.4, 9, 2],     [226900000000, 2.4, 6, 1],      [233550000000, 2.9, 8, 3]),
            R('Kredi/Mevduat %', [74100000000, 1.2, 3, 2],      [75600000000, 2.0, 6, 4],       [77800000000, 2.9, 10, 6]),
            R('Gecikmeli Kredi %',[19980000000, 6.7, 20, 12],   [20760000000, 3.9, 13, 7],      [21940000000, 5.7, 16, 9]),
            R('NPL Inflow',      [4880000000, 8.2, 25, 14],     [4630000000, -5.1, -11, -7],    [5010000000, 8.2, 21, 12])
        ],
        tarim: [
            R('NPL %',           [88450000000, 1.6, 4, -1],     [90120000000, 1.9, 5, 1],       [92340000000, 2.5, 6, 2]),
            R('Kredi/Mevduat %', [31200000000, 0.6, 1, 0],      [31900000000, 2.2, 7, 3],       [32800000000, 2.8, 9, 5]),
            R('Gecikmeli Kredi %',[8870000000, 3.9, 11, 5],     [9120000000, 2.1, 8, 3],        [9550000000, 4.7, 12, 6]),
            R('NPL Inflow',      [2010000000, 5.6, 16, 8],      [1930000000, -2.9, -7, -4],     [2080000000, 7.8, 18, 9])
        ],
        bireysel: [
            R('NPL %',           [654980000000, 3.8, 10, 1],    [668210000000, 2.0, 7, 2],      [684550000000, 2.4, 9, 3]),
            R('Kredi/Mevduat %', [198400000000, 1.1, 3, 1],     [202300000000, 1.8, 5, 4],      [207900000000, 2.7, 8, 5]),
            R('Gecikmeli Kredi %',[52310000000, 5.9, 17, 10],   [53880000000, 3.4, 12, 6],      [56120000000, 5.2, 15, 8]),
            R('NPL Inflow',      [12480000000, 7.4, 23, 13],    [11890000000, -4.6, -10, -6],   [12760000000, 8.1, 20, 11])
        ]
    };

    // ===== Biçimlendiriciler =====
    function fmtNumber(v) {
        return new Intl.NumberFormat('tr-TR').format(v);
    }
    function fmtPercent(v) {
        return '%' + new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 2 }).format(v);
    }

    function renderHead() {
        var h1 = '<tr>';
        h1 += '<th class="col-text valign-bottom" rowspan="2"></th>';
        MONTHS.forEach(function (m) {
            h1 += '<th colspan="4" class="col-group-header">' +
                    '<div class="col-group-header-content"><span>' + m + '</span></div>' +
                  '</th>';
        });
        h1 += '</tr>';

        var h2 = '<tr>';
        MONTHS.forEach(function () {
            SUB_COLUMNS.forEach(function (c, ci) {
                // Her ay grubunun ilk kolonu, dikey ayraç çizgisi için işaretlenir
                var cls = ci === 0 ? ' class="col-group-start"' : '';
                // Çok kelimeli başlıklar tasarımdaki gibi ikinci satıra düşsün
                h2 += '<th' + cls + '>' + c.replace(' ', '<br>') + '</th>';
            });
        });
        h2 += '</tr>';

        $('#nplReportHead').html(h1 + h2);
    }

    function renderBody() {
        var tab = $('#nplTabList .tab.active').data('npltab') || 'tumu';
        var query = ($('#nplSearchInput').val() || '').trim().toLowerCase();
        var data = (NPL_RESPONSES[tab] || NPL_RESPONSES.tumu).filter(function (r) {
            return !query || (r.metric || '').toLowerCase().indexOf(query) > -1;
        });

        var colCount = 1 + MONTHS.length * SUB_COLUMNS.length;

        if (!data.length) {
            $('#nplReportHead').empty();
            $('#nplReportBody').html(
                '<tr class="no-result-row"><td colspan="' + colCount + '" style="text-align:center;padding:48px 16px;">' +
                    '<div class="table-empty-state">' +
                        '<img src="/images/empty-state-seach.svg" alt="" />' +
                        '<span>Seçili döneme ait veri bulunmamaktadır.</span>' +
                    '</div>' +
                '</td></tr>'
            );
            return;
        }

        renderHead();

        var html = '';
        data.forEach(function (r) {
            html += '<tr class="table-row">';
            html += '<td class="col-text">' + r.metric + '</td>';
            MONTHS.forEach(function (m, mi) {
                var cell = r.cells[mi] || [0, 0, 0, 0];
                // Değer ve Bölge/Banka Gelişimi düz sayı; sadece Gelişim Oranı % ile gösterilir
                var vals = [fmtNumber(cell[0]), fmtPercent(cell[1]), fmtNumber(cell[2]), fmtNumber(cell[3])];
                vals.forEach(function (v, vi) {
                    // Her ay grubunun ilk hücresi dikey ayraç çizgisini taşır
                    var cls = vi === 0 ? ' class="col-group-start"' : '';
                    html += '<td' + cls + '>' + v + '</td>';
                });
            });
            html += '</tr>';
        });
        var $body = $('#nplReportBody').html(html);
        if (typeof reStripeTable === 'function') reStripeTable($body);
    }

    // ===== Breadcrumb (Tüm Bölgeler / Bölge / Şube) =====
    function updateBreadcrumb() {
        var region = $('#nplRegionLabel').text();
        var branch = $('#nplBranchLabel').text();
        var items = ['<span class="breadcrumb-item' + ((region === 'Bölge' || !region) ? ' active' : '') + '">Tüm Bölgeler</span>'];
        if (region && region !== 'Bölge') {
            items.push('<span class="breadcrumb-separator">/</span>');
            items.push('<span class="breadcrumb-item' + ((branch === 'Şube' || !branch) ? ' active' : '') + '">' + region + '</span>');
        }
        if (branch && branch !== 'Şube') {
            items.push('<span class="breadcrumb-separator">/</span>');
            items.push('<span class="breadcrumb-item active">' + branch + '</span>');
        }
        $('#nplBreadcrumb').html(items.join(''));
    }

    // ===== Sekmeler =====
    $('#nplTabList').on('click', '.tab', function () {
        $('#nplTabList .tab').removeClass('active');
        $(this).addClass('active');
        renderBody();
    });

    // ===== Arama =====
    $('#nplSearchInput').on('input', renderBody);

    // ===== Bölge / Şube filtreleri (paylaşılan veri) =====
    if (typeof loadRegionFilters === 'function') {
        loadRegionFilters(function () { renderRegionList('#nplRegionList', null); updateBreadcrumb(); });
    }
    if (typeof loadBranchFilters === 'function') {
        loadBranchFilters(function () { renderBranchList('#nplBranchList', null, null); });
    }
    $(document).on('click', '#nplRegionList .dropdown-item', function () {
        var code = $(this).data('code') || '';
        $('#nplRegionLabel').text($(this).text());
        $('#nplRegionPanel').removeClass('open');
        // Bölge değişince şube listesini filtrele ve şube seçimini sıfırla
        $('#nplBranchLabel').text('Şube');
        if (typeof renderBranchList === 'function') renderBranchList('#nplBranchList', null, code || null);
        updateBreadcrumb();
        renderBody();
    });
    $(document).on('click', '#nplBranchList .dropdown-item', function () {
        $('#nplBranchLabel').text($(this).text());
        $('#nplBranchPanel').removeClass('open');
        updateBreadcrumb();
        renderBody();
    });

    // ===== İlk render =====
    updateBreadcrumb();
    renderBody();
});
