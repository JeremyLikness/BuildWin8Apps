using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using PortableWintellog.Contracts;
using PortableWintellog.Data;

namespace WintellogWpf
{
    public class StorageUtility : IStorageUtility
    {
        public Task<string[]> ListItems(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            return Task.Run(() =>
                {
                    var directory = Path.Combine(GetRootPath(), folderName);
                    return
                        Directory.Exists(directory)
                            ? Directory.GetFiles(directory)
                            : new string[0];
                });
        }

        public Task SaveItem<T>(string folderName, T item) where T : BaseItem
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            return Task.Run(() =>
                {
                    var folder = Path.Combine(GetRootPath(), folderName);

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    var fileName = Path.Combine(
                        folder, 
                        item.Id.GetHashCode().ToString(
                        CultureInfo.InvariantCulture));
                    
                    using (var stream = File.Open(fileName, FileMode.Create, FileAccess.Write))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(T));
                        serializer.WriteObject(stream, item);
                    }
                });
        }

        public Task<T> RestoreItem<T>(string folderName, string hashCode) where T : BaseItem, new()
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName");
            }

            if (string.IsNullOrEmpty(hashCode))
            {
                throw new ArgumentNullException("hashCode");
            }

            return Task.Run(() =>
                {
                    var folder = Path.Combine(GetRootPath(), folderName);
                    var fileName = Path.Combine(folder, hashCode);
                    using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(T));
                        return (T)serializer.ReadObject(stream);
                    }
                });
        }

        private static string GetRootPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "Wintellog");
        }
    }
}
