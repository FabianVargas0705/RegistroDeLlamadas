using System.Text.Json.Serialization;

namespace RegistroLlamadas.UI.Models
{
    public class ClienteItem
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("identificacion")]
        public string Identificacion { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }
    }
}
