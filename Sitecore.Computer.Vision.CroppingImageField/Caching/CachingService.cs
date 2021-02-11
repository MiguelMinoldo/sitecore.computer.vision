using Sitecore.Caching;

namespace Sitecore.Computer.Vision.CroppingImageField.Caching
{
    public class CachingService : CustomCache
    {
        public CachingService(string name, long maxSize) : base(name, maxSize)
        {
        }

        public virtual void Set(string key, object value)
        {
            this.SetObject(key, value);
        }

        public virtual object Get(string key)
        {
            return this.GetObject(key);
        }
    }
}