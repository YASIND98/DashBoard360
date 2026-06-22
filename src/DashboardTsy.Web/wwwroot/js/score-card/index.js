// Skor Kart: ana rapor tablosu + detay modalı (tek dosya)
$(function () {

    if (!document.getElementById('scReportBody')) return;

    var COLUMNS = SCORE_CARD_REPORT_COLUMNS;

    // Ürün Tipi sütun filtresi (productTypeId üzerinden; '' = Tümü)
    var selectedTypeId = '';

    // Sıralama (client-side)
    var sortKey = null;
    var sortAsc = true;

    // Rapor tablosu verisi (mock.js -> window.MOCK.scoreCardReport)
    var SC_RESPONSE = (typeof getScoreCardReportMock === 'function')
        ? getScoreCardReportMock()
        : { mainTableData: [] };
    var ROWS = SC_RESPONSE.mainTableData;

    let _regionCode;
    let _branchCode;
    let _registerId;
    let _userRoleCode;
    let _dateNumber;
    let _scoreCardId;
    var _tabModel = [];
    var _overview = null;

    //users/authorities: kullanıcı rolü + başlangıç bölge/şube/sicil bağlamı. userCode/applicationCode sabittir.
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

    function applyUserAuthorities(auth) {
        if (!auth) return;
        if (auth.userInfo) {
            _userRoleCode = auth.userInfo.userRoleCode;   // her zaman number
        }
        if (auth.userDashboard) {
            // userDashboard: başlangıç bölge/şube/sicil bağlamı (number; -1 = Tümü). Rol bu kodlardan türer.
            var ud = auth.userDashboard;
            _regionCode = ud.regionCode;
            _branchCode = ud.branchCode;
            _registerId = ud.registerId;
        }
    }

    // prim-monitoring/periods: seçili periyot tipi (aylık/çeyreklik/yıllık) için dönem listesi.
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

    //sales-target-monitoring/pupa-types: kullanıcı rolüne göre pupa tiplerinin listesi.
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

    // Pupa tipi: Key -> statik etiket (PUPA_TYPE_LABELS)
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
                ScoreCard.filters.loadRegions(loadScoreCardTypes);   // filters.js
            });
        });
    }

    //dashboard/score-cards: seçili dönem + pupa tipi için skor kart listesi.
    function fetchScoreCards(dateNumber, pupaType, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/dashboard/score-cards',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ dateNumber: dateNumber, pupaType: pupaType, roleCode: _userRoleCode})
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

    // Pupa/period değişince çalışır: score-cards iste -> sekmeleri çiz (_scoreCardId) -> şube -> sicil -> tablo.
    function loadScoreCardTypes() {
        fetchScoreCards(_dateNumber, activePupaType(), function (scRes) {
            renderScoreCardTabs(scRes);                 // _scoreCardId set edilir
            // branches gövdesi scoreCardId + pupaType ister -> score-cards'tan SONRA, sicil/tablodan ÖNCE.
            // (pupa/period değişiminde loadScoreCardTypes tekrar çağrıldığından branches de yenilenir.)
            ScoreCard.filters.loadBranches(function () {            // filters.js
                ScoreCard.filters.loadRegisters(loadScoreCardTable);
            });
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
            registerId: _registerId,                        // seçili sicil;
            regionCode: _regionCode,                        // seçili bölge;
            branchCode: _branchCode,                        // seçili şube;
            pupaType: activePupaType(),                     // seçili pupa tipi;
            scoreCardId: _scoreCardId,                      // seçili skor kart;
            scoreCardTypeId: -1
        };
    }

    // scorecards/cumulatives: seçili bağlam için ana rapor tablosu (ürün/hedef satırları).
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

    // Genel Bakış bölge özeti istek gövdesi (tüm bölgeler)
    function buildOverviewRequest() {
        var rd = reportDateParts();
        var period = $('#scPeriod .period-btn.active').data('period') || 'aylik';
        return {
            year: rd.year,
            month: rd.month,
            quarter: (period === 'ceyreklik') ? 1 : -1,
            cumulativeFlag: (period === 'yillik') ? 1 : 0,
            registerId: _registerId,
            regionCode: _regionCode,
            branchCode: _branchCode,
            pupaType: activePupaType()
        };
    }

    // scorecards/region-overview: Genel Bakış (scoreCardId -1) bölge/şube özet tablosu (aynı endpoint).
    function fetchScoreCardOverview(body, mockFn, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecards/region-overview',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(body)
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(typeof mockFn === 'function' ? mockFn() : null);
        });
    }

    // Tabloyu doldur:
    //  - Genel Bakış sekmesi (scoreCardId === -1): seçili bölge/şube seviyesine göre özet
    //       • bölge seçili değil -> bölge özeti
    //       • bölge seçili, şube seçili değil -> şube özeti
    //       • şube seçili (şube müdürü / py) -> cumulatives ürün tablosu
    //  - Genel Bakış dışı sekme (scoreCardId !== -1) -> doğrudan cumulatives ürün tablosu
    function loadScoreCardTable() {
        // Yalnızca Genel Bakış sekmesinde özet tablosu çıkar
        if (_scoreCardId === -1) {
            // Bölge seçili değil -> bölge özeti
            if (_regionCode === -1) {
                fetchScoreCardOverview(buildOverviewRequest(), getScoreCardRegionOverviewMock, function (res) {
                    var rows = Array.isArray(res) ? res : ((res && res.rows) || []);
                    _overview = { mode: 'region', rows: rows };
                    renderReportBody();
                });
                return;
            }
            // Bölge seçili, şube seçili değil -> şube özeti
            if (_branchCode === -1) {
                fetchScoreCardOverview(buildOverviewRequest(), getScoreCardBranchOverviewMock, function (res) {
                    _overview = { mode: 'branch', rows: (res && res.rows) || [], months: (res && res.months) || [] };
                    renderReportBody();
                });
                return;
            }
            // Şube seçili -> aşağıdaki cumulatives ürün tablosuna düşer
        }
        // Genel Bakış dışı sekme ya da Genel Bakış'ta şube seçili -> ürün tablosu
        fetchScoreCardCumulatives(buildCumulativesRequest(), function (res) {
            _overview = null;
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
    // "Ürün Tipi" filtre seçenekleri: unique productTypeId'ler (etiket = productType adı, değer = productTypeId)
    function getTypeOptions() {
        var seen = {};
        var opts = [{ label: 'Tümü', value: '' }];
        ROWS.forEach(function (r) {
            if (r.productTypeId != null && !seen[r.productTypeId]) {
                seen[r.productTypeId] = true;
                opts.push({ label: r.productType, value: r.productTypeId });
            }
        });
        return opts;
    }

    // "Ürün Tipi" başlığı: tıklanınca açılan  filtre menüsü
    function typeFilterHtml() {
        var options = getTypeOptions();
        var items = options.map(function (o) {
            var sel = (String(o.value) === String(selectedTypeId)) ? ' selected' : '';
            return '<div class="sc-type-option' + sel + '" data-type="' + o.value + '">' + o.label + '</div>';
        }).join('');
        // Seçili tipin adını bul (Tümü hariç) -> başlıkta "Ürün Tipi (<ad>)" göster
        var selectedOpt = options.filter(function (o) { return o.value !== '' && String(o.value) === String(selectedTypeId); })[0];
        var triggerText = selectedOpt ? ('Ürün Tipi (' + selectedOpt.label + ')') : 'Ürün Tipi';
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

    // Genel Bakış bölge özeti: başlık (SCORE_CARD_OVERVIEW_COLUMNS)
    // Yüzde hücresi: değere göre renk (percentColor) + büyük sayı/küçük ondalık (formatPercent).
    function pctCell(v, extraCls) {
        let colorClass = percentColor(v);
        let cellClass = [colorClass, extraCls].filter(Boolean).join(' ');
        return `<td class="${cellClass}">${formatPercent(v)}</td>`;
    }

    function renderOverviewHead() {
        var html = '<tr>';
        SCORE_CARD_OVERVIEW_COLUMNS.forEach(function (c) {
            if (c === 'Bölge Adı') {
                html += '<th class="col-text" data-sort-key="regionName">' + c + sortIconHtml('regionName') + '</th>';
            } else {
                html += '<th>' + c + '</th>';
            }
        });
        html += '</tr>';
        $('#scReportHead').html(html);
    }

    // Genel Bakış bölge özeti gövdesi: her satır bir bölge, hücreler H/G %'si (değere göre renkli)
    function renderOverviewBody() {
        var query = ($('#scSearchInput').val() || '').trim().toLowerCase();
        var rows = ((_overview && _overview.rows) || []).filter(function (r) {
            return !query || (r.regionName || '').toLowerCase().indexOf(query) > -1;
        });
        if (sortKey === 'regionName') {
            rows = rows.slice().sort(function (a, b) {
                var cmp = String(a.regionName || '').localeCompare(String(b.regionName || ''), 'tr', { sensitivity: 'base' });
                return sortAsc ? cmp : -cmp;
            });
        }

        renderOverviewHead();

        if (!rows.length) {
            $('#scReportBody').html(
                '<tr class="no-result-row"><td colspan="' + SCORE_CARD_OVERVIEW_COLUMNS.length + '" style="text-align:center;padding:48px 16px;">' +
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
            html += '<td class="col-text">' + (r.regionName || '') + '</td>';
            html += pctCell(r.corporate);
            html += pctCell(r.commercial);
            html += pctCell(r.kbi);
            html += pctCell(r.obi);
            html += pctCell(r.agriculture);
            html += pctCell(r.sy);
            html += pctCell(r.bd);
            html += pctCell(r.gise);
            html += '</tr>';
        });
        var $body = $('#scReportBody').html(html);
        if (typeof reStripeTable === 'function') reStripeTable($body);
    }

    function renderBranchOverviewHead(months) {
        months = months || [];
        var top = '<tr>', sub = '<tr>';
        SCORE_CARD_BRANCH_OVERVIEW_COLUMNS.forEach(function (c) {
            if (c === '3 Aylık Gerçekleşen %') {
                top += '<th colspan="' + (months.length || 1) + '" class="col-group-header selected">' +
                        '<div class="col-group-header-content"><span>' + c + '</span></div>' +
                       '</th>';
                months.forEach(function (m, mi) {
                    var cls = (mi === 0) ? 'col-selected-first' : (mi === months.length - 1 ? 'col-selected-last' : 'col-selected-mid');
                    sub += '<th class="' + cls + '">' + m + '</th>';
                });
            } else if (c === 'Sıralama') {
                top += '<th rowspan="2" data-sort-key="rank">' + c + sortIconHtml('rank') + '</th>';
            } else if (c === 'Şube Adı') {
                top += '<th rowspan="2" class="col-text" data-sort-key="branchName">' + c + sortIconHtml('branchName') + '</th>';
            } else {
                top += '<th rowspan="2">' + c + '</th>';
            }
        });
        $('#scReportHead').html(top + '</tr>' + sub + '</tr>');
    }

    // Genel Bakış şube özeti gövdesi: her satır bir şube, hücreler H/G %'si (değere göre renkli)
    function renderBranchOverviewBody() {
        var data = _overview || {};
        var months = data.months || [];
        var query = ($('#scSearchInput').val() || '').trim().toLowerCase();
        var rows = (data.rows || []).filter(function (r) {
            return !query || (r.branchName || '').toLowerCase().indexOf(query) > -1;
        });
        if (sortKey === 'branchName' || sortKey === 'rank') {
            rows = rows.slice().sort(function (a, b) {
                var cmp = (sortKey === 'rank')
                    ? (a.rank || 0) - (b.rank || 0)
                    : String(a.branchName || '').localeCompare(String(b.branchName || ''), 'tr', { sensitivity: 'base' });
                return sortAsc ? cmp : -cmp;
            });
        }

        renderBranchOverviewHead(months);

        var leafCount = (SCORE_CARD_BRANCH_OVERVIEW_COLUMNS.length - 1) + months.length;
        if (!rows.length) {
            $('#scReportBody').html(
                '<tr class="no-result-row"><td colspan="' + leafCount + '" style="text-align:center;padding:48px 16px;">' +
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
            html += '<td>' + (r.rank != null ? r.rank : '') + '</td>';
            html += '<td class="col-text">' + (r.branchName || '') + '</td>';
            html += pctCell(r.month1, 'col-selected-first') + pctCell(r.month2, 'col-selected-mid') + pctCell(r.month3, 'col-selected-last');
            html += pctCell(r.corporate);
            html += pctCell(r.commercial);
            html += pctCell(r.kbi);
            html += pctCell(r.obi);
            html += pctCell(r.agriculture);
            html += pctCell(r.mass);
            html += pctCell(r.afili);
            html += pctCell(r.privateBanking);
            html += '</tr>';
        });
        var $body = $('#scReportBody').html(html);
        if (typeof reStripeTable === 'function') reStripeTable($body);
    }

    function renderReportBody() {
        // Genel Bakış özet modu: ürün tablosu yerine bölge/şube özet tablosu
        if (_overview) {
            if (_overview.mode === 'branch') renderBranchOverviewBody();
            else renderOverviewBody();
            return;
        }

        var query = ($('#scSearchInput').val() || '').trim().toLowerCase();
        var rows = ROWS.filter(function (r) {
            var matchesQuery = !query || (r.productName || '').toLowerCase().indexOf(query) > -1;
            var matchesType = !selectedTypeId || String(r.productTypeId) === String(selectedTypeId);
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
            // Detay drill-down bağlamı (scorecards/details): productId satırdan, productType aktif kanaldan.
            // dateNumber/registerId istekte doğrudan modül state'inden gönderilir.
            html += '<td><img class="sc-detail-icon" src="/images/detail.svg" alt="Detay"' +
                ' data-name="' + r.productName + '"' +
                ' data-product-id="' + (r.productId != null ? r.productId : -1) + '"' +
                ' data-product-type="' + activePupaType() + '" /></td>';
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
        selectedTypeId = $(this).attr('data-type') || '';
        renderReportBody();
    });
    // Dışarı tıklayınca kapat
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.sc-type-filter').length) {
            $('.sc-type-filter').removeClass('open');
        }
    });

    // İlk render: önce kullanıcı yetki/bağlamı (users/authorities) çekilir, sonra filtre zinciri kurulur
    fetchUserAuthorities(function (auth) {
        applyUserAuthorities(auth);
        loadPupaFilters($('#scPeriod .period-btn.active').data('period') || 'aylik');
    });
    renderLegend();
    renderReportBody();

    window.ScoreCard = window.ScoreCard || {};

    // Filtre modülü (filters.js) bu accessor üzerinden rapor durumunu okur/yazar ve aksiyon tetikler.
    window.ScoreCard.report = {
        get regionCode()  { return _regionCode; },
        set regionCode(v) { _regionCode = v; },
        get branchCode()  { return _branchCode; },
        set branchCode(v) { _branchCode = v; },
        get registerId()  { return _registerId; },
        set registerId(v) { _registerId = v; },
        get dateNumber()  { return _dateNumber; },
        get scoreCardId() { return _scoreCardId; },
        activePupaType: activePupaType,
        loadTable: loadScoreCardTable,
        renderBody: renderReportBody
    };

    // Detay modalı (target-detail.js / trend-analize.js) ana raporun seçili filtre bağlamını buradan okur.
    window.ScoreCard.getContext = function () {
        return {
            dateNumber: _dateNumber,
            registerId: _registerId,
            regionCode: _regionCode,
            branchCode: _branchCode,
            scoreCardId: _scoreCardId
        };
    };
});
