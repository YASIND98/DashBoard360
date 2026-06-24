// Score Card — Date Picker
// Dönem tipine (aylik=1, ceyreklik=2, yillik=3) göre farklı picker paneli gösterir.
// Seçim değişince $(document).trigger('sc:dateChanged', { periodType, dateNumber, year, month, quarter })

$(function () {
    'use strict';

    if (!$('#scDatePicker').length) return;

    // var today       = _reportDate ? new Date(_reportDate) : new Date();
    // var CUR_YEAR    = today.getFullYear();
    // var CUR_MONTH   = today.getMonth() + 1;         // 1–12
    // var CUR_QUARTER = Math.ceil(CUR_MONTH / 3);     // 1–4

    var CUR_YEAR;
    var CUR_MONTH;
    var CUR_QUARTER;

    // PUPA_PERIOD_TYPE: aylik=1, ceyreklik=2, yillik=3
    var PERIOD = (typeof PUPA_PERIOD_TYPE !== 'undefined')
        ? PUPA_PERIOD_TYPE
        : { aylik: 1, ceyreklik: 2, yillik: 3 };

    // "Bu Ay" / "Bu Çeyrek" / "Bu Yıl" sınırını gerçek tarih yerine
    // primMonitoringPeriods[periodType].keyValues[0] (ilk dönem) üzerinden belirle.
    var _initAylik = (typeof getPrimMonitoringPeriodsMock === 'function') ? getPrimMonitoringPeriodsMock(PERIOD.aylik) : null;
    var _initKv0   = _initAylik && _initAylik.keyValues && _initAylik.keyValues[0];
    if (_initKv0) {
        var _p0     = _initKv0.value.split(' - ');
        CUR_YEAR    = parseInt(_p0[0], 10);
        CUR_MONTH   = parseInt(_p0[1].replace(/[^0-9]/g, ''), 10);
        CUR_QUARTER = Math.ceil(CUR_MONTH / 3);
    }

    var _initCeyrek    = (typeof getPrimMonitoringPeriodsMock === 'function') ? getPrimMonitoringPeriodsMock(PERIOD.ceyreklik) : null;
    var _initCeyrekKv0 = _initCeyrek && _initCeyrek.keyValues && _initCeyrek.keyValues[0];
    var CUR_CEYREK_YEAR    = CUR_YEAR;
    var CUR_CEYREK_QUARTER = CUR_QUARTER;
    if (_initCeyrekKv0) {
        var _pc0            = _initCeyrekKv0.value.split(' - ');
        CUR_CEYREK_YEAR    = parseInt(_pc0[0], 10);
        CUR_CEYREK_QUARTER = parseInt(_pc0[1].replace(/[^0-9]/g, ''), 10);
    }

    var _initYillik    = (typeof getPrimMonitoringPeriodsMock === 'function') ? getPrimMonitoringPeriodsMock(PERIOD.yillik) : null;
    var _initYillikKv0 = _initYillik && _initYillik.keyValues && _initYillik.keyValues[0];
    var CUR_YILLIK_YEAR = CUR_YEAR;
    if (_initYillikKv0) {
        CUR_YILLIK_YEAR = parseInt(_initYillikKv0.value.split(' - ')[0], 10);
    }

    var state = {
        periodType : PERIOD.aylik,
        year       : CUR_YEAR,
        month      : CUR_MONTH,
        quarter    : CUR_QUARTER,
        panelYear  : CUR_YEAR   // yıl gezintisi için panel içi yıl
    };

    // ── Label / Badge ─────────────────────────────────────────────────────────

    // ≤4 karakter: tam ad, daha uzun: ilk 3 karakter (Mart→Mart, Nisan→Nis, Mayıs→May vb.)
    function _shortMonth(m) { var n = _trMonths[m - 1]; return n.length <= 4 ? n : n.slice(0, 3); }

    function getLabel() {
        if (state.periodType === PERIOD.aylik) return _trMonths[state.month - 1] + ' ' + state.year;
        if (state.periodType === PERIOD.ceyreklik) {
            // Banka çeyreği: Q1=Mar-Nis-May, Q2=Haz-Tem-Ağu, Q3=Eyl-Eki-Kas, Q4=Ara-Oca-Şub
            var _m1 = state.quarter * 3;           // Q1→3, Q2→6, Q3→9, Q4→12
            var _m2 = _m1 % 12 + 1;               // Q4: 12%12=0 → 1
            var _m3 = _m2 % 12 + 1;
            var _monthsPart = _shortMonth(_m1) + ' - ' + _shortMonth(_m2) + ' - ' + _shortMonth(_m3) + ' ' + state.year;
            var _isCurCeyrek = (state.year === CUR_CEYREK_YEAR && state.quarter === CUR_CEYREK_QUARTER);
            return _isCurCeyrek ? _monthsPart : _monthsPart + '  ' + state.quarter + '. Çeyrek';
        }
        return '' + state.year;
    }

    function getBadge() {
        if (state.periodType === PERIOD.aylik     && state.year === CUR_YEAR         && state.month   === CUR_MONTH)          return 'Bu Ay';
        if (state.periodType === PERIOD.ceyreklik && state.year === CUR_CEYREK_YEAR  && state.quarter === CUR_CEYREK_QUARTER) return 'Bu Çeyrek';
        if (state.periodType === PERIOD.yillik    && state.year === CUR_YILLIK_YEAR)                                          return 'Bu Yıl';
        return '';
    }

    function getDateNumber() {
        if (state.periodType === PERIOD.aylik)     return state.year * 100 + state.month;
        if (state.periodType === PERIOD.ceyreklik) return state.year * 10  + state.quarter;
        return state.year;
    }

    // ── Render ────────────────────────────────────────────────────────────────

    function refreshHeader() {
        $('#scDpLabel').text(getLabel());
        var b = getBadge();
        $('#scDpBadge').text(b).toggle(b.length > 0);
    }

    function buildYearNav(prevOff, nextOff) {
        return '<div class="sc-dp-year-nav">' +
               '<button type="button" class="sc-dp-year-btn" data-dp="prev-year"' + (prevOff ? ' disabled' : '') + '>&#8249;</button>' +
               '<span class="sc-dp-year-label">' + state.panelYear + '</span>' +
               '<button type="button" class="sc-dp-year-btn" data-dp="next-year"' + (nextOff ? ' disabled' : '') + '>&#8250;</button>' +
               '</div>';
    }

    function renderPanel() {
        var html = '';

        if (state.periodType === PERIOD.aylik) {
            // ── Aylık: yıl navigasyonu + 4x3 ay grid (sadece mock'ta olan aylar aktif)
            var panelAvail = _aylikAvailMap[state.panelYear] || {};
            html += buildYearNav(state.panelYear <= MIN_AYLIK_YEAR, state.panelYear >= CUR_YEAR);
            html += '<div class="sc-dp-grid">';
            for (var m = 1; m <= 12; m++) {
                var active   = (state.panelYear === state.year && m === state.month);
                var disabled = !panelAvail[m];
                var cls = 'sc-dp-cell' + (active ? ' sc-dp-active' : '') + (disabled ? ' sc-dp-disabled' : '');
                html += '<div class="' + cls + '"' +
                        (disabled ? '' : ' data-dp-month="' + m + '"') +
                        '>' + _trMonths[m - 1] + '</div>';
            }
            html += '</div>';

        } else if (state.periodType === PERIOD.ceyreklik) {
            // ── Çeyreklik: yıl navigasyonu + 2x2 çeyrek grid (sadece mock'ta olan çeyrekler aktif)
            var panelCeyrekAvail = _ceyrekAvailMap[state.panelYear] || {};
            html += buildYearNav(state.panelYear <= MIN_CEYREK_YEAR, state.panelYear >= CUR_YEAR);
            html += '<div class="sc-dp-grid sc-dp-grid-2">';
            for (var q = 1; q <= 4; q++) {
                var activeQ   = (state.panelYear === state.year && q === state.quarter);
                var disabledQ = !panelCeyrekAvail[q];
                html += '<div class="sc-dp-cell' + (activeQ ? ' sc-dp-active' : '') + (disabledQ ? ' sc-dp-disabled' : '') + '"' +
                        (disabledQ ? '' : ' data-dp-quarter="' + q + '"') +
                        '>' + q + '. Çeyrek</div>';
            }
            html += '</div>';

        } else {
            // ── Yıllık: mock type-3 keyValues'taki benzersiz yıllar (yeniden eskiye)
            html += '<div class="sc-dp-year-list">';
            for (var _yyi = 0; _yyi < _yillikYears.length; _yyi++) {
                var y = _yillikYears[_yyi];
                var activeY = (y === state.year);
                html += '<div class="sc-dp-year-item' + (activeY ? ' sc-dp-active' : '') +
                        '" data-dp-year="' + y + '">' + y + '</div>';
            }
            html += '</div>';
        }

        $('#scDpPanel')
            .html(html)
            .toggleClass('sc-dp-panel--yearly', state.periodType === PERIOD.yillik);
    }

    // ── Panel açma / kapama ───────────────────────────────────────────────────

    function openPanel() {
        state.panelYear = state.year;
        renderPanel();
        $('#scDpPanel').addClass('open');
        $('#scDpTrigger').addClass('open');
        $('#scDpPrev, #scDpNext').hide();
    }

    function closePanel() {
        $('#scDpPanel').removeClass('open');
        $('#scDpTrigger').removeClass('open');
        $('#scDpPrev, #scDpNext').show();
    }

    // ── İleri / Geri navigasyon ───────────────────────────────────────────────

    function isMonthDisabled(year, month) {
        return !(_aylikAvailMap[year] && _aylikAvailMap[year][month]);
    }

    var _yearData      = (typeof getPrimMonitoringPeriodsMock === 'function') ? getPrimMonitoringPeriodsMock(PERIOD.yillik) : null;
    var _yillikKVs     = _yearData ? (_yearData.keyValues || []) : [];

    // Yıllık için benzersiz yıllar listesi (yeniden eskiye) ve yıl→key haritası (mock'tan)
    var _yillikYears      = [];
    var _yillikYearKeyMap = {};
    for (var _yi = 0; _yi < _yillikKVs.length; _yi++) {
        var _yyear = parseInt(_yillikKVs[_yi].value.split(' - ')[0], 10);
        if (!_yillikYearKeyMap[_yyear]) {
            _yillikYears.push(_yyear);
            _yillikYearKeyMap[_yyear] = _yillikKVs[_yi].key;
        }
    }

    var _lastYearKv    = _yillikKVs.length ? _yillikKVs[_yillikKVs.length - 1] : null;
    var MIN_YEAR       = _lastYearKv ? parseInt(_lastYearKv.value, 10) : CUR_YEAR - 4;
    var MAX_YEAR       = CUR_YEAR;

    var _aylikData      = (typeof getPrimMonitoringPeriodsMock === 'function') ? getPrimMonitoringPeriodsMock(PERIOD.aylik) : null;
    var _lastAylikKv    = _aylikData && _aylikData.keyValues && _aylikData.keyValues[_aylikData.keyValues.length - 1];
    var MIN_AYLIK_YEAR  = _lastAylikKv ? parseInt(_lastAylikKv.value, 10) : CUR_YEAR - 4;

    // Aylık için yıl→ay available map (mock'tan)
    var _aylikAvailMap = {};
    var _aylikKVs = _aylikData ? (_aylikData.keyValues || []) : [];
    for (var _ai = 0; _ai < _aylikKVs.length; _ai++) {
        var _ap = _aylikKVs[_ai].value.split(' - ');
        var _ay = parseInt(_ap[0], 10);
        var _am = parseInt(_ap[1].replace(/[^0-9]/g, ''), 10);
        if (!_aylikAvailMap[_ay]) _aylikAvailMap[_ay] = {};
        _aylikAvailMap[_ay][_am] = true;
    }

    var _ceyrekData     = (typeof getPrimMonitoringPeriodsMock === 'function') ? getPrimMonitoringPeriodsMock(PERIOD.ceyreklik) : null;
    var _lastCeyrekKv   = _ceyrekData && _ceyrekData.keyValues && _ceyrekData.keyValues[_ceyrekData.keyValues.length - 1];
    var MIN_CEYREK_YEAR = _lastCeyrekKv ? parseInt(_lastCeyrekKv.value, 10) : CUR_YEAR - 4;

    // Çeyreklik için yıl→çeyrek available map (mock'tan)
    var _ceyrekAvailMap = {};
    var _ceyrekKVs = _ceyrekData ? (_ceyrekData.keyValues || []) : [];
    for (var _ci = 0; _ci < _ceyrekKVs.length; _ci++) {
        var _cp = _ceyrekKVs[_ci].value.split(' - ');
        var _cy = parseInt(_cp[0], 10);
        var _cq = parseInt(_cp[1].replace(/[^0-9]/g, ''), 10);
        if (!_ceyrekAvailMap[_cy]) _ceyrekAvailMap[_cy] = {};
        _ceyrekAvailMap[_cy][_cq] = _ceyrekKVs[_ci].key;
    }

    function refreshNavBtns() {
        $('#scDpPrev').prop('disabled', false);
        $('#scDpNext').prop('disabled', false);
    }

    function navigate(dir) {
        if (state.periodType === PERIOD.aylik) {
            var nextMonth = state.month + dir;
            var nextYear  = state.year;
            if (nextMonth < 1)  { nextMonth = 12; nextYear--; }
            if (nextMonth > 12) { nextMonth = 1;  nextYear++; }
            if (isMonthDisabled(nextYear, nextMonth)) return;
            state.month = nextMonth;
            state.year  = nextYear;
        } else if (state.periodType === PERIOD.ceyreklik) {
            var nextQ  = state.quarter + dir;
            var nextYc = state.year;
            if (nextQ < 1) { nextQ = 4; nextYc--; }
            if (nextQ > 4) { nextQ = 1; nextYc++; }
            if (!(_ceyrekAvailMap[nextYc] && _ceyrekAvailMap[nextYc][nextQ])) return;
            state.quarter = nextQ;
            state.year    = nextYc;
        } else {
            // _yillikYears yeniden eskiye sıralı; dir=-1 (önceki=eski) → idx artar
            var _curYIdx  = _yillikYears.indexOf(state.year);
            var _nextYIdx = _curYIdx - dir;
            if (_nextYIdx < 0 || _nextYIdx >= _yillikYears.length) return;
            state.year = _yillikYears[_nextYIdx];
        }
        refreshHeader();
        refreshNavBtns();
        notify();
        if (state.periodType === PERIOD.aylik) {
            var _navKv = null;
            for (var _ni = 0; _ni < _aylikKVs.length; _ni++) {
                var _np = _aylikKVs[_ni].value.split(' - ');
                if (parseInt(_np[0], 10) === state.year &&
                    parseInt(_np[1].replace(/[^0-9]/g, ''), 10) === state.month) {
                    _navKv = _aylikKVs[_ni];
                    break;
                }
            }
        }
    }

    // ── Event yayımla ─────────────────────────────────────────────────────────

    function notify() {
        $(document).trigger('sc:dateChanged', {
            periodType : state.periodType,
            dateNumber : getDateNumber(),
            year       : state.year,
            month      : state.month,
            quarter    : state.quarter
        });
    }

    // ── Event bağlantıları ────────────────────────────────────────────────────

    // Trigger tıklama → panel aç/kapat
    $('#scDpTrigger').on('click', function (e) {
        e.stopPropagation();
        $('#scDpPanel').hasClass('open') ? closePanel() : openPanel();
    });

    // Dışarı tıklama → kapat
    $(document).on('click.sc-dp', function (e) {
        if (!$(e.target).closest('#scDatePicker').length) closePanel();
    });

    // Önceki / Sonraki butonları
    $('#scDpPrev').on('click', function () { navigate(-1); });
    $('#scDpNext').on('click', function () { navigate(1);  });

    // Panel içi yıl navigasyonu
    $(document).on('click', '[data-dp="prev-year"]', function (e) {
        e.stopPropagation();
        if (state.periodType === PERIOD.aylik    && state.panelYear <= MIN_AYLIK_YEAR)  return;
        if (state.periodType === PERIOD.ceyreklik && state.panelYear <= MIN_CEYREK_YEAR) return;
        state.panelYear--;
        renderPanel();
    });
    $(document).on('click', '[data-dp="next-year"]', function (e) {
        e.stopPropagation();
        if (state.periodType === PERIOD.aylik    && state.panelYear >= CUR_YEAR) return;
        if (state.periodType === PERIOD.ceyreklik && state.panelYear >= CUR_CEYREK_YEAR) return;
        state.panelYear++;
        renderPanel();
    });

    // Ay seçimi
    $(document).on('click', '[data-dp-month]', function (e) {
        e.stopPropagation();
        state.month = +$(this).data('dp-month');
        state.year  = state.panelYear;
        closePanel(); refreshHeader(); notify();
        var matchedKv = null;
        for (var _mi = 0; _mi < _aylikKVs.length; _mi++) {
            var _mp = _aylikKVs[_mi].value.split(' - ');
            if (parseInt(_mp[0], 10) === state.year &&
                parseInt(_mp[1].replace(/[^0-9]/g, ''), 10) === state.month) {
                matchedKv = _aylikKVs[_mi];
                break;
            }
        }
    });

    // Çeyrek seçimi
    $(document).on('click', '[data-dp-quarter]', function (e) {
        e.stopPropagation();
        state.quarter = +$(this).data('dp-quarter');
        state.year    = state.panelYear;
        closePanel(); refreshHeader(); notify();
        var cKey = _ceyrekAvailMap[state.year] && _ceyrekAvailMap[state.year][state.quarter];
    });

    // Yıl seçimi (yıllık mod)
    $(document).on('click', '[data-dp-year]', function (e) {
        e.stopPropagation();
        state.year = +$(this).data('dp-year');
        closePanel(); refreshHeader(); notify();
    });

    // Dönem tipi değişimi (Aylık / Çeyreklik / Yıllık butonlarından)
    $('#scPeriod').on('click', '.period-btn', function () {
        var map  = { aylik: PERIOD.aylik, ceyreklik: PERIOD.ceyreklik, yillik: PERIOD.yillik };
        var type = map[$(this).data('period')];
        if (!type || type === state.periodType) return;

        state.periodType = type;
        // Seçim güncel döneme sıfırla (her dönem tipi kendi ilk keyValue'suna döner)
        if (type === PERIOD.ceyreklik) {
            state.year    = CUR_CEYREK_YEAR;
            state.quarter = CUR_CEYREK_QUARTER;
        } else if (type === PERIOD.yillik) {
            state.year    = CUR_YILLIK_YEAR;
        } else {
            state.year    = CUR_YEAR;
            state.month   = CUR_MONTH;
            state.quarter = CUR_QUARTER;
        }

        closePanel();
        refreshHeader();
        refreshNavBtns();
        notify();
    });

    // ── Başlat ────────────────────────────────────────────────────────────────
    refreshHeader();
    refreshNavBtns();
    notify();
});
