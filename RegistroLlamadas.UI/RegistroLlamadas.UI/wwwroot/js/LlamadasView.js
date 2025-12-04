$(document).ready(function () {
    // Inicializa DataTable
    const tabla = $('#tablaLlamadas').DataTable({
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.4/i18n/es-ES.json'
        },
        pageLength: 10,
        responsive: true,
        order: [[4, 'desc']]
    });

    function calcularEstadisticas() {
        const total = $('#tablaLlamadas tbody tr').length;
        let finalizadas = 0;
        let pendientes = 0;

        let totalDuracionMs = 0;
        let cantidadConTiempo = 0;

        $('#tablaLlamadas tbody tr').each(function () {

            // Leer estado
            const estadoTexto = $(this).find('.status-badge').text().trim().toLowerCase();

            // Leer hora inicio desde columna 6
            let horaInicio = $(this).find('td').eq(6).text().trim();
            horaInicio = limpiarHora(horaInicio);

            // Leer hora final desde columna 7
            let horaFinal = $(this).find('td').eq(7).text().trim();
            horaFinal = limpiarHora(horaFinal);

            // Finalizadas
            if (estadoTexto.includes('finalizado') || estadoTexto.includes('finalizada')) {
                finalizadas++;

                // Calcular solo si tiene hora final
                if (horaInicio && horaFinal && horaFinal !== '-') {
                    const duracionMs = calcularDiferenciaMs(horaInicio, horaFinal);
                    if (!isNaN(duracionMs) && duracionMs > 0) {
                        totalDuracionMs += duracionMs;
                        cantidadConTiempo++;
                    }
                }

            } else {
                pendientes++;
            }
        });

        // Calcular promedio
        let tiempoPromedioMs = cantidadConTiempo > 0
            ? totalDuracionMs / cantidadConTiempo
            : 0;

        const promedioFormateado = formatearDuracion(tiempoPromedioMs);

        // Mostrar resultados
        $('#totalLlamadas').text(total);
        $('#totalFinalizadas').text(finalizadas);
        $('#totalPendientes').text(pendientes);
        $('#tiempoPromedio').text(promedioFormateado);
    }
    calcularEstadisticas();

    function calcularDiferenciaMs(horaInicio, horaFinal) {
        const [h1, m1] = horaInicio.split(':').map(Number);
        const [h2, m2] = horaFinal.split(':').map(Number);

        const inicio = new Date(0, 0, 0, h1, m1);
        const final = new Date(0, 0, 0, h2, m2);

        return final - inicio; 
    }
    function formatearDuracion(ms) {
        if (!ms || ms <= 0) return "0 min";

        const totalMin = Math.floor(ms / 60000);
        const horas = Math.floor(totalMin / 60);
        const minutos = totalMin % 60;

        if (horas === 0) {
           
            return `${minutos} min`;
        } else if (minutos === 0) {
           
            return `${horas} hr${horas > 1 ? 's' : ''}`;
        } else {
            
            return `${horas} hr${horas > 1 ? 's' : ''} ${minutos} min`;
        }
    }
    function initSelect2() {
        // Destruir instancias previas
        $('.select2').each(function () {
            if ($(this).hasClass('select2-hidden-accessible')) {
                $(this).select2('destroy');
            }
        });

        // Inicializar Select2
        $('.select2').select2({
            theme: 'bootstrap-5',
            language: 'es',
            width: '100%',
            dropdownParent: $('#modalNuevaLlamada'),
            placeholder: function () {
                return $(this).find('option:first').text();
            },
            allowClear: true,
            minimumResultsForSearch: 0
        });
    }

    // Evento cuando se muestra el modal
    $('#modalNuevaLlamada').on('shown.bs.modal', function () {
        initSelect2();


        if (!$('#fecha').val()) {
            const hoy = new Date();
            const year = hoy.getFullYear();
            const month = ('0' + (hoy.getMonth() + 1)).slice(-2);
            const day = ('0' + hoy.getDate()).slice(-2);
            const fechaLocal = `${year}-${month}-${day}`;

            $('#fecha').val(fechaLocal);
        }


        $('#fecha').focus();
    });

    // Restablece el formulario al cerrar el modal
    $('#modalNuevaLlamada').on('hidden.bs.modal', function () {
        $('#formNuevaLlamada')[0].reset();
        $('#formNuevaLlamada').removeClass('was-validated');

        // Limpiar Select2
        $('.select2').each(function () {
            if ($(this).hasClass('select2-hidden-accessible')) {
                $(this).val(null).trigger('change');
            }
        });

        $('#modalNuevaLlamadaLabel').html('<i class="bi bi-telephone-plus"></i> Registrar Nueva Llamada');
    });

    // Manejo del formulario
    $('#formNuevaLlamada').on('submit', function (e) {
        e.preventDefault();

        const idLlamada = $(this).find('button[type="submit"]').data('id-llamada') || 0;

        var llamadaData = {
            IdLlamada: idLlamada,
            Fecha: $('#fecha').val(),
            HoraInicio: $('#horaInicio').val(),
            HoraFinal: $('#horaFinal').val() || null,
            Promat: $('#promat').val(),
            Asunto: $('#asunto').val(),
            CorreoEnviado: $('#correoEnviado').val() === 'true',
            UsuarioId: $('#usuarioId').val(),
            EquipoId: $('#equipoId').val() || null,
            ClienteId: $('#clienteId').val(),
            CentroId: $('#centroId').val(),
            EstadoId: $('#estadoId').val()
        };

        // Validación básica con SweetAlert2
        if (!llamadaData.Fecha || !llamadaData.HoraInicio || !llamadaData.CentroId) {
            Swal.fire({
                icon: 'warning',
                title: 'Campos incompletos',
                text: 'Por favor complete todos los campos obligatorios marcados con *',
                confirmButtonText: 'Entendido'
            });
            return;
        }

        // Determinar si es crear o actualizar
        const url = idLlamada > 0 ? '/Llamada/ActualizarLlamada' : '/Llamada/RegistrarLlamada';
        const mensaje = idLlamada > 0 ? 'actualizada' : 'registrada';
        const accion = idLlamada > 0 ? 'Actualizando' : 'Guardando';

        // Mostrar loading
        Swal.fire({
            title: accion + '...',
            text: 'Por favor espere',
            allowOutsideClick: false,
            allowEscapeKey: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(llamadaData),
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: '¡Éxito!',
                        text: 'Llamada ' + mensaje + ' correctamente',
                        confirmButtonText: 'Aceptar',
                        timer: 2000,
                        timerProgressBar: true
                    }).then(() => {
                        $('#modalNuevaLlamada').modal('hide');
                        location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.mensaje || 'Ocurrió un error al guardar',
                        confirmButtonText: 'Cerrar'
                    });
                }
            },
            error: function (xhr, status, error) {
                let mensajeError = 'Error al guardar la llamada';

                // Intentar obtener mensaje del servidor
                if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                    mensajeError = xhr.responseJSON.mensaje;
                } else if (xhr.responseText) {
                    try {
                        const response = JSON.parse(xhr.responseText);
                        mensajeError = response.mensaje || mensajeError;
                    } catch (e) {
                        mensajeError += ': ' + error;
                    }
                }

                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: mensajeError,
                    confirmButtonText: 'Cerrar'
                });
            }
        });
    });
    // --- BOTÓN VISITA ---
    $(document).on("click", ".btn-visita", function () {
        let id = $(this).data("id");
        let usuario = $(this).data("usuario");

        $("#visitaIdLlamada").val(id);
        $("#usuarioVisita").val(usuario);

        $("#modalVisita").modal("show");
    });

    // Mostrar select si NO es la misma persona
    $("#mismaPersona").on("change", function () {
        if ($(this).val() === "no") {
            $("#contenedorUsuariosVisita").removeClass("d-none");
        } else {
            $("#contenedorUsuariosVisita").addClass("d-none");
        }
    });

    // Guardar visita
    $("#formVisita").on("submit", function (e) {
        e.preventDefault();

        let idLlamada = $("#visitaIdLlamada").val();
        let usuarioOriginal = $("#usuarioVisita").val();
        let esMismaPersona = $("#mismaPersona").val() === "si";
        let usuarioSeleccionado = esMismaPersona ? usuarioOriginal : $("#usuarioVisita").val();

        // Validación si NO es misma persona
        if (!esMismaPersona && (!usuarioSeleccionado || usuarioSeleccionado === "")) {
            Swal.fire({
                icon: "warning",
                title: "Seleccione un usuario",
                text: "Debe elegir quién realizará la visita.",
                confirmButtonText: "Entendido"
            });
            return;
        }

        let payload = {
            IdLlamada: idLlamada,
            UsuarioId: parseInt(usuarioSeleccionado)
        };

        $.ajax({
            url: "/Llamada/RegistrarVisita",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),

            success: function (response) {
                if (response.success) {

                    Swal.fire({
                        icon: "success",
                        title: "¡Visita registrada!",
                        text: "La visita ha sido asignada correctamente.",
                        timer: 1800,
                        timerProgressBar: true,
                        showConfirmButton: false
                    }).then(() => {
                        $("#modalVisita").modal("hide");
                        location.reload();
                    });

                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: response.mensaje || "No se pudo registrar la visita."
                    });
                }
            },

            error: function (xhr) {
                let mensajeError = "Error al registrar la visita.";

                if (xhr.responseJSON?.mensaje) {
                    mensajeError = xhr.responseJSON.mensaje;
                }

                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: mensajeError
                });
            }
        });
    });


    // --- BOTÓN REASIGNAR ---
    $(document).on("click", ".btn-reasignar", function () {
        let id = $(this).data("id");

        $("#reasignarIdLlamada").val(id);
        $("#modalReasignar").modal("show");
    });

    // --- GUARDAR REASIGNACIÓN ---
    $("#formReasignar").on("submit", function (e) {
        e.preventDefault();

        let payload = {
            IdLlamada: $("#reasignarIdLlamada").val(),
            NuevoUsuarioId: $("#nuevoResponsable").val(),
            RealizadoPor: $("#usuarioActual").val(),
            Comentario: "Reasignado desde el panel"
        };

        $.ajax({
            url: "/Llamada/Reasignar",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),

            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        icon: "success",
                        title: "¡Reasignado!",
                        text: "La llamada fue reasignada correctamente.",
                        confirmButtonText: "Aceptar",
                        timer: 1800,
                        timerProgressBar: true
                    }).then(() => {
                        $("#modalReasignar").modal("hide");
                        location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: response.mensaje || "No se pudo reasignar la llamada.",
                    });
                }
            },

            error: function (xhr, status, error) {

                let mensajeError = "Error en la reasignación";

                // Intentar obtener el mensaje del backend
                if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                    mensajeError = xhr.responseJSON.mensaje;
                } else if (xhr.responseText) {
                    try {
                        const response = JSON.parse(xhr.responseText);
                        mensajeError = response.mensaje || mensajeError;
                    } catch (e) {
                        mensajeError += ": " + error;
                    }
                }

                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: mensajeError,
                    confirmButtonText: "Cerrar",
                });
            }
        });
    });
    // Botón editar
    $(document).on('click', '.btn-editar', function () {
        const idLlamada = $(this).data('id');
        console.log('Editar llamada:', idLlamada);

        // Cambiar título del modal
        $('#modalNuevaLlamadaLabel').html('<i class="bi bi-pencil-square"></i> Editar Llamada');

        // Cargar datos de la llamada
        $.ajax({
            url: '/Llamada/ObtenerLlamadaPorId',
            type: 'GET',
            data: { idLlamada: idLlamada },
            success: function (response) {
                if (response.success) {
                    const llamada = response.data;

                    // Llenar los campos del formulario
                    $('#fecha').val(llamada.fecha ? llamada.fecha.split('T')[0] : '');
                    $('#horaInicio').val(llamada.horaInicio);
                    $('#horaFinal').val(llamada.horaFinal);
                    $('#promat').val(llamada.promat);
                    $('#asunto').val(llamada.asunto);
                    $('#correoEnviado').val(llamada.correoEnviado ? 'true' : 'false');

                    // Llenar los selects
                    $('#usuarioId').val(llamada.usuarioId).trigger('change');
                    $('#equipoId').val(llamada.equipoId).trigger('change');
                    $('#clienteId').val(llamada.clienteId).trigger('change');
                    $('#centroId').val(llamada.centroId).trigger('change');
                    $('#estadoId').val(llamada.estadoId).trigger('change');

                    // Guardar el ID en un data attribute del botón guardar
                    $('#formNuevaLlamada button[type="submit"]').data('id-llamada', idLlamada);

                    // Mostrar modal
                    $('#modalNuevaLlamada').modal('show');
                } else {
                    alert('Error al cargar los datos: ' + response.mensaje);
                }
            },
            error: function (xhr, status, error) {
                alert('Error al cargar los datos: ' + error);
            }
        });
    });

    // Botón eliminar
    $(document).on('click', '.btn-eliminar', function () {
        const idLlamada = $(this).data('id');

        Swal.fire({
            title: '¿Está seguro?',
            text: "Esta acción no se puede deshacer",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Llamada/EliminarLlamada',
                    type: 'DELETE',
                    data: { idLlamada: idLlamada },
                    success: function (response) {
                        if (response.success) {
                            Swal.fire(
                                '¡Eliminado!',
                                'La llamada ha sido eliminada correctamente.',
                                'success'
                            ).then(() => {
                                location.reload();
                            });
                        } else {
                            Swal.fire(
                                'Error',
                                response.mensaje,
                                'error'
                            );
                        }
                    },
                    error: function (xhr, status, error) {
                        Swal.fire(
                            'Error',
                            'No se pudo eliminar la llamada: ' + error,
                            'error'
                        );
                    }
                });
            }
        });
    });

    $('#descripcionSolucion').on('input', function () {
        const length = $(this).val().length;
        $('#contadorCaracteres').text(length);
    });

    // Botón finalizar
    $(document).on('click', '.btn-finalizar', function () {
        const idLlamada = $(this).data('id');

        // Establecer hora actual
        const now = new Date();
        const horaActual = now.getHours().toString().padStart(2, '0') + ':' +
            now.getMinutes().toString().padStart(2, '0');
        $('#horaFinalizacion').val(horaActual);

        // Limpiar textarea
        $('#descripcionSolucion').val('');
        $('#contadorCaracteres').text('0');

        // Guardar ID en el botón submit
        $('#formFinalizarLlamada button[type="submit"]').data('id-llamada', idLlamada);

        // Mostrar modal
        $('#modalFinalizarLlamada').modal('show');
    });

    // Enviar formulario de finalización
    $('#formFinalizarLlamada').on('submit', function (e) {
        e.preventDefault();

        const idLlamada = $(this).find('button[type="submit"]').data('id-llamada');
        const descripcion = $('#descripcionSolucion').val().trim();
        const horaFinal = $('#horaFinalizacion').val();
        const correoEnviado = $('#correoEnviadoFinal').val() === 'true';

        // Validación
        if (!descripcion) {
            Swal.fire({
                icon: 'warning',
                title: 'Campo requerido',
                text: 'Por favor ingrese la descripción de la solución',
                confirmButtonText: 'Entendido'
            });
            return;
        }

        if (descripcion.length < 20) {
            Swal.fire({
                icon: 'warning',
                title: 'Descripción muy corta',
                text: 'Por favor proporcione una descripción más detallada (mínimo 20 caracteres)',
                confirmButtonText: 'Entendido'
            });
            return;
        }

        if (!horaFinal) {
            Swal.fire({
                icon: 'warning',
                title: 'Campo requerido',
                text: 'Por favor ingrese la hora de finalización',
                confirmButtonText: 'Entendido'
            });
            return;
        }

        // Confirmar finalización
        Swal.fire({
            title: '¿Finalizar llamada?',
            text: "Esta acción marcará la llamada como completada",
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#198754',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Sí, finalizar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                finalizarLlamada(idLlamada, descripcion, horaFinal, correoEnviado);
            }
        });
    });

    $(document).on("click", ".btn-ver", function () {
        let id = $(this).data("id");

        $("#contenedorDetallesLlamada").html(`
        <div class="text-center text-muted py-4">
            <div class="spinner-border text-info"></div>
            <p>Cargando detalles...</p>
        </div>
    `);

        $("#modalDetallesLlamada").modal("show");

        $.ajax({
            url: "/Llamada/ObtenerDetalles",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({ IdLlamada: id }),
            success: function (resp) {
                if (!resp.success) {
                    $("#contenedorDetallesLlamada").html(`<div class="alert alert-danger">${resp.mensaje}</div>`);
                    return;
                }

                const llamada = resp.llamada;
                const historial = resp.historial;
                const visitas = resp.visitas;

                let html = `
                <h5 class="mb-3">Información General</h5>
                <table class="table table-bordered">
                    <tr><th>Responsable</th><td>${llamada.usuarioNombre ?? '-'}</td></tr>
                    <tr><th>Cliente</th><td>${llamada.clienteNombre ?? '-'}</td></tr>
                    <tr><th>Centro</th><td>${llamada.centroNombre ?? '-'}</td></tr>
                    <tr><th>Asunto</th><td>${llamada.asunto ?? '-'}</td></tr>
                    <tr><th>Fecha</th><td>${llamada.fecha}</td></tr>
                    <tr><th>Hora Inicio</th><td>${llamada.horaInicio}</td></tr>
                    <tr><th>Hora Final</th><td>${llamada.horaFinal ?? '-'}</td></tr>
                </table>

                <hr>

                <h5 class="mt-4">Historial de la llamada</h5>
                <ul class="list-group mb-4">
            `;

                if (historial.length === 0) {
                    html += `<li class="list-group-item text-muted">Sin historial registrado.</li>`;
                } else {
                    historial.forEach(h => {
                        html += `
                        <li class="list-group-item">
                            <strong>${h.accion}</strong><br>
                            ${h.descripcion}<br>
                            <small class="text-muted">
                                Registrado por: ${h.registradoPorNombre} |
                                Afectado: ${h.usuarioAfectadoNombre} |
                                ${h.fechaRegistro}
                            </small>
                        </li>
                    `;
                    });
                }

                html += `</ul><hr><h5 class="mt-4">Visitas Relacionadas</h5>`;

                if (visitas.length === 0) {
                    html += `<p class="text-muted">No hay visitas relacionadas.</p>`;
                } else {
                    visitas.forEach(v => {
                        html += `
                        <div class="border rounded p-3 mb-3">
                            <strong>Visitado por:</strong> ${v.usuarioVisita}<br>
                            <strong>Fecha:</strong> ${v.fechaVisita}<br>
                            <strong>Inicio:</strong> ${v.horaInicio}<br>
                            <strong>Final:</strong> ${v.horaFinal ?? '-'}<br>
                            <strong>Comentario:</strong> ${v.comentario ?? '-'}
                        </div>`;
                    });
                }

                $("#contenedorDetallesLlamada").html(html);
            },
            error: function () {
                $("#contenedorDetallesLlamada").html(`<div class="alert alert-danger">Error al cargar detalles</div>`);
            }
        });
    });
    function finalizarLlamada(idLlamada, descripcion, horaFinal, correoEnviado) {
        Swal.fire({
            title: 'Finalizando...',
            text: 'Por favor espere',
            allowOutsideClick: false,
            allowEscapeKey: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        const data = {
            IdLlamada: idLlamada,
            DescripcionSolucion: descripcion,
            HoraFinal: horaFinal,
            CorreoEnviado: correoEnviado
        };

        $.ajax({
            url: '/Llamada/FinalizarLlamada',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: '¡Llamada finalizada!',
                        text: 'La llamada ha sido marcada como completada',
                        confirmButtonText: 'Aceptar',
                        timer: 2000,
                        timerProgressBar: true
                    }).then(() => {
                        $('#modalFinalizarLlamada').modal('hide');
                        location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.mensaje || 'No se pudo finalizar la llamada',
                        confirmButtonText: 'Cerrar'
                    });
                }
            },
            error: function (xhr, status, error) {
                let mensajeError = 'Error al finalizar la llamada';

                if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                    mensajeError = xhr.responseJSON.mensaje;
                }

                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: mensajeError,
                    confirmButtonText: 'Cerrar'
                });
            }
        });
    }

    function limpiarHora(texto) {
        return texto.replace(/[^0-9:]/g, '').trim() || null;
    }

    // Select All
    $('#selectAll').on('change', function () {
        $('.row-checkbox').prop('checked', this.checked);
    });

    // Búsqueda global personalizada
    $('#searchGlobal').on('keyup', function () {
        tabla.search(this.value).draw();
    });

    // Filtros
    $('#filtroEstado').on('change', function () {
        let valor = this.value;

        if (valor === 'Pendiente') {
            // Filtrar todo excepto Finalizado y Visita
            tabla.column(10)
                .search('^(?!Finalizado)(?!Visita$).*$', true, false)
                .draw();
        } else if (valor === '') {
            // Limpiar filtro si no selecciona nada
            tabla.column(10).search('').draw();
        } else {
            // Búsqueda normal
            tabla.column(10).search(valor).draw();
        }
    });

    // Limpiar filtros
    $('#btnLimpiarFiltros').on('click', function () {
        $('#searchGlobal').val('');
        $('#filtroEstado').val('');
        $('#filtroFechaDesde').val('');
        $('#filtroFechaHasta').val('');
        tabla.search('').columns().search('').draw();
    });
    // Limpiar modal al cerrar
    $('#modalFinalizarLlamada').on('hidden.bs.modal', function () {
        $('#formFinalizarLlamada')[0].reset();
        $('#contadorCaracteres').text('0');
        $('#formFinalizarLlamada button[type="submit"]').removeData('id-llamada');
    });
});