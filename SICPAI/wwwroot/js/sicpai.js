document.addEventListener('DOMContentLoaded', () => {

    // LOADER

    setTimeout(() => {

        const loader = document.getElementById('globalLoader');

        if (loader) {

            loader.classList.add('hide-loader');

        }

    }, 1200);

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