using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Sitecore.Computer.Vision.CroppingImageField.Fields;
using Sitecore.Data.Items;
using Sitecore.Mvc.Helpers;
using Sitecore.Resources.Media;
using Sitecore.Computer.Vision.CroppingImageField.Models;

namespace Sitecore.Computer.Vision.CroppingImageField.Extensions
{
    public static class HtmlHelperExtensions
    {
        #region Constants
        private const string Hash = "hash";
        private const string Img = "img";
        private const string Src = "src";
        private const string Alt = "alt";
        #endregion

        public static HtmlString AICroppingImageField(this SitecoreHelper helper, string field, Item item, AdvancedImageParameters imageParams)
        {
            if (item == null)
            {
                return new HtmlString("No datasource set");
            }

            var imageField = new AICroppedImageField(item.Fields[field]);
            var xml = new XmlDocument();

            if (string.IsNullOrEmpty(imageField.Value))
            {
                return new HtmlString("No field data set");
            }

            xml.LoadXml(imageField.Value);

            if (xml.DocumentElement == null)
            {
                return new HtmlString("No cropping image parameters found.");
            }
             
            var imageSrc = MediaManager.GetMediaUrl(imageField.MediaItem);
            var src = $"{imageSrc}?w={imageParams.Width}&h={imageParams.Height}&{Constants.QueryStringKeys.SmartCropping}=true";

            src = $"{src}&{Hash}={HashingUtils.GetAssetUrlHash(src)}";

            if (imageParams.OnlyUrl)
            {
                return MvcHtmlString.Create(src);
            }

            var builder = new TagBuilder(Img);

            if (imageParams.Width > 0 && imageParams.Height > 0)
            {
                builder.Attributes.Add(Src, src);
            }
            else
            {
                builder.Attributes.Add(Src, "data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==");

                if (!string.IsNullOrEmpty(imageParams.Sizes))
                {
                    builder.Attributes.Add(imageParams.SizesTag, imageParams.Sizes);
                }

                var widthsList = imageParams.Widths.Split(',').ToList();

                List<string> srcSets;

                var srcSetUrls = GetSrcSetUrls(imageField.MediaItem, widthsList, out srcSets);

                if (srcSets.Any())
                {
                    builder.Attributes.Add(imageParams.SrcSetTag, srcSetUrls);
                }
            }

            var alt = imageField.Alt.Split(new string[] { Constants.Separators.AltSeparator }, StringSplitOptions.None);

            if (imageParams.AutoAltText && alt.Length > 1)
            {
                builder.Attributes.Add(Alt, alt[1]);
            }
            else
            {
                builder.Attributes.Add(Alt, alt.FirstOrDefault());
            }

            if (!string.IsNullOrEmpty(imageParams.CssClass))
            {
                builder.AddCssClass(imageParams.CssClass);
            }

            var htmlTagRes = builder.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(htmlTagRes);
        }

        private static string GetSrcSetUrls(MediaItem mediaItem, List<string> widthsList, out List<string> srcList)
        {
            srcList = widthsList.Select(width =>
                    $"{HashingUtils.ProtectAssetUrl(MediaManager.GetMediaUrl(mediaItem, new MediaUrlOptions {Width = int.Parse(width)}))} {width}w")
                .ToList();

            return string.Join(",", srcList);
        }
    }
}
