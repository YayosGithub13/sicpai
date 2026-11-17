document.addEventListener('DOMContentLoaded', () => {

    // LOADER

    setTimeout(() => {

        const loader = document.getElementById('globalLoader');

        if (loader) {

            loader.classList.add('hide-loader');

        }

    }, 1200);

});

document.addEventListener('DOMContentLoaded', () => {

    const content = document.getElementById('appContentBody');
    const sidebar = document.getElementById('sidebar');
    const scriptHost = document.getElementById('dynamicPageScripts');

    if (!content || !sidebar) return;

    const destroyDynamicScripts = () => {
        document.querySelectorAll('script[data-dynamic-script="true"]').forEach(node => node.remove());
    };

    const executeDynamicScripts = source => {
        if (!source) return;
        const scripts = source.querySelectorAll('script');
        scripts.forEach(script => {
            const clone = document.createElement('script');
            clone.dataset.dynamicScript = 'true';
            [...script.attributes].forEach(attribute => clone.setAttribute(attribute.name, attribute.value));
            if (!clone.src) clone.textContent = script.textContent;
            document.body.appendChild(clone);
        });
    };

    const showLoader = () => {
        const loader = document.getElementById('globalLoader');
        loader?.classList.remove('hide-loader');
    };

    const hideLoader = () => {
        const loader = document.getElementById('globalLoader');
        loader?.classList.add('hide-loader');
    };

    const setActiveLink = url => {
        const path = new URL(url, window.location.origin).pathname.toLowerCase();
        document.querySelectorAll('.sidebar-menu li').forEach(item => item.classList.remove('active'));
        document.querySelectorAll('.sidebar-menu a[data-app-nav="true"]').forEach(anchor => {
            const hrefPath = new URL(anchor.href, window.location.origin).pathname.toLowerCase();
            if (path === hrefPath || path.startsWith(`${hrefPath}/`)) {
                anchor.closest('li')?.classList.add('active');
            }
        });
    };

    const navigateWithinApp = async (targetUrl, preserveSidebar = true, pushHistory = true) => {
        if (!targetUrl) return;

        const sidebarScroll = preserveSidebar ? sidebar.scrollTop : 0;
        showLoader();

        try {
            const response = await fetch(targetUrl, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            if (!response.ok) {
                hideLoader();
                return;
            }

            const html = await response.text();
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const nextBody = doc.getElementById('appContentBody');
            const nextScripts = doc.getElementById('dynamicPageScripts');

            if (!nextBody) {
                hideLoader();
                return;
            }

            content.innerHTML = nextBody.innerHTML;
            destroyDynamicScripts();
            executeDynamicScripts(nextScripts);

            if (pushHistory) {
                window.history.pushState({ appNav: true }, '', targetUrl);
            }

            setActiveLink(targetUrl);
            if (preserveSidebar) sidebar.scrollTop = sidebarScroll;
            hideLoader();
        } catch {
            hideLoader();
        }
    };

    const bindMenuNavigation = () => {
        document.querySelectorAll('.sidebar-menu a[data-app-nav="true"]').forEach(anchor => {
            if (anchor.dataset.navBound === 'true') return;
            anchor.dataset.navBound = 'true';

            anchor.addEventListener('click', async event => {
                event.preventDefault();
                const targetUrl = anchor.getAttribute('href');
                await navigateWithinApp(targetUrl, true, true);
            });
        });
    };

    window.addEventListener('app:navigate', async event => {
        const targetUrl = event?.detail?.url;
        if (!targetUrl) return;
        await navigateWithinApp(targetUrl, true, true);
    });

    window.addEventListener('popstate', async () => {
        const targetUrl = `${window.location.pathname}${window.location.search}`;
        await navigateWithinApp(targetUrl, true, false);
    });

    setActiveLink(window.location.pathname + window.location.search);
    bindMenuNavigation();
});

// TOAST

function showToast(title, icon = 'success') {

    Swal.fire({

        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        icon: icon,
        title: title

    });

}

// ALERTA PREMIUM

function showDeleteAlert() {

    Swal.fire({

        title: '¿Eliminar registro?',
        text: 'Esta acción no podrá revertirse.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#0d6efd',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, eliminar'

    });

}

// DARK MODE

//function toggleDarkMode() {

//    document.body.classList.toggle('dark-mode');

//    localStorage.setItem(

//        'darkMode',

//        document.body.classList.contains('dark-mode')

//    );

//}

// LOAD PREFERENCE

//document.addEventListener('DOMContentLoaded', () => {

//    if (localStorage.getItem('darkMode') === 'true') {

//        document.body.classList.add('dark-mode');

//    }

//});

// SIDEBAR TOGGLE

//document.addEventListener('DOMContentLoaded', () => {

//    const sidebar = document.getElementById('sidebar');

//    const sidebarToggle = document.getElementById('sidebarToggle');

//    if (sidebarToggle) {

//        sidebarToggle.addEventListener('click', () => {

//            sidebar.classList.toggle('show');

//        });

//    }

//});

document.addEventListener('DOMContentLoaded', () => {

    const sidebar = document.getElementById('sidebar');

    const sidebarToggle = document.getElementById('sidebarToggle');

    const overlay = document.getElementById('sidebarOverlay');

    if (sidebarToggle) {

        sidebarToggle.addEventListener('click', () => {

            sidebar.classList.toggle('show');

            overlay.classList.toggle('show');

        });

    }

    if (overlay) {

        overlay.addEventListener('click', () => {

            sidebar.classList.remove('show');

            overlay.classList.remove('show');

        });

    }

});