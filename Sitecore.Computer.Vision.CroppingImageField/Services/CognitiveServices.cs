using Newtonsoft.Json;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Sitecore.Computer.Vision.CroppingImageField.Models.AreaOfInterest;
using Sitecore.Computer.Vision.CroppingImageField.Models.ImagesDetails;

namespace Sitecore.Computer.Vision.CroppingImageField.Services
{
    public class CognitiveServices : ICognitiveServices
    {
        private readonly string CognitiveServicesKey = Settings.GetSetting($"Sitecore.Computer.Vision.CroppingImageField.AICroppingField.CognitiveServices.ApiKey", "");
        private readonly string CognitiveServicesUrl = Settings.GetSetting($"Sitecore.Computer.Vision.CroppingImageField.AICroppingField.CognitiveServices.ApiUrl", "");
        private readonly string CognitiveServicesZone = Settings.GetSetting($"Sitecore.Computer.Vision.CroppingImageField.AICroppingField.CognitiveServices.ApiZone", "");

        public ImageDetails AnalyzeImage(byte[] image)
        {
            var requestUri = CognitiveServicesUrl + "analyze?" + Settings.GetSetting($"Sitecore.Computer.Vision.CroppingImageField.AICroppingField.CognitiveServices.Analyze.Parameters", "");

            using (var response = this.CallApi(image, requestUri))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var responeData = JsonConvert.DeserializeObject<ImageDetails>(result, new JsonSerializerSettings());

                    return responeData;
                }

                var errorMessage = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                Log.Error(errorMessage, this);

                return null;
            }
        }

        public byte[] GetThumbnail(byte[] image, int width, int height)
        {
            var requestUri = CognitiveServicesUrl + $"generateThumbnail?width={width}&height={height}&{Constants.QueryStringKeys.SmartCropping}=true";

            using (var response = this.CallApi(image, requestUri))
            {
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                }

                var errorMessage = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                Log.Error(errorMessage, this);

                return null;
            }
        }

        public AreaOfInterestResult GetAreaOfImportance(byte[] image)
        {
            var requestUri = CognitiveServicesUrl + "areaOfInterest";

            using (var response = this.CallApi(image, requestUri))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var responeData = JsonConvert.DeserializeObject<AreaOfInterestResult>(result, new JsonSerializerSettings());

                    return responeData;
                }

                var errorMessage = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                Log.Error(errorMessage, this);

                return null;
            }
        }

        private HttpResponseMessage CallApi(byte[] image, string requestUri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", CognitiveServicesKey);
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", CognitiveServicesZone);

                using (var content = new ByteArrayContent(image))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    return client.PostAsync(requestUri, content).GetAwaiter().GetResult();
                }
            }
        }
    }
}
