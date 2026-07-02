// Skor Kart: ana rapor tablosu + detay modalı (tek dosya)
$(function () {

    if (!document.getElementById('scReportBody')) return;

    var COLUMNS = SCORE_CARD_REPORT_COLUMNS;
    var TABLE_NOTE = 'Tabloda yer alan tutarlar /1000 olarak verilmektedir.';   // legend + PDF footer

    // Ürün Tipi sütun filtresi (productTypeId üzerinden; '' = Tümü)
    var selectedTypeId = '';

    // Sıralama (client-side)
    var sortKey = null;
    var sortAsc = true;

    // Seçili kolon: açılışta "Ağırlıklı H/G %"; th'ye tıklanınca değişir. Dashed kutu ile vurgulanır.
    var SC_SELECTABLE_COLS = ['Hedef', 'Gerçekleşen', 'H/G %', 'Ağırlık %', 'Ağırlıklı H/G %', 'Bekleyen'];
    var selectedCol = 'Ağırlıklı H/G %';

    // Tablo toplam satırı HER ZAMAN "Ağırlıklı H/G %" kolonu içindir (seçili kolondan bağımsız, sabit).
    var SC_TOTAL_COL = 'Ağırlıklı H/G %';

    // Rapor tablosu verisi; ilk servis cevabına kadar boş (mock kullanılmaz).
    var SC_RESPONSE = { mainTableData: [] };
    var ROWS = SC_RESPONSE.mainTableData;

    let _regionCode;
    let _branchCode;
    let _registerId;
    let _userRoleCode;
    let _dateNumber;
    let _scoreCardId;
    var _tabModel = [];
    var _overview = null;
    var _firstLoad = true;   // ilk veri gelene kadar tam ekran loader göstermek için

    //scorecard/authorities: kullanıcı rolü + başlangıç bölge/şube/sicil bağlamı. userCode/applicationCode sabittir
    function fetchUserAuthorities(callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecard/authorities',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ userCode: window.USER_CODE, applicationCode: PUPA_APPLICATION_CODE })
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(null);
        });
    }

    function applyUserAuthorities(auth) {
        if (!auth) return;
        if (auth.userInfo) {
            _userRoleCode = auth.userInfo.userRoleCode;
        }
        if (auth.userDashboard) {
            var ud = auth.userDashboard;
            _regionCode = ud.regionCode;
            _branchCode = ud.branchCode;
            _registerId = ud.registerId;
        }
    }

    // scorecard/periods: seçili periyot tipi (aylık/çeyreklik/yıllık) için dönem listesi.
    function fetchPrimMonitoringPeriods(periodType, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecard/periods',
            type: 'GET',
            data: { periodTypes: periodType }
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(null);
        });
    }

    //scorecard/pupa-types: kullanıcı rolüne göre pupa tiplerinin listesi.
    function fetchPupaTypes(dateNumber, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecard/pupa-types',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ dateNumber: dateNumber, roleCode: _userRoleCode })
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(null);
        });
    }

    // Pupa tipi: Key -> statik etiket (PUPA_TYPE_LABELS)
    function renderPupaChannels(pupaRes) {
        var kv = (pupaRes && pupaRes.KeyValues) || [];
        // Servis boş/başarısızsa segmentleri temizle (statik placeholder kalmasın).
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

    function reloadByDateNumber() {
        fetchPupaTypes(_dateNumber, function (pupaRes) {
            renderPupaChannels(pupaRes);
            ScoreCard.filters.loadRegions(loadScoreCardTypes);
        });
    }

    function loadPupaFilters(period) {
        var periodType = PUPA_PERIOD_TYPE[period] || PUPA_PERIOD_TYPE.aylik;
        fetchPrimMonitoringPeriods(periodType, function (periodsRes) {
            var kv = (periodsRes && periodsRes.keyValues) || [];
            _dateNumber = kv.length ? kv[0].key : -1;
            reloadByDateNumber();
        });
    }

    //scorecard/score-cards: seçili dönem + pupa tipi için skor kart listesi.
    function fetchScoreCards(dateNumber, pupaType, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecard/score-cards',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ dateNumber: dateNumber, pupaType: pupaType, roleCode: _userRoleCode})
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(null);
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
        // "Genel Bakış" (-1) servisten gelmez; ön yüzde statik eklenir (tüm kullanıcılarda, en başta).
        kv = kv.filter(function (item) { return Number(item.Key) !== SCORE_CARD_OVERVIEW_KEY; });
        kv = [{ Key: SCORE_CARD_OVERVIEW_KEY }].concat(kv);
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

    // scorecard/cumulatives: ana rapor tablosunu doldurur
    // İstek gövdesi ekrandaki seçimlerden kurulur; servis hata verirse tablo boş kalır.
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
            // Genel Bakış'ta (-1) şube seçili -> cumulatives'e düşülür; bu durumda Şube Müdürü Skorkart (21) gönderilir.
            scoreCardId: (_scoreCardId === -1 ? 21 : _scoreCardId),
            scoreCardTypeId: -1
        };
    }

    // scorecard/cumulatives: seçili bağlam için ana rapor tablosu (ürün/hedef satırları).
    function fetchScoreCardCumulatives(body, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecard/cumulatives',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(body)
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(null);
        });
    }

    // main-view-regions / main-view-branches ortak istek gövdesi; tek fark statik group:
    //   bölge özeti (regions) -> group 4, şube özeti (branches) -> group 3.
    function buildMainViewRequest(group) {
        var rd = reportDateParts();
        var period = $('#scPeriod .period-btn.active').data('period') || 'aylik';
        return {
            roleCode: _userRoleCode,
            regionCode: _regionCode,
            year: rd.year,
            month: rd.month,
            cumulativeFlag: (period === 'yillik') ? 1 : 0,
            quarter: (period === 'ceyreklik') ? 1 : -1,
            pupaType: activePupaType(),
            scorecardId: _scoreCardId,
            group: group
        };
    }

    // scorecard/main-view-regions: Genel Bakış bölge özeti (bölge seçili değilken).
    function fetchScoreCardMainViewRegions(body, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecard/main-view-regions',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(body)
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(null);
        });
    }

    // scorecard/main-view-branches: Genel Bakış şube özeti (bölge seçili, şube seçili değil). Dinamik kolonlu.
    function fetchScoreCardMainViewBranches(body, callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecard/main-view-branches',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(body)
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback(null);
        });
    }

    // scorecard/employee-order-summaries: sıralama kartlarını besler, yalnızca şube seçiliyken.
    function fetchEmployeeOrderSummaries(callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/scorecard/employee-order-summaries',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                dateNumber: _dateNumber,
                registerId: _registerId,
                branchCode: _branchCode,
                scoreCardId: _scoreCardId
            })
        }).done(function (res) {
            callback(Array.isArray(res) ? res : ((res && res.rows) || []));
        }).fail(function () {
            callback([]);
        });
    }

    // Sıralama kartları (yalnızca belirli bir şube seçiliyken gösterilir):
    //   Banka Sıralaması = orderBankOrderNo / orderByBankCodeCount
    //   Bölge Sıralaması = orderRegionOrderNo / orderByRegionCodeCount
    function loadRankings() {
        var $bar = $('.sc-ranking-bar');
        if (_branchCode === -1) {
            $bar.hide();
            return;
        }
        $bar.show();
        fetchEmployeeOrderSummaries(function (rows) {
            var d = (rows && rows[0]) || {};
            $('#scBankRank').text(d.orderBankOrderNo != null ? d.orderBankOrderNo : '-');
            $('#scBankRankTotal').text('/' + (d.orderByBankCodeCount != null ? d.orderByBankCodeCount : '-'));
            $('#scRegionRank').text(d.orderRegionOrderNo != null ? d.orderRegionOrderNo : '-');
            $('#scRegionRankTotal').text('/' + (d.orderByRegionCodeCount != null ? d.orderByRegionCodeCount : '-'));
        });
    }

    // Ağırlıklı H/G % (weightedPercentage) değerlerinin toplamı. Hem "Toplam Skor" kartı hem
    // tablo toplam satırı aynı hesabı kullanır.
    function sumWeightedPercentage(rows) {
        return (rows || []).reduce(function (sum, r) {
            return sum + (Number(r.weightedPercentage) || 0);
        }, 0);
    }

    // Toplam Skor = tablodaki Ağırlıklı H/G % (weightedPercentage) değerlerinin toplamı.
    function renderTotalScore() {
        var total = sumWeightedPercentage(ROWS);
        // percentColor 'ratio-*' döner; kart varyant sınıfına çevrilir ('' ise renksiz/varsayılan kart).
        var cardClass = percentColor(total).replace('ratio-', 'sc-ranking-card--');
        $('#scTotalScore').closest('.sc-ranking-card')
            .removeClass('sc-ranking-card--red sc-ranking-card--orange sc-ranking-card--green sc-ranking-card--blue')
            .addClass(cardClass);
        // Tablo toplam satırıyla aynı görünsün diye aynı biçim (formatPercent: 1 ondalık + küçük ondalık).
        $('#scTotalScore').html(String(formatPercent(total)));
    }

    // ===== Breadcrumb: Bölge / Şube / Sicil =====
    function scCrumb(text, action, disabled) {
        var cls = disabled ? ' disabled' : '';
        var attr = (!disabled && action) ? ' data-sc-breadcrumb="' + action + '"' : '';
        return '<span class="breadcrumb-item' + cls + '"' + attr + '>' + text + '</span>';
    }

    function renderBreadcrumb() {
        var regionSel = _regionCode != null && _regionCode !== -1;
        var branchSel = _branchCode != null && _branchCode !== -1;
        var registerSel = _registerId != null && _registerId !== -1;

        // Hiçbir kademe seçili değilse (hepsi Tümü) breadcrumb gizli
        if (!regionSel && !branchSel && !registerSel) {
            $('#scBreadcrumbBar').hide();
            return;
        }

        // Tek seçenekli (kilitli) bölge dropdown'u -> "Tüm Bölgeler" tıklanamaz
        var regionLocked = $('#scRegionSelect').hasClass('disabled');
        var deepest = registerSel ? 'register' : (branchSel ? 'branch' : 'region');

        // Kök "Tüm Bölgeler" + seçili kademeler. Aktif (en alt) kademe tıklanamaz.
        var parts = [scCrumb('Tüm Bölgeler', 'allRegions', regionLocked)];
        if (regionSel) {
            parts.push(scCrumb($('#scRegionLabel').text(), 'region', regionLocked || deepest === 'region'));
        }
        if (branchSel) {
            parts.push(scCrumb($('#scBranchLabel').text(), 'branch', deepest === 'branch'));
        }
        if (registerSel) {
            parts.push(scCrumb($('#scRegisterLabel').text(), null, true));
        }

        $('#scBreadcrumb').html(parts.join('<span class="breadcrumb-separator">/</span>'));
        $('#scBreadcrumbBar').show();
    }

    // Üst kademeye tıklayınca ilgili dropdown'ın "Tümü" (-1) seçimini tetikle:
    //   Tüm Bölgeler -> bölge Tümü (şube+sicil sıfırlanır)
    //   Bölge        -> şube Tümü (sicil sıfırlanır)
    //   Şube         -> sicil Tümü
    $(document).on('click', '[data-sc-breadcrumb="allRegions"]', function () {
        $('#scRegionList .dropdown-item[data-code="-1"]').trigger('click');
    });
    $(document).on('click', '[data-sc-breadcrumb="region"]', function () {
        $('#scBranchList .dropdown-item[data-code="-1"]').trigger('click');
    });
    $(document).on('click', '[data-sc-breadcrumb="branch"]', function () {
        $('#scRegisterList .dropdown-item[data-code="-1"]').trigger('click');
    });

    // Sayfa ilk açıldığında tam ekran loader (diğer ekranlardaki gibi: loading.js + brand-spinner).
    function showLoadingOverlay() {
        $('body').loading({
            stoppable: false,
            message: '<div><div class="brand-spinner"></div><p class="loading-text">Yükleniyor<span class="loading-dots"><span>.</span><span>.</span><span>.</span></span></p></div>'
        });
    }
    function hideLoadingOverlay() {
        $('body').loading('stop');
    }

    // Servis isteği sırasında tablo iskeleti (hedef raporlarındaki gibi: gerçek tabloyu gizle, .page-skeleton göster).
    function showTableSkeleton() {
        $('#scTableContainer').hide();
        $('#scTableSkeleton').show();
    }
    function hideTableSkeleton() {
        $('#scTableSkeleton').hide();
        $('#scTableContainer').show();
    }

    // Tabloyu doldur:
    //  - Genel Bakış sekmesi (scoreCardId === -1): seçili bölge/şube seviyesine göre özet
    //       • bölge seçili değil -> bölge özeti
    //       • bölge seçili, şube seçili değil -> şube özeti
    //       • şube seçili (şube müdürü / py) -> cumulatives ürün tablosu
    //  - Genel Bakış dışı sekme (scoreCardId !== -1) -> doğrudan cumulatives ürün tablosu
    function loadScoreCardTable() {
        loadRankings();
        renderBreadcrumb();
        showTableSkeleton();   // servis cevabı gelene kadar iskelet; renderReportBody çağrısı üzerine yerini gerçek tabloya bırakır
        // Yalnızca Genel Bakış sekmesinde özet tablosu çıkar
        if (_scoreCardId === -1) {
            // Bölge seçili değil -> bölge özeti (main-view-regions, group 4)
            if (_regionCode === -1) {
                fetchScoreCardMainViewRegions(buildMainViewRequest(4), function (res) {
                    _overview = buildRegionOverviewModel(res);
                    renderReportBody();
                });
                return;
            }
            // Bölge seçili, şube seçili değil -> şube özeti (main-view-branches, group 3).
            // Tek cevap: lastTargets (SUBE_KODU/ADI + 3 aylık) + scoreCardRegionSummary (dinamik HG) + summaryMainSum (Toplam).
            if (_branchCode === -1) {
                fetchScoreCardMainViewBranches(buildMainViewRequest(3), function (branchRes) {
                    _overview = buildBranchOverviewModel(branchRes);
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
            renderTotalScore();                         // Toplam Skor = weightedPercentage toplamı
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
    // "Ürün Tipi" filtre seçenekleri: unique productTypeId'ler (label = productType adı, value = productTypeId)
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

    // Seçili kolonun gövde hücresi için sınıf — table.css'teki dashed kutu kenarları (+ son satır alt kavis)
    function _selCol(c) { return c === selectedCol ? 'col-selected-first col-selected-last' : ''; }
    
    function weightedTotal(rows) {
        var branchSelected = _branchCode != null && _branchCode !== -1;
        if (branchSelected || !rows || !rows.length) return null;
        return sumWeightedPercentage(rows);
    }

    // Ürün tablosu toplam satırı: toplam DAİMA "Ağırlıklı H/G %" kolonunda gösterilir (seçili kolondan bağımsız).
    function productTotalRowHtml(rows) {
        var sum = weightedTotal(rows);
        if (sum == null) return '';
        var colIndex = COLUMNS.indexOf(SC_TOTAL_COL);
        var trailing = COLUMNS.length - colIndex - 1;
        return '<tr class="table-row sc-total-row">' +
            '<td colspan="' + colIndex + '" class="col-right">' + SC_TOTAL_COL + ' Toplamı:</td>' +
            '<td class="' + _selCol(SC_TOTAL_COL) + '">' + formatPercent(sum) + '</td>' +
            (trailing > 0 ? '<td colspan="' + trailing + '"></td>' : '') +
            '</tr>';
    }

    function renderReportHead() {
        var html = '<tr>';
        COLUMNS.forEach(function (c) {
            if (c === 'Ürün Tipi') {
                // Filtre menüsü (seçilebilir kolon değil)
                html += '<th class="sc-type-th">' + typeFilterHtml() + '</th>';
            } else if (c === 'Ürün / Hedef Adı') {
                // Sıralama (seçilebilir kolon değil)
                html += '<th class="col-left" data-sort-key="productName">' + c + sortIconHtml('productName') + '</th>';
            } else if (SC_SELECTABLE_COLS.indexOf(c) > -1) {
                var sel = (c === selectedCol) ? ' selected' : '';   // tek kolon kutusu: table.css thead th.selected
                html += '<th class="sc-col-selectable' + sel + '" data-col="' + c + '">' + c + '</th>';
            } else {
                html += '<th>' + c + '</th>';
            }
        });
        html += '</tr>';
        $('#scReportHead').html(html);
    }

    // Yüzde hücresi: değere göre renk (percentColor) + büyük sayı/küçük ondalık (formatPercent).
    function pctCell(v, extraCls) {
        let colorClass = percentColor(v);
        let cellClass = [colorClass, extraCls].filter(Boolean).join(' ');
        return `<td class="${cellClass}">${formatPercent(v)}</td>`;
    }

    // Ad kolonu = ilk kolon (BOLGE_ADI / SUBE_ADI). Değeri boşlukla doldurulmuş gelebilir -> trim.
    function overviewNameKey() {
        var cols = (_overview && _overview.columns) || [];
        return cols.length ? cols[0].key : null;
    }
    function overviewNameOf(r) {
        var k = overviewNameKey();
        return String((k && r && r[k]) || '').trim();
    }

    function buildRegionOverviewModel(res) {
        var raw = res && res.scoreCardRegionSummary && res.scoreCardRegionSummary.scoreCardRegionSummary;
        var rows = [];
        try { rows = raw ? (JSON.parse(raw) || []) : (Array.isArray(res) ? res : []); }
        catch (e) { rows = []; }

        var sample = rows.length ? rows[0] : {};
        var columns = Object.keys(sample).filter(isOverviewVisibleColumn).map(function (k) {
            return { key: k, label: overviewColLabel(k) };
        });

        return { columns: columns, rows: rows, totals: buildOverviewTotals(res) };
    }

    // summaryMainSum -> { <kolon adı(trim)>: değer }
    function buildOverviewTotals(res) {
        var totals = {};
        ((res && res.summaryMainSum) || []).forEach(function (t) {
            totals[String(t.columnName).trim()] = t.columnValue;
        });
        return totals;
    }
    // Adında HG geçen kolon mu? (skor kart etiketine eşlenir)
    function isOverviewHgColumn(key) {
        return /HG/i.test(String(key).trim());
    }
    // Genel Bakış özet tablosunda gösterilecek kolon mu?
    //  - HG kolonları + SCORE_CARD_OVERVIEW_STATIC_COLUMNS (SUBE_ADI/BOLGE_ADI).
    //  - Bunların dışında servisten gelen kolonlar (BOLGE_KODU, SUBE_KODU ...) gizlenir.
    function isOverviewVisibleColumn(key) {
        var k = String(key).trim();
        return isOverviewHgColumn(k) || SCORE_CARD_OVERVIEW_STATIC_COLUMNS[k] != null;
    }
    function overviewColLabel(key) {
        var k = String(key).trim();
        if (SCORE_CARD_OVERVIEW_STATIC_COLUMNS[k]) return SCORE_CARD_OVERVIEW_STATIC_COLUMNS[k];
        var m = /^HG(\d+)$/i.exec(k);
        if (m && SCORE_CARD_LABELS[m[1]] != null) return SCORE_CARD_LABELS[m[1]] + ' %';
        return k;
    }

    // Hücre biçimi: ilk (ad) kolon metin (sol); diğerleri yüzde + renk (+ varsa grup hücre sınıfı, ör. ay vurgusu)
    function overviewCell(col, value, isFirst) {
        if (isFirst) return '<td class="col-left">' + String(value == null ? '' : value).trim() + '</td>';
        return pctCell(value, col.cellClass);
    }

    // Genel Bakış başlığı (bölge + şube tek path): ad kolonu sola yaslı + sıralanabilir; gruplu kolonlar
    // (ör. "3 Aylık Gerçekleşen %") iki satırlı başlık olur; diğer kolonlar ortalı (CSS varsayılanı).
    function renderOverviewHead() {
        var cols = (_overview && _overview.columns) || [];
        var hasGroups = cols.some(function (c) { return !!c.group; });
        var top = '<tr>', sub = '<tr>';
        for (var i = 0; i < cols.length; i++) {
            var c = cols[i];
            if (c.group) {
                // Aynı gruba ait ardışık kolonları tek üst başlıkta birleştir; alt başlıklar sub satırına
                var j = i;
                while (j + 1 < cols.length && cols[j + 1].group === c.group) j++;
                top += '<th colspan="' + (j - i + 1) + '" class="col-group-header selected">' +
                           '<div class="col-group-header-content"><span>' + c.group + '</span></div>' +
                       '</th>';
                for (var g = i; g <= j; g++) sub += '<th class="' + (cols[g].cellClass || '') + '">' + cols[g].label + '</th>';
                i = j;
                continue;
            }
            var rs = hasGroups ? ' rowspan="2"' : '';
            if (i === 0) {
                top += '<th' + rs + ' class="col-left" data-sort-key="overviewName">' + c.label + sortIconHtml('overviewName') + '</th>';
            } else {
                top += '<th' + rs + '>' + c.label + '</th>';
            }
        }
        $('#scReportHead').html(top + '</tr>' + (hasGroups ? (sub + '</tr>') : ''));
    }

    // Genel Bakış gövdesi (bölge + şube tek path): dinamik kolonlar + en altta "Toplam" satırı
    function renderOverviewBody() {
        var cols = (_overview && _overview.columns) || [];
        var rows = visibleOverviewRows();

        if (!rows.length) {
            $('#scReportHead').html('');   // Veri yoksa başlıklar (th) da gizlenir
            $('#scReportBody').html(
                '<tr class="no-result-row"><td colspan="' + (cols.length || 1) + '" style="text-align:center;padding:48px 16px;">' +
                    '<div class="table-empty-state">' +
                        '<img src="/images/empty-state-seach.svg" alt="" />' +
                        '<span>Seçili döneme ait veri bulunmamaktadır.</span>' +
                    '</div>' +
                '</td></tr>'
            );
            return;
        }

        renderOverviewHead();

        var html = '';
        rows.forEach(function (r) {
            html += '<tr class="table-row">';
            cols.forEach(function (c, i) { html += overviewCell(c, r[c.key], i === 0); });
            html += '</tr>';
        });

        // "Toplam" satırı: ilk hücre "Toplam:", diğerleri summaryMainSum'dan (formatPercent dönmeyeni "-" yapar)
        var totals = (_overview && _overview.totals) || {};
        html += '<tr class="table-row sc-total-row">';
        cols.forEach(function (c, i) {
            if (i === 0) { html += '<td class="col-right">Toplam:</td>'; return; }
            var v = totals[String(c.key).trim()];
            html += '<td class="' + (c.cellClass || '') + '">' + formatPercent(v) + '</td>';
        });
        html += '</tr>';

        var $body = $('#scReportBody').html(html);
        if (typeof reStripeTable === 'function') reStripeTable($body);
    }

    // Ay anahtarı -> başlık (NISAN_2026 -> "NISAN 2026")
    function monthLabel(key) { return String(key).trim().replace(/_/g, ' '); }

    // Ay grubu için hücre sınıfı (ilk/orta/son -> col-selected-*)
    function monthCellClass(mi, total) {
        return (mi === 0) ? 'col-selected-first' : (mi === total - 1 ? 'col-selected-last' : 'col-selected-mid');
    }

    function buildBranchOverviewModel(branchRes) {
        var srcRows = (branchRes && branchRes.lastTargets) || [];
        if (!Array.isArray(srcRows)) srcRows = srcRows.rows || [];
        var rows = srcRows.map(function (r) { return Object.assign({}, r); });
        var lastSample = rows.length ? rows[0] : {};
        
        var leadCols = [], monthKeys = [];
        Object.keys(lastSample).forEach(function (k) {
            var kk = k.trim();
            if (kk === 'SUBE_KODU' || kk === 'SUBE_ADI') {
                if (SCORE_CARD_OVERVIEW_STATIC_COLUMNS[kk] != null) leadCols.push({ key: k, label: overviewColLabel(k) });
            } else {
                monthKeys.push(k); 
            }
        });
        var monthCols = monthKeys.map(function (k, mi) {
            return { key: k, label: monthLabel(k), group: '3 Aylık Gerçekleşen %', cellClass: monthCellClass(mi, monthKeys.length) };
        });

        // scoreCardRegionSummary: yalnızca adında HG geçen kolonlar; değerleri SUBE_ADI ile satırlara ekle
        var raw = branchRes && branchRes.scoreCardRegionSummary && branchRes.scoreCardRegionSummary.scoreCardRegionSummary;
        var branchRows = [];
        try { branchRows = raw ? (JSON.parse(raw) || []) : []; }
        catch (e) { branchRows = []; }
        var branchSample = branchRows.length ? branchRows[0] : {};
        var hgKeys = Object.keys(branchSample).filter(isOverviewHgColumn);
        var hgCols = hgKeys.map(function (k) { return { key: k, label: overviewColLabel(k) }; });

        var byName = {};
        branchRows.forEach(function (br) { byName[String(br.SUBE_ADI || '').trim()] = br; });
        rows.forEach(function (r) {
            var br = byName[String(r.SUBE_ADI || '').trim()];
            if (br) hgKeys.forEach(function (hk) { r[hk] = br[hk]; });
        });

        return { columns: leadCols.concat(monthCols).concat(hgCols), rows: rows, totals: buildOverviewTotals(branchRes) };
    }

    // Skor kart ana raporu PDF verisi (servis cevabından; DOM'dan değil). data-pdf="report" bunu kullanır.
    function _scInfoLines() {
        var date = ($('.date-text').text() || '').trim();
        var crumb = $('#scBreadcrumbBar').is(':visible')
            ? ($('#scBreadcrumb').text() || '').replace(/\s+/g, ' ').trim()
            : '';
        var scoreCard = [
            ($('#scTabList .tab.active').text() || '').trim(),
            ($('#scSubTabList .sub-tab.active').text() || '').trim()
        ].filter(Boolean).join(' - ');
        var pupa = ($('#scChannels .segment.active').text() || '').trim();
        var period = ($('#scPeriod .period-btn.active').text() || '').trim();

        var lines = [];
        var l1 = crumb ? ((date ? date + ' tarihine ait ' : '') + crumb) : date;
        if (l1) lines.push(l1);
        if (scoreCard) lines.push('Skor Kart: ' + scoreCard);
        if (pupa) lines.push('Pupa Tipi: ' + pupa);
        if (period) lines.push('Periyot: ' + period);
        return lines;
    }
    // Görünür satırlar: ekrandaki filtre (arama / tip) + client sıralama uygulanmış hali.
    // Hem tablo render'ı hem PDF aynı kaynağı kullanır -> PDF her zaman ekranda görüneni indirir.
    function visibleProductRows() {
        var query = ($('#scSearchInput').val() || '').trim().toLowerCase();
        var rows = ROWS.filter(function (r) {
            var matchesQuery = !query || (r.productName || '').toLowerCase().indexOf(query) > -1;
            var matchesType = !selectedTypeId || String(r.productTypeId) === String(selectedTypeId);
            return matchesQuery && matchesType;
        });
        if (sortKey) {
            rows.sort(function (a, b) {
                var cmp = String(a[sortKey] || '').localeCompare(String(b[sortKey] || ''), 'tr', { sensitivity: 'base' });
                return sortAsc ? cmp : -cmp;
            });
        }
        return rows;
    }
    // Genel Bakış görünür satırları (bölge + şube tek path): ad kolonuna göre arama + sıralama.
    function visibleOverviewRows() {
        var query = ($('#scSearchInput').val() || '').trim().toLowerCase();
        var rows = ((_overview && _overview.rows) || []).filter(function (r) {
            return !query || overviewNameOf(r).toLowerCase().indexOf(query) > -1;
        });
        if (sortKey === 'overviewName') {
            rows = rows.slice().sort(function (a, b) {
                var cmp = overviewNameOf(a).localeCompare(overviewNameOf(b), 'tr', { sensitivity: 'base' });
                return sortAsc ? cmp : -cmp;
            });
        }
        return rows;
    }

    function setScoreCardPdfReport() {
        var cols, rows;
        if (_overview) {
            // Genel Bakış (bölge + şube tek path): ilk kolon sola yaslı metin; gruplu (ay) ve HG kolonları yüzde.
            cols = (_overview.columns || []).map(function (c, i) {
                if (i === 0) return { header: c.label, key: c.key, align: 'left', format: function (v) { return String(v == null ? '' : v).trim(); } };
                var def = { header: c.label, key: c.key, format: formatPercent };
                if (c.group) def.group = c.group;
                return def;
            });
            rows = visibleOverviewRows().slice();
            // Ekrandaki gibi "Toplam" satırını da PDF'e ekle (summaryMainSum); "Toplam:" ilk kolonda
            var totalRow = {};
            (_overview.columns || []).forEach(function (c, i) {
                if (i === 0) { totalRow[c.key] = 'Toplam:'; return; }
                var v = (_overview.totals || {})[String(c.key).trim()];
                if (v != null) totalRow[c.key] = v;
            });
            rows.push(totalRow);
        } else {
            var pctOrTotal = function (role) {
                return function (v, row) {
                    if (row && row.__isTotal) return role === 'label' ? (SC_TOTAL_COL + ' Toplamı:') : '';
                    return formatPercent(v);
                };
            };
            cols = [
                { header: 'Ürün / Hedef Adı', key: 'productName', align: 'left' }, { header: 'Ürün Tipi', key: 'productType' },
                { header: 'Hedef', key: 'targetValue' }, { header: 'Gerçekleşen', key: 'realizedValue' },
                { header: 'H/G %', key: 'targetRealizationPercentage', format: pctOrTotal('blank') }, { header: 'Ağırlık %', key: 'productWeight', format: pctOrTotal('label') },
                { header: 'Ağırlıklı H/G %', key: 'weightedPercentage', format: formatPercent }, { header: 'Bekleyen', key: 'pending' }
            ];
            rows = visibleProductRows();
            var ptSum = weightedTotal(rows);
            if (ptSum != null) {
                rows = rows.concat([{ __isTotal: true, weightedPercentage: ptSum }]);
            }
        }
        window.PdfReport = {
            title: ($('.page-title').text() || 'Skor Kart').trim(),
            infoLines: _scInfoLines(),
            columns: cols,
            rows: rows || [],
            footerNote: TABLE_NOTE,
            filename: 'SkorKart.pdf'
        };
    }

    function renderReportBody() {
        hideTableSkeleton();       // gerçek veri çiziliyor -> iskeleti gizle, tabloyu göster
        if (_firstLoad) { hideLoadingOverlay(); _firstLoad = false; }   // ilk veride tam ekran loader'ı kapat
        setScoreCardPdfReport();   // PDF verisini güncel tut (servis cevabından)
        // Genel Bakış özet modu: ürün tablosu yerine bölge/şube özet tablosu (tek render path)
        if (_overview) {
            renderOverviewBody();
            return;
        }

        var rows = visibleProductRows();

        if (!rows.length) {
            $('#scReportHead').html('');   // Veri yoksa başlıklar (th) da gizlenir
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

        // Başlık yalnız sonuç varken çizilir
        renderReportHead();

        var html = '';
        rows.forEach(function (r) {
            html += '<tr class="table-row">';
            // ProductInfo varsa info ikonu + hover tooltip; yoksa hücre boş
            var infoCell = r.productInfo
                ? '<img class="info-icon" tabindex="0" data-tooltip="' + escapeAttr(r.productInfo) + '" src="/images/table-info.svg" alt="Bilgi" />'
                : '';
            html += '<td class="col-info">' + infoCell + '</td>';
            html += '<td class="col-left sc-product-name">' + r.productName + '</td>';
            html += '<td>' + r.productType + '</td>';
            html += '<td class="' + _selCol('Hedef') + '">' + r.targetValue + '</td>';
            html += '<td class="' + _selCol('Gerçekleşen') + '">' + r.realizedValue + '</td>';
            html += '<td class="' + (percentColor(r.targetRealizationPercentage) + ' ' + _selCol('H/G %')).trim() + '">' + formatPercent(r.targetRealizationPercentage) + '</td>';
            html += '<td class="' + _selCol('Ağırlık %') + '">' + formatPercent(r.productWeight) + '</td>';
            html += '<td class="' + _selCol('Ağırlıklı H/G %') + '">' + formatPercent(r.weightedPercentage) + '</td>';
            html += '<td class="' + _selCol('Bekleyen') + '">' + r.pending + '</td>';
            // Detay drill-down bağlamı (scorecard/details): productId satırdan, productType aktif kanaldan.
            // dateNumber/registerId istekte doğrudan modül state'inden gönderilir.
            html += '<td><img class="sc-detail-icon" src="/images/detail.svg" alt="Detay"' +
                ' data-name="' + r.productName + '"' +
                ' data-product-id="' + (r.productId != null ? r.productId : -1) + '"' +
                ' data-product-type="' + activePupaType() + '" /></td>';
            html += '</tr>';
        });
        // Şube seçili değilken seçili kolonun toplam satırı en alta eklenir
        html += productTotalRowHtml(rows);
        var $body = $('#scReportBody').html(html);
        if (typeof reStripeTable === 'function') reStripeTable($body);
    }

    function renderLegend() {
        if (typeof renderTableLegend === 'function') {
            renderTableLegend('#scReportLegend', {
                note: TABLE_NOTE,
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

    // Period tipi (Aylık / Çeyreklik / Yıllık): sadece aktif buton görünümü.
    // Yeniden yükleme, date-picker yeni dönemin dateNumber'ını sc:dateChanged ile
    // yayınladığında (aşağıdaki dinleyici) tetiklenir.
    $('#scPeriod').on('click', '.period-btn', function () {
        $('#scPeriod .period-btn').removeClass('active');
        $(this).addClass('active');
    });

    // Date picker'da dateNumber değişince: dateNumber içeren servisleri
    // (pupa-types -> score-cards -> employee-order-summaries) sırasıyla yeniden çağır.
    $(document).on('sc:dateChanged', function (e, payload) {
        if (!payload || payload.dateNumber == null) return;
        if (_userRoleCode == null) return;                 // ilk yükleme (auth) bitmeden tetiklenmesin; bootstrap loadPupaFilters yapar
        if (payload.dateNumber === _dateNumber) return;    // değişmediyse atla
        _dateNumber = payload.dateNumber;
        reloadByDateNumber();
    });

    // Seçili kolon: değer kolonu başlığına tıklanınca seçili kolon o olur (dashed kutu taşınır)
    $(document).on('click', '#scReportHead th.sc-col-selectable', function () {
        var c = $(this).data('col');
        if (!c || c === selectedCol) return;
        selectedCol = c;
        renderReportBody();   // head + body yeniden çizilir
    });

    $(document).on('click', '#scReportHead th[data-sort-key]', function () {
        var key = $(this).data('sort-key');
        if (!key) return;
        if (sortKey === key) {
            if (sortAsc) { sortAsc = false; }
            else { sortKey = null; sortAsc = true; }
        } else {
            sortKey = key;
            sortAsc = true;
        }
        renderReportBody();
    });

    // Ürün Tipi sütun filtresi menüsü (yalnız trigger'da; th'nin gerisi kolon seçer)
    $(document).on('click', '.sc-type-trigger', function (e) {
        e.stopPropagation();
        $(this).closest('.sc-type-filter').toggleClass('open');
    });
    // Seçim (renderReportBody başlığı yeniden kurar -> menü kapanır). stopPropagation: kolon seçimini tetiklemesin.
    $(document).on('click', '.sc-type-option', function (e) {
        e.stopPropagation();
        selectedTypeId = $(this).attr('data-type') || '';
        renderReportBody();
    });
    // Dışarı tıklayınca kapat
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.sc-type-filter').length) {
            $('.sc-type-filter').removeClass('open');
        }
    });

    // İlk render: kullanıcı yetki/bağlamı (scorecard/authorities) çekilir ve filtre zinciri kurulur.
    // Token yönetimi backend (ScoreCardTokenService) tarafından yapılır.
    fetchUserAuthorities(function (auth) {
        applyUserAuthorities(auth);
        loadPupaFilters($('#scPeriod .period-btn.active').data('period') || 'aylik');
    });
    renderLegend();
    showLoadingOverlay();   // sayfa ilk açılışında tam ekran loader (diğer ekranlardaki gibi)
    showTableSkeleton();    // arkada tablo iskeleti; gerçek veri auth -> servis zinciriyle gelince ikisi de kapanır

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
