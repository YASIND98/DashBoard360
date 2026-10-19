window.ScoreCardColumnPicker = (function () {

    var COLUMNS_ICON = '<img class="sc-colpick-btn-icon" src="/images/column-settings.svg" alt="" />';
    var CHECKBOX_ICON = '<img class="sc-colpick-box-off" src="/images/checkbox.svg" alt="" />' +
                     '<img class="sc-colpick-box-on" src="/images/selected-checkbox.svg" alt="" />';
    var LOCK_ICON = '<img src="/images/lock.svg" alt="" />';
    var DONE_ICON = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M7.75 12L10.58 14.83L16.25 9.17"/></svg>';
    var HANDLE_ICON = '<img src="/images/drag-handle.svg" alt="" draggable="false" />';

    var _mobileMq = window.matchMedia('(max-width: 767px)');
    var _tabletMq = window.matchMedia('(max-width: 1199px)');

    var _ctx = null;
    var _prefs = null;
    var _draft = null;
    var _drag = null;
    var _anchor = null;
    var _coachHideTimer = null;

    function orderedUnits(units, prefs) {
        var order = (prefs && prefs.order) || [];
        function rank(u, i) {
            var o = order.indexOf(u.id);
            return o === -1 ? order.length + i : o;
        }
        var locked = units.filter(function (u) { return u.locked; });
        var rest = units.filter(function (u) { return !u.locked; })
            .map(function (u, i) { return { u: u, r: rank(u, i) }; })
            .sort(function (a, b) { return a.r - b.r; })
            .map(function (x) { return x.u; });
        return locked.concat(rest);
    }

    function visibleUnits(units) {
        var hidden = (_prefs && _prefs.hidden) || [];
        return orderedUnits(units, _prefs).filter(function (u) { return u.locked || hidden.indexOf(u.id) === -1; });
    }

    function escapeHtml(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    function toggleMobileMenu(show) {
        $('#scColPickMobileBtn').closest('.mobile-more').toggleClass('sc-mobile-more-hidden', !show);
    }

    function attach(legendSelector, ctx) {
        _ctx = ctx;
        $(legendSelector).find('.legend-actions').append(
            '<button type="button" class="sc-colpick-btn" id="scColPickBtn" aria-haspopup="dialog">' +
                '<span>Kolon Seçimi</span>' + COLUMNS_ICON +
            '</button>'
        );
        toggleMobileMenu(true);
        closePanel();
    }

    function detach() {
        _ctx = null;
        toggleMobileMenu(false);
        closePanel();
        hideCoach();
    }

    function showCoach() {
        clearTimeout(_coachHideTimer);
        if (isPanelOpen()) return;
        var $btn = $('#scColPickBtn');
        if (!$btn.length || !$btn.is(':visible')) return;
        if (!$('#scColPickCoach').length) {
            $('body').append(
                '<div class="sc-colpick-coach" id="scColPickCoach" role="tooltip">' +
                    '<div class="sc-colpick-coach-body">' +
                        '<span class="sc-colpick-coach-badge">Yeni Özellik</span>' +
                        '<div class="sc-colpick-coach-title">Notlarınızı tabloyla birlikte tutun</div>' +
                        '<div class="sc-colpick-coach-text">Not ekleyin, ekip arkadaşlarınızla paylaşın ve ihtiyaç duyduğunuzda tekrar görüntüleyin.</div>' +
                    '</div>' +
                    '<span class="sc-colpick-coach-arrow"></span>' +
                '</div>'
            );
        }
        $btn.addClass('active');
        positionCoach();
    }

    function hideCoach() {
        clearTimeout(_coachHideTimer);
        $('#scColPickCoach').remove();
        if (!isPanelOpen()) $('#scColPickBtn').removeClass('active');
    }

    function positionCoach() {
        var $coach = $('#scColPickCoach');
        var btn = document.getElementById('scColPickBtn');
        if (!$coach.length || !btn) return;
        var r = btn.getBoundingClientRect();
        if (!r.width) { $coach.hide(); return; }
        $coach.show();
        var w = $coach.outerWidth();
        var center = r.left + r.width / 2;
        var left = Math.max(16, Math.min(center - w / 2, window.innerWidth - w - 16));
        $coach.css({ left: left, bottom: window.innerHeight - r.top + 14 });
        $coach.find('.sc-colpick-coach-arrow').css('left', center - left);
    }

    function isPanelOpen() { return $('#scColPickPanel').length > 0; }

    function openPanel(anchor) {
        if (!_ctx) return;
        _anchor = anchor;
        hideCoach();
        var hidden = {};
        ((_prefs && _prefs.hidden) || []).forEach(function (id) { hidden[id] = true; });
        _draft = {
            order: orderedUnits(_ctx.units, _prefs).map(function (u) { return u.id; }),
            hidden: hidden
        };
        $('body').append(
            '<div class="sc-colpick-panel" id="scColPickPanel" role="dialog" aria-labelledby="scColPickTitle">' +
                '<div class="sc-colpick-head">' +
                    '<div class="sc-colpick-title" id="scColPickTitle">Kolon Seçimi</div>' +
                    '<button type="button" class="sc-colpick-close" aria-label="Kapat"><img src="/images/close-circle.svg" alt="" /></button>' +
                '</div>' +
                '<div class="sc-colpick-desc">Tabloda görmek istediğiniz kolonları seçin, sırasını düzenleyin.</div>' +
                '<div class="sc-colpick-search">' +
                    '<img src="/images/search.svg" alt="" />' +
                    '<input type="text" placeholder="Ara" id="scColPickSearch" autocomplete="off" />' +
                '</div>' +
                '<div class="sc-colpick-scroll" id="scColPickScroll"></div>' +
                '<div class="sc-colpick-footer">' +
                    '<button type="button" class="sc-colpick-link" id="scColPickReset">Varsayılana Dön</button>' +
                    '<button type="button" class="sc-colpick-primary" id="scColPickApply">Uygula</button>' +
                '</div>' +
            '</div>'
        );
        $('#scColPickBtn').addClass('active').attr('aria-expanded', 'true');
        renderLists();
        positionPanel();
    }

    function closePanel() {
        $('#scColPickPanel').remove();
        _draft = null;
        $('#scColPickBtn').removeClass('active').attr('aria-expanded', 'false');
    }

    // Mobilde bottom sheet (CSS); tablette sağ altta legend'in üstünde; masaüstünde butonun üstünde.
    function positionPanel() {
        var $panel = $('#scColPickPanel');
        if (!$panel.length || !_anchor) return;
        if (_mobileMq.matches) {
            $panel.css({ left: '', bottom: '', maxHeight: '' });
            return;
        }
        var w = $panel.outerWidth();
        var right = window.innerWidth - 16;
        var top = window.innerHeight - 8;
        if (_tabletMq.matches) {
            var legend = document.getElementById('scReportLegend');
            var lr = legend && legend.getBoundingClientRect();
            if (lr && lr.height) top = lr.top;
        } else {
            var r = _anchor.getBoundingClientRect();
            right = r.right;
            top = r.top;
        }
        $panel.css({
            left: Math.max(16, Math.min(right - w, window.innerWidth - w - 16)),
            bottom: window.innerHeight - top + 8,
            maxHeight: Math.max(240, Math.min(560, top - 24))
        });
    }

    function itemHtml(u, checked) {
        return '<div class="sc-colpick-item' + (u.locked ? ' locked' : '') + '" data-id="' + escapeHtml(u.id) + '">' +
                '<span class="sc-colpick-handle" aria-hidden="true">' + HANDLE_ICON + '</span>' +
                '<label class="sc-colpick-check">' +
                    '<input type="checkbox"' + (checked ? ' checked' : '') + (u.locked ? ' disabled' : '') + ' />' +
                    CHECKBOX_ICON +
                    '<span class="sc-colpick-label">' + escapeHtml(u.label) + '</span>' +
                '</label>' +
                (u.locked ? '<span class="sc-colpick-lock" title="Bu kolon her zaman gösterilir">' + LOCK_ICON + '</span>' : '') +
            '</div>';
    }

    function renderLists() {
        var $scroll = $('#scColPickScroll');
        var scrollTop = $scroll.scrollTop();
        var byId = {};
        _ctx.units.forEach(function (u) { byId[u.id] = u; });
        var shown = [], others = [];
        _draft.order.forEach(function (id) {
            var u = byId[id];
            if (!u) return;
            if (u.locked || !_draft.hidden[id]) shown.push(u);
            else others.push(u);
        });

        var html =
            '<div class="sc-colpick-section-title">Görünen Kolonlar (' + shown.length + '/' + _ctx.units.length + ')</div>' +
            '<div class="sc-colpick-list" id="scColPickShown">' + shown.map(function (u) { return itemHtml(u, true); }).join('') + '</div>' +
            '<div class="sc-colpick-section-title">Diğer Kolonlar (' + others.length + ')</div>';
        if (others.length) {
            html += '<div class="sc-colpick-list" id="scColPickOthers">' + others.map(function (u) { return itemHtml(u, false); }).join('') + '</div>';
        } else {
            html += '<div class="sc-colpick-list sc-colpick-empty">' + DONE_ICON + '<span>Tüm Kolonlar Seçildi.</span></div>';
        }
        $scroll.html(html).scrollTop(scrollTop);
        applySearch();
    }

    function applySearch() {
        var q = normalizeTurkish(($('#scColPickSearch').val() || '').trim());
        $('#scColPickScroll .sc-colpick-item').each(function () {
            var label = normalizeTurkish($(this).find('.sc-colpick-label').text());
            $(this).toggle(!q || label.indexOf(q) > -1);
        });
    }

    function toggleUnit(id, checked) {
        if (checked) {
            delete _draft.hidden[id];
            _draft.order = _draft.order.filter(function (x) { return x !== id; }).concat([id]);
        } else {
            _draft.hidden[id] = true;
        }
        renderLists();
    }

    function startDrag(e, $item) {
        if ($item.hasClass('locked')) return;
        e.preventDefault();
        var $list = $item.parent();
        $item.addClass('dragging');
        $list.addClass('is-dragging');
        _drag = { $item: $item, $list: $list, $scroll: $('#scColPickScroll'), y: e.clientY, timer: setInterval(autoScroll, 16) };
        $(document).on('pointermove.sccolpick', onDragMove).on('pointerup.sccolpick pointercancel.sccolpick', endDrag);
    }

    function autoScroll() {
        if (!_drag) return;
        var el = _drag.$scroll[0];
        var r = el.getBoundingClientRect();
        if (_drag.y < r.top + 32) el.scrollTop -= 8;
        else if (_drag.y > r.bottom - 32) el.scrollTop += 8;
        else return;
        reorderAt(_drag.y);
    }

    function onDragMove(e) {
        if (!_drag) return;
        _drag.y = e.clientY;
        reorderAt(e.clientY);
    }

    function reorderAt(y) {
        var item = _drag.$item[0];
        var siblings = _drag.$list.children('.sc-colpick-item').not(item).filter(':visible').toArray();
        var before = null;
        for (var i = 0; i < siblings.length; i++) {
            var r = siblings[i].getBoundingClientRect();
            if (y < r.top + r.height / 2) { before = siblings[i]; break; }
        }
        while (before && $(before).hasClass('locked')) before = $(before).nextAll('.sc-colpick-item').not(item)[0] || null;
        if (before) { if (item.nextSibling !== before) _drag.$list[0].insertBefore(item, before); }
        else if (_drag.$list[0].lastElementChild !== item) _drag.$list[0].appendChild(item);
    }

    function endDrag() {
        if (!_drag) return;
        clearInterval(_drag.timer);
        _drag.$item.removeClass('dragging');
        _drag.$list.removeClass('is-dragging');
        _draft.order = $('#scColPickShown, #scColPickOthers').children('.sc-colpick-item')
            .map(function () { return $(this).attr('data-id'); }).get();
        _drag = null;
        $(document).off('.sccolpick');
    }

    function apply() {
        _prefs = { order: _draft.order, hidden: Object.keys(_draft.hidden) };
        closePanel();
        _ctx.onApply();
    }

    function resetDraft() {
        _draft = { order: _ctx.units.map(function (u) { return u.id; }), hidden: {} };
        $('#scColPickSearch').val('');
        renderLists();
    }

    $(document).on('click', '#scColPickBtn', function (e) {
        e.stopPropagation();
        if (isPanelOpen()) closePanel(); else openPanel(this);
    });
    $(document).on('click', '#scColPickMobileBtn', function (e) {
        e.stopPropagation();
        $(this).closest('.mobile-more').removeAttr('open');
        openPanel(this);
    });
    $(document).on('click', '#scColPickPanel .sc-colpick-close', closePanel);
    $(document).on('click', '#scColPickApply', apply);
    $(document).on('click', '#scColPickReset', resetDraft);
    $(document).on('input', '#scColPickSearch', applySearch);
    $(document).on('change', '#scColPickScroll .sc-colpick-item input[type="checkbox"]', function () {
        toggleUnit($(this).closest('.sc-colpick-item').attr('data-id'), this.checked);
    });
    $(document).on('pointerdown', '#scColPickScroll .sc-colpick-handle', function (e) {
        startDrag(e, $(this).closest('.sc-colpick-item'));
    });
    $(document).on('mouseenter', '#scColPickBtn, #scColPickCoach', showCoach);
    $(document).on('mouseleave', '#scColPickBtn, #scColPickCoach', function () {
        clearTimeout(_coachHideTimer);
        _coachHideTimer = setTimeout(hideCoach, 200);
    });
    $(document).on('mousedown', function (e) {
        if (!isPanelOpen() || _drag) return;
        if (!$(e.target).closest('#scColPickPanel, #scColPickBtn, #scColPickMobileBtn').length) closePanel();
    });
    $(document).on('keydown', function (e) {
        if (e.key === 'Escape' && isPanelOpen()) closePanel();
    });

    function reposition() { positionPanel(); positionCoach(); }
    $(window).on('resize', reposition);
    document.addEventListener('scroll', reposition, true);

    return {
        visibleUnits: visibleUnits,
        attach: attach,
        detach: detach
    };
})();
