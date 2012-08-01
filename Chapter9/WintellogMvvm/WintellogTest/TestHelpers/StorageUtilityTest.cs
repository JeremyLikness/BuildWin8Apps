using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PortableWintellog.Contracts;
using PortableWintellog.Data;

namespace WintellogTest.TestHelpers
{
    public class StorageUtilityTest : IStorageUtility 
    {
        public string[] Items { get; set; }
        public List<Tuple<string,object>> SavedItems 
            = new List<Tuple<string, object>>();

        public Func<Type, string, string, object> Restore { get; set; }

        public Task<string[]> ListItems(string folderName)
        {
            return Task.Run(() => Items);
        }

        public Task SaveItem<T>(string folderName, T item) where T : BaseItem
        {
            return Task.Run(()=> SavedItems.Add(Tuple.Create(folderName, (object)item)));
        }

        public Task<T> RestoreItem<T>(string folderName, string hashCode) where T : BaseItem, new()
        {
            return Task.Run(() => (T)Restore(typeof(T), folderName, hashCode));
        }
    }
}