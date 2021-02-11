using System;
using Newtonsoft.Json;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Sitecore.Computer.Vision.CroppingImageField.Models.AreaOfInterest;
using Sitecore.Computer.Vision.CroppingImageField.Models.ImagesDetails;
using Sitecore.Computer.Vision.CroppingImageField.Caching;
using Sitecore.Computer.Vision.CroppingImageField.Extensions;

namespace Sitecore.Computer.Vision.CroppingImageField.Services
{
    public class CognitiveServices : ICognitiveServices
    {
        private readonly string _cognitiveServicesKey = Settings.GetSetting($"Sitecore.Computer.Vision.CroppingImageField.AICroppingField.CognitiveServices.ApiKey", "");
        private readonly string _cognitiveServicesUrl = Settings.GetSetting($"Sitecore.Computer.Vision.CroppingImageField.AICroppingField.CognitiveServices.ApiUrl", "");
        private readonly string _cognitiveServicesZone = Settings.GetSetting($"Sitecore.Computer.Vision.CroppingImageField.AICroppingField.CognitiveServices.ApiZone", "");

        public ImageDetails AnalyzeImage(byte[] image)
        {
            var requestUri = _cognitiveServicesUrl + "analyze?" + Settings.GetSetting(
            $"Sitecore.Computer.Vision.CroppingImageField.AICroppingField.CognitiveServices.Analyze.Parameters", "");

            return CacheManager.GetCachedObject(image.GetHashKey() + requestUri, () =>
            {
                using (var response = this.CallApi(image, requestUri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        var responeData =
                            JsonConvert.DeserializeObject<ImageDetails>(result, new JsonSerializerSettings());

                        return responeData;
                    }

                    var errorMessage = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    Log.Error(errorMessage, this);

                    return null;
                }
            });
        }

        public byte[] GetThumbnail(byte[] image, int width, int height)
        {
            var requestUri = _cognitiveServicesUrl +
                $"generateThumbnail?width={width}&height={height}&{Constants.QueryStringKeys.SmartCropping}=true";

            return CacheManager.GetCachedObject(image.GetHashKey() + requestUri, () =>
            {
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
            });
        }

        public AreaOfInterestResult GetAreaOfImportance(byte[] image)
        {
            var requestUri = _cognitiveServicesUrl + "areaOfInterest";

            return CacheManager.GetCachedObject(image.GetHashKey() + requestUri, () =>
            {
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
            });
        }

        private HttpResponseMessage CallApi(byte[] image, string requestUri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _cognitiveServicesKey);
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", _cognitiveServicesZone);

                using (var content = new ByteArrayContent(image))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    return client.PostAsync(requestUri, content).GetAwaiter().GetResult();
                }
            }
        }
    }
}
