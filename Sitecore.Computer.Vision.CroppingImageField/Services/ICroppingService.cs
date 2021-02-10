using Sitecore.Data.Items;
using System.IO;

namespace Sitecore.Computer.Vision.CroppingImageField.Services
{
    public interface ICroppingService
    {
        Stream GetCroppedImage(int width, int height, MediaItem mediaItem); 

        string GenerateThumbnailUrl(int width, int height, MediaItem mediaItem);
    }
}
