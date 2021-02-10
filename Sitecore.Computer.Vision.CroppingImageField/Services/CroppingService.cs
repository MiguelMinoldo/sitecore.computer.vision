using Sitecore.Data.Items;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Sitecore.DependencyInjection;
using Sitecore.Resources.Media;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Sitecore.Computer.Vision.CroppingImageField.Services
{
    public class CroppingService : ICroppingService
    {
        private readonly ICognitiveServices _cognitiveServices;

        public CroppingService(ICognitiveServices cognitiveServices)
        {
            _cognitiveServices = cognitiveServices;
        }

        public CroppingService()
        {
            _cognitiveServices = ServiceLocator.ServiceProvider.GetService<ICognitiveServices>();
        }

        public Stream GetCroppedImage(int width, int height, MediaItem mediaItem)
        {
            using (var streamReader = new MemoryStream())
            {
                var mediaStrm = mediaItem.GetMediaStream();

                mediaStrm.CopyTo(streamReader);
                mediaStrm.Position = 0;

                var img = Image.FromStream(mediaStrm);

                // The cropping size shouldn't be higher than the original image
                if (width > img.Width || height > img.Height)
                {
                    Sitecore.Diagnostics.Log.Warn($"Media file is smaller than the requested crop size. " +
                        $"This can result on a low quality result. Please upload a proper image: " +
                        $"Min Height:{height}, Min Width:{width}. File: {mediaItem.DisplayName}, Path{mediaItem.MediaPath}", this);
                }

                // if the cropping size exceeds the cognitive services limits, get the focus point and crop 
                if (width > 1025 || height > 1024)
                {

                    var area = _cognitiveServices.GetAreaOfImportance(streamReader.ToArray());
                    var cropImage = CropImage(img, area.areaOfInterest.X, area.areaOfInterest.Y, width, height);

                    return cropImage;
                }

                var thumbnailResult = _cognitiveServices.GetThumbnail(streamReader.ToArray(), width, height);

                return new MemoryStream(thumbnailResult);
            }
        }

        public string GenerateThumbnailUrl(int width, int height, MediaItem mediaItem)
        {
            var streamReader = MediaManager.GetMedia(mediaItem).GetStream();
            {
                using (var memStream = new MemoryStream())
                {
                    streamReader.Stream.CopyTo(memStream);

                    var thumbnail = _cognitiveServices.GetThumbnail(memStream.ToArray(), width, height);
                    var imreBase64Data = System.Convert.ToBase64String(thumbnail);

                    return $"data:image/png;base64,{imreBase64Data}";
                }
            }
        }

        private Stream CropImage(Image source, int x, int y, int width, int height)
        {
            var bmp = new Bitmap(width, height);
            var outputStrm = new MemoryStream();

            using (var gr = Graphics.FromImage(bmp))
            {
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    gr.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), x, y, width, height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            bmp.Save(outputStrm, source.RawFormat);

            return outputStrm;
        }
    }
}
