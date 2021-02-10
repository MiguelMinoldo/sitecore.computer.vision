using Sitecore.Computer.Vision.CroppingImageField.Mappers;
using Sitecore.Resources.Media;

namespace Sitecore.Computer.Vision.CroppingImageField.Extensions
{
    public static class GlassMapperExtensions
    {
        public static string GetImageUrl(this AICroppedImage imageField, int width, int height)
        {
            if (imageField == null)
            {
                return string.Empty;
            }

            if (width <= 0 || height <= 0)
            {
                return string.Empty;
            }

            var src = $"{imageField.Src}?w={width}&h={height}";

            var hash = HashingUtils.GetAssetUrlHash(src);

            return $"{src}&hash={hash}";
        }
    }
}