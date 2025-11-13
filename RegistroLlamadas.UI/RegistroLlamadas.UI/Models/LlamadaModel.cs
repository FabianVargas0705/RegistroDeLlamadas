namespace RegistroLlamadas.UI.Models
{
    public class LlamadaModel
    {
        public int IdLlamada { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan? HoraFinal { get; set; }
        public string? Promat { get; set; }
        public string? Asunto { get; set; }
        public bool CorreoEnviado { get; set; }

        // Foreign Keys
        public int? EquipoId { get; set; }
        public int? UsuarioId { get; set; }
        public int? ClienteId { get; set; }
        public int? CentroId { get; set; }
        public int? EstadoId { get; set; }
        public int? TipoAtencionId { get; set; }

        // Navigation Properties
        public EquipoModel? Equipo { get; set; }
        public UsuarioModel? Usuario { get; set; }
        public ClienteModel? Cliente { get; set; }
        public CentroModel? Centro { get; set; }
        public EstadoModel? Estado { get; set; }
        public TipoAtencionModel? TipoAtencion { get; set; }
    }
}
