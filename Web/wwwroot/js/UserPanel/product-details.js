
// ── History Filtering ──────────────────────────────────
var activeColorFilter = 'all';
var activeSellerFilter = 'all';

function filterHistory(type, value, clickedChip) {
    // Update active filter
    if (type === 'color') activeColorFilter = value;
    if (type === 'seller') activeSellerFilter = value;

    // Update chip active states within the same group
    var parentGroup = clickedChip.parentElement;
    var siblings = parentGroup.querySelectorAll('.filter-chip');
    for (var i = 0; i < siblings.length; i++) {
        siblings[i].classList.remove('active');
    }
    clickedChip.classList.add('active');

    // Filter history items
    var items = document.querySelectorAll('#history-container .history-item');
    var visibleCount = 0;

    for (var i = 0; i < items.length; i++) {
        var item = items[i];
        var itemColor = item.getAttribute('data-color');
        var itemSeller = item.getAttribute('data-seller');

        var colorMatch = activeColorFilter === 'all' || itemColor === activeColorFilter;
        var sellerMatch = activeSellerFilter === 'all' || itemSeller === activeSellerFilter;

        if (colorMatch && sellerMatch) {
            item.classList.remove('hidden');
            visibleCount++;
        } else {
            item.classList.add('hidden');
        }
    }

    // Update count
    var countEl = document.getElementById('history-count');
    if (countEl) countEl.textContent = visibleCount + ' تغییر';

    // Show/hide empty state
    var emptyEl = document.getElementById('history-empty');
    if (emptyEl) emptyEl.style.display = visibleCount === 0 ? 'block' : 'none';
}

function toggleColor(id) {
    var body = document.getElementById('cb-' + id);
    var header = document.getElementById('ch-' + id);
    var chevron = document.getElementById('chev-' + id);

    if (!body) return;

    var isOpen = body.classList.contains('show');

    if (isOpen) {
        body.classList.remove('show');
        header.classList.remove('expanded');
        if (chevron) chevron.classList.remove('rotated');
    } else {
        body.classList.add('show');
        header.classList.add('expanded');
        if (chevron) chevron.classList.add('rotated');
    }
}