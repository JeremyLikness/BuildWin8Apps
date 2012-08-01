using System.Threading.Tasks;
using PortableWintellog.Data;

namespace PortableWintellog.Contracts
{
    public interface IStorageUtility
    {
        Task<string[]> ListItems(string folderName);

        Task SaveItem<T>(string folderName, T item)
            where T : BaseItem;

        Task<T> RestoreItem<T>(string folderName, string hashCode)
            where T : BaseItem, new();
    }
}