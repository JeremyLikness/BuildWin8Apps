using System.Collections.Generic;

namespace Panels.Data
{
    public class SimpleItemList : List<SimpleItem>
    {
        public SimpleItemList()
        {
            var x = 20;
            while (x-- > 0)
            {
                Add(SimpleItem.GenerateItem());
            }
        }
    }
}
