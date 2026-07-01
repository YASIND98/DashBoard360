// Score Card — Date Picker
// Dönem tipine (aylik=1, ceyreklik=2, yillik=3) göre panel gösterir.
// Dönem listesi prim-monitoring/periods servisinden gelir; servis TEK tip döndürür
// (periodTypes=N -> sadece o tipin {keyValues:[...]}'i). Bu yüzden yalnızca AKTİF tip yüklenir;
// dönem tipi değişince yeni tip yeniden yüklenir.
// Seçim değişince: $(document).trigger('sc:dateChanged', { dateNumber })  // dateNumber = servis "key"i.

$(function () {
    'use strict';

    if (!$('#scDatePicker').length) return;

    var PERIOD = PUPA_PERIOD_TYPE;

    // Aktif dönemin parse edilmiş verisi. (Her tip değişiminde yeniden yüklenir.)
    //   { type, byYear: { yıl: { alt: key } }, years: [azalan benzersiz], first: { year, sub } }
    //   alt = aylık:ay(1-12), çeyreklik:çeyrek(1-4), yıllık:0
    var D = null;
    // sub = aktif tipe göre ay / çeyrek / 0 (yıllık).
    var state = { year: 0, sub: 0, panelYear: 0 };

    // ── Servis dönemini parse et ────────────────────────────────────────────────
    function periodKVs(type) {
        return ((typeof getPrimMonitoringPeriodsMock === 'function' ? getPrimMonitoringPeriodsMock(type) : null) || {}).keyValues || [];
    }

    // value formatı: "YIL - AYn" / "YIL - CEYREKn"; yıllıkta yıl bazında İLK key alınır.
    function parsePeriods(type) {
        var isYear = (type === PERIOD.yillik);
        var byYear = {}, years = [], first = null;
        periodKVs(type).forEach(function (it) {
            var p = String(it.value).split(' - ');
            var year = parseInt(p[0], 10);
            var sub = isYear ? 0 : parseInt((p[1] || '').replace(/[^0-9]/g, ''), 10);
            if (!byYear[year]) { byYear[year] = {}; years.push(year); }
            if (byYear[year][sub] == null) byYear[year][sub] = it.key;   // aynı alt için ilk key
            if (!first) first = { year: year, sub: sub };
        });
        return { type: type, byYear: byYear, years: years, first: first || { year: 0, sub: 0 } };
    }

    // Dönem tipini yükle + seçimi servisten dönen İLK objeye sıfırla.
    function loadPeriod(type) {
        D = parsePeriods(type);
        state.year = D.first.year;
        state.sub  = D.first.sub;
        state.panelYear = D.first.year;
    }

    // ── Etiket / Badge / dateNumber ─────────────────────────────────────────────
    function shortMonth(m) { var n = _trMonths[m - 1]; return n.length <= 4 ? n : n.slice(0, 3); }
    // Takvim çeyreği: q -> [(q-1)*3+1 .. +3]  (Q1=Oca-Şub-Mar, Q2=Nis-May-Haz ...)
    function quarterMonths(q) { return [(q - 1) * 3 + 1, (q - 1) * 3 + 2, (q - 1) * 3 + 3]; }

    function isFirstSelected() { return state.year === D.first.year && state.sub === D.first.sub; }

    function getLabel() {
        if (D.type === PERIOD.aylik) return _trMonths[state.sub - 1] + ' ' + state.year;
        if (D.type === PERIOD.ceyreklik) {
            var months = quarterMonths(state.sub).map(shortMonth).join(' - ') + ' ' + state.year;
            return isFirstSelected() ? months : months + '  ' + state.sub + '. Çeyrek';
        }
        return '' + state.year;
    }

    function getBadge() {
        if (!isFirstSelected()) return '';
        return D.type === PERIOD.aylik ? 'Bu Ay' : (D.type === PERIOD.ceyreklik ? 'Bu Çeyrek' : 'Bu Yıl');
    }

    // Seçili dönemin servis dateNumber'ı (key).
    function getDateNumber() {
        return (D.byYear[state.year] && D.byYear[state.year][state.sub]) || -1;
    }

    function refreshHeader() {
        $('#scDpLabel').text(getLabel());
        var b = getBadge();
        $('#scDpBadge').text(b).toggle(b.length > 0);
        // Üst oklar: gidilecek dönem servis dışıysa disabled (panel yıl oklarındaki gibi).
        $('#scDpPrev').prop('disabled', !nextSelection(-1));
        $('#scDpNext').prop('disabled', !nextSelection(1));
    }

    // ── Panel render ────────────────────────────────────────────────────────────
    function yearNav() {
        var i = D.years.indexOf(state.panelYear);          // years azalan: [0]=en yeni, [son]=en eski
        var prevOff = (i < 0) || (i >= D.years.length - 1);  // daha eski yıl yok
        var nextOff = (i <= 0);                              // daha yeni yıl yok
        return '<div class="sc-dp-year-nav">' +
               '<button type="button" class="sc-dp-year-btn" data-dp="prev-year"' + (prevOff ? ' disabled' : '') + '>&#8249;</button>' +
               '<span class="sc-dp-year-label">' + state.panelYear + '</span>' +
               '<button type="button" class="sc-dp-year-btn" data-dp="next-year"' + (nextOff ? ' disabled' : '') + '>&#8250;</button>' +
               '</div>';
    }

    function gridCell(label, dataAttr, active, disabled) {
        return '<div class="sc-dp-cell' + (active ? ' sc-dp-active' : '') + (disabled ? ' sc-dp-disabled' : '') + '"' +
               (disabled ? '' : ' ' + dataAttr) + '>' + label + '</div>';
    }

    function renderPanel() {
        var html = '';
        if (D.type === PERIOD.aylik) {
            var availM = D.byYear[state.panelYear] || {};
            html += yearNav() + '<div class="sc-dp-grid">';
            for (var m = 1; m <= 12; m++) {
                html += gridCell(_trMonths[m - 1], 'data-dp-month="' + m + '"',
                                 state.panelYear === state.year && m === state.sub, availM[m] == null);
            }
            html += '</div>';

        } else if (D.type === PERIOD.ceyreklik) {
            var availQ = D.byYear[state.panelYear] || {};
            html += yearNav() + '<div class="sc-dp-grid sc-dp-grid-2">';
            for (var q = 1; q <= 4; q++) {
                html += gridCell(q + '. Çeyrek', 'data-dp-quarter="' + q + '"',
                                 state.panelYear === state.year && q === state.sub, availQ[q] == null);
            }
            html += '</div>';

        } else {
            // Yıllık: yalnızca servisten dönen benzersiz yıllar.
            html += '<div class="sc-dp-year-list">';
            D.years.forEach(function (y) {
                html += '<div class="sc-dp-year-item' + (y === state.year ? ' sc-dp-active' : '') + '" data-dp-year="' + y + '">' + y + '</div>';
            });
            html += '</div>';
        }
        $('#scDpPanel').html(html).toggleClass('sc-dp-panel--yearly', D.type === PERIOD.yillik);
    }

    // ── Panel açma / kapama ─────────────────────────────────────────────────────
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

    // ── İleri / geri (header okları): bir sonraki dönem servis dışıysa durur ──────
    // dir yönündeki bir sonraki dönem ({year, sub}); servis dışıysa null.
    function nextSelection(dir) {
        if (D.type === PERIOD.yillik) {
            var yi = D.years.indexOf(state.year) - dir;   // azalan: önceki(eski)=index+1
            return (yi >= 0 && yi < D.years.length) ? { year: D.years[yi], sub: 0 } : null;
        }
        var max = (D.type === PERIOD.aylik) ? 12 : 4;
        var y = state.year, s = state.sub + dir;
        if (s < 1) { s = max; y--; } else if (s > max) { s = 1; y++; }
        return (D.byYear[y] && D.byYear[y][s] != null) ? { year: y, sub: s } : null;
    }

    function navigate(dir) {
        var t = nextSelection(dir);
        if (!t) return;
        state.year = t.year; state.sub = t.sub;
        refreshHeader();
        notify();
    }

    // Panel içi yıl gezintisi (sadece servisten dönen yıllar arasında).
    function shiftPanelYear(dir) {
        var i = D.years.indexOf(state.panelYear) - dir;   // prev(-1)->eski(index+1), next(+1)->yeni
        if (i < 0 || i >= D.years.length) return;
        state.panelYear = D.years[i];
        renderPanel();
    }

    // ── Event yayımla ───────────────────────────────────────────────────────────
    function notify() {
        var dateNumber = getDateNumber();
        console.log('Date picker dateNumber:', dateNumber);
        $(document).trigger('sc:dateChanged', { dateNumber: dateNumber });
    }

    // ── Event bağlantıları ──────────────────────────────────────────────────────
    $('#scDpTrigger').on('click', function (e) {
        e.stopPropagation();
        $('#scDpPanel').hasClass('open') ? closePanel() : openPanel();
    });

    $(document).on('click.sc-dp', function (e) {
        if (!$(e.target).closest('#scDatePicker').length) closePanel();
    });

    $('#scDpPrev').on('click', function () { navigate(-1); });
    $('#scDpNext').on('click', function () { navigate(1);  });

    $(document).on('click', '[data-dp="prev-year"]', function (e) { e.stopPropagation(); shiftPanelYear(-1); });
    $(document).on('click', '[data-dp="next-year"]', function (e) { e.stopPropagation(); shiftPanelYear(1);  });

    $(document).on('click', '[data-dp-month], [data-dp-quarter]', function (e) {
        e.stopPropagation();
        state.sub  = +($(this).data('dp-month') != null ? $(this).data('dp-month') : $(this).data('dp-quarter'));
        state.year = state.panelYear;
        closePanel(); refreshHeader(); notify();
    });

    $(document).on('click', '[data-dp-year]', function (e) {
        e.stopPropagation();
        state.year = +$(this).data('dp-year');
        closePanel(); refreshHeader(); notify();
    });

    // Dönem tipi değişimi: yeni tipi yükle (kendi İLK dönemine sıfırlanır).
    $('#scPeriod').on('click', '.period-btn', function () {
        var map  = { aylik: PERIOD.aylik, ceyreklik: PERIOD.ceyreklik, yillik: PERIOD.yillik };
        var type = map[$(this).data('period')];
        if (!type || (D && type === D.type)) return;
        loadPeriod(type);
        closePanel(); refreshHeader(); notify();
    });

    // ── Başlat: aktif tip aylık ─────────────────────────────────────────────────
    loadPeriod(PERIOD.aylik);
    refreshHeader();
    notify();
});
