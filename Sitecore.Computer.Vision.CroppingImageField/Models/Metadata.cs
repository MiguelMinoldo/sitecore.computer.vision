using Newtonsoft.Json;

namespace Sitecore.Computer.Vision.CroppingImageField.Models
{
    public class Metadata
    {
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("format")]
        public string Format { get; set; }
    }
}
