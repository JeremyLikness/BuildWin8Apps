using System.Collections.Generic;
using System.Threading.Tasks;
using PortableWintellog.Contracts;
using PortableWintellog.Data;

namespace WintellogTest.TestHelpers
{
    public class SyndicationHelperTest : ISyndicationHelper 
    {
        public List<BlogGroup> BlogGroups { get; set; }
        public List<BlogItem> BlogItems { get; set; } 

        public SyndicationHelperTest()
        {
            BlogGroups = new List<BlogGroup>();
            BlogItems = new List<BlogItem>();
        }

        public Task<IList<BlogGroup>> LoadBlogsAsync()
        {
            return Task.Run(() => ((IList<BlogGroup>) BlogGroups));
        }

        public Task<IList<BlogItem>> LoadItemsAsync(BlogGroup group)
        {
            return Task.Run(() => ((IList<BlogItem>) BlogItems));
        }
    }
}
