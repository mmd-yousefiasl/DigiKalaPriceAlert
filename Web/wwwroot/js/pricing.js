(function () {
    const toggleBtns = document.querySelectorAll('.digito-toggle-btn');
    const autoGrid = document.getElementById('autoPricing');
    const notifySec = document.getElementById('notifyPricing');

    // ROI banner elements
    const roiTitle = document.getElementById('roiTitle');
    const roiDesc = document.getElementById('roiDesc');
    const roiCta = document.getElementById('roiCta');

    const roiContent = {
        auto: {
            title: 'آیا ۶۹۰ هزار تومان در ماه هزینه زیادی است؟',
            desc: 'تصور کنید روزانه فقط ۵ فروش را به خاطر تغییر قیمت رقیبتان از دست بدهید. با میانگین سود ۵۰ هزار تومان روی هر کالا، روزانه ۲۵۰ هزار تومان از دست می‌دهید! <b>قیمت‌گذاری خودکار</b> دیجیتو این ضرر را صفر می‌کند — قیمت را بدون دخالت شما همیشه در محدوده رقابتی نگه می‌دارد.',
            ctaText: 'شروع تست رایگان!',
            ctaHref: roiCta.dataset.autoPlan ? `/subscription/purchase/${roiCta.dataset.autoPlan}` : '#'
        },
        notify: {
            title: 'آیا ۴۶۰ هزار تومان در ماه هزینه زیادی است؟',
            desc: 'تصور کنید فقط یک بار تغییر قیمت رقیب را از دست بدهید و یک روز کامل بای‌باکس را از دست بدهید. با میانگین سود ۵۰ هزار تومان روی هر کالا، همین یک روز می‌تواند ۲۵۰ هزار تومان ضرر باشد. <b>اعلان فوری قیمت</b> دیجیتو در تلگرام، این ریسک را حذف می‌کند.',
            ctaText: 'شروع تست ۷ روزه!',
            ctaHref: roiCta.dataset.notifyPlan ? `/subscription/purchase/${roiCta.dataset.notifyPlan}` : '#'
        }
    };

    toggleBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const service = btn.dataset.service;

            toggleBtns.forEach(b => {
                b.classList.toggle('active', b === btn);
                b.setAttribute('aria-selected', b === btn);
            });

            // --- Swap grids ---
            const show = service === 'auto' ? autoGrid : notifySec;
            const hide = service === 'auto' ? notifySec : autoGrid;

            hide.style.opacity = '0';
            hide.style.transform = 'translateY(8px)';

            // --- Swap ROI banner text ---
            const roi = roiContent[service];
            roiTitle.style.opacity = '0';
            roiDesc.style.opacity = '0';

            setTimeout(() => {
                hide.hidden = true;
                show.hidden = false;
                show.style.opacity = '0';
                show.style.transform = 'translateY(8px)';

                roiTitle.textContent = roi.title;
                roiDesc.innerHTML = roi.desc;
                roiCta.textContent = roi.ctaText;
                roiCta.setAttribute('href', roi.ctaHref);

                requestAnimationFrame(() => {
                    show.style.opacity = '1';
                    show.style.transform = 'translateY(0)';

                    show.querySelectorAll('.digito-pricing-card').forEach((c, i) => {
                        c.classList.remove('in-view');
                        setTimeout(() => c.classList.add('in-view'), i * 80);
                    });

                    roiTitle.style.opacity = '1';
                    roiDesc.style.opacity = '1';
                });
            }, 180);
        });
    });

    const cards = document.querySelectorAll('.digito-pricing-card');
    const io = new IntersectionObserver((entries) => {
        entries.forEach((e, i) => {
            if (e.isIntersecting) {
                setTimeout(() => e.target.classList.add('in-view'), i * 80);
                io.unobserve(e.target);
            }
        });
    }, { threshold: 0.15 });
    cards.forEach(c => io.observe(c));
})();