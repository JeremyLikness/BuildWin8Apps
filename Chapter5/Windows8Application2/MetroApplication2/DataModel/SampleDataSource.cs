using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows8Application2.Common;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace Windows8Application2.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [WebHostHidden]
    public abstract class SampleDataCommon : BindableBase
    {
        private static readonly Uri BaseUri = new Uri("ms-appx:///");
        private string _description = string.Empty;
        private ImageSource _image;
        private String _imagePath;
        private string _subtitle = string.Empty;
        private string _title = string.Empty;

        private string _uniqueId = string.Empty;

        protected SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            _uniqueId = uniqueId;
            _title = title;
            _subtitle = subtitle;
            _description = description;
            _imagePath = imagePath;
        }

        public string UniqueId
        {
            get { return _uniqueId; }
            set { SetProperty(ref _uniqueId, value); }
        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public string Subtitle
        {
            get { return _subtitle; }
            set { SetProperty(ref _subtitle, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        public ImageSource Image
        {
            get
            {
                if (_image == null && _imagePath != null)
                {
                    _image = new BitmapImage(new Uri(BaseUri, _imagePath));
                }
                return _image;
            }

            set
            {
                _imagePath = null;
                SetProperty(ref _image, value);
            }
        }

        public void SetImage(String path)
        {
            _image = null;
            _imagePath = path;
// ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("Image");
// ReSharper restore ExplicitCallerInfoArgument
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        private string _content = string.Empty;

        private SampleDataGroup _group;

        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description,
                              String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            _content = content;
            _group = group;
        }

        public string Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }

        public SampleDataGroup Group
        {
            get { return _group; }
            set { SetProperty(ref _group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items = new ObservableCollection<SampleDataItem>();
// ReSharper disable ExplicitCallerInfoArgument
            Items.CollectionChanged += (o, e) => OnPropertyChanged("TopItems");
// ReSharper restore ExplicitCallerInfoArgument
        }
        
        public ObservableCollection<SampleDataItem> Items { get; private set; }
        
        public IEnumerable<SampleDataItem> TopItems
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed
            get { return Items.Take(12); }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// </summary>
    public sealed class SampleDataSource
    {
        private readonly ObservableCollection<SampleDataGroup> _itemGroups = new ObservableCollection<SampleDataGroup>();

        public ObservableCollection<SampleDataGroup> ItemGroups
        {
            get { return _itemGroups; }
        }

        public readonly string ItemContent = String.Format(
            "Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
            "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat");

        public Task Initialize()
        {
            return Task.Run(() => InitializeDataSource());
        }

        private void InitializeDataSource()
        {
            var group1 = new SampleDataGroup("Group-1",
                                             "Group Title: 1",
                                             "Group Subtitle: 1",
                                             "Assets/DarkGray.png",
                                             "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                                                "Item Title: 1",
                                                "Item Subtitle: 1",
                                                "Assets/LightGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                                                "Item Title: 2",
                                                "Item Subtitle: 2",
                                                "Assets/DarkGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                                                "Item Title: 3",
                                                "Item Subtitle: 3",
                                                "Assets/MediumGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                                                "Item Title: 4",
                                                "Item Subtitle: 4",
                                                "Assets/DarkGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                                                "Item Title: 5",
                                                "Item Subtitle: 5",
                                                "Assets/MediumGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group1));
            ItemGroups.Add(group1);

            var group2 = new SampleDataGroup("Group-2",
                                             "Group Title: 2",
                                             "Group Subtitle: 2",
                                             "Assets/LightGray.png",
                                             "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                                                "Item Title: 1",
                                                "Item Subtitle: 1",
                                                "Assets/DarkGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                                                "Item Title: 2",
                                                "Item Subtitle: 2",
                                                "Assets/MediumGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                                                "Item Title: 3",
                                                "Item Subtitle: 3",
                                                "Assets/LightGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group2));
            ItemGroups.Add(group2);

            var group3 = new SampleDataGroup("Group-3",
                                             "Group Title: 3",
                                             "Group Subtitle: 3",
                                             "Assets/MediumGray.png",
                                             "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group3.Items.Add(new SampleDataItem("Group-3-Item-1",
                                                "Item Title: 1",
                                                "Item Subtitle: 1",
                                                "Assets/MediumGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-2",
                                                "Item Title: 2",
                                                "Item Subtitle: 2",
                                                "Assets/LightGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-3",
                                                "Item Title: 3",
                                                "Item Subtitle: 3",
                                                "Assets/DarkGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-4",
                                                "Item Title: 4",
                                                "Item Subtitle: 4",
                                                "Assets/LightGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-5",
                                                "Item Title: 5",
                                                "Item Subtitle: 5",
                                                "Assets/MediumGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-6",
                                                "Item Title: 6",
                                                "Item Subtitle: 6",
                                                "Assets/DarkGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-7",
                                                "Item Title: 7",
                                                "Item Subtitle: 7",
                                                "Assets/MediumGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group3));
            ItemGroups.Add(group3);

            var group4 = new SampleDataGroup("Group-4",
                                             "Group Title: 4",
                                             "Group Subtitle: 4",
                                             "Assets/LightGray.png",
                                             "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group4.Items.Add(new SampleDataItem("Group-4-Item-1",
                                                "Item Title: 1",
                                                "Item Subtitle: 1",
                                                "Assets/DarkGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-2",
                                                "Item Title: 2",
                                                "Item Subtitle: 2",
                                                "Assets/LightGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
                                                "Item Title: 3",
                                                "Item Subtitle: 3",
                                                "Assets/DarkGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-4",
                                                "Item Title: 4",
                                                "Item Subtitle: 4",
                                                "Assets/LightGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-5",
                                                "Item Title: 5",
                                                "Item Subtitle: 5",
                                                "Assets/MediumGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-6",
                                                "Item Title: 6",
                                                "Item Subtitle: 6",
                                                "Assets/LightGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group4));
            ItemGroups.Add(group4);

            var group5 = new SampleDataGroup("Group-5",
                                             "Group Title: 5",
                                             "Group Subtitle: 5",
                                             "Assets/MediumGray.png",
                                             "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group5.Items.Add(new SampleDataItem("Group-5-Item-1",
                                                "Item Title: 1",
                                                "Item Subtitle: 1",
                                                "Assets/LightGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group5));
            group5.Items.Add(new SampleDataItem("Group-5-Item-2",
                                                "Item Title: 2",
                                                "Item Subtitle: 2",
                                                "Assets/DarkGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group5));
            group5.Items.Add(new SampleDataItem("Group-5-Item-3",
                                                "Item Title: 3",
                                                "Item Subtitle: 3",
                                                "Assets/LightGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group5));
            group5.Items.Add(new SampleDataItem("Group-5-Item-4",
                                                "Item Title: 4",
                                                "Item Subtitle: 4",
                                                "Assets/MediumGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group5));
            ItemGroups.Add(group5);

            var group6 = new SampleDataGroup("Group-6",
                                             "Group Title: 6",
                                             "Group Subtitle: 6",
                                             "Assets/DarkGray.png",
                                             "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group6.Items.Add(new SampleDataItem("Group-6-Item-1",
                                                "Item Title: 1",
                                                "Item Subtitle: 1",
                                                "Assets/LightGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group6));
            group6.Items.Add(new SampleDataItem("Group-6-Item-2",
                                                "Item Title: 2",
                                                "Item Subtitle: 2",
                                                "Assets/DarkGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group6));
            group6.Items.Add(new SampleDataItem("Group-6-Item-3",
                                                "Item Title: 3",
                                                "Item Subtitle: 3",
                                                "Assets/MediumGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group6));
            group6.Items.Add(new SampleDataItem("Group-6-Item-4",
                                                "Item Title: 4",
                                                "Item Subtitle: 4",
                                                "Assets/DarkGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group6));
            group6.Items.Add(new SampleDataItem("Group-6-Item-5",
                                                "Item Title: 5",
                                                "Item Subtitle: 5",
                                                "Assets/LightGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group6));
            group6.Items.Add(new SampleDataItem("Group-6-Item-6",
                                                "Item Title: 6",
                                                "Item Subtitle: 6",
                                                "Assets/MediumGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group6));
            group6.Items.Add(new SampleDataItem("Group-6-Item-7",
                                                "Item Title: 7",
                                                "Item Subtitle: 7",
                                                "Assets/DarkGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group6));
            group6.Items.Add(new SampleDataItem("Group-6-Item-8",
                                                "Item Title: 8",
                                                "Item Subtitle: 8",
                                                "Assets/LightGray.png",
                                                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                                                ItemContent,
                                                group6));
            ItemGroups.Add(group6);
        }

        public SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            return _itemGroups.FirstOrDefault(group => @group.UniqueId.Equals(uniqueId));
        }

        public SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            return
                _itemGroups.SelectMany(group => @group.Items).FirstOrDefault(item => item.UniqueId.Equals(uniqueId));
        }

        public void AddItem(SampleDataGroup group)
        {
            var number = DateTime.Now.Ticks;
            var item = new SampleDataItem(
                string.Format("{0}-Item-{1}", group.Title, number),
                string.Format("Item Title: {0}", number),
                string.Format("Item Subtitle: {0}", number),
                "Assets/LightGray.png",
                "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                ItemContent,
                group);
            group.Items.Add(item);
        }
    }
}