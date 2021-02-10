using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sitecore.Computer.Vision.CroppingImageField.Models.ImagesDetails
{
    public class Description
    {
        [JsonProperty("tags")]
        public IList<string> Tags { get; set; }

        [JsonProperty("captions")]
        public IList<Caption> Captions{ get; set; }
    }
}
