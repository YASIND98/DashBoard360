$(function () {
    'use strict';

    if (!$('#datePicker').length) return;

    var WEEKDAYS = ['Pzt', 'Salı', 'Çrş', 'Prş', 'Cuma', 'Cmt', 'Pzr'];

    var $root = $('#datePicker'),
        $trigger = $('#dpTrigger'),
        $panel = $('#dpPanel'),
        $label = $('#dpLabel'),
        $badge = $('#dpBadge'),
        $prev = $('#dpPrev'),
        $next = $('#dpNext');

    var selected = null;
    var panel = { year: 0, month: 0 };
    var minDate = null, maxDate = null;
    var onChange = null;
    var readOnly = false, staticLabel = null;
    var monthMode = false;

    function startOfDay(d) { return new Date(d.getFullYear(), d.getMonth(), d.getDate()); }
    function today() { return startOfDay(new Date()); }
    function addDays(d, n) { return new Date(d.getFullYear(), d.getMonth(), d.getDate() + n); }
    function sameDay(a, b) { return a.getTime() === b.getTime(); }   // ikisi de gün başı
    function pad2(n) { return (n < 10 ? '0' : '') + n; }
    function toISO(d) { return d.getFullYear() + '-' + pad2(d.getMonth() + 1) + '-' + pad2(d.getDate()); }
    function toDate(v) {
        if (v == null) return null;
        if (v instanceof Date) return startOfDay(v);
        var p = String(v).split('-');
        return p.length === 3 ? new Date(+p[0], +p[1] - 1, +p[2]) : null;
    }
    function fmtLong(d) { return d.getDate() + ' ' + _trMonths[d.getMonth()] + ' ' + d.getFullYear(); }
    function outOfRange(d) { return (minDate && d < minDate) || (maxDate && d > maxDate); }

    // ── Ay modu yardımcıları ──
    function endOfMonth(y, m) { return new Date(y, m + 1, 0); }
    // Seçilen ayın rapor tarihi: ayın son günü; içinde bulunulan ayda son veri günü (max).
    function monthReportDate(y, m) {
        var last = endOfMonth(y, m);
        return (maxDate && last > maxDate) ? new Date(maxDate) : last;
    }
    // Ayın tamamı aralık dışındaysa seçilemez.
    function monthOutOfRange(y, m) {
        return (maxDate && new Date(y, m, 1) > maxDate) || (minDate && endOfMonth(y, m) < minDate);
    }
    // Rozetin ("Bu Ay") referans ayı: max varsa o, yoksa bugün.
    function isCurrentMonth(d) {
        var ref = maxDate || today();
        return d.getFullYear() === ref.getFullYear() && d.getMonth() === ref.getMonth();
    }
    // dir yönündeki bir sonraki ay ({year, month}); aralık dışıysa null.
    function shiftMonth(dir) {
        var y = selected.getFullYear(), m = selected.getMonth() + dir;
        if (m < 0) { m = 11; y--; } else if (m > 11) { m = 0; y++; }
        return monthOutOfRange(y, m) ? null : { year: y, month: m };
    }

    function refreshHeader() {
        if (readOnly) {
            $label.text(staticLabel || fmtLong(selected));
            $badge.hide();
            return;
        }
        if (monthMode) {
            var isCurrent = isCurrentMonth(selected);
            $label.text(_trMonths[selected.getMonth()] + ' ' + selected.getFullYear());
            $badge.text(isCurrent ? 'Bu Ay' : '').toggle(isCurrent);
            $prev.prop('disabled', !shiftMonth(-1));
            $next.prop('disabled', !shiftMonth(1));
            return;
        }
        var isToday = sameDay(selected, today());
        $label.text(fmtLong(selected));
        $badge.text(isToday ? 'Bugün' : '').toggle(isToday);
        $prev.prop('disabled', outOfRange(addDays(selected, -1)));
        $next.prop('disabled', outOfRange(addDays(selected, 1)));
    }

    function monthNav() {
        var prevOff = minDate && new Date(panel.year, panel.month, 0) < minDate;      // önceki ayın son günü < min
        var nextOff = maxDate && new Date(panel.year, panel.month + 1, 1) > maxDate;   // sonraki ayın ilk günü > max
        return '<div class="dp-month-nav">' +
            '<button type="button" class="dp-month-btn" data-dp="prev-month"' + (prevOff ? ' disabled' : '') + '>&#8249;</button>' +
            '<span class="dp-month-label">' + _trMonths[panel.month] + ' ' + panel.year + '</span>' +
            '<button type="button" class="dp-month-btn" data-dp="next-month"' + (nextOff ? ' disabled' : '') + '>&#8250;</button>' +
            '</div>';
    }

    // Ay modundaki yıl gezinme satırı (‹ 2026 ›).
    function yearNav() {
        var prevOff = minDate && new Date(panel.year - 1, 11, 31) < minDate;   // önceki yılın son günü < min
        var nextOff = maxDate && new Date(panel.year + 1, 0, 1) > maxDate;     // sonraki yılın ilk günü > max
        return '<div class="dp-month-nav">' +
            '<button type="button" class="dp-month-btn" data-dp="prev-year"' + (prevOff ? ' disabled' : '') + '>&#8249;</button>' +
            '<span class="dp-month-label">' + panel.year + '</span>' +
            '<button type="button" class="dp-month-btn" data-dp="next-year"' + (nextOff ? ' disabled' : '') + '>&#8250;</button>' +
            '</div>';
    }

    function renderMonthPanel() {
        var html = yearNav() + '<div class="dp-months">';
        for (var m = 0; m < 12; m++) {                                   // data-dp-month: 0-11
            var off = monthOutOfRange(panel.year, m);
            var active = panel.year === selected.getFullYear() && m === selected.getMonth();
            var cls = 'dp-month' + (active ? ' dp-active' : '') + (off ? ' dp-disabled' : '');
            html += '<div class="' + cls + '"' + (off ? '' : ' data-dp-month="' + m + '"') + '>' + _trMonths[m] + '</div>';
        }
        $panel.html(html + '</div>');
    }

    function renderPanel() {
        if (monthMode) { renderMonthPanel(); return; }

        var first = new Date(panel.year, panel.month, 1);
        var gridStart = addDays(first, -((first.getDay() + 6) % 7));   // ızgara Pazartesi'den başlar (önceki aya taşabilir)

        var html = monthNav() + '<div class="dp-weekdays">';
        WEEKDAYS.forEach(function (w) { html += '<div class="dp-weekday">' + w + '</div>'; });
        html += '</div><div class="dp-grid">';
        for (var i = 0; i < 42; i++) {
            var d = addDays(gridStart, i);
            var cls = 'dp-day' +
                (d.getMonth() !== panel.month ? ' dp-muted' : '') +
                (sameDay(d, selected) ? ' dp-active' : '') +
                (outOfRange(d) ? ' dp-disabled' : '');
            var attr = outOfRange(d) ? '' : ' data-dp-date="' + toISO(d) + '"';
            html += '<div class="' + cls + '"' + attr + '>' + d.getDate() + '</div>';
        }
        $panel.html(html + '</div>');
    }

    function openPanel() {
        panel.year = selected.getFullYear();
        panel.month = selected.getMonth();
        renderPanel();
        $panel.addClass('open');
        $trigger.addClass('open');
        $root.addClass('dp-panel-open');   // header okları CSS ile gizlenir
    }
    function closePanel() {
        $panel.removeClass('open');
        $trigger.removeClass('open');
        $root.removeClass('dp-panel-open');
    }

    // Ay modunda gelen tarih, ait olduğu ayın rapor tarihine normalize edilir.
    function setSelected(d, notify) {
        d = startOfDay(d);
        selected = monthMode ? monthReportDate(d.getFullYear(), d.getMonth()) : d;
        refreshHeader();
        if (notify && onChange) onChange(toISO(selected), selected);
    }

    // Header okları: gün modunda günü, ay modunda ayı ±1 kaydırır (aralık dışıysa durur).
    function navigate(dir) {
        if (monthMode) {
            var t = shiftMonth(dir);
            if (t) setSelected(monthReportDate(t.year, t.month), true);
            return;
        }
        var d = addDays(selected, dir);
        if (!outOfRange(d)) setSelected(d, true);
    }

    // Panel içi ay gezintisi (seçimi değiştirmez).
    function shiftPanelMonth(dir) {
        var m = panel.month + dir, y = panel.year;
        if (m < 0) { m = 11; y--; } else if (m > 11) { m = 0; y++; }
        panel.year = y; panel.month = m;
        renderPanel();
    }

    // Ay modunda panel içi yıl gezintisi (seçimi değiştirmez).
    function shiftPanelYear(dir) {
        panel.year += dir;
        renderPanel();
    }

    $trigger.on('click', function (e) {
        e.stopPropagation();
        if (readOnly) return;
        $panel.hasClass('open') ? closePanel() : openPanel();
    });
    $(document).on('click.date-picker', function (e) {
        if (!$(e.target).closest('#datePicker').length) closePanel();
    });
    $prev.on('click', function () { if (!readOnly) navigate(-1); });
    $next.on('click', function () { if (!readOnly) navigate(1); });
    $(document).on('click', '[data-dp="prev-month"]', function (e) { e.stopPropagation(); shiftPanelMonth(-1); });
    $(document).on('click', '[data-dp="next-month"]', function (e) { e.stopPropagation(); shiftPanelMonth(1); });
    $(document).on('click', '[data-dp="prev-year"]', function (e) { e.stopPropagation(); shiftPanelYear(-1); });
    $(document).on('click', '[data-dp="next-year"]', function (e) { e.stopPropagation(); shiftPanelYear(1); });
    $(document).on('click', '[data-dp-date]', function (e) {
        e.stopPropagation();
        setSelected(toDate($(this).data('dp-date')), true);
        closePanel();
    });
    $(document).on('click', '[data-dp-month]', function (e) {
        e.stopPropagation();
        setSelected(monthReportDate(panel.year, +$(this).data('dp-month')), true);
        closePanel();
    });

    // ── Dış API — opts: { mode, initial, min, max, onChange } ──
    window.DatePicker = {
        init: function (opts) {
            opts = opts || {};
            monthMode = opts.mode === 'month';
            minDate = toDate(opts.min);
            maxDate = toDate(opts.max);
            onChange = typeof opts.onChange === 'function' ? opts.onChange : null;
            readOnly = !!opts.readOnly;          // panel açılmaz, oklar gizli
            staticLabel = opts.label || null;    // sabit etiket (ör. "Haziran 2026")
            $root.toggleClass('dp-readonly', readOnly);
            setSelected(toDate(opts.initial) || today(), false);
        }
    };

    // Varsayılan: bugün (ekran init ile değiştirir).
    setSelected(today(), false);
});
