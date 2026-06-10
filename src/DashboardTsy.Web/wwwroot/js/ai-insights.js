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

    // Sayfadaki (Verim Raporları) filtrelerde seçili bölge/şubeyi oku
    function getPageSelection() {
        var $r = $('#yieldBolgeList .dropdown-item.selected');
        var rcode = $r.attr('data-code');
        var $b = $('#yieldSubeList .dropdown-item.selected');
        var bcode = $b.attr('data-code');
        return {
            region: rcode ? { code: rcode, name: $r.text() } : null,
            branch: bcode ? { code: bcode, name: $b.text() } : null
        };
    }

    // ===== Open / Close =====
    function openAiDrawer() {
        var pre = getPageSelection();

        loadRegionFilters(function () {
            var single = aiRenderRegionDropdown();
            // Öncelik: sayfada seçili bölge; yoksa tek seçenek varsa o
            if (pre.region) {
                aiSelectedRegion = pre.region;
            } else if (single) {
                aiSelectedRegion = { code: single.Code, name: single.Name };
            }
            if (aiSelectedRegion) {
                $('#aiBolgeLabel').text(aiSelectedRegion.name);
                $('#aiBolgeSelect').addClass('ai-has-value');
                aiRenderRegionDropdown();
            }

            loadBranchFilters(function () {
                if (pre.branch) {
                    aiSelectedBranch = pre.branch;
                } else {
                    var singleBranch = aiRenderBranchDropdown();
                    if (singleBranch) {
                        aiSelectedBranch = { code: singleBranch.Code, name: singleBranch.Name };
                    }
                }
                if (aiSelectedBranch) {
                    $('#aiSubeLabel').text(aiSelectedBranch.name);
                    $('#aiSubeSelect').addClass('ai-has-value');
                }
                aiRenderBranchDropdown();
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
        $btn.text('Oluşturuluyor...');
        // AI "düşünüyor" loader'ı (sonuç gelene kadar)
        $result.html(aiThinkingHtml());

        // Aşamalı durum metni (AI gerçekten çalışıyormuş hissi)
        var statusMsgs = [
            'Veriler analiz ediliyor',
            'Metrikler değerlendiriliyor',
            'Bölge ve banka ortalamaları karşılaştırılıyor',
            'İçgörüler derleniyor'
        ];
        var msgIdx = 0;
        $('#aiThinkStatus').text(statusMsgs[0]);
        var statusTimer = setInterval(function () {
            msgIdx = (msgIdx + 1) % statusMsgs.length;
            $('#aiThinkStatus').text(statusMsgs[msgIdx]);
        }, 1400);

        // AI'nin gerçekten düşündüğü hissi için 3-5 sn'lik bekleme
        var delay = 3000 + Math.floor(Math.random() * 2000);
        var captured = null; // { summary } | { error: true }

        function finish() {
            clearInterval(statusTimer);
            $btn.text('AI İçgörüsü Oluştur');
            aiUpdateSubmitState();

            if (!captured) return;
            if (captured.error) {
                aiShowNotice('İçgörü alınırken bir hata oluştu. Lütfen tekrar deneyin.');
                return;
            }
            if (!captured.summary) {
                aiShowNotice('Bu bölge ve şubeye ait AI içgörüsü bulunmamaktadır.');
                return;
            }
            $('#aiNotice').empty();
            $('#aiDrawer').addClass('has-result');
            $result.html('<div class="ai-result-content">' + renderMarkdown(captured.summary) + '</div>');
        }

        $.ajax({
            url: '/AiInsight/GetBranchAiInsights',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (data) {
                var items = (data && data.Items) || (data && data.items) || [];
                captured = { summary: items.length ? (items[0].Summary || items[0].summary || '') : '' };
            },
            error: function () {
                captured = { error: true };
            },
            complete: function () {
                // Cevap hızlı gelse bile loader en az `delay` kadar görünür
                setTimeout(finish, delay);
            }
        });
    });

    // AI "düşünüyor" göstergesi (cam efektli kart + dönen gradient halka + ilerleme)
    function aiThinkingHtml() {
        return '<div class="ai-thinking">' +
                '<div class="ai-thinking-card">' +
                    '<div class="ai-orb">' +
                        '<span class="ai-orb-ring"></span>' +
                        '<span class="ai-orb-core"><img src="/images/ai-logo.svg" alt="" /></span>' +
                    '</div>' +
                    '<div class="ai-thinking-title">Yapay Zeka İçgörünüzü Hazırlıyor</div>' +
                    '<div class="ai-thinking-status">' +
                        '<span id="aiThinkStatus">Veriler analiz ediliyor</span>' +
                        '<span class="ai-thinking-dots"><span>.</span><span>.</span><span>.</span></span>' +
                    '</div>' +
                    '<div class="ai-thinking-bar"><span></span></div>' +
                '</div>' +
            '</div>';
    }

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
