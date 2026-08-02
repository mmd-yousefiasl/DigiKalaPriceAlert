document.addEventListener("DOMContentLoaded", function () {
    const searchForm = document.getElementById("searchForm");
    const urlInput = document.getElementById("urlInput");
    const modal = new bootstrap.Modal(document.getElementById('productModal'));
    const modalBody = document.getElementById("productModalBody");

    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    /* ============================================================
       Helper: SweetAlert (Styled)
    ============================================================ */
    function showSwal({ title, text, icon = 'success', confirmText = 'متوجه شدم' }) {
        const iconColors = {
            success: '#10b981',
            error: '#ef4444',
            warning: '#f59e0b',
            info: '#3b82f6'
        };

        return Swal.fire({
            title: title,
            text: text,
            icon: icon,
            iconColor: iconColors[icon],
            confirmButtonText: confirmText,
            confirmButtonColor: '#1a1a1a',
            background: '#fff',
            color: '#1a1a1a',
            buttonsStyling: true,
            showClass: { popup: 'animate__animated animate__fadeIn animate__faster' },
            hideClass: { popup: 'animate__animated animate__fadeOut animate__faster' },
            customClass: {
                popup: 'custom-swal-popup',
                title: 'custom-swal-title',
                htmlContainer: 'custom-swal-text',
                confirmButton: 'custom-swal-btn',
                icon: 'swal2-icon-styled'
            }
        });
    }

    /* ============================================================
       Helper: Button State Management
    ============================================================ */
    function setButtonLoading(btn) {
        if (!btn.dataset.originalHtml) {
            btn.dataset.originalHtml = btn.innerHTML;
        }
        btn.disabled = true;
        btn.classList.remove('btn-tracked-error');
        btn.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> در حال افزودن...`;
    }

    function setButtonSuccess(btn, customText = 'در حال رصد') {
        btn.classList.remove('btn-dark', 'btn-add-tracking', 'btn-tracked-error');
        btn.classList.add('btn-light', 'btn-tracked');
        btn.disabled = true;
        btn.removeAttribute('data-seller-id');
        btn.innerHTML = `<i class="mdi mdi-check-circle text-success"></i> ${customText}`;
    }

    function setButtonError(btn, errorText = 'افزودن ناموفق') {
        btn.disabled = false;
        btn.classList.remove('btn-dark', 'btn-add-tracking');
        btn.classList.add('btn-tracked-error');
        btn.innerHTML = `<i class="mdi mdi-alert-circle-outline"></i> ${errorText}`;

        // After 2.5s, restore to original button so user can retry
        setTimeout(() => {
            if (btn.dataset.originalHtml) {
                btn.innerHTML = btn.dataset.originalHtml;
                delete btn.dataset.originalHtml;
            }
            btn.classList.remove('btn-tracked-error');
            btn.classList.add('btn-dark', 'btn-add-tracking');
            btn.disabled = false;
        }, 2500);
    }

    function setButtonAlreadyTracked(btn) {
        btn.classList.remove('btn-dark', 'btn-add-tracking', 'btn-tracked-error');
        btn.classList.add('btn-light', 'btn-tracked');
        btn.disabled = true;
        btn.removeAttribute('data-seller-id');
        btn.innerHTML = `<i class="mdi mdi-check-circle text-success"></i> قبلاً اضافه شده`;
    }

    /* ============================================================
       Detect "Already Tracked" error from server message
    ============================================================ */
    function isAlreadyTrackedMessage(message) {
        if (!message) return false;
        const msg = message.toLowerCase();
        const keywords = ['قبلا', 'اضافه شده', 'وجود دارد', 'تکراری', 'در حال رصد'];
        return keywords.some(k => message.includes(k) || msg.includes(k));
    }

    /* ============================================================
       Search Logic
    ============================================================ */
    async function performSearch(url) {
        modalBody.innerHTML = `
            <div class="text-center p-5">
                <div class="spinner-border text-dark" role="status"></div>
                <p class="mt-2 font-size-12 text-muted">در حال دریافت اطلاعات...</p>
            </div>`;
        modal.show();

        try {
            const response = await fetch('/user/products/search', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiForgeryToken || ''
                },
                body: JSON.stringify({ url: url })
            });

            const result = await response.json();

            if (result.success) {
                renderModalContent(result.data);
            } else {
                modalBody.innerHTML = `
                    <div class="alert alert-danger font-size-12 mb-0 d-flex align-items-center gap-2" style="border-radius: 10px;">
                        <i class="mdi mdi-alert-circle-outline font-size-18"></i>
                        <span>${result.message}</span>
                    </div>`;
            }
        } catch (error) {
            modalBody.innerHTML = `
                <div class="alert alert-danger font-size-12 mb-0 d-flex align-items-center gap-2" style="border-radius: 10px;">
                    <i class="mdi mdi-wifi-off font-size-18"></i>
                    <span>خطا در ارتباط با سرور. لطفاً دوباره تلاش کنید.</span>
                </div>`;
        }
    }

    searchForm.addEventListener("submit", function (e) {
        e.preventDefault();
        const url = urlInput.value.trim();
        if (!url) {
            showSwal({
                title: 'لینک محصول الزامی است',
                text: 'لطفاً لینک محصول دیجی‌کالا را وارد کنید.',
                icon: 'warning'
            });
            return;
        }
        performSearch(url);
    });

    /* ============================================================
       Render Modal Content
    ============================================================ */
    function renderModalContent(product) {
        let html = `
        <div class="d-flex gap-3 mb-4 align-items-center">
            <img src="${product.imageUrl}" alt="product" 
                 style="width: 80px; height: 80px; object-fit: contain; border-radius: 8px; border: 1px solid #f0f0f0;" />
            <div>
                <h6 class="fw-bold font-size-14 text-dark mb-0">${product.title}</h6>
                <small class="text-muted font-size-11">در زیر، رنگ‌ها و فروشندگان فعال این محصول نمایش داده شده‌اند.</small>
            </div>
        </div>`;

        product.options.forEach(option => {
            html += `
            <div class="variant-group mb-4">
                <div class="d-flex align-items-center gap-2 mb-2">
                    <span class="color-circle" style="width: 14px; height: 14px; background-color: ${option.colorHex}; border-radius: 50%; display: inline-block;"></span>
                    <span class="fw-bold font-size-13 text-dark">${option.colorName}</span>
                </div>
                <div class="list-group">`;

            option.sellers.forEach(seller => {
                const isWinner = seller.isBuyBoxWinner;
                const isTracked = seller.isAlreadyTracked;

                let buttonHtml = "";
                if (isTracked) {
                    buttonHtml = `<button type="button" class="btn btn-sm btn-light font-size-11 btn-tracked" disabled style="border-radius: 8px; min-width: 130px;"><i class="mdi mdi-check-circle text-success"></i> در حال رصد</button>`;
                } else {
                    buttonHtml = `<button type="button" class="btn btn-sm btn-dark font-size-11 btn-add-tracking" data-seller-id="${seller.sellerId}" style="border-radius: 8px; min-width: 130px;"><i class="mdi mdi-radar"></i> افزودن به رصد</button>`;
                }

                html += `
                <div class="list-group-item d-flex justify-content-between align-items-center" style="border-radius: 8px; margin-bottom: 8px; border: 1px solid #f0f0f0; padding: 12px 15px;">
                    <div>
                        <span class="font-size-12 fw-bold text-dark">${seller.sellerName}</span>
                        ${isWinner ? '<span class="badge bg-success-subtle text-success font-size-10 ms-2">برنده بای‌باکس</span>' : ''}
                        <div class="font-size-12 text-muted mt-1">${seller.price.toLocaleString('fa-IR')} تومان</div>
                    </div>
                    <div>${buttonHtml}</div>
                </div>`;
            });

            html += `</div></div>`;
        });

        modalBody.innerHTML = html;

        // Attach click handlers
        document.querySelectorAll('.btn-add-tracking').forEach(btn => {
            btn.addEventListener('click', handleAddTrackingClick);
        });
    }

    /* ============================================================
       Handle Add-Tracking Click
    ============================================================ */
    async function handleAddTrackingClick() {
        const btn = this;
        const sellerId = btn.getAttribute('data-seller-id');

        setButtonLoading(btn);

        try {
            const response = await fetch('/user/products/add-tracking', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiForgeryToken || ''
                },
                body: JSON.stringify({ sellerId: parseInt(sellerId) })
            });

            const result = await response.json();

            if (result.success) {
                // ✅ Success
                setButtonSuccess(btn);
                showSwal({
                    title: 'افزوده شد!',
                    text: result.message,
                    icon: 'success',
                    confirmText: 'عالیه'
                });
            } else {
                // Check if it's an "already tracked" scenario
                if (isAlreadyTrackedMessage(result.message)) {
                    setButtonAlreadyTracked(btn);
                    showSwal({
                        title: 'قبلاً اضافه شده',
                        text: result.message,
                        icon: 'info',
                        confirmText: 'باشه'
                    });
                } else {
                    // ❌ Real error → show error state on button, then restore
                    setButtonError(btn, 'افزودن ناموفق');
                    showSwal({
                        title: 'خطا در افزودن',
                        text: result.message,
                        icon: 'error',
                        confirmText: 'تلاش مجدد'
                    });
                }
            }
        } catch (error) {
            setButtonError(btn, 'خطای شبکه');
            showSwal({
                title: 'خطای ارتباط با سرور',
                text: 'امکان برقراری ارتباط با سرور وجود ندارد. لطفاً اتصال اینترنت خود را بررسی کنید.',
                icon: 'error',
                confirmText: 'متوجه شدم'
            });
        }
    }
});