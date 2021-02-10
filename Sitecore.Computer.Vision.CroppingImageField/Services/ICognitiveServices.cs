using Sitecore.Computer.Vision.CroppingImageField.Models.AreaOfInterest;
using Sitecore.Computer.Vision.CroppingImageField.Models.ImagesDetails;

namespace Sitecore.Computer.Vision.CroppingImageField.Services
{
    public interface ICognitiveServices
    {
        ImageDetails AnalyzeImage(byte[] image);

        byte[] GetThumbnail(byte[] image, int width, int height);

        AreaOfInterestResult GetAreaOfImportance(byte[] image);
    }
}
