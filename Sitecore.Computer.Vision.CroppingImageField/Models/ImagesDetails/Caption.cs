using Newtonsoft.Json;

namespace Sitecore.Computer.Vision.CroppingImageField.Models.ImagesDetails
{
    public class Caption
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }
}
