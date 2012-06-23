using System;
using System.Runtime.Serialization;
using Wintellog.Common;

namespace Wintellog.DataModel
{
    [DataContract]
    public abstract class BaseItem : BindableBase 
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public Uri PageUri { get; set; }              
    }
}
