using Newtonsoft.Json;

namespace Sitecore.Computer.Vision.CroppingImageField.Models.ImagesDetails
{
    public class Tag
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }
}
