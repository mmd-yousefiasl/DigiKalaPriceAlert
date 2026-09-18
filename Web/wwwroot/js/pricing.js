(function () {
    'use strict';

    // --- Any element with [data-service] acts as a toggle button ---
    // --- Any element with [data-service-panel="<name>"] acts as a panel ---
    const toggleBtns = document.querySelectorAll('[data-service]');
    const panels = document.querySelectorAll('[data-service-panel]');

    if (!toggleBtns.length || !panels.length) return;

    // --- ROI banner (optional — will be skipped if not in DOM) ---
    const roiTitle = document.getElementById('roiTitle');
    const roiDesc = document.getElementById('roiDesc');
    const roiCta = document.getElementById('roiCta');
    const hasRoi = roiTitle && roiDesc && roiCta;

    // Per-service ROI copy. Add/remove services freely.
    const roiContent = {
        auto: {
            title: 'آیا ۶۹۰ هزار تومان در ماه هزینه زیادی است؟',
            desc: 'تصور کنید روزانه فقط ۵ فروش را به خاطر تغییر قیمت رقیبتان از دست بدهید. با میانگین سود ۵۰ هزار تومان روی هر کالا، روزانه ۲۵۰ هزار تومان از دست می‌دهید! <b>قیمت‌گذاری خودکار</b> دیجیتو این ضرر را صفر می‌کند — قیمت را بدون دخالت شما همیشه در محدوده رقابتی نگه می‌دارد.',
            ctaText: 'شروع تست رایگان!',
            ctaPlan: '6'
        },
        notify: {
            title: 'آیا ۴۶۰ هزار تومان در ماه هزینه زیادی است؟',
            desc: 'تصور کنید فقط یک بار تغییر قیمت رقیب را از دست بدهید و یک روز کامل بای‌باکس را از دست بدهید. با میانگین سود ۵۰ هزار تومان روی هر کالا، همین یک روز می‌تواند ۲۵۰ هزار تومان ضرر باشد. <b>اعلان فوری قیمت</b> دیجیتو در پیامرسان بله، این ریسک را حذف می‌کند.',
            ctaText: 'شروع تست ۷ روزه!',
            ctaPlan: '1'
        }
    };

    const findPanel = (name) =>
        document.querySelector(`[data-service-panel="${name}"]`);

    // A button is "available" if it isn't disabled AND isn't marked coming-soon
    const isAvailable = (btn) =>
        !btn.disabled && !btn.hasAttribute('data-coming-soon');

    function activate(service, animate) {
        const show = findPanel(service);
        if (!show) return;                       // panel not in DOM → no-op

        // Sync all buttons
        toggleBtns.forEach(b => {
            const on = b.dataset.service === service;
            b.classList.toggle('active', on);
            b.setAttribute('aria-selected', on ? 'true' : 'false');
        });

        // Hide all other panels (instant — no need to animate them out)
        panels.forEach(p => {
            if (p !== show) {
                p.hidden = true;
                p.style.opacity = '';
                p.style.transform = '';
            }
        });

        // Show target panel
        if (animate) {
            show.style.opacity = '0';
            show.style.transform = 'translateY(8px)';
            show.hidden = false;
            requestAnimationFrame(() => {
                show.style.opacity = '1';
                show.style.transform = 'translateY(0)';
            });
        } else {
            show.hidden = false;
            show.style.opacity = '';
            show.style.transform = '';
        }

        // Animate cards inside the visible panel
        show.querySelectorAll('.digito-pricing-card').forEach((c, i) => {
            c.classList.remove('in-view');
            setTimeout(() => c.classList.add('in-view'), i * 80);
        });

        // Update ROI banner copy (only if banner is on the page)
        if (hasRoi && roiContent[service]) {
            const roi = roiContent[service];
            roiTitle.textContent = roi.title;
            roiDesc.innerHTML = roi.desc;
            roiCta.textContent = roi.ctaText;
            if (roi.ctaPlan) {
                roiCta.setAttribute('href', `/subscription/purchase/${roi.ctaPlan}`);
            }
        }
    }

    // --- Click handlers ---
    toggleBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            if (!isAvailable(btn)) return;
            activate(btn.dataset.service, true);
        });
    });

    // --- Init: pick whichever button is .active (and available), else the first available one ---
    const initial =
        Array.from(toggleBtns).find(b => b.classList.contains('active') && isAvailable(b)) ||
        Array.from(toggleBtns).find(isAvailable);

    if (initial) activate(initial.dataset.service, false);

    // --- Scroll-in animation ---
    const cards = document.querySelectorAll('.digito-pricing-card');
    if ('IntersectionObserver' in window) {
        const io = new IntersectionObserver((entries) => {
            entries.forEach((e, i) => {
                if (e.isIntersecting) {
                    setTimeout(() => e.target.classList.add('in-view'), i * 80);
                    io.unobserve(e.target);
                }
            });
        }, { threshold: 0.15 });
        cards.forEach(c => io.observe(c));
    } else {
        cards.forEach(c => c.classList.add('in-view'));
    }
})();