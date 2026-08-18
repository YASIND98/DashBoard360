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
        // Önce ekranı geniş (sonuç) layout'una al; orb yazının başlayacağı yerde yüklensin
        $('#aiNotice').empty();
        $('#aiDrawer').addClass('has-result');
        $result.html(aiLoaderHtml());
        var thinkStart = Date.now();

        function finish(captured) {
            $btn.text('AI İçgörüsü Oluştur');
            aiUpdateSubmitState();

            if (captured.error) {
                $('#aiDrawer').removeClass('has-result');
                aiShowNotice('İçgörü alınırken bir hata oluştu. Lütfen tekrar deneyin.');
                return;
            }
            if (!captured.summary) {
                $('#aiDrawer').removeClass('has-result');
                aiShowNotice('Bu bölge ve şubeye ait AI içgörüsü bulunmamaktadır.');
                return;
            }
            $('#aiNotice').empty();
            // Servis cevabını doğrudan "AI yazıyormuş" gibi kademeli olarak ekrana yaz
            aiTypeMarkdown($result, captured.summary, captured.openSections);
        }

        $.ajax({
            url: '/AiInsight/GetBranchAiInsights',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (data) {
                var items = (data && data.Items) || (data && data.items) || [];
                var item = items.length ? items[0] : null;
                var captured = {
                    summary: item ? (item.Summary || item.summary || '') : '',
                    openSections: (item && (item.OpenSections || item.openSections)) || []
                };
                aiAfterThinking(thinkStart, function () { finish(captured); });
            },
            error: function () {
                aiAfterThinking(thinkStart, function () { finish({ error: true }); });
            }
        });
    });

    // "Düşünüyor" orb'u en az bu kadar görünür (hızlı cevap gelse bile efekt fark edilir)
    function aiAfterThinking(startedAt, cb) {
        var minThink = 900;
        var wait = Math.max(0, minThink - (Date.now() - startedAt));
        setTimeout(cb, wait);
    }

    // Yazının başlayacağı yerde beliren, nefes alıp dönen "yükleniyor" yuvarlağı
    function aiLoaderHtml() {
        return '<div class="ai-loader"><span class="ai-loader-orb"></span></div>';
    }

    // Servis cevabını gerçek bir AI yazıyormuş gibi kademeli olarak ekrana yansıtır.
    // Markdown bir kez nihai HTML'e çevrilir (biçim/sözdizimi asla ekranda görünmez);
    // ardından metin, doğru yapı içine akıtılır: bloklar yazıya ulaşıldıkça belirir.
    // Zaman güdümlüdür: tüm cevap en fazla ~5 saniyede tamamlanır.
    var AI_TYPE_BUDGET = 8500; // ms — tüm metnin yazılacağı azami süre (gerçekten yazıyor hissi)

    function aiTypeMarkdown($container, md, openSections, onDone) {
        var full = md || '';
        var $content = $('<div class="ai-result-content ai-typing"></div>');
        $container.empty().append($content);

        function complete() {
            if (caret && caret.parentNode) caret.parentNode.removeChild(caret);
            $content.removeClass('ai-typing');
            $content.find('.ai-pending').removeClass('ai-pending');
            if (typeof onDone === 'function') onDone();
        }

        var caret = null;
        if (!full) { complete(); return; }

        // 1) Nihai HTML'i tek seferde oluştur — kalın/renk/tablo ilk karakterden itibaren doğru
        $content.html(renderMarkdown(full, openSections));
        var root = $content[0];

        // Kapalı bölümün gövdesi animasyona girmez; görünmeyen metne yazılırken takılmış gibi görünürdü
        function isSkipped(el) {
            return el.classList && el.classList.contains('ai-collapse-body') &&
                   el.parentNode && el.parentNode.classList.contains('is-closed');
        }

        // 2) Kademeli belirecek blokları başta gizle (yazıya ulaşınca açılırlar).
        //    Collapse kutusu da dahil; yoksa çerçevesi baştan görünüp içi boş kalıyor.
        var blocks = root.querySelectorAll('section.ai-collapse, p, h1, h2, h3, h4, h5, h6, hr, table, li, tr');
        for (var b = 0; b < blocks.length; b++) {
            var el = blocks[b];
            if (el.closest('.ai-collapse.is-closed > .ai-collapse-body')) continue;
            if (el.classList.contains('ai-collapse-head')) continue;   // kutuyla birlikte belirir
            el.classList.add('ai-pending');
        }

        // 3) Belge sırasına göre işlem listesi kur; metin düğümlerini boşalt
        var ops = [];
        var typedTotal = 0;
        (function walk(node) {
            for (var i = 0; i < node.childNodes.length; i++) {
                var child = node.childNodes[i];
                if (child.nodeType === 1) {
                    if (isSkipped(child)) continue;
                    if (child.classList && child.classList.contains('ai-pending')) {
                        ops.push({ kind: 'show', el: child });
                    }
                    walk(child);
                } else if (child.nodeType === 3 && child.nodeValue.length) {
                    ops.push({ kind: 'text', node: child, text: child.nodeValue, start: typedTotal });
                    typedTotal += child.nodeValue.length;
                    child.nodeValue = '';
                }
            }
        })(root);

        if (!typedTotal) { // sadece blok/çizgi varsa: hepsini göster
            ops.forEach(function (o) { if (o.kind === 'show') o.el.classList.remove('ai-pending'); });
            complete();
            return;
        }

        caret = document.createElement('span');
        caret.className = 'ai-caret';

        function caretAfter(node) {
            if (node.parentNode) node.parentNode.insertBefore(caret, node.nextSibling);
        }

        // Kaydırma kabı drawer gövdesidir (#aiResult'ın overflow'u yok)
        var scroller = $container.closest('.ai-drawer-body')[0] || $container[0];
        function autoScroll() {
            if (scroller.scrollHeight > scroller.clientHeight) {
                scroller.scrollTop = scroller.scrollHeight;
            }
        }

        // 4) Zaman güdümlü akış: her karede geçen süreye göre kaç karakter görünmeli hesaplanır.
        //    Böylece metnin uzunluğundan bağımsız olarak toplam süre ~AI_TYPE_BUDGET'tir.
        var start = Date.now();
        var opIdx = 0;

        function frame() {
            // Drawer kapandıysa / yeniden gönderildiyse içerik DOM'dan koptu: döngüyü durdur
            if (!root.isConnected) return;

            var progress = (Date.now() - start) / AI_TYPE_BUDGET;
            if (progress > 1) progress = 1;
            var revealCount = Math.ceil(typedTotal * progress);

            while (opIdx < ops.length) {
                var op = ops[opIdx];

                if (op.kind === 'show') {
                    op.el.classList.remove('ai-pending');
                    op.el.classList.add('ai-reveal');
                    opIdx++;
                    continue;
                }

                var need = revealCount - op.start;        // bu düğümden kaç karakter görünmeli
                if (need <= 0) break;                     // sıra henüz bu metne gelmedi
                var show = op.text.length < need ? op.text.length : need;
                op.node.nodeValue = op.text.slice(0, show);
                caretAfter(op.node);
                if (show < op.text.length) break;         // bu düğüm henüz bitmedi
                opIdx++;                                  // tamamlandı, sonrakine geç
            }

            autoScroll();

            if (progress >= 1 || opIdx >= ops.length) { complete(); return; }
            requestAnimationFrame(frame);
        }

        requestAnimationFrame(frame);
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

    // ===== Katlanabilir bölümler =====
    // "## " ve "### " başlıkları katlanır bölüm olur. Anahtar belge yapısından üretilir:
    // 2. "## " -> "2", onun 3. "### "i -> "2.3". Hangilerinin açık geleceğini servis söyler:
    // { Summary: "...", OpenSections: ["1", "2.3"] }
    // Alt bölüm listedeyse üstü de açılır; kapalı bir kutunun içindeki açık bölüm görünmezdi.
    var COLLAPSE_MIN_LEVEL = 2;   // "## "
    var COLLAPSE_MAX_LEVEL = 3;   // "### "

    // Servis şimdilik OpenSections göndermiyor; boş geldiğinde bu liste kullanılır.
    // Belgede karşılığı olmayan anahtarlar sessizce yok sayılır.
    var DEFAULT_OPEN_SECTIONS = ['1', '2.1', '3.1', '4.1', '5', '6.1', '7', '8'];

    $(document).on('click', '.ai-collapse-head', function () {
        var isClosed = $(this).closest('.ai-collapse').toggleClass('is-closed').hasClass('is-closed');
        $(this).attr('aria-expanded', isClosed ? 'false' : 'true');
    });

    $(document).on('keydown', '.ai-collapse-head', function (e) {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            $(this).trigger('click');
        }
    });

    // ===== Markdown -> HTML (başlık, kalın, tablo, liste, hr; inline HTML korunur) =====
    function mdInline(text) {
        // **kalın** -> <strong> (mevcut <span> gibi inline HTML olduğu gibi korunur)
        return text.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
    }

    function renderTableCells(line) {
        var raw = line.trim().replace(/^\|/, '').replace(/\|$/, '');
        return raw.split('|').map(function (c) { return c.trim(); });
    }

    function renderMarkdown(md, openSections) {
        if (!md) return '';
        var open = (openSections && openSections.length ? openSections : DEFAULT_OPEN_SECTIONS).map(String);
        var lines = md.replace(/\r\n/g, '\n').split('\n');
        var html = [];
        var i = 0;
        var listOpen = false;
        var collapseStack = [];   // açık collapse bölümlerinin başlık seviyeleri
        var counters = [];        // seviye başına sıra sayacı; anahtar bunlardan kurulur

        function closeList() {
            if (listOpen) { html.push('</ul>'); listOpen = false; }
        }

        // Bölüm, aynı/daha üst seviyeli başlıkta (veya <hr>/metin sonunda) kapanır; kaç tane kapattığını döner
        function closeCollapses(level) {
            var closed = 0;
            while (collapseStack.length && collapseStack[collapseStack.length - 1] >= level) {
                html.push('</div></section>');
                collapseStack.pop();
                closed++;
            }
            return closed;
        }

        // Anahtar listede varsa ya da listedeki bir anahtarın atasıysa ("2.3" -> "2") açık gelir
        function isSectionOpen(key) {
            return open.some(function (k) { return k === key || k.indexOf(key + '.') === 0; });
        }

        // Bu seviyedeki sırayı bir artırır, altındaki sayaçları sıfırlar; "2" / "2.3" anahtarını döner
        function sectionKey(level) {
            counters[level] = (counters[level] || 0) + 1;
            for (var deeper = level + 1; deeper <= COLLAPSE_MAX_LEVEL; deeper++) counters[deeper] = 0;
            var parts = [];
            for (var l = COLLAPSE_MIN_LEVEL; l <= level; l++) parts.push(counters[l] || 0);
            return parts.join('.');
        }

        while (i < lines.length) {
            var line = lines[i];
            var trimmed = line.trim();

            // Boş satır
            if (trimmed === '') { closeList(); i++; continue; }

            // Yatay çizgi — bölüm ayracı olduğu için açık collapse'leri kapatır. Bölüm kapattıysa
            // çizgi basılmaz (kutu kenarlıkları zaten ayırıyor); açık bölüm yokken çizilir.
            if (/^-{3,}$/.test(trimmed)) {
                closeList();
                if (!closeCollapses(1)) html.push('<hr />');
                i++; continue;
            }

            // Başlıklar
            var h = /^(#{1,6})\s+(.*)$/.exec(trimmed);
            if (h) {
                closeList();
                var level = h[1].length;
                var headText = h[2];
                closeCollapses(level);

                if (level >= COLLAPSE_MIN_LEVEL && level <= COLLAPSE_MAX_LEVEL) {
                    var isOpen = isSectionOpen(sectionKey(level));
                    // Baştan kapalı basılır; animasyon atladığı için "açılıp sonra kapanma" olmaz
                    html.push('<section class="ai-collapse' + (isOpen ? '' : ' is-closed') + '">');
                    html.push(
                        '<h' + level + ' class="ai-collapse-head" role="button" tabindex="0" aria-expanded="' + (isOpen ? 'true' : 'false') + '">' +
                            '<span class="ai-collapse-title">' + mdInline(headText) + '</span>' +
                            '<span class="ai-collapse-icon" aria-hidden="true"></span>' +
                        '</h' + level + '>'
                    );
                    html.push('<div class="ai-collapse-body">');
                    collapseStack.push(level);
                } else {
                    html.push('<h' + level + '>' + mdInline(headText) + '</h' + level + '>');
                }
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
        closeCollapses(1);
        return html.join('');
    }
});
