using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Wintellog.DataModel
{
    [DataContract]
    public class BlogItem : BaseItem
    {
        private static Random _random = new Random();

        private static Random RandomNumber
        {
            get
            {
                _random = _random ?? new Random();
                return _random;
            }
        }

        public BlogItem()
        {
            Initialize();
        }

        [OnDeserialized]
        public void Init(StreamingContext c)
        {
            Initialize();
        }

        public void Initialize()
        {            
            if (ImageUriList == null)
            {
                ImageUriList = new ObservableCollection<Uri>();
            }

// ReSharper disable ExplicitCallerInfoArgument
            ImageUriList.CollectionChanged += (o, e) => OnPropertyChanged("DefaultImageUri");
// ReSharper restore ExplicitCallerInfoArgument
        }

        public BlogGroup Group { get; set; }

        [DataMember]
        public string Description { get; set; }

        private ObservableCollection<Uri> _imageUriList;

        [DataMember]
        public ObservableCollection<Uri> ImageUriList
        {
            get
            {
                return _imageUriList;
            }

            set
            {
                _imageUriList = value;

                if (_imageUriList != null)
                {
                    _imageUriList.CollectionChanged += (o, e) =>
                    {
// ReSharper disable ExplicitCallerInfoArgument
                        OnPropertyChanged("DefaultImageUri");
                        OnPropertyChanged("FilteredList");
// ReSharper restore ExplicitCallerInfoArgument
                    };
                }

                OnPropertyChanged();
            }
        }        

        public Uri DefaultImageUri
        {
            get
            {
                var filteredList = ImageUriManager.FilteredImageSet(Id, ImageUriList).ToArray();
                if (filteredList == null || filteredList.Length < 1)
                {
                    return new Uri("http://www.wintellect.com/images/WintellectLogo.png", UriKind.Absolute);
                }
                var x = RandomNumber.Next(1, filteredList.Length) - 1;
                return filteredList[x];
            }
        }

        [DataMember]
        public DateTime PostDate { get; set; }        
    }
}
