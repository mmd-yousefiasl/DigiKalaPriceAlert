// ─── Utility Functions ───
function toEnNum(str) {
    if (!str) return '';
    return str.replace(/[۰-۹]/g, function (d) {
        return '۰۱۲۳۴۵۶۷۸۹'.indexOf(d);
    }).replace(/,/g, '');
}

function toFaNum(num) {
    if (num === null || num === undefined) return '';
    return num.toString().replace(/\d/g, function (d) {
        return '۰۱۲۳۴۵۶۷۸۹'[d];
    });
}

function formatNumber(num) {
    if (isNaN(num) || num === null) return '—';
    var isNeg = num < 0;
    var abs = Math.abs(Math.round(num));
    var formatted = abs.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    var fa = toFaNum(formatted);
    return (isNeg ? '−' : '') + fa + ' تومان';
}

function formatPercent(num) {
    if (isNaN(num)) return '۰٪';
    return toFaNum(Math.abs(num).toFixed(1)) + '٪';
}

function getNumVal(id, defaultVal) {
    var el = document.getElementById(id);
    if (!el) return defaultVal || 0;
    var val = toEnNum(el.value.trim());
    var num = parseFloat(val);
    return isNaN(num) ? (defaultVal || 0) : num;
}

// ─── Format inputs live ───
function formatInputLive(el) {
    if (!el) return;
    el.addEventListener('input', function () {
        var cursorPos = this.selectionStart;
        var oldLen = this.value.length;

        var raw = toEnNum(this.value);
        raw = raw.replace(/[^\d]/g, '');
        if (raw === '') {
            this.value = '';
            return;
        }
        var formatted = parseInt(raw).toLocaleString('en-US');
        this.value = toFaNum(formatted);

        var newLen = this.value.length;
        var diff = newLen - oldLen;
        var newPos = cursorPos + diff;
        if (newPos < 0) newPos = 0;
        try { this.setSelectionRange(newPos, newPos); } catch (e) { }
    });
}

var numericFieldIds = ['purchasePrice', 'sellingPrice', 'packageWeight', 'shippingCost', 'returnReserve', 'vatRate'];
numericFieldIds.forEach(function (id) {
    formatInputLive(document.getElementById(id));
});

// ─── Main Calculation ───
function calculateProfit() {
    var purchasePrice = getNumVal('purchasePrice');
    var sellingPrice = getNumVal('sellingPrice');
    var categoryEl = document.getElementById('categorySelect');
    var commissionRate = categoryEl.value ? parseFloat(categoryEl.value) / 100 : 0.07;
    var shippingCost = getNumVal('shippingCost', 22500);
    var returnReserve = getNumVal('returnReserve', 20000);
    var vatRate = getNumVal('vatRate', 10) / 100;

    var shippingType = document.getElementById('shippingType').value;
    if (shippingType === 'fbd') {
        shippingCost = Math.max(shippingCost, 35000);
    }

    // Validation
    if (purchasePrice <= 0 || sellingPrice <= 0) {
        alert('لطفاً قیمت خرید و قیمت فروش را وارد کنید.');
        return;
    }
    if (!categoryEl.value) {
        alert('لطفاً دسته‌بندی کالا را انتخاب کنید.');
        return;
    }

    // Calculations
    var commission = sellingPrice * commissionRate;
    var vat = commission * vatRate;
    var totalCosts = purchasePrice + commission + vat + shippingCost + returnReserve;
    var netProfit = sellingPrice - totalCosts;
    var marginPercent = (netProfit / sellingPrice) * 100;

    // Price Floor formula
    var divisor = 1 - commissionRate - (vatRate * commissionRate);
    var priceFloor = divisor > 0 ? (purchasePrice + shippingCost + returnReserve) / divisor : 0;

    // Show results
    document.getElementById('resultPlaceholder').style.display = 'none';
    document.getElementById('resultContent').style.display = 'block';
    document.getElementById('resultCta').style.display = 'block';

    // Profit box styling
    var profitBox = document.getElementById('profitBox');
    profitBox.className = 'result-profit-box';
    if (netProfit > 100) {
        profitBox.classList.add('profit-positive');
        document.getElementById('profitLabel').textContent = '✅ سود خالص هر سفارش';
    } else if (netProfit < -100) {
        profitBox.classList.add('profit-negative');
        document.getElementById('profitLabel').textContent = '❌ زیان خالص هر سفارش';
    } else {
        profitBox.classList.add('profit-zero');
        document.getElementById('profitLabel').textContent = '⚖️ نقطه سربه‌سر';
    }

    document.getElementById('profitAmount').textContent = formatNumber(netProfit);
    document.getElementById('profitMargin').textContent = 'مارجین: ' + (netProfit >= 0 ? '' : '−') + formatPercent(marginPercent);

    // Breakdown values
    document.getElementById('bdSelling').textContent = formatNumber(sellingPrice);
    document.getElementById('bdPurchase').textContent = '−' + formatNumber(purchasePrice).replace('−', '');
    document.getElementById('bdCommission').textContent = '−' + formatNumber(commission).replace('−', '') + ' (' + toFaNum((commissionRate * 100).toFixed(0)) + '٪)';
    document.getElementById('bdVat').textContent = '−' + formatNumber(vat).replace('−', '');
    document.getElementById('bdShipping').textContent = '−' + formatNumber(shippingCost).replace('−', '');
    document.getElementById('bdReturn').textContent = '−' + formatNumber(returnReserve).replace('−', '');

    // Price floor
    document.getElementById('priceFloorValue').textContent = formatNumber(Math.ceil(priceFloor));

    // Visual bar percentages
    var barBase = sellingPrice > 0 ? sellingPrice : 1;
    var commPct = (commission / barBase) * 100;
    var vatPct = (vat / barBase) * 100;
    var shipPct = (shippingCost / barBase) * 100;
    var retPct = (returnReserve / barBase) * 100;
    var profPct = Math.max(0, (netProfit / barBase) * 100);

    document.getElementById('barCommission').style.width = commPct.toFixed(1) + '%';
    document.getElementById('barVat').style.width = vatPct.toFixed(1) + '%';
    document.getElementById('barShipping').style.width = shipPct.toFixed(1) + '%';
    document.getElementById('barReturn').style.width = retPct.toFixed(1) + '%';
    document.getElementById('barProfit').style.width = profPct.toFixed(1) + '%';

    // CTA dynamic text
    if (netProfit > 0) {
        var priceDiffPercent = (netProfit / sellingPrice) * 100;
        document.getElementById('ctaDiffPercent').textContent = formatPercent(priceDiffPercent);
        document.getElementById('ctaNewProfit').textContent = 'صفر';
    } else {
        document.getElementById('ctaDiffPercent').textContent = '—';
        document.getElementById('ctaNewProfit').textContent = 'منفی‌تر';
    }

    // Scroll to results on mobile
    if (window.innerWidth <= 900) {
        setTimeout(function () {
            var el = document.getElementById('resultContent');
            if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }, 100);
    }
}