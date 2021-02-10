using Sitecore.Diagnostics;
using Sitecore.Pipelines.RenderField;

namespace Sitecore.Computer.Vision.CroppingImageField.Pipelines
{
    public class RenderAICroppingImageField : GetImageFieldValue
    {
        public override void Process(RenderFieldArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));

            if (!this.IsImage(args))
            {
                return;
            }

            var renderer = this.CreateRenderer();

            this.ConfigureRenderer(args, renderer);
            this.SetRenderFieldResult(renderer.Render(), args);
        }

        protected override bool IsImage(RenderFieldArgs args)
        {
            return args.FieldTypeKey == "AI Cropped Image";
        }
    }
}
