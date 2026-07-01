// Skor Kart - Detay modalı hedef detayı içerik tablosu
$(function () {

    if (!document.getElementById('scReportBody')) return;

    function scCtx() { return (window.ScoreCard && window.ScoreCard.getContext) ? window.ScoreCard.getContext() : {}; }

    window.ScoreCardDetail = window.ScoreCardDetail || {};
    window.ScoreCardDetail.ctx = {};

    var FILTER_ICON = {
        realized:   { base: '/images/realized.svg',   active: '/images/realized-selected.svg' },
        pending:    { base: '/images/pending.svg',    active: '/images/pending-selected.svg' },
        unrealized: { base: '/images/unrealized.svg', active: '/images/unrealized-selected.svg' }
    };

    var PAGE_SIZE = 13;

    var _currentPage = 1;
    var _statusFilter = 'realized';
    var _response = { columns: [], rows: [] }; // o anki (mock) servis cevabı

    // HTML metnini güvenli hale getir (hücre içeriği)
    function escapeHtml(s) {
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }

    // Servis (ScoreCardDetail) cevabını dinamik kolonlu tabloya çevirir.
    // Yalnızca SCORE_CARD_DETAIL_COLUMN_LABELS'te karşılığı (label'ı) olan key'ler gösterilir
    function buildDynamicResponse(scoreCardDetail) {
        var rows = [];
        try {
            rows = JSON.parse(scoreCardDetail) || [];
        } catch (e) {
            rows = [];
        }
        var labels = (typeof SCORE_CARD_DETAIL_COLUMN_LABELS !== 'undefined')
            ? SCORE_CARD_DETAIL_COLUMN_LABELS : [];
        var labelByKey = {};
        labels.forEach(function (c) { labelByKey[c.key] = c.label; });
        var sample = rows.length ? rows[0] : {};
        var columns = Object.keys(sample)
            .filter(function (key) { return labelByKey[key] != null; })
            .map(function (key) { return { key: key, label: labelByKey[key] }; });
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
            // Servis cevabı casing'i tutarsız olabilir (realized/pending: ScoreCardDetail, unrealized: scoreCardDetail).
            var detail = raw && (raw.ScoreCardDetail != null ? raw.ScoreCardDetail : raw.scoreCardDetail);
            if (detail != null) {
                callback(buildDynamicResponse(detail));
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
                productId: ScoreCardDetail.ctx.productId,
                status: statusCode,
                productType: ScoreCardDetail.ctx.productType
            })
        }).done(function (res) {
            handle(res);
        }).fail(function () {
            handle(null);
        });
    }

    function totalPages() {
        return Math.max(1, Math.ceil(_response.rows.length / PAGE_SIZE));
    }

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

    // Sekme görünümünü değiştir:
    function showTab(tab) {
        if (tab === 'trend') {
            $('#scTargetDetail').hide();
            $('#scTrend').show();
            if (ScoreCardDetail.loadTrend) ScoreCardDetail.loadTrend();
        } else {
            $('#scTrend').hide();
            $('#scTargetDetail').show();
            loadDetail();
        }
    }

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
    $(document).on('click', '.sc-detail-icon', function () {
        var $i = $(this);
        var name = $i.data('name');
        if (name) $('#scModalTitle').text(name);
        ScoreCardDetail.ctx = {
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
        loadDetail();
    });

    updateFilterIcons();

    // "PDF İndir" (Hedef Detayı): ekrandaki detay tablosunu PDF olarak indir.
    // Genel PDF motoru (download-pdf.js) bu config'i data-pdf="scDetail" üzerinden çeker.
    // PDF her zaman ekranda görüneni (aynı _response) indirir.
    var SC_DETAIL_STATUS_LABEL = { realized: 'Gerçekleşen', pending: 'Bekleyen', unrealized: 'Gerçekleşmeyen' };
    window.PdfSources = window.PdfSources || {};
    window.PdfSources.scDetail = function () {
        var cols = (_response.columns || []).map(function (c, i) {
            var key = (c && c.key != null) ? c.key : c;
            var label = (c && c.label != null) ? c.label : String(c);
            return {
                header: label,
                key: key,
                align: i === 0 ? 'left' : 'center',
                format: function (v) { return formatDetailCell(key, v); }
            };
        });
        return {
            title: $('#scModalTitle').text().trim(),
            infoLines: ['Hedef Detayı', 'Durum: ' + (SC_DETAIL_STATUS_LABEL[_statusFilter] || '-')],
            columns: cols,
            rows: _response.rows || [],
            filename: 'SkorKart-Hedef-Detayi.pdf'
        };
    };

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
});
