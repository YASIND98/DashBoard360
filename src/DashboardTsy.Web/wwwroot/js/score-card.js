// Skor Kart: ana rapor tablosu + detay modalı (tek dosya)
$(function () {

    if (!document.getElementById('scReportBody')) return;

    var COLUMNS = SCORE_CARD_REPORT_COLUMNS;

    // Ürün Tipi sütun filtresi
    var selectedType = '';

    // Sıralama (client-side)
    var sortKey = null;
    var sortAsc = true;

    // Rapor tablosu verisi (mock.js -> window.MOCK.scoreCardReport)
    var SC_RESPONSE = (typeof getScoreCardReportMock === 'function')
        ? getScoreCardReportMock()
        : { mainTableData: [] };
    var ROWS = SC_RESPONSE.mainTableData;

    // Seçili filtre durumu
    let _regionCode;
    let _branchCode;
    let _registerId;
    let _userRoleCode;
    let _dateNumber;
    let _scoreCardId;
    var _tabModel = [];

    // Kullanıcı yetki/bağlam servisi (users/authorities). userCode/applicationCode sabittir.
    // Gerçek servise POST atılır; hata (örn. 500) olursa mock cevaba düşülür.
    function fetchUserAuthorities(callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/users/authorities',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ userCode: PUPA_USER_CODE, applicationCode: PUPA_APPLICATION_CODE })
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(typeof getUserAuthoritiesMock === 'function' ? getUserAuthoritiesMock() : null);
        });
    }

    // Servis cevabını sakla; kullanıcı bağlamını (sicil/bölge/şube/rol) istek değişkenlerine al.
    // cumulatives'e regionCode/branchCode -1 yerine kullanıcının bölge/şubesi gider; roleCode = userRoleCode.
    function applyUserAuthorities(auth) {
        if (!auth) return;
        let _userInfo = auth.userInfo;
        function num(v) { var n = parseInt(v, 10); return isNaN(n) ? -1 : n; }
        if (_userInfo) {
            _registerId = num(_userInfo.registerId);
            _regionCode = num(_userInfo.regionCode);
            _branchCode = num(_userInfo.branchCode);
            _userRoleCode = num(_userInfo.userRoleCode);
        }
    }

    // Gerçek servise istek atılır; hata olursa mock cevaba düşülür.
    function fetchPrimMonitoringPeriods(periodType, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/prim-monitoring/periods',
            type: 'GET',
            data: { periodTypes: periodType }
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(getPrimMonitoringPeriodsMock(periodType));
        });
    }

    function fetchPupaTypes(dateNumber, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/sales-target-monitoring/pupa-types',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ dateNumber: dateNumber, roleCode: _userRoleCode })
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(getPupaTypesMock());
        });
    }

    // Pupa tipi (kanal): Key -> statik etiket (PUPA_TYPE_LABELS)
    function renderPupaChannels(pupaRes) {
        var kv = (pupaRes && pupaRes.KeyValues) || [];
        if (!kv.length) return;
        var html = '';
        kv.forEach(function (item, i) {
            var key = item.Key;
            var label = PUPA_TYPE_LABELS[key];
            if (i > 0) html += '<div class="divider"></div>';
            html += '<button type="button" class="segment' + (i === 0 ? ' active' : '') +
                    '" data-channel="' + key + '">' + label + '</button>';
        });
        $('#scChannels').html(html);
    }

    function activePupaType() {
        var p = parseInt($('#scChannels .segment.active').data('channel'), 10);
        return isNaN(p) ? 1 : p;
    }

    function loadPupaFilters(period) {
        var periodType = PUPA_PERIOD_TYPE[period] || PUPA_PERIOD_TYPE.aylik;
        fetchPrimMonitoringPeriods(periodType, function (periodsRes) {
            var kv = (periodsRes && periodsRes.keyValues) || [];
            _dateNumber = kv.length ? kv[0].key : -1;
            fetchPupaTypes(_dateNumber, function (pupaRes) {
                renderPupaChannels(pupaRes);
                loadScoreCardTypes();
            });
        });
    }

    // Skor kart Tipleri
    function fetchScoreCards(dateNumber, pupaType, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/dashboard/score-cards',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ dateNumber: dateNumber, pupaType: pupaType, roleCode: PUPA_SCORECARDS_ROLE_CODE })
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(getScoreCardsMock());
        });
    }

    // key'in bağlı olduğu grup (SCORE_CARD_GROUPS); yoksa null
    function findScoreCardGroup(key) {
        var groups = (typeof SCORE_CARD_GROUPS !== 'undefined') ? SCORE_CARD_GROUPS : [];
        return groups.filter(function (g) { return g.keys.indexOf(key) > -1; })[0] || null;
    }

    // Servis Key'lerinden sekme modeli: gruba ait key'ler tek üst sekmede (sub-tab) toplanır
    function buildScoreCardTabModel(kv) {
        var model = [], byGroup = {};
        kv.forEach(function (item) {
            var key = item.Key;
            var label = SCORE_CARD_LABELS[key];
            var group = findScoreCardGroup(key);
            if (!group) return void model.push({ type: 'single', key: key, label: label });
            if (byGroup[group.label] == null) {
                byGroup[group.label] = model.push({ type: 'group', label: group.label, subs: [] }) - 1;
            }
            model[byGroup[group.label]].subs.push({ key: key, label: label });
        });
        return model;
    }

    // Alt sekmeleri çiz (ilk sub aktif)
    function renderSubTabs(subs) {
        $('#scSubTabList').html(subs.map(function (s, i) {
            return '<button class="sub-tab' + (i ? '' : ' active') +
                   '" data-scorecard="' + s.key + '">' + s.label + '</button>';
        }).join(''));
        $('#scSubTabBar').show();
    }

    // Üst sekmeyi aktifle: grup ise sub-tab'ları aç, değilse gizle; scoreCardId'yi seç
    function activateMainTab($tab, reload) {
        $('#scTabList .tab').removeClass('active');
        $tab.addClass('active');
        var t = _tabModel[$tab.data('tabindex')];
        if (t && t.type === 'group') {
            renderSubTabs(t.subs);
            _scoreCardId = t.subs.length ? t.subs[0].key : -1; // ilk sub-tab
        } else {
            $('#scSubTabBar').hide();
            _scoreCardId = t ? t.key : -1;
        }
        if (reload) loadScoreCardTable();
    }

    function renderScoreCardTabs(scRes) {
        var kv = (scRes && scRes.KeyValues) || [];
        _tabModel = buildScoreCardTabModel(kv);
        $('#scTabList').html(_tabModel.map(function (t, i) {
            var attr = (t.type === 'single') ? ' data-scorecard="' + t.key + '"' : '';
            return '<button class="tab" data-tabindex="' + i + '"' + attr + '>' + t.label + '</button>';
        }).join(''));
        if (_tabModel.length) {
            activateMainTab($('#scTabList .tab').first(), false); // reload loadScoreCardTypes'te
        } else {
            $('#scSubTabBar').hide();
            _scoreCardId = -1;
        }
    }

    // Pupa tipi / period tipi değişince: score-cards iste -> sekmeleri çiz -> tabloyu doldur
    function loadScoreCardTypes() {
        fetchScoreCards(_dateNumber, activePupaType(), function (scRes) {
            renderScoreCardTabs(scRes);
            loadScoreCardTable();
        });
    }

    // scorecards/cumulatives: ana rapor tablosunu doldurur
    // İstek gövdesi ekrandaki seçimlerden kurulur; hata olursa mock rapora düşülür.
    // session _reportDate (site.js, ISO) -> { year, month }
    function reportDateParts() {
        var d = (typeof _reportDate !== 'undefined' && _reportDate) ? new Date(_reportDate) : new Date();
        if (isNaN(d.getTime())) d = new Date();
        return { year: d.getFullYear(), month: d.getMonth() + 1 };
    }

    function buildCumulativesRequest() {
        var rd = reportDateParts();
        var period = $('#scPeriod .period-btn.active').data('period') || 'aylik';
        return {
            year: rd.year,                                  // session _reportDate yılı
            month: rd.month,                                // session _reportDate ayı (4, 5 ...)
            quarter: (period === 'ceyreklik') ? 1 : -1,     // çeyreklik seçiliyse 1, değilse -1
            cumulativeFlag: (period === 'yillik') ? 1 : 0,  // yıllık seçiliyse 1, değilse 0
            registerId: _registerId,                        // users/authorities -> kullanıcı sicili (yoksa -1)
            regionCode: _regionCode,                        // seçili bölge; varsayılan kullanıcının bölgesi (users/authorities)
            branchCode: _branchCode,                        // seçili şube; varsayılan kullanıcının şubesi (users/authorities)
            pupaType: activePupaType(),                     // pupa-types seçimi (aktif kanal)
            scoreCardId: _scoreCardId,                      // seçili skor kart (aktif sekme)
            scoreCardTypeId: -1
        };
    }

    function fetchScoreCardCumulatives(body, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecards/cumulatives',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(body)
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(getScoreCardReportMock());
        });
    }

    // Tabloyu cumulatives servisinden (mock fallback) doldur ve yeniden çiz
    function loadScoreCardTable() {
        fetchScoreCardCumulatives(buildCumulativesRequest(), function (res) {
            ROWS = (res && res.mainTableData) ? res.mainTableData : [];
            renderReportBody();
        });
    }

    // HTML attribute içine güvenli yerleştirme (tooltip metni)
    function escapeAttr(s) {
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }
    // "Ürün Tipi" filtre seçenekleri: tablo verisindeki unique productType'lar
    function getTypeOptions() {
        var seen = {};
        var opts = [{ label: 'Tümü', value: '' }];
        ROWS.forEach(function (r) {
            if (r.productType && !seen[r.productType]) {
                seen[r.productType] = true;
                opts.push({ label: r.productType, value: r.productType });
            }
        });
        return opts;
    }

    // "Ürün Tipi" başlığı: tıklanınca açılan  filtre menüsü
    function typeFilterHtml() {
        var items = getTypeOptions().map(function (o) {
            var sel = (o.value === selectedType) ? ' selected' : '';
            return '<div class="sc-type-option' + sel + '" data-type="' + o.value + '">' + o.label + '</div>';
        }).join('');
        // Tip seçiliyse (Tümü hariç) başlıkta "Ürün Tipi (<seçim>)" göster
        var triggerText = selectedType ? ('Ürün Tipi (' + selectedType + ')') : 'Ürün Tipi';
        return '' +
            '<div class="sc-type-filter">' +
                '<button type="button" class="sc-type-trigger">' +
                    '<span class="sc-type-trigger-text">' + triggerText + '</span>' +
                    '<img src="/images/sort-dec.svg" alt="" />' +
                '</button>' +
                '<div class="sc-type-menu">' + items + '</div>' +
            '</div>';
    }

    // Sort ikonu (mevcut .sort-icon deseni); aktif sütunda asc/desc yansıtılır
    function sortIconHtml(key) {
        var cls = (sortKey === key) ? (sortAsc ? ' asc' : ' desc') : '';
        return ' <i class="sort-icon' + cls + '"><img class="sort-up" src="/images/sort-asc.svg" alt="" /><img class="sort-down" src="/images/sort-dec.svg" alt="" /></i>';
    }

    function renderReportHead() {
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

    function renderReportBody() {
        var query = ($('#scSearchInput').val() || '').trim().toLowerCase();
        var rows = ROWS.filter(function (r) {
            var matchesQuery = !query || (r.productName || '').toLowerCase().indexOf(query) > -1;
            var matchesType = !selectedType || (r.productType || '') === selectedType;
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
        renderReportHead();

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
            html += '<td>' + formatNumber(r.targetValue) + '</td>';
            html += '<td>' + formatNumber(r.realizedValue) + '</td>';
            html += '<td class="' + percentColor(r.targetRealizationPercentage) + '">' + formatPercent(r.targetRealizationPercentage) + '</td>';
            html += '<td>' + formatPercent(r.productWeight) + '</td>';
            html += '<td>' + formatPercent(r.weightedPercentage) + '</td>';
            html += '<td>' + formatNumber(r.pending) + '</td>';
            // Detay drill-down bağlamı (scorecards/details): productId satırdan, productType aktif kanaldan,
            // dateNumber seçili periyottan. Gerçek cumulatives cevabında alanlar farklıysa burası güncellenir.
            html += '<td><img class="sc-detail-icon" src="/images/detail.svg" alt="Detay"' +
                ' data-name="' + r.productName + '"' +
                ' data-product-id="' + (r.productId != null ? r.productId : -1) + '"' +
                ' data-product-type="' + activePupaType() + '"' +
                ' data-date-number="' + _dateNumber + '" /></td>';
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

    // Skor kart tipi sekmeleri
    // Üst sekme: grup ise sub-tab'ları açar (ilk sub aktif), tekil ise skor kartı seçer
    $('#scTabList').on('click', '.tab', function () {
        activateMainTab($(this), true);
    });

    // Alt sekme (ör. Bireysel -> SY / BD): scoreCardId aynı parametre, tabloyu yenile
    $('#scSubTabList').on('click', '.sub-tab', function () {
        $('#scSubTabList .sub-tab').removeClass('active');
        $(this).addClass('active');
        _scoreCardId = parseInt($(this).data('scorecard'), 10);
        if (isNaN(_scoreCardId)) _scoreCardId = -1;
        loadScoreCardTable();
    });

    // Pupa tipi (kanal) seçimi
    $('#scChannels').on('click', '.segment', function () {
        $('#scChannels .segment').removeClass('active');
        $(this).addClass('active');
        // pupaType değişti -> skor kart tipleri (sekmeler) + tablo yenilensin
        loadScoreCardTypes();
    });

    // Period tipi (Aylık / Çeyreklik / Yıllık)
    $('#scPeriod').on('click', '.period-btn', function () {
        $('#scPeriod .period-btn').removeClass('active');
        $(this).addClass('active');
        // Her periyot değişiminde periods + pupa-types + cumulatives zinciri tetiklenir
        loadPupaFilters($(this).data('period'));
    });

    // Arama
    $('#scSearchInput').on('input', renderReportBody);

    // Sıralama (başlığa tıkla: asc -> desc -> sırasız)
    $(document).on('click', '#scReportHead th[data-sort-key]', function () {
        var key = $(this).data('sort-key');
        if (sortKey === key) {
            if (sortAsc) { sortAsc = false; }
            else { sortKey = null; sortAsc = true; }
        } else {
            sortKey = key;
            sortAsc = true;
        }
        renderReportBody();
    });

    // Ürün Tipi sütun filtresi menüsü
    $(document).on('click', '.sc-type-trigger', function (e) {
        e.stopPropagation();
        $(this).closest('.sc-type-filter').toggleClass('open');
    });
    // Seçim (renderReportBody başlığı yeniden kurar -> menü kapanır)
    $(document).on('click', '.sc-type-option', function () {
        selectedType = $(this).attr('data-type') || '';
        renderReportBody();
    });
    // Dışarı tıklayınca kapat
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.sc-type-filter').length) {
            $('.sc-type-filter').removeClass('open');
        }
    });

    // Bölge / Şube filtreleri
    if (typeof loadRegionFilters === 'function') {
        loadRegionFilters(function () { renderRegionList('#scRegionList', null); });
    }
    if (typeof loadBranchFilters === 'function') {
        loadBranchFilters(function () { renderBranchList('#scBranchList', null, null); });
    }
    $(document).on('click', '#scRegionList .dropdown-item', function () {
        $('#scRegionLabel').text($(this).text());
        $('#scRegionPanel').removeClass('open');
        _regionCode = ($(this).data('code') != null) ? $(this).data('code') : -1;
        loadScoreCardTable();
    });
    $(document).on('click', '#scBranchList .dropdown-item', function () {
        $('#scBranchLabel').text($(this).text());
        $('#scBranchPanel').removeClass('open');
        _branchCode = ($(this).data('code') != null) ? $(this).data('code') : -1;
        loadScoreCardTable();
    });

    // İlk render: önce kullanıcı yetki/bağlamı (users/authorities) çekilir, sonra filtre zinciri kurulur
    fetchUserAuthorities(function (auth) {
        applyUserAuthorities(auth);
        loadPupaFilters($('#scPeriod .period-btn.active').data('period') || 'aylik');
    });
    renderLegend();
    renderReportBody();

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

    // Hedef Detayı (scorecards/details) bağlamı — tıklanan ürün satırından (.sc-detail-icon data-*)
    // doldurulur; modal açıldığında bu değerlerle istek atılır.
    var _detailCtx = { dateNumber: -1, registerId: -1, productId: -1, productType: -1 };

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

    // Hedef Detayı servisi (scorecards/details)
    // Gerçekleşen / Bekleyen: servis -> { ScoreCardDetail: "<json string>" } -> dinamik kolonlu tablo.
    // Gerçekleşmeyen / Hedef Dışı: eski tablo -> { columns:[...], rows:[...], expandable? } olduğu gibi kullanılır.
    // status (filtre anahtarı) SCORE_CARD_DETAIL_STATUS ile sayısal koda çevrilir
    //   (Gerçekleşen 1, Bekleyen 0, Gerçekleşmeyen -1, Hedef Dışı Satış 2).
    // dateNumber/productId/productType, tıklanan ürün satırından gelen _detailCtx'ten beslenir
    // (registerId şimdilik -1). Hata olursa mock cevaba düşer.
    function requestScoreCardDetail(status, callback) {
        var statusCode = (typeof SCORE_CARD_DETAIL_STATUS !== 'undefined' && SCORE_CARD_DETAIL_STATUS[status] != null)
            ? SCORE_CARD_DETAIL_STATUS[status] : 0;

        // Servis/mock cevabını ortak işle: dinamik (ScoreCardDetail) ya da eski tablo
        function handle(raw) {
            if (raw && raw.ScoreCardDetail != null) {
                callback(buildDynamicResponse(raw.ScoreCardDetail));
            } else {
                callback(raw || { columns: [], rows: [] });
            }
        }

        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecards/details',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                dateNumber: _detailCtx.dateNumber,
                registerId: _detailCtx.registerId,
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
        // Veri yoksa kolon başlıkları gösterilmez
        if (!_response.rows.length) {
            $('#scTableHead').empty();
            return;
        }
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

    // Trend servisi: seçili trendPeriod ile istek at; hata olursa mock cevaba düş
    function fetchScoreCardTrend(trendPeriod, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecards/trends/product-sale-realized',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                productId: 237,
                registerId: _registerId,
                branchCode: 3580,
                regionCode: 1,
                trendPeriod: trendPeriod,
                scoreCardId: 21
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
            dateNumber: ($i.data('date-number') != null) ? $i.data('date-number') : -1,
            registerId: _registerId,
            productId: ($i.data('product-id') != null) ? $i.data('product-id') : -1,
            productType: ($i.data('product-type') != null) ? $i.data('product-type') : -1
        };
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
