using System.Text.Json.Serialization;

namespace RegistroLlamadas.Api.Models
{
    public class EquipoItem
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("tipoestado")]
        public int TipoEstado { get; set; }

        public string? NombreCentro { get; set; }
    }
}
