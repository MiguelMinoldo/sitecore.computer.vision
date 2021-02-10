using Glass.Mapper.Sc;
using Glass.Mapper.Sc.Configuration;
using Glass.Mapper.Sc.DataMappers;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using System;
using Sitecore.Computer.Vision.CroppingImageField.Fields;

namespace Sitecore.Computer.Vision.CroppingImageField.Mappers
{
    public class AICroppedImageFieldMapper : AbstractSitecoreFieldMapper
    {
        public AICroppedImageFieldMapper(): base(typeof(AICroppedImage))
        {
        }

        public override object GetField(Field field, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            var img = new AICroppedImage();
            var sitecoreImage = new AICroppedImageField(field);

            SitecoreFieldImageMapper.MapToImage(img, sitecoreImage);

            return img;
        }

        public override void SetField(Field field, object value, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            var img = value as AICroppedImage;

            if (field == null || img == null)
            {
                return;
            }

            var item = field.Item;
            var sitecoreImage = new AICroppedImageField(field);

            SitecoreFieldImageMapper.MapToField(sitecoreImage, img, item);
        }

        public override string SetFieldValue(object value, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            throw new NotImplementedException();
        }

        public override object GetFieldValue(string fieldValue, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            var item = context.Service.Database.GetItem(new ID(fieldValue));

            if (item == null)
            {
                return null;
            }

            var imageItem = new MediaItem(item);
            var image = new AICroppedImage();

            SitecoreFieldImageMapper.MapToImage(image, imageItem);

            return image;
        }
    }
}
