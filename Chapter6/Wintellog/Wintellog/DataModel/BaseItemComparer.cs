using System.Collections.Generic;

namespace Wintellog.DataModel
{
    public class BaseItemComparer : IEqualityComparer<BaseItem>
    {
        public bool Equals(BaseItem x, BaseItem y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(BaseItem obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
