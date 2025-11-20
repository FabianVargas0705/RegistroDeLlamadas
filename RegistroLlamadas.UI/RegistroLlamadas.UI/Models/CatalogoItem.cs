using System.Text.Json.Serialization;

namespace RegistroLlamadas.UI.Models
{
    public class CatalogoItem
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("tipoestado")]
        public int TipoEstado { get; set; }
    }
}
