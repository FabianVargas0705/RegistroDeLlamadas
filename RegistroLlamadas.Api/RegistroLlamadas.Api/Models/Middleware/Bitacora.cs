namespace RegistroLlamadas.Api.Models.Middleware
{
    public class Bitacora
    {
        public int IdBitacora { get; set; }
        public int UsuarioId { get; set; }
        public string Accion { get; set; }
        public DateTime Fecha { get; set; }
        public string TablaAfectada { get; set; }
        public string Metodo { get; set; }
        public string Ruta { get; set; }
        public string QueryString { get; set; }
        public string Ip { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public int? StatusCode { get; set; }
        public int? TiempoMs { get; set; }
        public string Detalles { get; set; }
        public string NombreUsuario { get; set; }
        public string RolUsuario { get; set; }
    }
}
