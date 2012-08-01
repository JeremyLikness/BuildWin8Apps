using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Storage;
using PortableWintellog.Data;
using PortableWintellog.Contracts;

namespace WintellogMvvm.DataModel
{    
    public class StorageUtility : IStorageUtility
    {
        public async Task<string[]> ListItems(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            var folder = await ApplicationData.Current.LocalFolder
                                             .CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
            return (from file in await folder.GetFilesAsync() select file.DisplayName).ToArray();
        }

        public async Task SaveItem<T>(string folderName, T item)
            where T : BaseItem
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            try
            {
                var folder = await ApplicationData.Current.LocalFolder
                                                 .CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
                var file =
                    await
                    folder.CreateFileAsync(item.Id.GetHashCode().ToString(), CreationCollisionOption.ReplaceExisting);
                var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                using (var outStream = stream.GetOutputStreamAt(0))
                {
                    var serializer = new DataContractJsonSerializer(typeof (T));
                    serializer.WriteObject(outStream.AsStreamForWrite(), item);
                    await outStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public async Task<T> RestoreItem<T>(string folderName, string hashCode)
            where T : BaseItem, new()
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            if (string.IsNullOrEmpty(hashCode))
            {
                throw new ArgumentNullException("hashCode");
            }

            var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(folderName);
            var file = await folder.GetFileAsync(hashCode);
            var inStream = await file.OpenSequentialReadAsync();
            var serializer = new DataContractJsonSerializer(typeof (T));
            var retVal = (T) serializer.ReadObject(inStream.AsStreamForRead());
            return retVal;
        }
    }
}