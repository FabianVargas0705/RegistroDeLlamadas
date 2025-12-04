// --- ABRIR MODAL FINALIZAR VISITA ---
$(document).on("click", ".btn-finalizar-visita", function () {
    let id = $(this).data("id");

    $("#finalizarVisitaId").val(id);

    // Hora actual formateada
    let now = new Date();
    let hora = now.getHours().toString().padStart(2, '0') + ":" +
        now.getMinutes().toString().padStart(2, '0');

    $("#horaFinalVisita").val(hora);

    // Limpiar campos
    $("#descripcionVisita").val('');
    $("#contadorVisita").text(0);

    $("#modalFinalizarVisita").modal("show");
});


// --- CONTADOR DE CARACTERES ---
$("#descripcionVisita").on("input", function () {
    $("#contadorVisita").text($(this).val().length);
});


// --- ENVIAR FINALIZACIÓN ---
$("#formFinalizarVisita").on("submit", function (e) {
    e.preventDefault();

    let desc = $("#descripcionVisita").val().trim();
    let horaFinal = $("#horaFinalVisita").val();
    let visitaId = $("#finalizarVisitaId").val();
    let enviarCorreo = $("#correoEnviadoVisita").val() === "true";

    // Validaciones
    if (desc.length < 10) {
        Swal.fire({
            icon: "warning",
            title: "Descripción insuficiente",
            text: "Debe ingresar mínimo 10 caracteres."
        });
        return;
    }

    if (!horaFinal) {
        Swal.fire({
            icon: "warning",
            title: "Hora requerida",
            text: "Debe seleccionar la hora final."
        });
        return;
    }

    Swal.fire({
        title: "¿Finalizar visita?",
        text: "La visita será marcada como completada.",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#198754",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "Sí, finalizar",
        cancelButtonText: "Cancelar"
    }).then((result) => {

        if (result.isConfirmed) {

            Swal.fire({
                title: "Procesando...",
                text: "Por favor espere",
                allowOutsideClick: false,
                allowEscapeKey: false,
                showConfirmButton: false,
                didOpen: () => Swal.showLoading()
            });

            $.ajax({
                url: "/Visita/FinalizarVisita",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    IdVisita: visitaId,
                    Descripcion: desc,
                    HoraFinal: horaFinal,
                    EnviarCorreo: enviarCorreo
                }),
                success: function (response) {

                    if (response.success) {

                        Swal.fire({
                            icon: "success",
                            title: "Visita finalizada",
                            timer: 1500,
                            showConfirmButton: false
                        }).then(() => {
                            $("#modalFinalizarVisita").modal("hide");
                            location.reload();
                        });

                    } else {
                        Swal.fire("Error", response.mensaje, "error");
                    }
                },
                error: function (xhr) {

                    let msg = "No se pudo finalizar la visita";

                    if (xhr.responseJSON?.mensaje)
                        msg = xhr.responseJSON.mensaje;

                    Swal.fire("Error", msg, "error");
                }
            });

        }

    });

});
