using System.Collections.Generic;
using System.Threading.Tasks;
using PortableWintellog.Data;

namespace PortableWintellog.Contracts
{
    public interface ISyndicationHelper
    {
        Task<IList<BlogGroup>> LoadBlogsAsync();
        Task<IList<BlogItem>> LoadItemsAsync(BlogGroup group);
    }
}
