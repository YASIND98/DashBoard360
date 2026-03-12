function formatPercent(ratio) {
  return Math.round(ratio * 100) + '%';
}

function ratioClass(ratio) {
  var pct = ratio * 100;
  if (pct <= 49) return 'ratio-red';
  if (pct <= 100) return 'ratio-green';
  return 'ratio-blue';
}

function formatNumber(value) {
  return new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 0 }).format(value);
}
