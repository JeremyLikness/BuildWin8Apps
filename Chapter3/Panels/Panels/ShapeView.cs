using Panels.Data;
using Windows.UI.Xaml.Controls;

namespace Panels
{
    public class ShapeView : GridView
    {
        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            var itemdetail = item as SimpleItem;
            if (itemdetail != null)
            {
                if (itemdetail.Type.Equals(ItemType.Ellipse) ||
                    itemdetail.Type.Equals(ItemType.Rectangle))
                {
                    element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 2.0);
                }
            }
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
