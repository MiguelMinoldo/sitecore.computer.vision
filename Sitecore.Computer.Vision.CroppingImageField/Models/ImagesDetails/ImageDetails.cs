using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sitecore.Computer.Vision.CroppingImageField.Models.ImagesDetails
{
    public class ImageDetails
    {
        [JsonProperty("tags")]
        public IList<Tag> Tags { get; set; }

        [JsonProperty("description")]
        public Description Description{get; set;}

        [JsonProperty("brands")]
        public IList<Brand> Brands { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("metadata")]
        public Metadata metadata { get; set; }
    }
}
