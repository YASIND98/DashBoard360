// Skor Kart - Bölge / Şube / Sicil / Arama filtreleri
$(function () {

    if (!document.getElementById('scReportBody')) return;

    var SC = window.ScoreCard = window.ScoreCard || {};
    function R() { return SC.report || {}; }

    var _scRegions = [];
    var _scBranches = [];

    // dashboard/regions: kullanıcının görebildiği bölgelerin listesi.
    function fetchRegions(callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/dashboard/regions',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                regionCode: R().regionCode,
                branchCode: R().branchCode,
                dateNumber: R().dateNumber,
                registerId: R().registerId
            })
        }).done(function (res) {
            callback(Array.isArray(res) ? res : ((res && res.rows) || []));
        }).fail(function () {
            callback([]);
        });
    }

    //dashboard/branches: kullanıcının görebildiği şubelerin listesi.
    function fetchBranches(callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/dashboard/branches',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                pupaType: R().activePupaType(),
                branchCode: R().branchCode,
                dateNumber: R().dateNumber,
                registerId: R().registerId,
                scoreCardTypeId: 0,
                regionCode: R().regionCode,
                scoreCardId: R().scoreCardId
            })
        }).done(function (res) {
            callback(Array.isArray(res) ? res : ((res && res.rows) || []));
        }).fail(function () {
            callback([]);
        });
    }

    // Bölge listesini doldur (#scRegionList); tek bölge ise "Tümü" gösterilmez (otomatik seçili).
    function renderScRegionList() {
        var $list = $('#scRegionList');
        $list.empty();
        var single = _scRegions.length === 1;
        if (!single) {
            var tumuCls = (R().regionCode === -1) ? ' selected' : '';
            $list.append('<div class="dropdown-item tumu-item' + tumuCls + '" data-code="-1">Tümü</div>');
        }
        _scRegions.forEach(function (r) {
            var cls = (single || r.regionCode === R().regionCode) ? ' selected' : '';
            $list.append('<div class="dropdown-item' + cls + '" data-code="' + r.regionCode + '">' + r.regionName + '</div>');
        });
    }

    // Şube listesini doldur (#scBranchList); liste servisten seçili bölgeye göre gelir.
    function renderScBranchList() {
        var $list = $('#scBranchList');
        $list.empty();
        var single = _scBranches.length === 1;
        if (!single) {
            var tumuCls = (R().branchCode === -1) ? ' selected' : '';
            $list.append('<div class="dropdown-item tumu-item' + tumuCls + '" data-code="-1">Tümü</div>');
        }
        _scBranches.forEach(function (b) {
            var cls = (single || b.branchCode === R().branchCode) ? ' selected' : '';
            $list.append('<div class="dropdown-item' + cls + '" data-code="' + b.branchCode + '" data-region="' + b.regionCode + '">' + b.branchName + '</div>');
        });
    }

    // Bölgeleri yükle; tek bölge dönerse otomatik seçili setlenir.
    function loadRegions(done) {
        fetchRegions(function (regions) {
            _scRegions = regions || [];
            var single = _scRegions.length === 1;
            if (single) {
                R().regionCode = _scRegions[0].regionCode;
                $('#scRegionLabel').text(_scRegions[0].regionName);
            }
            // Tek bölge dönerse dropdown kilitlenir (hedef ekranındaki gibi disabled)
            $('#scRegionSelect').toggleClass('disabled', single);
            renderScRegionList();
            if (done) done();
        });
    }

    // Şubeleri yükle; tek şube dönerse otomatik seçili setlenir.
    function loadBranches(done) {
        fetchBranches(function (branches) {
            _scBranches = branches || [];
            var single = _scBranches.length === 1;
            if (single) {
                R().branchCode = _scBranches[0].branchCode;
                $('#scBranchLabel').text(_scBranches[0].branchName);
            }
            // Tek şube dönerse dropdown kilitlenir (hedef ekranındaki gibi disabled)
            $('#scBranchSelect').toggleClass('disabled', single);
            renderScBranchList();
            if (done) done();
        });
    }

    //dashboard/registers: kullanıcının görebildiği şubelerin sicillerin (PY) listesi.
    function fetchRegisters(callback) {
        $.ajax({
            url: SCORE_CARD_BASE_URL + '/dashboard/registers',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                regionCode: R().regionCode,
                branchCode: R().branchCode,
                registerId: R().registerId,
                scoreCardId: R().scoreCardId,
                dateNumber: R().dateNumber,
                pupaTypeId: R().activePupaType(),
                registerText: -1,
                scoreCardTypeId: 0
            })
        }).done(function (res) {
            callback(res);
        }).fail(function () {
            callback([]);
        });
    }

    // Sicil listesini doldur (#scRegisterList); ilk eleman "Tümü" (seçim yok), seçili olan işaretlenir.
    function renderRegisterList(registers) {
        let $list = $('#scRegisterList');
        $list.empty();
        let tumuCls = (R().registerId === -1) ? ' selected' : '';
        $list.append('<div class="dropdown-item tumu-item' + tumuCls + '" data-code="-1">Tümü</div>');
        (registers || []).forEach(function (r) {
            let cls = (r.registerId === R().registerId) ? ' selected' : '';
            $list.append('<div class="dropdown-item' + cls + '" data-code="' + r.registerId + '" data-branch="' + r.branchCode + '" data-region="' + r.regionCode + '">' + r.registerName + '</div>');
        });
    }

    // Liste, seçili bölge/şube bağlamına göre yüklenir; şube değişince o şubenin sicilleri gelir.
    // Servis tek sicil dönerse otomatik seçili setlenir.
    function loadRegisters(done) {
        if (R().dateNumber == null) {
            if (done) done();
            return;
        }
        fetchRegisters(function (res) {
            let registers = Array.isArray(res) ? res : ((res && res.rows) || []);
            var single = registers.length === 1;
            if (single) {
                R().registerId = registers[0].registerId;
                $('#scRegisterLabel').text(registers[0].registerName);
            }
            // Tek sicil dönerse dropdown kilitlenir (hedef ekranındaki gibi disabled)
            $('#scRegisterSelect').toggleClass('disabled', single);
            renderRegisterList(registers);
            if (done) done();
        });
    }

    // Seçili sicili sıfırla (bölge/şube değişince): -1 (Tümü), etiket varsayılana döner.
    function resetRegisterFilter() {
        R().registerId = -1;
        $('#scRegisterLabel').text('Sicil No');
    }

    // Arama: tabloyu yeniden çizer (query renderReportBody içinde uygulanır)
    $('#scSearchInput').on('input', function () { R().renderBody(); });

    // Bölge / Şube / Sicil dropdown seçim handler'ları (kodlar number; -1 = Tümü).
    // Init: bölgeler loadPupaFilters'ta, şubeler loadScoreCardTypes'ta (scoreCardId hazır olunca) yüklenir.
    $(document).on('click', '#scRegionList .dropdown-item', function () {
        var code = parseInt($(this).attr('data-code'), 10);   // number; -1 = Tümü
        $('#scRegionLabel').text(code === -1 ? 'Bölge' : $(this).text());
        $('#scRegionPanel').removeClass('open');
        $('#scRegionList .dropdown-item').removeClass('selected');
        $(this).addClass('selected');
        R().regionCode = code;

        // Bölge değişti -> şube sıfırla, şubeleri bu bölge için yeniden yükle, sonra sicil + tablo
        R().branchCode = -1;
        $('#scBranchLabel').text('Şube');
        resetRegisterFilter();
        loadBranches(function () {
            loadRegisters(R().loadTable);
        });
    });
    $(document).on('click', '#scBranchList .dropdown-item', function () {
        var code = parseInt($(this).attr('data-code'), 10);   // number; -1 = Tümü
        $('#scBranchLabel').text(code === -1 ? 'Şube' : $(this).text());
        $('#scBranchPanel').removeClass('open');
        $('#scBranchList .dropdown-item').removeClass('selected');
        $(this).addClass('selected');
        R().branchCode = code;

        // Şube seçilince bağlı bölge otomatik setlensin (data-region number)
        if (code !== -1) {
            R().regionCode = parseInt($(this).attr('data-region'), 10);
            var region = _scRegions.filter(function (r) { return r.regionCode === R().regionCode; })[0];
            $('#scRegionLabel').text(region ? region.regionName : 'Bölge');
            renderScRegionList();
        }

        resetRegisterFilter();                       // şube değişti -> seçili sicil geçersiz
        loadRegisters(R().loadTable);           // o şubenin sicilleri + tablo
    });
    $(document).on('click', '#scRegisterList .dropdown-item', function () {
        var code = parseInt($(this).attr('data-code'), 10);   // number; -1 = Tümü
        $('#scRegisterLabel').text(code === -1 ? 'Sicil No' : $(this).text());
        $('#scRegisterPanel').removeClass('open');
        $('#scRegisterList .dropdown-item').removeClass('selected');
        $(this).addClass('selected');
        R().registerId = code;

        // Sicil seçilince bağlı şube (ve bölge) seçili gelsin: register.branchCode == branch.branchCode (tip kontrolü yok)
        if (code !== -1) {
            var branchCode = $(this).data('branch');
            var branch = _scBranches.filter(function (b) { return b.branchCode == branchCode; })[0];
            if (branch) {
                R().branchCode = branch.branchCode;
                $('#scBranchLabel').text(branch.branchName);
                renderScBranchList();
                R().regionCode = branch.regionCode;
                var region = _scRegions.filter(function (r) { return r.regionCode === branch.regionCode; })[0];
                $('#scRegionLabel').text(region ? region.regionName : 'Bölge');
                renderScRegionList();
            }
        }

        R().loadTable();
    });

    SC.filters = { loadRegions: loadRegions, loadBranches: loadBranches, loadRegisters: loadRegisters };
});
