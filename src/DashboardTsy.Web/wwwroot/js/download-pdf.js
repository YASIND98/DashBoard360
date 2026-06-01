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
      // PDF'te max-height ile içerik kesilmesin, dikey scroll/sınır kalkmasın
      $tableClone.find('.table-wrapper').css({ 'overflow': 'visible', 'max-height': 'none', 'min-height': 'auto' });
      // Sticky header html2canvas'ta yanlış konumlanmasın diye normal akışa al
      $tableClone.find('thead th').css({ 'position': 'static', 'top': 'auto' });

      // PDF için tablo renklerini beyaz arka plana uygun yap
      $tableClone.find('th').css({ 'color': '#333333', 'background': '#ffffff' });
      $tableClone.find('td').css({ 'color': '#000000', 'border-color': '#e0e0e0' });
      $tableClone.find('.col-text').each(function () {
        this.style.setProperty('color', '#000000', 'important');
      });
      $tableClone.find('.sub-index').each(function () {
        this.style.setProperty('color', '#000000', 'important');
      });
      $tableClone.find('.diff-label').css('color', '#454b54');
      $tableClone.find('.legend-note').css('color', '#000000');
      $tableClone.find('.legend-value').css('color', '#000000');

      // Tüm alt kırılımları açık göster
      $tableClone.find('.sub-row').addClass('visible');
      $tableClone.find('.expandable').addClass('expanded');

      
      $tableClone.find('.legend-actions').hide();

      // PDF'te ikonları beyaz zeminde okunur yap. html2canvas CSS `filter`'ı img'lere
      // güvenilir uygulamadığı için expand & top-10 ikonlarını siyah renkli inline
      // SVG ile (img öğesini doğrudan svg ile) değiştiriyoruz.
      var blackExpandSvg =
        '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none">' +
          '<path d="M15 22.75H9C3.57 22.75 1.25 20.43 1.25 15V9C1.25 3.57 3.57 1.25 9 1.25H15C20.43 1.25 22.75 3.57 22.75 9V15C22.75 20.43 20.43 22.75 15 22.75ZM9 2.75C4.39 2.75 2.75 4.39 2.75 9V15C2.75 19.61 4.39 21.25 9 21.25H15C19.61 21.25 21.25 19.61 21.25 15V9C21.25 4.39 19.61 2.75 15 2.75H9Z" fill="#000000"/>' +
          '<path d="M15.5302 14.2091C15.3402 14.2091 15.1502 14.1391 15.0002 13.9891L12.0002 10.9891L9.00016 13.9891C8.71016 14.2791 8.23016 14.2791 7.94016 13.9891C7.65016 13.6991 7.65016 13.2191 7.94016 12.9291L11.4702 9.39914C11.7602 9.10914 12.2402 9.10914 12.5302 9.39914L16.0602 12.9291C16.3502 13.2191 16.3502 13.6991 16.0602 13.9891C15.9102 14.1391 15.7202 14.2091 15.5302 14.2091Z" fill="#000000"/>' +
        '</svg>';
      $tableClone.find('.expand-icon img').replaceWith(blackExpandSvg);

      var blackTopTenSvg =
        '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none">' +
          '<path d="M13 22.75H9C3.57 22.75 1.25 20.43 1.25 15V9C1.25 3.57 3.57 1.25 9 1.25H15C20.43 1.25 22.75 3.57 22.75 9V13C22.75 13.41 22.41 13.75 22 13.75C21.59 13.75 21.25 13.41 21.25 13V9C21.25 4.39 19.61 2.75 15 2.75H9C4.39 2.75 2.75 4.39 2.75 9V15C2.75 19.61 4.39 21.25 9 21.25H13C13.41 21.25 13.75 21.59 13.75 22C13.75 22.41 13.41 22.75 13 22.75Z" fill="#000000"/>' +
          '<path d="M7.33009 15.24C7.17009 15.24 7.0101 15.19 6.8701 15.08C6.5401 14.83 6.48012 14.36 6.73012 14.03L9.11009 10.94C9.40009 10.57 9.81011 10.33 10.2801 10.27C10.7501 10.21 11.2101 10.34 11.5801 10.63L13.4101 12.07C13.4801 12.13 13.5501 12.12 13.6001 12.12C13.6401 12.12 13.7101 12.1 13.7701 12.02L16.0801 9.04C16.3301 8.71 16.8001 8.65001 17.1301 8.91001C17.4601 9.16001 17.5201 9.63001 17.2601 9.96001L14.9501 12.94C14.6601 13.31 14.2501 13.55 13.7801 13.6C13.3201 13.66 12.8501 13.53 12.4901 13.24L10.6601 11.8C10.5901 11.74 10.5101 11.74 10.4701 11.75C10.4301 11.75 10.3601 11.77 10.3001 11.85L7.92012 14.94C7.78012 15.14 7.56009 15.24 7.33009 15.24Z" fill="#000000"/>' +
          '<path d="M20.26 22.75C19.91 22.75 19.46 22.64 18.93 22.32L18.68 22.17C18.61 22.13 18.3999 22.13 18.3299 22.17L18.0799 22.32C16.9299 23.01 16.2 22.72 15.88 22.48C15.55 22.24 15.04 21.64 15.34 20.32L15.39 20.11C15.41 20.03 15.3499 19.84 15.2999 19.78L14.95 19.43C14.36 18.83 14.1299 18.13 14.3299 17.5C14.5299 16.88 15.12 16.44 15.95 16.3L16.3299 16.24C16.3999 16.22 16.5399 16.12 16.5799 16.05L16.8599 15.48C17.2499 14.69 17.85 14.24 18.51 14.24C19.17 14.24 19.77 14.69 20.16 15.48L20.44 16.04C20.48 16.11 20.62 16.21 20.69 16.23L21.07 16.29C21.9 16.43 22.49 16.87 22.69 17.49C22.89 18.11 22.67 18.81 22.07 19.42L21.72 19.77C21.67 19.83 21.61 20.02 21.63 20.1L21.68 20.31C21.98 21.63 21.47 22.23 21.14 22.47C20.96 22.61 20.67 22.75 20.26 22.75ZM18.49 15.75C18.48 15.76 18.34 15.86 18.2 16.15L17.92 16.72C17.68 17.21 17.1099 17.63 16.5799 17.72L16.2 17.78C15.88 17.83 15.77 17.94 15.76 17.96C15.76 17.98 15.79 18.14 16.02 18.37L16.37 18.72C16.78 19.14 16.9899 19.86 16.8599 20.43L16.81 20.64C16.72 21.03 16.76 21.2 16.78 21.26C16.81 21.24 16.98 21.22 17.31 21.02L17.56 20.87C18.11 20.54 18.9 20.54 19.45 20.87L19.7 21.02C20.11 21.27 20.28 21.24 20.29 21.24C20.25 21.24 20.3 21.04 20.21 20.64L20.16 20.43C20.03 19.85 20.24 19.14 20.65 18.72L21 18.37C21.23 18.14 21.26 17.98 21.26 17.95C21.25 17.93 21.14 17.83 20.82 17.77L20.44 17.71C19.9 17.62 19.34 17.2 19.1 16.71L18.82 16.15C18.66 15.85 18.52 15.76 18.49 15.75Z" fill="#000000"/>' +
        '</svg>';
      $tableClone.find('.top10-icon').replaceWith(blackTopTenSvg);

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
