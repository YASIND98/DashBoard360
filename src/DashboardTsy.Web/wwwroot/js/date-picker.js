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

    // ── Tarih yardımcıları ──
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

    // ── Header (etiket + rozet + ok durumları) ──
    function refreshHeader() {
        if (readOnly) {
            $label.text(staticLabel || fmtLong(selected));
            $badge.hide();
            return;
        }
        var isToday = sameDay(selected, today());
        $label.text(fmtLong(selected));
        $badge.text(isToday ? 'Bugün' : '').toggle(isToday);
        $prev.prop('disabled', outOfRange(addDays(selected, -1)));
        $next.prop('disabled', outOfRange(addDays(selected, 1)));
    }

    // ── Panel render (ay gezinme + hafta günleri + 6 haftalık gün ızgarası) ──
    function monthNav() {
        var prevOff = minDate && new Date(panel.year, panel.month, 0) < minDate;      // önceki ayın son günü < min
        var nextOff = maxDate && new Date(panel.year, panel.month + 1, 1) > maxDate;   // sonraki ayın ilk günü > max
        return '<div class="dp-month-nav">' +
            '<button type="button" class="dp-month-btn" data-dp="prev-month"' + (prevOff ? ' disabled' : '') + '>&#8249;</button>' +
            '<span class="dp-month-label">' + _trMonths[panel.month] + ' ' + panel.year + '</span>' +
            '<button type="button" class="dp-month-btn" data-dp="next-month"' + (nextOff ? ' disabled' : '') + '>&#8250;</button>' +
            '</div>';
    }

    function renderPanel() {
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

    // ── Panel aç/kapat ──
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

    // ── Seçimi uygula + yayımla ──
    function setSelected(d, notify) {
        selected = startOfDay(d);
        refreshHeader();
        if (notify) {
            var iso = toISO(selected);
            if (typeof onChange === 'function') onChange(iso, selected);
            $(document).trigger('datepicker:change', { date: selected, iso: iso });
        }
    }

    // Header okları: günü ±1 kaydır (aralık dışıysa durur).
    function navigate(dir) {
        var t = addDays(selected, dir);
        if (!outOfRange(t)) setSelected(t, true);
    }

    // Panel içi ay gezintisi (seçimi değiştirmez).
    function shiftPanelMonth(dir) {
        var m = panel.month + dir, y = panel.year;
        if (m < 0) { m = 11; y--; } else if (m > 11) { m = 0; y++; }
        panel.year = y; panel.month = m;
        renderPanel();
    }

    // ── Eventler ──
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
    $(document).on('click', '[data-dp-date]', function (e) {
        e.stopPropagation();
        setSelected(toDate($(this).data('dp-date')), true);
        closePanel();
    });

    // ── Dış API — opts: { initial, min, max, onChange } ──
    window.DatePicker = {
        init: function (opts) {
            opts = opts || {};
            minDate = toDate(opts.min);
            maxDate = toDate(opts.max);
            onChange = typeof opts.onChange === 'function' ? opts.onChange : null;
            readOnly = !!opts.readOnly;          // panel açılmaz, oklar gizli
            staticLabel = opts.label || null;    // sabit etiket (ör. "Haziran 2026")
            $root.toggleClass('dp-readonly', readOnly);
            setSelected(toDate(opts.initial) || today(), false);
        },
        setDate: function (d, notify) { setSelected(toDate(d) || today(), !!notify); },
        getDate: function () { return selected ? new Date(selected) : null; },
        getISO: function () { return selected ? toISO(selected) : null; }
    };

    // Varsayılan: bugün (ekran init ile değiştirir).
    setSelected(today(), false);
});
