using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Computer.Vision.CroppingImageField.Models.ImagesDetails;
using Sitecore.Computer.Vision.CroppingImageField.Services;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;

namespace Sitecore.Computer.Vision.CroppingImageField.Fields
{
    public class AICroppedImageField : ImageField
    {
        private readonly ICognitiveServices _cognitiveServices;

        public new string Alt
        {
            get
            {
                var  str = this.GetAttribute("alt");
                var mediaItem = this.MediaItem;

                if (str.Length == 0 && mediaItem != null)
                {
                    str = ((MediaItem)mediaItem).Alt;

                    if (str.Length == 0)
                    {
                        str = mediaItem[nameof(Alt)];
                    }
                }

                ImageDetails imageDetails;

                using (var streamReader = new MemoryStream())
                {
                    var mediaStrm = ((MediaItem)mediaItem).GetMediaStream();

                    mediaStrm.CopyTo(streamReader);

                    imageDetails = _cognitiveServices.AnalyzeImage(streamReader.ToArray());
                }

                var autoAlt = imageDetails?.Description?.Captions?.FirstOrDefault()?.Text;

                return str + Constants.Separators.AltSeparator + autoAlt;
            }

            set => this.SetAttribute("alt", value);
        }

        public AICroppedImageField(Field innerField) : base(innerField)
        {
            _cognitiveServices = ServiceLocator.ServiceProvider.GetService<ICognitiveServices>();
        }

        public AICroppedImageField(Field innerField, string runtimeValue) : base(innerField, runtimeValue)
        {
            _cognitiveServices = ServiceLocator.ServiceProvider.GetService<ICognitiveServices>();
        }
    }
}
