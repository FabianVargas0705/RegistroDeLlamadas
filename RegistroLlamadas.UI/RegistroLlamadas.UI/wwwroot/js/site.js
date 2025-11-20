document.addEventListener('DOMContentLoaded', function () {
    const toggleBtn = document.getElementById('toggleSidebar');
    const closeBtn = document.getElementById('closeSidebar');
    const hideBtn = document.getElementById('hideSidebar');
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');
    const btnTheme = document.getElementById('btnTheme');
    const html = document.documentElement;
    const body = document.body;

    // === TEMA OSCURO/CLARO ===
    function applyTheme(theme) {
        html.setAttribute('data-bs-theme', theme);

        if (btnTheme) {
            const icon = btnTheme.querySelector('i');
            const text = btnTheme.querySelector('span');

            if (theme === 'dark') {
                icon.className = 'bi bi-sun-fill';
                if (text) text.textContent = 'Modo claro';
            } else {
                icon.className = 'bi bi-moon-stars-fill';
                if (text) text.textContent = 'Modo oscuro';
            }
        }
    }

    const savedTheme = localStorage.getItem('theme') || 'light';
    applyTheme(savedTheme);

    if (btnTheme) {
        btnTheme.addEventListener('click', function () {
            const currentTheme = html.getAttribute('data-bs-theme');
            const newTheme = currentTheme === 'light' ? 'dark' : 'light';
            localStorage.setItem('theme', newTheme);
            applyTheme(newTheme);

            btnTheme.style.transform = 'rotate(360deg)';
            setTimeout(() => {
                btnTheme.style.transform = 'rotate(0deg)';
            }, 300);
        });
        btnTheme.style.transition = 'transform 0.3s ease';
    }

    // === SIDEBAR ===
    function isDesktop() {
        return window.innerWidth >= 992;
    }

    function openSidebar() {
        sidebar.classList.add('active');
        overlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeSidebar() {
        sidebar.classList.remove('active');
        overlay.classList.remove('active');
        document.body.style.overflow = '';
    }

    function hideSidebarCompletely() {
        body.classList.add('sidebar-hidden');
        body.classList.remove('sidebar-collapsed');
        localStorage.setItem('sidebarHidden', 'true');
        localStorage.setItem('sidebarCollapsed', 'false');
    }

    function showSidebar() {
        body.classList.remove('sidebar-hidden');
        localStorage.setItem('sidebarHidden', 'false');
    }

    function toggleSidebar() {
        if (isDesktop()) {
            // Si está completamente escondida, mostrar normal
            if (body.classList.contains('sidebar-hidden')) {
                showSidebar();
            }
            // Si está abierta (normal o colapsada), esconder completamente
            else {
                hideSidebarCompletely();
            }
        } else {
            // En móvil, abrir/cerrar con overlay
            if (sidebar.classList.contains('active')) {
                closeSidebar();
            } else {
                openSidebar();
            }
        }
    }

    // Restaurar estados
    if (isDesktop()) {
        if (localStorage.getItem('sidebarHidden') === 'true') {
            body.classList.add('sidebar-hidden');
        } else if (localStorage.getItem('sidebarCollapsed') === 'true') {
            body.classList.add('sidebar-collapsed');
        }
    }

    // Event listeners
    if (toggleBtn) {
        toggleBtn.addEventListener('click', function (e) {
            e.preventDefault();
            toggleSidebar();
        });
    }

    if (closeBtn) {
        closeBtn.addEventListener('click', function (e) {
            e.preventDefault();
            closeSidebar();
        });
    }

    if (hideBtn) {
        hideBtn.addEventListener('click', function (e) {
            e.preventDefault();
            hideSidebarCompletely();
        });
    }

    if (overlay) overlay.addEventListener('click', closeSidebar);

    const sidebarLinks = sidebar.querySelectorAll('.nav-link');
    sidebarLinks.forEach(link => {
        link.addEventListener('click', function () {
            if (!isDesktop()) closeSidebar();
        });
    });

    // Ajuste en resize
    let resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            if (isDesktop()) {
                closeSidebar();
                body.style.overflow = '';
            } else {
                body.classList.remove('sidebar-collapsed', 'sidebar-hidden');
            }
        }, 250);
    });

    // Cerrar sidebar con Escape
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && !isDesktop() && sidebar.classList.contains('active')) {
            closeSidebar();
        }
    });

    // Animación suave al cargar
    setTimeout(() => {
        sidebar.style.transition = 'transform 0.3s ease-in-out, width 0.3s ease-in-out';
    }, 100);

    // Tooltips de Bootstrap
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});