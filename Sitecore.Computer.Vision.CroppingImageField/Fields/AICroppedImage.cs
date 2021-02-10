using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources.Media;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.Sheer;
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Sitecore.Computer.Vision.CroppingImageField.Models.ImagesDetails;
using Sitecore.Computer.Vision.CroppingImageField.Services;

namespace Sitecore.Computer.Vision.CroppingImageField.Fields
{
    public class AICroppedImage : Image
    {
        private readonly string ThumbnailsId = Settings.GetSetting("Sitecore.Computer.Vision.CroppingImageField.AICroppingField.ThumbnailsFolderId");
        private readonly ICognitiveServices _cognitiveServices;
        private readonly ICroppingService _croppingService;
       
        public AICroppedImage(ICognitiveServices cognitiveServices, ICroppingService croppingService) : base()
        {
            _cognitiveServices = cognitiveServices;
            _croppingService = croppingService;
        }

        public AICroppedImage() : base()
        {
            _cognitiveServices = ServiceLocator.ServiceProvider.GetService<ICognitiveServices>();
            _croppingService = ServiceLocator.ServiceProvider.GetService<ICroppingService>();
        }

        protected override void DoRender(HtmlTextWriter output)
        {
            Assert.ArgumentNotNull((object)output, nameof(output));

            Item mediaItem = this.GetMediaItem();

            string src;

            this.GetSrc(out src);

            string str1 = " src=\"" + src + "\"";
            string str2 = " id=\"" + this.ID + "_image\"";
            string str3 = " alt=\"" + (mediaItem != null ? HttpUtility.HtmlEncode(mediaItem["Alt"]) : string.Empty) + "\"";
            this.Attributes["placeholder"] = Translate.Text(this.Placeholder);
            string str = this.Password ? " type=\"password\"" : (this.Hidden ? " type=\"hidden\"" : "");
            this.SetWidthAndHeightStyle();
            output.Write("<input" + this.ControlAttributes + str + ">");
            this.RenderChildren(output);
            output.Write("<div id=\"" + this.ID + "_pane\" class=\"scContentControlImagePane\">");

            string clientEvent = Sitecore.Context.ClientPage.GetClientEvent(this.ID + ".Browse");

            output.Write("<div class=\"scContentControlImageImage\" onclick=\"" + clientEvent + "\">");
            output.Write("<iframe" + str2 + str1 + str3 + " frameborder=\"0\" marginwidth=\"0\" marginheight=\"0\" width=\"100%\" height=\"128\" " +
                "allowtransparency=\"allowtransparency\"></iframe>");
            output.Write("<div id=\"" + this.ID + "_thumbnails\">");
            output.Write(GetThumbnails());
            output.Write("</div>");
            output.Write("</div>");
            output.Write("<div>");
            output.Write("<div id=\"" + this.ID + "_details\" class=\"scContentControlImageDetails\">");
            string details = this.GetDetails();
            output.Write(details);
            output.Write("</div>");
            output.Write("</div>");
        }

        protected override void DoChange(Message message)
        {
            Assert.ArgumentNotNull((object)message, nameof(message));

            base.DoChange(message);

            if (Sitecore.Context.ClientPage.Modified)
            {
                this.Update();
            }

            if (string.IsNullOrEmpty(this.Value))
            {
                this.ClearImage();
            }

            SheerResponse.SetReturnValue(true);
        }

        protected new void BrowseImage(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));

            base.BrowseImage(args);

            if (Sitecore.Context.ClientPage.Modified)
            {
                this.Update();
            }
        }

        protected new void ShowProperties(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));

            base.ShowProperties(args);

            if (Sitecore.Context.ClientPage.Modified)
            {
                this.Update();
            }
        }

        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull((object)message, nameof(message));

            base.HandleMessage(message);
           
            string name = message.Name;

            if (name == "contentimage:clear")
            {
                this.ClearImage();
            }
            else if (name == "contentimage:refresh")
            {
                this.Update();
            }
        }

        private void ClearImage()
        {
            if (this.Disabled)
            {
                return;
            }

            if (this.Value.Length > 0)
            {
                this.SetModified();
            }

            this.XmlValue = new XmlValue(string.Empty, "image");
            this.Value = string.Empty;
            this.Update();
        }

        protected new void Update()
        {
            string src;
            this.GetSrc(out src);
            SheerResponse.SetAttribute(this.ID + "_image", "src", src);
            SheerResponse.SetInnerHtml(this.ID + "_thumbnails", this.GetThumbnails());
            SheerResponse.SetInnerHtml(this.ID + "_details", this.GetDetails());
            SheerResponse.Eval("scContent.startValidators()");
        }

        private string GetDetails()
        {
            var empty = string.Empty;

            MediaItem mediaItem = this.GetMediaItem();

            if (mediaItem != null)
            {
                var innerItem = mediaItem.InnerItem;
                var stringBuilder = new StringBuilder();
                var xmlValue = this.XmlValue;

                stringBuilder.Append("<div>");

                var item = innerItem["Dimensions"];
                var str = HttpUtility.HtmlEncode(xmlValue.GetAttribute("width"));
                var str1 = HttpUtility.HtmlEncode(xmlValue.GetAttribute("height"));

                ImageDetails imageDetails;

                using (var streamReader = new MemoryStream())
                {
                    var mediaStrm = mediaItem.GetMediaStream();

                    mediaStrm.CopyTo(streamReader);

                    imageDetails = _cognitiveServices.AnalyzeImage(streamReader.ToArray());
                }

                if (!string.IsNullOrEmpty(str) || !string.IsNullOrEmpty(str1))
                {
                    var objArray = new object[] { str, str1, item };
                    stringBuilder.Append(Translate.Text("Dimensions: {0} x {1} (Original: {2})", objArray));
                }
                else
                {
                    var objArray1 = new object[] { item };
                    stringBuilder.Append(Translate.Text("Dimensions: {0}", objArray1));
                }

                stringBuilder.Append("</div>");
                stringBuilder.Append("<div style=\"padding:2px 0px 0px 0px; text-align=left; \">");

                var str2 = HttpUtility.HtmlEncode(innerItem["Alt"]);
                var str3 = imageDetails.Description.Captions.FirstOrDefault()?.Text;

                if (!string.IsNullOrEmpty(str3) && !string.IsNullOrEmpty(str2))
                {
                    var objArray2 = new object[] { str3, str2 };
                    stringBuilder.Append(Translate.Text("AI Alternate Text: \"{0}\" (Default Alternate Text: \"{1}\")", objArray2));
                }
                else if (!string.IsNullOrEmpty(str3))
                {
                    var objArray3 = new object[] { str3 };
                    stringBuilder.Append(Translate.Text("AI Alternate Text: \"{0}\"", objArray3));
                }
                else
                {
                    var objArray4 = new object[] { str2 };
                    stringBuilder.Append(Translate.Text("Default Alternate Text: \"{0}\"", objArray4));
                }

                stringBuilder.Append("</br>");
                var objArray5 = new object[] { str3 };
                stringBuilder.Append(Translate.Text("Tags: \"{0}\"", string.Join(",", imageDetails.Description.Tags), objArray5));

                stringBuilder.Append("</div>");

                empty = stringBuilder.ToString();
            }
            if (empty.Length == 0)
            {
                empty = Translate.Text("This media item has no details.");
            }
            return empty;
        }

        private Item GetMediaItem()
        {
            var attribute = this.XmlValue.GetAttribute("mediaid");

            if (attribute.Length <= 0)
            {
                return null;
            }

            Language language = Language.Parse(this.ItemLanguage);

            return Sitecore.Client.ContentDatabase.GetItem(attribute, language);
        }

        private MediaItem GetSrc(out string src)
        {
            src = string.Empty;

            MediaItem mediaItem = (MediaItem)this.GetMediaItem();

            if (mediaItem == null)
            {
                return null;
            }

            var thumbnailOptions = MediaUrlOptions.GetThumbnailOptions(mediaItem);
            int result;

            if (!int.TryParse(mediaItem.InnerItem["Height"], out result))
            {
                result = 128;
            }

            thumbnailOptions.Height = Math.Min(128, result);
            thumbnailOptions.MaxWidth = 640;
            thumbnailOptions.UseDefaultIcon = true;

            src = MediaManager.GetMediaUrl(mediaItem, thumbnailOptions);

            return mediaItem;
        }

        private string GetThumbnails()
        {
            var html = new StringBuilder();
            var src = string.Empty;
            var mediaItem = this.GetSrc(out src);

            if (mediaItem == null)
            {
                return string.Empty;
            }

            html.Append("<ul id=" + this.ID + "_frame\" style=\"display: -ms-flexbox;display: flex;-ms-flex-direction: row;flex-direction: row;-ms-flex-wrap: wrap;flex-wrap: wrap;\">");

            var thumbnailFolderItem = Sitecore.Client.ContentDatabase.GetItem(new Sitecore.Data.ID(ThumbnailsId));

            if (thumbnailFolderItem != null && thumbnailFolderItem.HasChildren)
            {
                foreach (Item item in thumbnailFolderItem.Children)
                {
                    GetThumbnailHtml(item, html, mediaItem);
                }
            }
           
            html.Append("</ul>");

            return html.ToString();
        }

        private void GetThumbnailHtml(Item item, StringBuilder html, MediaItem mediaItem)
        {
            if (item.Fields["Size"]?.Value != null)
            {
                var values = item.Fields["Size"].Value.Split('x');
                var width = values[0];
                var height = values[1];

                int w, h;

                if (int.TryParse(width, out w) && Int32.TryParse(height, out h) && w > 0 && h > 0)
                {
                    var imageSrc = _croppingService.GenerateThumbnailUrl(w, h, mediaItem);

                    html.Append(string.Format("<li id=\"Frame_{0}_{1}\" style=\"width: {2}px; height: {3}px; position: relative; overflow: hidden; display: inline-block;border: solid 3px #fff;margin: 5px 5px 5px 0;\">" +
                        "<img style=\"position: relative;position: absolute;left: 0;top: 0;margin: 0;display: block;width: auto; height: auto;min-width: 100%; min-height: 100%;max-height: none; max-width: none;\" " +
                        "src=\"{4}\"><img /><span style=\"position: absolute;" +
                        "top: 0;left: 0;padding: 2px 3px;background-color: #fff;opacity: 0.8;\">{5}</span></li>", this.ID, item.ID.ToShortID(), w, h, imageSrc, item.DisplayName));
                }
            }
        }
    }
}
