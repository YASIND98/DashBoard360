$(function () {
  function getPdfInfo() {
    var pageTitle = $('.page-title').text().trim() || 'Hedef Raporları';
    var dateText = $('.date-text').text().trim() || '';

    var region = $('#indexRegionLabel').text().trim();
    if (region === 'Bölge' || !region) region = 'Tüm Bölgeler';

    var branch = $('#indexBranchLabel').text().trim();
    if (branch === 'Şube' || !branch) branch = 'Tüm Şubeler';

    var segment = $('.segment.active').text().trim() || '';
    var tab = $('.tab.active').text().trim() || '';
    var period = '';
    if ($('.period-toggle').is(':visible')) {
      period = $('.period-btn.active').text().trim() || '';
    }

    // Rapor türü: Hacim/Adet + Bakiye/H/G
    var reportType = segment;
    if (period) reportType += ' - ' + period;

    // Segment: aktif tab
    var segmentLabel = tab;

    // Alt segment: görünen sub-tab-bar'daki aktif sub-tab
    var subSegment = '';
    var $visibleSubBar = $('.sub-tab-bar:visible');
    if ($visibleSubBar.length) {
      var subText = $visibleSubBar.find('.sub-tab.active').text().trim();
      if (subText) subSegment = subText;
    }

    return {
      title: pageTitle,
      date: dateText,
      region: region,
      branch: branch,
      reportType: reportType,
      segment: segmentLabel,
      subSegment: subSegment
    };
  }

  function buildInfoHeader(info) {
    var lineStyle = 'color: #8b95b8; font-size: 14px; font-weight: 400; margin-bottom: 4px;';

    var $header = $('<div></div>').css({
      'background-color': '#060b28',
      'padding': '24px 20px 16px',
      'font-family': 'Inter, sans-serif'
    });

    // Title
    $header.append(
      $('<div></div>').text(info.title).css({
        'color': '#ffffff',
        'font-size': '24px',
        'font-weight': '700',
        'margin-bottom': '12px'
      })
    );

    // Line 1: Tarih + Bölge/Şube
    $header.append(
      $('<div></div>').attr('style', lineStyle)
        .text(info.date + ' tarihine ait ' + info.region + ' / ' + info.branch)
    );

    // Line 2: Rapor türü
    $header.append(
      $('<div></div>').attr('style', lineStyle)
        .text('Rapor Türü: ' + info.reportType)
    );

    // Line 3: Segment + Alt Segment
    var segmentText = 'Segment: ' + info.segment;
    if (info.subSegment) {
      segmentText += '  >  ' + info.subSegment;
    }
    $header.append(
      $('<div></div>').attr('style', lineStyle).text(segmentText)
    );

    return $header;
  }

  $(document).on('click', '.download-pdf-btn', function () {
    var $btn = $(this);
    $btn.css('pointer-events', 'none');

    var $visibleTable = $('.table-container:visible');
    if (!$visibleTable.length) {
      $btn.css('pointer-events', '');
      return;
    }

    var info = getPdfInfo();
    var target = $visibleTable[0];

    var $wrapper = $('<div></div>').css({
      'position': 'absolute',
      'left': '-9999px',
      'top': '0',
      'background-color': '#060b28',
      'width': target.scrollWidth + 'px'
    });

    var $infoHeader = buildInfoHeader(info);
    var $tableClone = $(target).clone().css({ 'display': 'block' });

    // Tüm alt kırılımları açık göster
    $tableClone.find('.sub-row').addClass('visible');
    $tableClone.find('.expandable').addClass('expanded');

    // PDF'te download-pdf-btn (PDF İndir) gizle
    $tableClone.find('.download-pdf-btn').hide();

    // Stripe'ları yeniden hesapla
    var stripeIndex = 0;
    var mainIndex = 0;
    var $lastVisible = null;
    $tableClone.find('tbody tr').each(function () {
      var $tr = $(this);
      $tr.removeClass('stripe-odd stripe-even last-visible-row');
      stripeIndex++;
      if (!$tr.hasClass('sub-row')) {
        mainIndex++;
        $tr.find('td:first').text(mainIndex);
      }
      $tr.addClass(stripeIndex % 2 === 1 ? 'stripe-odd' : 'stripe-even');
      $lastVisible = $tr;
    });
    if ($lastVisible) $lastVisible.addClass('last-visible-row');

    $wrapper.append($infoHeader).append($tableClone);
    $('body').append($wrapper);

    html2canvas($wrapper[0], {
      scale: 2,
      useCORS: true,
      backgroundColor: '#060b28',
      scrollX: 0,
      scrollY: 0,
      width: $wrapper[0].scrollWidth,
      height: $wrapper[0].scrollHeight
    }).then(function (canvas) {
      $wrapper.remove();

      var imgData = canvas.toDataURL('image/png');
      var pdfWidth = canvas.width * 0.264583;
      var pdfHeight = canvas.height * 0.264583;

      var orientation = pdfWidth > pdfHeight ? 'l' : 'p';
      var jsPDF = (window.jspdf && window.jspdf.jsPDF) || window.jsPDF;
      var pdf = new jsPDF(orientation, 'mm', [pdfWidth, pdfHeight]);

      pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, pdfHeight);
      pdf.save('Hedef-Raporlari.pdf');

      $btn.css('pointer-events', '');
    }).catch(function (err) {
      console.error('PDF oluşturma hatası:', err);
      $wrapper.remove();
      $btn.css('pointer-events', '');
    });
  });
});
