// Paylaşımlı USD Kur Detayı modal — Hedef Raporları ve Verim Raporları sayfalarında kullanılır.
// Aktif rapor tarihini okumak için her sayfa şunu tanımlar:
//   window._getUsdKurDate = function () { return <aktif tarih değişkeni>; };
(function () {

    function formatUsdRate(rate) {
        if (rate == null) return '-';
        return Number(rate).toLocaleString('tr-TR', { minimumFractionDigits: 4, maximumFractionDigits: 4 });
    }

    function getReportDate() {
        if (typeof window._getUsdKurDate === 'function') return window._getUsdKurDate();
        if (typeof _todayDate !== 'undefined') return _todayDate;
        return new Date().toISOString();
    }

    function renderUsdKurRows(data, reportDate) {
        var rows = [
            { label: 'Geçen Yıl',         date: data.PreviousYearDate, rate: data.PreviousYearRate },
            { label: 'Geçen Hafta',        date: data.PreviousWeekDate, rate: data.PreviousWeekRate },
            { label: 'Önceki Gün (T-2)',   date: data.PreviousDayDate,  rate: data.PreviousDayRate  },
            { label: 'Dün (T-1)',          date: data.YesterdayDate,    rate: data.YesterdayRate    }
        ];
        var html = '';
        for (var i = 0; i < rows.length; i++) {
            if (i > 0) html += '<div class="usd-kur-divider"></div>';
            var r = rows[i];
            html += '<div class="usd-kur-row">' +
                '<span class="usd-kur-label">' + r.label + '</span>' +
                '<span class="usd-kur-date">' + fmtIsoDate(r.date) + '</span>' +
                '<span class="usd-kur-rate">1 USD = ' + formatUsdRate(r.rate) + ' ₺</span>' +
                '</div>';
        }
        $('#usdKurRows').html(html);
        $('#usdKurFooter').html(
            '<span>Seçili rapor tarihi:</span> <strong>' + formatReportDateTr(reportDate) + '</strong>'
        );
    }

    function openUsdKurModal(triggerEl) {
        if (triggerEl && window.innerWidth > 767) {
            var rect = triggerEl.getBoundingClientRect();
            var modalW = 440, gap = 10;
            var left = rect.left + rect.width / 2 - modalW / 2;
            var bottom = window.innerHeight - rect.top + gap;
            left = Math.max(16, Math.min(left, window.innerWidth - modalW - 16));
            $('.usd-kur-modal').css({ left: left + 'px', bottom: bottom + 'px', top: 'auto', right: 'auto' });
        }
        var reportDate = getReportDate();
        $('#usdKurRows').html('<div class="usd-kur-loading">Yükleniyor…</div>');
        $('#usdKurFooter').html('');
        $('#usdKurOverlay').fadeIn(150);
        $.ajax({
            url: '/ExchangeRate/GetUsdExchangeRates',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ sessionId: '1', reportDate: reportDate }),
            success: function (data) { renderUsdKurRows(data, reportDate); },
            error: function () { $('#usdKurRows').html('<div class="usd-kur-loading">Veriler yüklenemedi.</div>'); }
        });
    }

    $(function () {
        // Event delegation — #usdKurBtn verim sayfasında renderTableLegend ile dinamik oluşturulur.
        // closest('details').removeAttr('open') → mobil menüyü kapatır (tüm sayfalarda).
        $(document).on('click', '#usdKurBtn, #usdKurBtnQuantity, #mobileUsdKurBtn', function () {
            $(this).closest('details').removeAttr('open');
            openUsdKurModal(this);
        });
        $('#usdKurClose').on('click', function () { $('#usdKurOverlay').fadeOut(200); });
        $('#usdKurOverlay').on('click', function (e) {
            if ($(e.target).is('#usdKurOverlay')) $('#usdKurOverlay').fadeOut(200);
        });
    });

})();
