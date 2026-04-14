$(function () {
  function getPdfInfo() {
    var pageTitle = $('.page-title').text().trim();
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
    var lineStyle = 'color: #000000; font-size: 14px; font-weight: 400; margin-bottom: 4px;';

    var $header = $('<div></div>').css({
      'background-color': '#ffffff',
      'padding': '24px 20px 16px',
      'font-family': 'Inter, sans-serif'
    });

    // Title
    $header.append(
      $('<div></div>').text(info.title).css({
        'color': '#000000',
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
    if (info.reportType) {
      $header.append(
        $('<div></div>').attr('style', lineStyle)
          .text('Rapor Türü: ' + info.reportType)
      );
    }

    // Line 3: Segment + Alt Segment
    if (info.segment) {
      var segmentText = 'Segment: ' + info.segment;
      if (info.subSegment) {
        segmentText += '  >  ' + info.subSegment;
      }
      $header.append(
        $('<div></div>').attr('style', lineStyle).text(segmentText)
      );
    }

    return $header;
  }

  $(document).on('click', '.download-pdf-btn', function () {
    var $btn = $(this);
    $btn.css('pointer-events', 'none');
    var originalHtml = $btn.html();
    $btn.html('<div class="pdf-spinner"></div> <span>PDF Yükleniyor...</span>');

    var $visibleTable = $('.table-container:visible');
    if (!$visibleTable.length) {
      $btn.css('pointer-events', '');
      return;
    }

    var info = getPdfInfo();

    var $wrapper = $('<div></div>').css({
      'position': 'absolute',
      'left': '-9999px',
      'top': '0',
      'background-color': '#ffffff',
      'color': '#000000'
    });

    var $infoHeader = buildInfoHeader(info);
    $wrapper.append($infoHeader);

    // Her görünür tablo container'ını klonla ve ekle
    $visibleTable.each(function () {
      var $tableClone = $(this).clone().css({ 'display': 'block', 'margin-bottom': '16px', 'overflow': 'visible', 'background': '#ffffff' });
      $tableClone.find('.table-wrapper').css('overflow', 'visible');

      // PDF için tablo renklerini beyaz arka plana uygun yap
      $tableClone.find('th').css('color', '#333333');
      $tableClone.find('td').css({ 'color': '#000000', 'border-color': '#e0e0e0' });
      $tableClone.find('.col-text').each(function () {
        this.style.setProperty('color', '#000000', 'important');
      });
      $tableClone.find('.diff-label').css('color', '#454b54');
      $tableClone.find('.legend-note').css('color', '#000000');
      $tableClone.find('.legend-value').css('color', '#000000');

      // Tüm alt kırılımları açık göster
      $tableClone.find('.sub-row').addClass('visible');
      $tableClone.find('.expandable').addClass('expanded');

      
      $tableClone.find('.legend-actions').hide();

      // Stripe'ları yeniden hesapla
      $tableClone.find('.data-table').each(function () {
        var stripeIndex = 0;
        var mainIndex = 0;
        var $lastVisible = null;
        $(this).find('tbody tr').each(function () {
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
      });

      // Stripe renklerini beyaz tema için override et
      $tableClone.find('.stripe-odd').css('background', '#f5f5f5');
      $tableClone.find('.stripe-even').css('background', '#ffffff');

      $wrapper.append($tableClone);
    });

    $('body').append($wrapper);

    // DOM'a eklendikten sonra gerçek genişliği hesapla ve uygula
    var realWidth = $wrapper[0].scrollWidth;
    $wrapper.css('width', realWidth + 'px');
    $wrapper.find('.table-container').css('width', realWidth + 'px');
    $wrapper.find('.data-table').css('width', '100%');

    // Minimum yükseklik: A4 landscape oranı (297/210) ile genişliğe göre hesapla
    var minHeight = Math.round(realWidth * (210 / 297));
    var actualHeight = $wrapper[0].scrollHeight;
    if (actualHeight < minHeight) {
      $wrapper.css('height', minHeight + 'px');
    }

    html2canvas($wrapper[0], {
      scale: 2,
      useCORS: true,
      backgroundColor: '#ffffff',
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

      $btn.html(originalHtml);
      $btn.css('pointer-events', '');
    }).catch(function (err) {
      console.error('PDF oluşturma hatası:', err);
      $wrapper.remove();
      $btn.html(originalHtml);
      $btn.css('pointer-events', '');
    });
  });
});
