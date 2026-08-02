// ── History Filtering ──────────────────────────────────
var activeSellerFilter = 'all';

function filterHistory(value, clickedChip) {
    activeSellerFilter = value;

    // Update chip active states
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
        var itemSeller = item.getAttribute('data-seller');

        var sellerMatch = activeSellerFilter === 'all' || itemSeller === activeSellerFilter;

        if (sellerMatch) {
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