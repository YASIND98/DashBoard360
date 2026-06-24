function formatPercent(ratio) {
    if (!ratio) return "-";
    var rounded = Math.round(ratio * 10) / 10;
    if (rounded % 1 === 0) return rounded;
    return rounded.toFixed(1).replace('.', '.<small>') + '</small>';
}


function percentColor(ratio) {
    if (!ratio) return '';
    if (ratio < 75) return 'ratio-red';
    if (ratio < 100) return 'ratio-orange';
    if (ratio < 120) return 'ratio-green';
    return 'ratio-blue';
}

function formatNumber(value, isPrice, productName) {
    if (!value) return "-";
    var num = new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 0 }).format(value);
    if (!isPrice) return num;
    var currency = (productName && productName.indexOf('YP') !== -1) ? '$' : '₺';
    return currency + ' ' + num;
}

// Türkçe ay adları (formatReportDateTr için)
var _trMonths = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'];

// Rapor tarihi (ISO) -> "5 Mayıs 2026"
function formatReportDateTr(iso) {
    var d = new Date(iso);
    if (isNaN(d.getTime())) return '';
    return d.getDate() + ' ' + _trMonths[d.getMonth()] + ' ' + d.getFullYear();
}

// ISO tarih ("2026-05-05T00:00:00") -> "05.05.2026"
function fmtIsoDate(v) {
    if (!v) return '';
    var m = /^(\d{4})-(\d{2})-(\d{2})/.exec(String(v));   // yıl-ay-gün yakala
    return m ? (m[3] + '.' + m[2] + '.' + m[1]) : String(v);   // gün.ay.yıl
}
