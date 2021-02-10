using Sitecore.Diagnostics;
using Sitecore.Resources.Media;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using Sitecore.Computer.Vision.CroppingImageField.Services;
using Sitecore.DependencyInjection;

namespace Sitecore.Computer.Vision.CroppingImageField.Processors
{
    public class AICroppingProcessor
    {
        private static readonly string[] AllowedExtensions = { "bmp", "jpeg", "jpg", "png", "gif" };

        private readonly ICroppingService _croppingService;

        public AICroppingProcessor(ICroppingService croppingService)
        {
            _croppingService = croppingService;
        }

        public AICroppingProcessor()
        {
            _croppingService = ServiceLocator.ServiceProvider.GetService<ICroppingService>();
        }

        public void Process(GetMediaStreamPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            var outputStream = args.OutputStream;

            if (outputStream == null)
            {
                return;
            }

            if (!AllowedExtensions.Any(i => i.Equals(args.MediaData.Extension, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            var smartCrop = args.Options.CustomOptions[Constants.QueryStringKeys.SmartCropping];

            if (!string.IsNullOrEmpty(smartCrop) && bool.Parse(smartCrop))
            {
                Stream outputStrm;

                outputStrm = Stream.Synchronized(_croppingService.GetCroppedImage(args.Options.Width, args.Options.Height, outputStream.MediaItem));
                args.OutputStream = new MediaStream(outputStrm, args.MediaData.Extension, outputStream.MediaItem);
            }
            else if (args.Options.Thumbnail)
            {
                var transformationOptions = args.Options.GetTransformationOptions();
                var thumbnailStream = args.MediaData.GetThumbnailStream(transformationOptions);

                if (thumbnailStream != null)
                {
                    args.OutputStream = thumbnailStream;
                }
            }
        }
    }
}
