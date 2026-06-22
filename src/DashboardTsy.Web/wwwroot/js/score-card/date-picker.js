// Score Card — Date Picker
// Dönem tipine (aylik=1, ceyreklik=2, yillik=3) göre farklı picker paneli gösterir.
// Seçim değişince $(document).trigger('sc:dateChanged', { periodType, dateNumber, year, month, quarter })

$(function () {
    'use strict';

    if (!$('#scDatePicker').length) return;

    var MONTHS = [
        'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
        'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'
    ];

    var today       = new Date();
    var CUR_YEAR    = today.getFullYear();
    var CUR_MONTH   = today.getMonth() + 1;         // 1–12
    var CUR_QUARTER = Math.ceil(CUR_MONTH / 3);     // 1–4

    // PUPA_PERIOD_TYPE: aylik=1, ceyreklik=2, yillik=3
    var PERIOD = (typeof PUPA_PERIOD_TYPE !== 'undefined')
        ? PUPA_PERIOD_TYPE
        : { aylik: 1, ceyreklik: 2, yillik: 3 };

    var state = {
        periodType : PERIOD.aylik,
        year       : CUR_YEAR,
        month      : CUR_MONTH,
        quarter    : CUR_QUARTER,
        panelYear  : CUR_YEAR   // yıl gezintisi için panel içi yıl
    };

    // ── Label / Badge ─────────────────────────────────────────────────────────

    function getLabel() {
        if (state.periodType === PERIOD.aylik)     return MONTHS[state.month - 1] + ' ' + state.year;
        if (state.periodType === PERIOD.ceyreklik) return state.quarter + '. Çeyrek';
        return '' + state.year;
    }

    function getBadge() {
        if (state.periodType === PERIOD.aylik     && state.year === CUR_YEAR && state.month   === CUR_MONTH)   return 'Bu Ay';
        if (state.periodType === PERIOD.ceyreklik && state.year === CUR_YEAR && state.quarter === CUR_QUARTER) return 'Bu Çeyrek';
        if (state.periodType === PERIOD.yillik    && state.year === CUR_YEAR)                                  return 'Bu Yıl';
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
            // ── Aylık: yıl navigasyonu + 4x3 ay grid
            html += buildYearNav(state.panelYear <= MIN_AYLIK_YEAR, state.panelYear >= CUR_YEAR);
            html += '<div class="sc-dp-grid">';
            for (var m = 1; m <= 12; m++) {
                var active   = (state.panelYear === state.year && m === state.month);
                var disabled = (state.panelYear > CUR_YEAR) ||
                               (state.panelYear === CUR_YEAR && m > CUR_MONTH);
                var cls = 'sc-dp-cell' + (active ? ' sc-dp-active' : '') + (disabled ? ' sc-dp-disabled' : '');
                html += '<div class="' + cls + '"' +
                        (disabled ? '' : ' data-dp-month="' + m + '"') +
                        '>' + MONTHS[m - 1] + '</div>';
            }
            html += '</div>';

        } else if (state.periodType === PERIOD.ceyreklik) {
            // ── Çeyreklik: yıl navigasyonu + 2x2 çeyrek grid
            html += buildYearNav(state.panelYear <= MIN_CEYREK_YEAR, state.panelYear >= CUR_YEAR);
            html += '<div class="sc-dp-grid sc-dp-grid-2">';
            for (var q = 1; q <= 4; q++) {
                var activeQ = (state.panelYear === state.year && q === state.quarter);
                html += '<div class="sc-dp-cell' + (activeQ ? ' sc-dp-active' : '') +
                        '" data-dp-quarter="' + q + '">' + q + '. Çeyrek</div>';
            }
            html += '</div>';

        } else {
            // ── Yıllık: CUR_YEAR → MIN_YEAR (type-3 keyValues sırası: yeniden eskiye)
            html += '<div class="sc-dp-year-list">';
            for (var y = CUR_YEAR; y >= MIN_YEAR; y--) {
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
        return year > CUR_YEAR || (year === CUR_YEAR && month > CUR_MONTH);
    }

    var _yearData      = (typeof getPrimMonitoringPeriodsMock === 'function') ? getPrimMonitoringPeriodsMock(PERIOD.yillik) : null;
    var _lastYearKv    = _yearData && _yearData.keyValues && _yearData.keyValues[_yearData.keyValues.length - 1];
    var MIN_YEAR       = _lastYearKv ? parseInt(_lastYearKv.value, 10) : CUR_YEAR - 4;
    var MAX_YEAR       = CUR_YEAR;

    var _aylikData      = (typeof getPrimMonitoringPeriodsMock === 'function') ? getPrimMonitoringPeriodsMock(PERIOD.aylik) : null;
    var _lastAylikKv    = _aylikData && _aylikData.keyValues && _aylikData.keyValues[_aylikData.keyValues.length - 1];
    var MIN_AYLIK_YEAR  = _lastAylikKv ? parseInt(_lastAylikKv.value, 10) : CUR_YEAR - 4;

    var _ceyrekData     = (typeof getPrimMonitoringPeriodsMock === 'function') ? getPrimMonitoringPeriodsMock(PERIOD.ceyreklik) : null;
    var _lastCeyrekKv   = _ceyrekData && _ceyrekData.keyValues && _ceyrekData.keyValues[_ceyrekData.keyValues.length - 1];
    var MIN_CEYREK_YEAR = _lastCeyrekKv ? parseInt(_lastCeyrekKv.value, 10) : CUR_YEAR - 4;

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
            state.quarter = nextQ;
            state.year    = nextYc;
        } else {
            state.year += dir;
        }
        refreshHeader();
        refreshNavBtns();
        notify();
        console.log(state.month);
        console.log(state.year);
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
        if (state.periodType === PERIOD.ceyreklik && state.panelYear >= CUR_YEAR) return;
        state.panelYear++;
        renderPanel();
    });

    // Ay seçimi
    $(document).on('click', '[data-dp-month]', function (e) {
        e.stopPropagation();
        state.month = +$(this).data('dp-month');
        state.year  = state.panelYear;
        closePanel(); refreshHeader(); notify();
        console.log(state.month);
        console.log(state.year);
    });

    // Çeyrek seçimi
    $(document).on('click', '[data-dp-quarter]', function (e) {
        e.stopPropagation();
        state.quarter = +$(this).data('dp-quarter');
        state.year    = state.panelYear;
        closePanel(); refreshHeader(); notify();
        console.log(state.quarter);
        console.log(state.year);
    });

    // Yıl seçimi (yıllık mod)
    $(document).on('click', '[data-dp-year]', function (e) {
        e.stopPropagation();
        state.year = +$(this).data('dp-year');
        closePanel(); refreshHeader(); notify();
        console.log(state.year);
    });

    // Dönem tipi değişimi (Aylık / Çeyreklik / Yıllık butonlarından)
    $('#scPeriod').on('click', '.period-btn', function () {
        var map  = { aylik: PERIOD.aylik, ceyreklik: PERIOD.ceyreklik, yillik: PERIOD.yillik };
        var type = map[$(this).data('period')];
        if (!type || type === state.periodType) return;

        state.periodType = type;
        // Seçim güncel döneme sıfırla
        state.year    = CUR_YEAR;
        state.month   = CUR_MONTH;
        state.quarter = CUR_QUARTER;

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
