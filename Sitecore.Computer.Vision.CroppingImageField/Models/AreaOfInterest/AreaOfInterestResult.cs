using Newtonsoft.Json;

namespace Sitecore.Computer.Vision.CroppingImageField.Models.AreaOfInterest
{
    public class AreaOfInterestResult
    {
        [JsonProperty("areaOfInterest")]
        public AreaOfInterest areaOfInterest { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("metadata")]
        public Metadata metadata{ get; set; }
    }
}
