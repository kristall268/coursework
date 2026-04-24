// FPGA Learn - Site JavaScript

(function () {
    'use strict';

    // ── Theme Toggle ────────────────────────────────────────────────
    const themeToggle = document.getElementById('themeToggle');
    const html = document.documentElement;

    const savedTheme = localStorage.getItem('fpga-theme') || 'dark';
    html.setAttribute('data-theme', savedTheme);

    if (themeToggle) {
        themeToggle.addEventListener('click', () => {
            const current = html.getAttribute('data-theme');
            const next = current === 'dark' ? 'light' : 'dark';
            html.setAttribute('data-theme', next);
            localStorage.setItem('fpga-theme', next);
        });
    }

    // ── User Dropdown ───────────────────────────────────────────────
    const userMenuBtn  = document.getElementById('userMenuBtn');
    const userDropdown = document.getElementById('userDropdown');

    if (userMenuBtn && userDropdown) {
        userMenuBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            userDropdown.classList.toggle('open');
        });

        document.addEventListener('click', () => {
            userDropdown.classList.remove('open');
        });
    }

    // ── Smooth scroll for anchor links ──────────────────────────────
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                e.preventDefault();
                target.scrollIntoView({ behavior: 'smooth' });
            }
        });
    });

    // ── Explorer search filter ───────────────────────────────────────
    const searchInput = document.getElementById('hdlSearch');
    if (searchInput) {
        searchInput.addEventListener('input', () => {
            const q = searchInput.value.toLowerCase();
            document.querySelectorAll('.hdl-item').forEach(item => {
                const text = item.textContent.toLowerCase();
                item.style.display = text.includes(q) ? '' : 'none';
            });
        });
    }

    // ── Language / Difficulty filter ────────────────────────────────
    const langSelect = document.getElementById('langFilter');
    const diffSelect = document.getElementById('diffFilter');

    function applyFilters() {
        const lang = langSelect ? langSelect.value : 'all';
        const diff = diffSelect ? diffSelect.value : 'all';

        document.querySelectorAll('.hdl-item').forEach(item => {
            const itemLang = (item.dataset.lang || '').toLowerCase();
            const itemDiff = (item.dataset.diff || '').toLowerCase();

            const langMatch = lang === 'all' || itemLang === lang;
            const diffMatch = diff === 'all' || itemDiff === diff;
            item.style.display = (langMatch && diffMatch) ? '' : 'none';
        });
    }

    if (langSelect) langSelect.addEventListener('change', applyFilters);
    if (diffSelect) diffSelect.addEventListener('change', applyFilters);

    // ── Card hover glow ─────────────────────────────────────────────
    document.querySelectorAll('.component-card, .project-card, .practice-card').forEach(card => {
        card.addEventListener('mousemove', (e) => {
            const rect = card.getBoundingClientRect();
            const x = ((e.clientX - rect.left) / rect.width) * 100;
            const y = ((e.clientY - rect.top) / rect.height) * 100;
            card.style.setProperty('--mouse-x', x + '%');
            card.style.setProperty('--mouse-y', y + '%');
        });
    });

})();
