// ===== AI İçgörüleri Drawer =====
// Bölge & şube seçim mantığı, verim raporları filtreleriyle aynı:
// aynı veri (_regionFilters / _branchFilters) ve aynı render fonksiyonları
// (renderRegionList / renderBranchList / findRegion) kullanılır.
$(document).ready(function () {

    // ===== State (filtrelerden bağımsız) =====
    var aiSelectedRegion = null;
    var aiSelectedBranch = null;

    // ===== Filter Lists (filtreleme ile aynı logic) =====
    function aiRenderRegionDropdown() {
        return renderRegionList('#aiBolgeList', aiSelectedRegion ? aiSelectedRegion.code : null);
    }

    function aiRenderBranchDropdown() {
        return renderBranchList('#aiSubeList', aiSelectedBranch ? aiSelectedBranch.code : null, aiSelectedRegion ? aiSelectedRegion.code : null);
    }

    function aiUpdateSubmitState() {
        // Hem bölge hem şube seçilmeden içgörü oluşturulamaz
        $('#aiDrawerSubmit').prop('disabled', !(aiSelectedRegion && aiSelectedBranch));
    }

    // ===== Open / Close =====
    function openAiDrawer() {
        loadRegionFilters(function () {
            var single = aiRenderRegionDropdown();
            if (single) {
                aiSelectedRegion = { code: single.Code, name: single.Name };
                $('#aiBolgeLabel').text(single.Name);
                $('#aiBolgeSelect').addClass('ai-has-value');
            }
            loadBranchFilters(function () {
                var singleBranch = aiRenderBranchDropdown();
                if (singleBranch) {
                    aiSelectedBranch = { code: singleBranch.Code, name: singleBranch.Name };
                    $('#aiSubeLabel').text(singleBranch.Name);
                    $('#aiSubeSelect').addClass('ai-has-value');
                }
                aiUpdateSubmitState();
            });
        });

        $('#aiDrawerOverlay').addClass('open');
        $('#aiDrawer').addClass('open');
    }

    function closeAiDrawer() {
        $('#aiDrawer').removeClass('open has-result');
        $('#aiDrawerOverlay').removeClass('open');
        $('.dropdown-panel').removeClass('open');
        $('#aiResult').empty();
        $('#aiNotice').empty();
    }

    $(document).on('click', '#aiInsightsBtn', function (e) {
        e.preventDefault();
        openAiDrawer();
    });

    $('#aiDrawerClose').on('click', closeAiDrawer);
    $('#aiDrawerOverlay').on('click', closeAiDrawer);

    // ===== Region Selection (filtreleme ile aynı) =====
    $(document).on('click', '#aiBolgeList .dropdown-item', function () {
        var code = $(this).attr('data-code');
        var name = $(this).text();

        aiSelectedRegion = code ? { code: code, name: name } : null;
        $('#aiBolgeLabel').text(code ? name : 'Bölge Seçiniz');
        $('#aiBolgeSelect').toggleClass('ai-has-value', !!code);

        aiSelectedBranch = null;
        $('#aiSubeLabel').text('Şube Seçiniz');
        $('#aiSubeSelect').removeClass('ai-has-value');

        $('#aiBolgeList .dropdown-item').removeClass('selected');
        $(this).addClass('selected');
        $('#aiBolgePanel').removeClass('open');
        $('#aiBolgeSearch').val('');

        aiRenderBranchDropdown();
        aiUpdateSubmitState();
    });

    // ===== Branch Selection (filtreleme ile aynı) =====
    $(document).on('click', '#aiSubeList .dropdown-item', function () {
        var code = $(this).attr('data-code');
        var name = $(this).text();

        if (!code) {
            aiSelectedBranch = null;
            $('#aiSubeLabel').text('Şube Seçiniz');
            $('#aiSubeSelect').removeClass('ai-has-value');
        } else {
            var regionCode = $(this).attr('data-region');
            aiSelectedBranch = { code: code, name: name };
            $('#aiSubeLabel').text(name);
            $('#aiSubeSelect').addClass('ai-has-value');

            var region = findRegion(regionCode);
            if (region) {
                aiSelectedRegion = { code: region.Code, name: region.Name };
                $('#aiBolgeLabel').text(region.Name);
                $('#aiBolgeSelect').addClass('ai-has-value');
                $('#aiBolgeList .dropdown-item').removeClass('selected');
                $('#aiBolgeList .dropdown-item[data-code="' + region.Code + '"]').addClass('selected');
            }
        }

        $('#aiSubeList .dropdown-item').removeClass('selected');
        $(this).addClass('selected');
        $('#aiSubePanel').removeClass('open');
        $('#aiSubeSearch').val('');

        aiUpdateSubmitState();
    });

    // ===== Submit =====
    $('#aiDrawerSubmit').on('click', function () {
        if ($(this).prop('disabled')) return;

        var $btn = $(this);
        var $result = $('#aiResult');

        var payload = {
            regionCode: aiSelectedRegion ? aiSelectedRegion.code : null,
            branchCode: aiSelectedBranch ? aiSelectedBranch.code : null
        };

        $btn.prop('disabled', true);
        aiHideNotice();
        // Geniş ekran yalnızca servisten geçerli veri dönerse açılır
        $btn.text('Oluşturuluyor...');

        $.ajax({
            url: '/AiInsight/GetBranchAiInsights',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (data) {
                var items = (data && data.Items) || (data && data.items) || [];
                var summary = items.length ? (items[0].Summary || items[0].summary || '') : '';

                if (!summary) {
                    // Veri yok: geniş ekrandaysa orada kal ve mesajı sonuç alanında göster,
                    // küçük ekrandaysa mesajı filtrelerin üstünde göster.
                    aiShowNotice('Bu bölge ve şubeye ait AI içgörüsü bulunmamaktadır.');
                    return;
                }

                $('#aiNotice').empty();
                $('#aiDrawer').addClass('has-result');
                $result.html('<div class="ai-result-content">' + renderMarkdown(summary) + '</div>');
            },
            error: function () {
                aiShowNotice('İçgörü alınırken bir hata oluştu. Lütfen tekrar deneyin.');
            },
            complete: function () {
                $btn.text('AI İçgörüsü Oluştur');
                aiUpdateSubmitState();
            }
        });
    });

    // ===== Bildirim (hata / veri yok) =====
    function aiNoticeHtml(message) {
        return '<div class="ai-notice-error">' +
                '<img class="ai-notice-icon" src="/images/error-info.svg" alt="" />' +
                '<span class="ai-notice-text">' + message + '</span>' +
            '</div>';
    }

    function aiShowNotice(message) {
        if ($('#aiDrawer').hasClass('has-result')) {
            // Geniş ekran açık: ekran açık kalır, mesaj sonuç alanında gösterilir.
            $('#aiNotice').empty();
            $('#aiResult').html(aiNoticeHtml(message));
        } else {
            // Küçük ekran: mesaj filtrelerin üstünde gösterilir.
            $('#aiResult').empty();
            $('#aiNotice').html(aiNoticeHtml(message));
        }
    }

    function aiHideNotice() {
        $('#aiNotice').empty();
    }

    // ===== Markdown -> HTML (başlık, kalın, tablo, liste, hr; inline HTML korunur) =====
    function mdInline(text) {
        // **kalın** -> <strong> (mevcut <span> gibi inline HTML olduğu gibi korunur)
        return text.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
    }

    function renderTableCells(line) {
        var raw = line.trim().replace(/^\|/, '').replace(/\|$/, '');
        return raw.split('|').map(function (c) { return c.trim(); });
    }

    function renderMarkdown(md) {
        if (!md) return '';
        var lines = md.replace(/\r\n/g, '\n').split('\n');
        var html = [];
        var i = 0;
        var listOpen = false;

        function closeList() {
            if (listOpen) { html.push('</ul>'); listOpen = false; }
        }

        while (i < lines.length) {
            var line = lines[i];
            var trimmed = line.trim();

            // Boş satır
            if (trimmed === '') { closeList(); i++; continue; }

            // Yatay çizgi
            if (/^-{3,}$/.test(trimmed)) { closeList(); html.push('<hr />'); i++; continue; }

            // Başlıklar
            var h = /^(#{1,6})\s+(.*)$/.exec(trimmed);
            if (h) {
                closeList();
                var level = h[1].length;
                html.push('<h' + level + '>' + mdInline(h[2]) + '</h' + level + '>');
                i++; continue;
            }

            // Tablo: "| ... |" satırı ardından "|---|" ayraç satırı
            if (trimmed.indexOf('|') === 0 && i + 1 < lines.length && /^\|?[\s:|-]+\|?$/.test(lines[i + 1].trim()) && lines[i + 1].indexOf('-') !== -1) {
                closeList();
                var headers = renderTableCells(trimmed);
                var t = ['<table><thead><tr>'];
                headers.forEach(function (c) { t.push('<th>' + mdInline(c) + '</th>'); });
                t.push('</tr></thead><tbody>');
                i += 2; // başlık + ayraç
                while (i < lines.length && lines[i].trim().indexOf('|') === 0) {
                    var cells = renderTableCells(lines[i].trim());
                    t.push('<tr>');
                    cells.forEach(function (c) { t.push('<td>' + mdInline(c) + '</td>'); });
                    t.push('</tr>');
                    i++;
                }
                t.push('</tbody></table>');
                html.push(t.join(''));
                continue;
            }

            // Liste öğesi
            if (/^[-*]\s+/.test(trimmed)) {
                if (!listOpen) { html.push('<ul>'); listOpen = true; }
                html.push('<li>' + mdInline(trimmed.replace(/^[-*]\s+/, '')) + '</li>');
                i++; continue;
            }

            // Paragraf
            closeList();
            html.push('<p>' + mdInline(trimmed) + '</p>');
            i++;
        }

        closeList();
        return html.join('');
    }
});
