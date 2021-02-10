using Newtonsoft.Json;

namespace Sitecore.Computer.Vision.CroppingImageField.Models.AreaOfInterest
{
    public class AreaOfInterest
    {
        [JsonProperty("x")]
        public int X { get; set; }
        [JsonProperty("y")]
        public int Y { get; set; }
        [JsonProperty("w")]
        public int W { get; set; }
        [JsonProperty("h")]
        public int H { get; set; }
    }
}
