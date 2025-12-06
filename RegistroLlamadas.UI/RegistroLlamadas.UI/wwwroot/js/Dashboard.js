$(document).ready(function () {
    let estadisticas = {
        total: 0,
        finalizadas: 0,
        visitas: 0,
        enAtencion: 0,
        tiempoTotal: 0,
        tiempoTotalFinalizado: 0,
        atrasadas:0
    };

    // Función para calcular tiempo transcurrido
    function calcularTiempoTranscurrido(fechaStr, horaInicioStr, horaFinalStr) {
        const [year, month, day] = fechaStr.split('-').map(Number);
        const [hInicio, mInicio] = horaInicioStr.split(':').map(Number);

        const inicio = new Date(year, month - 1, day, hInicio, mInicio, 0);
        let final;

        if (horaFinalStr) {
            const [hFinal, mFinal] = horaFinalStr.split(':').map(Number);
            final = new Date(year, month - 1, day, hFinal, mFinal, 0);
        } else {
            final = new Date();
        }

        return final - inicio;
    }

    // Función para formatear tiempo
    function formatearTiempo(ms) {
        const totalSegundos = Math.floor(ms / 1000);
        const horas = Math.floor(totalSegundos / 3600);
        const minutos = Math.floor((totalSegundos % 3600) / 60);
        const segundos = totalSegundos % 60;

        return `${String(horas).padStart(2, '0')}:${String(minutos).padStart(2, '0')}:${String(segundos).padStart(2, '0')}`;
    }

    // Función para formatear tiempo en horas
    function formatearTiempoHrs(ms) {
        const horas = ms / (1000 * 60 * 60);
        const h = Math.floor(horas);
        const m = Math.floor((horas - h) * 60);
        return `${h}:${String(m).padStart(2, '0')} hrs`;
    }

    // Procesar cada fila
    $('#tablaLlamadas tbody tr').each(function () {
        const fila = $(this);
        const estadoId = parseInt(fila.data('estado-id'));
        const estadoDesc = (fila.data('estado-desc') || '').toString().toLowerCase();
        const fecha = fila.data('fecha');
        const horaInicio = fila.data('hora-inicio');
        const horaFinal = fila.data('hora-final');

        estadisticas.total++;

        const tiempoMs = calcularTiempoTranscurrido(fecha, horaInicio, horaFinal);
        const tiempoFormateado = formatearTiempo(tiempoMs);
        const tiempoHoras = tiempoMs / (1000 * 60 * 60);

        fila.find('.tiempo-transcurrido .tiempo-badge').text(tiempoFormateado);

        // Determinar estado y aplicar estilos
        if (estadoId === 3 || estadoDesc.includes('finalizada')) {
            // Finalizada
            estadisticas.finalizadas++;
            estadisticas.tiempoTotalFinalizado += tiempoMs;
            fila.addClass('row-finalizada');
            fila.find('.condicion-cell').html('<span class="badge-estado badge-finalizada">Finalizada</span>');
            fila.find('.hora-final-cell').text(horaFinal || '-');
            fila.find('.tiempo-badge').addClass('badge-finalizada');
        } else if (estadoId === 12) {
            // Visita
            estadisticas.visitas++;
            estadisticas.tiempoTotal += tiempoMs;
            fila.addClass('row-visita');
            fila.find('.condicion-cell').html('<span class="badge-estado badge-visita">Visita</span>');
            fila.find('.hora-final-cell').text(horaFinal || '-');
            fila.find('.tiempo-badge').addClass('badge-visita');
        }
        else if (estadoId === 4) {
            estadisticas.atrasadas++;
            fila.addClass('row-critica');
            fila.find('.condicion-cell').html('<span class="badge-estado badge-critica">ATRASADO</span>');
            fila.find('.tiempo-badge').addClass('badge-critica');
        }
         else {
            // En atención 
            estadisticas.enAtencion++;
            estadisticas.tiempoTotal += tiempoMs;

            if (tiempoHoras >= 2) {

                fila.addClass('row-critica');
                fila.find('.condicion-cell').html('<span class="badge-estado badge-critica">ATRASADO</span>');
                fila.find('.tiempo-badge').addClass('badge-critica');
            } else if (tiempoHoras >= 1) {
                // Alerta
                fila.addClass('row-alerta');
                fila.find('.condicion-cell').html('<span class="badge-estado badge-alerta">EN ALERTA</span>');
                fila.find('.tiempo-badge').addClass('badge-alerta');
            } else {
                // Normal
                fila.addClass('row-normal');
                fila.find('.condicion-cell').html('<span class="badge-estado badge-normal">En Atención</span>');
                fila.find('.tiempo-badge').addClass('badge-normal');
            }
        }
    });

    // Actualizar estadísticas
    $('#totalLlamadas').text(estadisticas.total);
    $('#totalFinalizadas').text(estadisticas.finalizadas);
    $('#totalVisitas').text(estadisticas.visitas);
    $('#totalAtencion').text(estadisticas.enAtencion);
    $('#countAtrasadas').text(estadisticas.atrasadas);

    // Calcular tiempo promedio
    const tiempoPromedioMs = estadisticas.finalizadas > 0
        ? estadisticas.tiempoTotalFinalizado / estadisticas.finalizadas
        : (estadisticas.total > 0 ? estadisticas.tiempoTotal / estadisticas.total : 0);
    $('#tiempoPromedio').text(formatearTiempoHrs(tiempoPromedioMs));

    // Actualizar barras de distribución
    const pctFinalizada = estadisticas.total > 0 ? (estadisticas.finalizadas / estadisticas.total * 100).toFixed(1) : 0;
    const pctVisita = estadisticas.total > 0 ? (estadisticas.visitas / estadisticas.total * 100).toFixed(1) : 0;
    const pctAtrada = estadisticas.total > 0 ? (estadisticas.atrasadas / estadisticas.total * 100).toFixed(1) : 0;
    setTimeout(() => {
        $('#barFinalizada').css('width', pctFinalizada + '%');
        $('#pctFinalizada').text(pctFinalizada + '%');
        $('#countFinalizada').text(estadisticas.finalizadas + ' llamadas');

        $('#barVisita').css('width', pctVisita + '%');
        $('#pctVisita').text(pctVisita + '%');
        $('#barAtrasadas').css('width', pctAtrada + '%');
        $('#pctAtrasadas').text(pctAtrada + '%');
        $('#countVisita').text(estadisticas.visitas + ' llamadas');
        $('#countAtrasadas').text(estadisticas.atrasadas + ' llamadas');
    }, 300);

    // Inicializar DataTable
    $('#tablaLlamadas').DataTable({
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.4/i18n/es-ES.json'
        },
        pageLength: 15,
        order: [[7, 'asc']],
        responsive: true,
        dom: 'frtip'
    });

    // Detectar tema actual
    const isDarkTheme = document.documentElement.getAttribute('data-bs-theme') === 'dark';

    // Crear gráfico de dona
    const ctx = document.getElementById('chartDistribucion').getContext('2d');

    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Finalizadas', 'Visitas', 'En Atención', 'Atrasadas'],
            datasets: [{
                data: [
                    estadisticas.finalizadas,
                    estadisticas.visitas,
                    estadisticas.enAtencion,
                    estadisticas.atrasadas
                ],
                backgroundColor: [
                    'rgba(40, 167, 69, 0.8)',   // Verde Finalizadas
                    'rgba(23, 162, 184, 0.8)',  // Celeste Visitas
                    'rgba(255, 193, 7, 0.8)',    // Amarillo En Atención
                    'rgba(220, 53, 69, 0.8)'     //  Rojo Atrasadas
                ],
                borderColor: [
                    '#28a745',
                    '#17a2b8',
                    '#ffc107',
                    '#dc3545'  
                ],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        color: isDarkTheme ? '#c9d1d9' : '#212529',
                        font: {
                            size: 12
                        }
                    }
                }
            }
        }
    });


    // Función para actualizar datos sin recargar la página
    function actualizarDatos() {
        $.ajax({
            url: window.location.href,
            type: 'GET',
            dataType: 'html',
            success: function (response) {
                // Extraer solo el tbody de la nueva respuesta
                const nuevoTbody = $(response).find('#tablaLlamadas tbody').html();

                if (nuevoTbody) {
                    // Destruir DataTable actual
                    const table = $('#tablaLlamadas').DataTable();
                    table.destroy();

                    // Actualizar tbody
                    $('#tablaLlamadas tbody').html(nuevoTbody);

                    // Reinicializar estadísticas
                    estadisticas = {
                        total: 0,
                        finalizadas: 0,
                        visitas: 0,
                        enAtencion: 0,
                        tiempoTotal: 0,
                        tiempoTotalFinalizado: 0
                    };

                    // Recalcular todas las filas
                    $('#tablaLlamadas tbody tr').each(function () {
                        const fila = $(this);
                        const estadoId = parseInt(fila.data('estado-id'));
                        const estadoDesc = (fila.data('estado-desc') || '').toString().toLowerCase();
                        const fecha = fila.data('fecha');
                        const horaInicio = fila.data('hora-inicio');
                        const horaFinal = fila.data('hora-final');

                        estadisticas.total++;

                        const tiempoMs = calcularTiempoTranscurrido(fecha, horaInicio, horaFinal);
                        const tiempoFormateado = formatearTiempo(tiempoMs);
                        const tiempoHoras = tiempoMs / (1000 * 60 * 60);

                        fila.find('.tiempo-transcurrido .tiempo-badge').text(tiempoFormateado);

                        if (estadoId === 3 || estadoDesc.includes('finalizada')) {
                            estadisticas.finalizadas++;
                            estadisticas.tiempoTotalFinalizado += tiempoMs;
                            fila.addClass('row-finalizada');
                            fila.find('.condicion-cell').html('<span class="badge-estado badge-finalizada">Finalizada</span>');
                            fila.find('.hora-final-cell').text(horaFinal || '-');
                            fila.find('.tiempo-badge').addClass('badge-finalizada');
                        } else if (estadoDesc.includes('visita')) {
                            estadisticas.visitas++;
                            estadisticas.tiempoTotal += tiempoMs;
                            fila.addClass('row-visita');
                            fila.find('.condicion-cell').html('<span class="badge-estado badge-visita">Visita</span>');
                            fila.find('.hora-final-cell').text(horaFinal || '-');
                            fila.find('.tiempo-badge').addClass('badge-visita');
                        }  else if (estadoId === 4) {
                            estadisticas.atrasadas++;
                            fila.addClass('row-critica');
                            fila.find('.condicion-cell').html('<span class="badge-estado badge-critica">ATRASADO</span>');
                            fila.find('.tiempo-badge').addClass('badge-critica');
                        } else {
                            estadisticas.enAtencion++;
                            estadisticas.tiempoTotal += tiempoMs;

                            if (tiempoHoras >= 2) {
                                fila.addClass('row-critica');
                                fila.find('.condicion-cell').html('<span class="badge-estado badge-critica">ATRASADO</span>');
                                fila.find('.tiempo-badge').addClass('badge-critica');
                            } else if (tiempoHoras >= 1) {
                                fila.addClass('row-alerta');
                                fila.find('.condicion-cell').html('<span class="badge-estado badge-alerta">EN ALERTA</span>');
                                fila.find('.tiempo-badge').addClass('badge-alerta');
                            } else {
                                fila.addClass('row-normal');
                                fila.find('.condicion-cell').html('<span class="badge-estado badge-normal">En Atención</span>');
                                fila.find('.tiempo-badge').addClass('badge-normal');
                            }
                        }
                    });

                    // Actualizar estadísticas en el dashboard
                    $('#totalLlamadas').text(estadisticas.total);
                    $('#totalFinalizadas').text(estadisticas.finalizadas);
                    $('#totalVisitas').text(estadisticas.visitas);
                    $('#totalAtencion').text(estadisticas.enAtencion);

                    const tiempoPromedioMs = estadisticas.finalizadas > 0
                        ? estadisticas.tiempoTotalFinalizado / estadisticas.finalizadas
                        : (estadisticas.total > 0 ? estadisticas.tiempoTotal / estadisticas.total : 0);
                    $('#tiempoPromedio').text(formatearTiempoHrs(tiempoPromedioMs));

                    const pctFinalizada = estadisticas.total > 0 ? (estadisticas.finalizadas / estadisticas.total * 100).toFixed(1) : 0;
                    const pctVisita = estadisticas.total > 0 ? (estadisticas.visitas / estadisticas.total * 100).toFixed(1) : 0;

                    $('#barFinalizada').css('width', pctFinalizada + '%');
                    $('#pctFinalizada').text(pctFinalizada + '%');
                    $('#countFinalizada').text(estadisticas.finalizadas + ' llamadas');

                    $('#barVisita').css('width', pctVisita + '%');
                    $('#pctVisita').text(pctVisita + '%');
                    $('#countVisita').text(estadisticas.visitas + ' llamadas');

                    // Reinicializar DataTable
                    $('#tablaLlamadas').DataTable({
                        language: {
                            url: 'https://cdn.datatables.net/plug-ins/1.13.4/i18n/es-ES.json'
                        },
                        pageLength: 15,
                        order: [[7, 'asc']],
                        responsive: true,
                        dom: 'frtip'
                    });

                    console.log('Dashboard actualizado:', new Date().toLocaleTimeString());
                }
            },
            error: function (error) {
                console.error('Error al actualizar datos:', error);
            }
        });
    }

    // Actualizar cada 30 segundos
    setInterval(actualizarDatos, 5000);

});