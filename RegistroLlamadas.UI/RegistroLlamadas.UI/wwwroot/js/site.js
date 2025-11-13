document.addEventListener('DOMContentLoaded', function () {
    const toggleBtn = document.getElementById('toggleSidebar');
    const closeBtn = document.getElementById('closeSidebar');
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');
    const btnTheme = document.getElementById('btnTheme');
    const html = document.documentElement;
    const body = document.body;

    // === TEMA (MODO OSCURO/CLARO) ===
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

    // Cargar tema guardado
    const savedTheme = localStorage.getItem('theme') || 'light';
    applyTheme(savedTheme);

    // Cambiar tema
    if (btnTheme) {
        btnTheme.addEventListener('click', function () {
            const currentTheme = html.getAttribute('data-bs-theme');
            const newTheme = currentTheme === 'light' ? 'dark' : 'light';

            localStorage.setItem('theme', newTheme);
            applyTheme(newTheme);

            // Animación suave
            btnTheme.style.transform = 'rotate(360deg)';
            setTimeout(() => {
                btnTheme.style.transform = 'rotate(0deg)';
            }, 300);
        });

        // Transición suave para el botón
        btnTheme.style.transition = 'transform 0.3s ease';
    }

    // === SIDEBAR ===
    function isDesktop() {
        return window.innerWidth >= 992;
    }

    function openSidebar() {
        sidebar.classList.add('active');
        overlay.classList.add('active');
        document.body.style.overflow = 'hidden'; // Prevenir scroll en móvil
    }

    function closeSidebar() {
        sidebar.classList.remove('active');
        overlay.classList.remove('active');
        document.body.style.overflow = ''; // Restaurar scroll
    }

    function toggleSidebar() {
        if (isDesktop()) {
            // En desktop: colapsar/expandir
            body.classList.toggle('sidebar-collapsed');
            const isCollapsed = body.classList.contains('sidebar-collapsed');
            localStorage.setItem('sidebarCollapsed', isCollapsed);
        } else {
            // En móvil: abrir/cerrar
            if (sidebar.classList.contains('active')) {
                closeSidebar();
            } else {
                openSidebar();
            }
        }
    }

    // Restaurar estado colapsado en desktop
    if (isDesktop() && localStorage.getItem('sidebarCollapsed') === 'true') {
        body.classList.add('sidebar-collapsed');
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

    if (overlay) {
        overlay.addEventListener('click', closeSidebar);
    }

    // Cerrar sidebar en móvil al hacer clic en un enlace
    const sidebarLinks = sidebar.querySelectorAll('.nav-link');
    sidebarLinks.forEach(link => {
        link.addEventListener('click', function () {
            if (!isDesktop()) {
                closeSidebar();
            }
        });
    });

    // Ajustar en resize
    let resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            if (isDesktop()) {
                // Limpiar estados de móvil
                closeSidebar();
                body.style.overflow = '';
            } else {
                // Limpiar estado colapsado de desktop
                body.classList.remove('sidebar-collapsed');
            }
        }, 250);
    });

    // === MEJORAS DE ACCESIBILIDAD ===
    // Cerrar sidebar con tecla Escape
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && !isDesktop() && sidebar.classList.contains('active')) {
            closeSidebar();
        }
    });

    // === ANIMACIÓN SUAVE AL CARGAR ===
    setTimeout(() => {
        sidebar.style.transition = 'transform 0.3s ease-in-out, width 0.3s ease-in-out';
    }, 100);

    // === TOOLTIP DE BOOTSTRAP (opcional) ===
    // Inicializar tooltips si existen
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});