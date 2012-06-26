using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Wintellog2.DataModel
{
    [DataContract]
    public class BlogGroup : BaseItem 
    {
        private ObservableCollection<BlogItem> _items; 
              
        [DataMember]
        public Uri RssUri { get; set; }

        private int _itemCount;

        public int ItemCount
        {
            get { return _itemCount; }
            set
            {
                SetProperty(ref _itemCount, value);
            }
        }

        private int _newItemCount;

        public int NewItemCount
        {
            get { return _newItemCount; }
            set
            {
                SetProperty(ref _newItemCount, value); 
            }
        }

        public ObservableCollection<BlogItem> Items
        {
            get
            {
                return _items = _items ?? new ObservableCollection<BlogItem>();
            }
            set
            {
                _items = value;
            }
        }
    }
}
