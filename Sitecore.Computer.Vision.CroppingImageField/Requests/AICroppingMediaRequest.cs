using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;

using System.Web;

namespace Sitecore.Computer.Vision.CroppingImageField.Requests
{
    using System.Collections.Specialized;

    public class AICroppingMediaRequest : MediaRequest
    {
        private HttpRequest _innerRequest;
        private MediaUrlOptions _mediaQueryString;
        private MediaUri _mediaUri;
        private MediaOptions _options;

        protected override MediaOptions GetOptions()
        {
            var queryString = this.InnerRequest.QueryString;

            if (queryString == null || queryString.Count == 0)
            {
                _options = new MediaOptions();
            }
            else
            {
                SetMediaOptionsFromMediaQueryString(queryString);

                if (!string.IsNullOrEmpty(queryString.Get(Constants.QueryStringKeys.SmartCropping)))
                {
                    SetCustomOptionsFromQueryString(queryString);
                }
            }

            if (!this.IsRawUrlSafe)
            {
                if (Settings.Media.RequestProtection.LoggingEnabled)
                {
                    string urlReferrer = this.GetUrlReferrer();

                    Log.SingleError(string.Format("MediaRequestProtection: An invalid/missing hash value was encountered. " +
                        "The expected hash value: {0}. Media URL: {1}, Referring URL: {2}",
                        HashingUtils.GetAssetUrlHash(this.InnerRequest.RawUrl), this.InnerRequest.RawUrl,
                        string.IsNullOrEmpty(urlReferrer) ? "(empty)" : urlReferrer), this);
                }

                _options = new MediaOptions();
            }

            return _options;
        }

        private void SetCustomOptionsFromQueryString(NameValueCollection queryString)
        {
            this.ProcessCustomParameters(_options);

            if (!string.IsNullOrEmpty(queryString.Get(Constants.QueryStringKeys.SmartCropping))
                    && !_options.CustomOptions.ContainsKey(Constants.QueryStringKeys.SmartCropping)
                    && !string.IsNullOrEmpty(queryString.Get(Constants.QueryStringKeys.SmartCropping)))
            {
                _options.CustomOptions.Add(Constants.QueryStringKeys.SmartCropping, queryString.Get(Constants.QueryStringKeys.SmartCropping));
            }
        }

        private void SetMediaOptionsFromMediaQueryString(NameValueCollection queryString)
        {
            MediaUrlOptions mediaQueryString = this.GetMediaQueryString();

            _options = new MediaOptions()
            {
                AllowStretch = mediaQueryString.AllowStretch,
                BackgroundColor = mediaQueryString.BackgroundColor,
                IgnoreAspectRatio = mediaQueryString.IgnoreAspectRatio,
                Scale = mediaQueryString.Scale,
                Width = mediaQueryString.Width,
                Height = mediaQueryString.Height,
                MaxWidth = mediaQueryString.MaxWidth,
                MaxHeight = mediaQueryString.MaxHeight,
                Thumbnail = mediaQueryString.Thumbnail,
                UseDefaultIcon = mediaQueryString.UseDefaultIcon
            };

            if (mediaQueryString.DisableMediaCache)
            {
                _options.UseMediaCache = false;
            }

            foreach (string allKey in queryString.AllKeys)
            {
                if (allKey != null && queryString[allKey] != null)
                {
                    _options.CustomOptions[allKey] = queryString[allKey];
                }
            }
        }

        public override MediaRequest Clone()
        {
            Assert.IsTrue((base.GetType() == typeof(AICroppingMediaRequest)), "The Clone() method must be overridden to support prototyping.");

            return new AICroppingMediaRequest
            {
                _innerRequest = this._innerRequest,
                _mediaUri = this._mediaUri,
                _options = this._options,
                _mediaQueryString = this._mediaQueryString
            };
        }
    }
}
