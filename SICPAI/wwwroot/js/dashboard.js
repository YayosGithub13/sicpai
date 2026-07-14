const assetsChart = document.getElementById('assetsChart');

if (assetsChart) {

    new Chart(assetsChart, {

        type: 'bar',

        data: {

            labels: [
                'Administración',
                'TI',
                'Finanzas',
                'RH',
                'Almacén'
            ],

            datasets: [{

                label: 'Bienes',

                data: [3200, 5400, 1800, 1200, 850],

                borderRadius: 8

            }]
        }
    });
}

const statusChart = document.getElementById('statusChart');

if (statusChart) {

    new Chart(statusChart, {

        type: 'doughnut',

        data: {

            labels: [
                'Activos',
                'Reparación',
                'Extraviados',
                'Baja'
            ],

            datasets: [{

                data: [11203, 153, 12, 25]

            }]
        }
    });
}