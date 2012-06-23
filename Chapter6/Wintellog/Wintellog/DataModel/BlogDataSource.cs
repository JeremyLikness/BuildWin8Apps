using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.Web.Syndication;

namespace Wintellog.DataModel
{
    public class BlogDataSource
    {
        private const string GROUP_FOLDER = "Groups";

        private const string USER_AGENT =
            "Wintellect/Wintellog (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";

        private const string JEREMY_BLOG = "http://www.wintellect.com/CS/blogs/jlikness/default.aspx";

        private const string SAMPLE_BLOG =
            "http://wintellect.com/CS/blogs/jlikness/archive/2012/02/09/windows-8-the-facts-about-arm-metro-and-the-blue-stack.aspx";

        private static readonly string Utf8ByteOrderMark =
            Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble(), 0, Encoding.UTF8.GetPreamble().Length);

        public BlogDataSource()
        {
            GroupList = new ObservableCollection<BlogGroup>();

            if (DesignMode.DesignModeEnabled)
            {
                LoadTestGroups();
            }
        }

        public ObservableCollection<BlogGroup> GroupList { get; set; }

        public BlogGroup GetGroup(string groupId)
        {
            return GroupList.FirstOrDefault(g => g.Id == groupId);
        }

        public BlogItem GetItem(string itemId)
        {
            return (from g in GroupList
                    from i in g.Items
                    where i.Id == itemId
                    select i).FirstOrDefault();
        }

        public async Task LoadGroups()
        {
            if (Config.TestOnly)
            {
                LoadTestGroups();
            }
            else
            {
                await LoadAllGroups();
            }
        }

        private void LoadTestGroups()
        {
            var jeremyGroup = new BlogGroup
                                  {
                                      Id = JEREMY_BLOG,
                                      PageUri = new Uri(JEREMY_BLOG, UriKind.Absolute),
                                      Title = "Jeremy Likness' Blog",
                                      RssUri =
                                          new Uri("http://www.wintellect.com/CS/blogs/jlikness/rss.aspx",
                                                  UriKind.Absolute),
                                  };
            GroupList.Add(jeremyGroup);

            var item = new BlogItem
                           {
                               Id =
                                   SAMPLE_BLOG,
                               PageUri = new Uri(SAMPLE_BLOG, UriKind.Absolute),
                               Title = "Windows 8: The Facts about ARM, Metro, and the Blue Stack",
                               Description =
                                   "Many eyes will be focused on Barcelona on February 29, 2012 when Microsoft releases the Windows 8 Consumer Preview or what many are calling the beta version of the new platform. You’ve probably heard quite a bit about the Metro interface. It has design...",
                               PostDate = DateTime.Now,
                               ImageUriList = new ObservableCollection<Uri>(new[]
                                                                                {
                                                                                    new Uri(
                                                                                        "http://lh6.ggpht.com/-qhW3FfZ7vXI/TzScQ_3eEEI/AAAAAAAAAds/3en8ijjglEg/stacks_thumb%25255B1%25255D.jpg?imgmax=800",
                                                                                        UriKind.Absolute),
                                                                                    new Uri(
                                                                                        "http://lh5.ggpht.com/-mUJv5DN5sOQ/TzScRSjHfJI/AAAAAAAAAd8/XErEJslVnKI/stackclr_thumb%25255B1%25255D.png?imgmax=800",
                                                                                        UriKind.Absolute),
                                                                                    new Uri(
                                                                                        "http://lh3.ggpht.com/-yCXImOFtSrc/TzScR2uTZ-I/AAAAAAAAAeM/43a7SGj5Uwo/stacklanguage_thumb%25255B1%25255D.png?imgmax=800",
                                                                                        UriKind.Absolute)
                                                                                }),
                               Group = jeremyGroup
                           };
            jeremyGroup.Items.Add(item);
        }

        private async Task LoadAllGroups()
        {
            var existing = await LoadCachedGroups();
            var live = await LoadLiveGroups();

            foreach (BlogGroup liveGroup in live
                .Where(liveGroup => !existing.Contains(liveGroup, new BaseItemComparer())))
            {
                existing.Add(liveGroup);
                await StorageUtility.SaveItem(GROUP_FOLDER, liveGroup);
            }

            foreach (var group in existing.OrderBy(e => e.Title))
            {
                GroupList.Add(group);
            }
        }

        private async Task<IList<BlogGroup>> LoadCachedGroups()
        {
            var retVal = new List<BlogGroup>();
            foreach (var item in await StorageUtility.ListItems(GROUP_FOLDER))
            {
                try
                {
                    var group = await StorageUtility.RestoreItem<BlogGroup>(GROUP_FOLDER, item);
                    retVal.Add(group);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            return retVal;
        }

        private async Task<IList<BlogGroup>> LoadLiveGroups()
        {
            var retVal = new List<BlogGroup>();
            var info = NetworkInformation.GetInternetConnectionProfile();

            if (info == null || info.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.InternetAccess)
            {
                return retVal;
            }

            var content = await PathIO.ReadTextAsync("ms-appx:///Assets/Blogs.js");
            content = content.Trim(Utf8ByteOrderMark.ToCharArray());
            var blogs = JsonArray.Parse(content);

            foreach (JsonValue item in blogs)
            {
                MessageDialog dialog = null;
                try
                {
                    var uri = item.GetObject()["BlogUri"].GetString();
                    var group = new BlogGroup
                                    {
                                        Id = uri,
                                        RssUri = new Uri(uri, UriKind.Absolute)
                                    };

                    var client = GetSyndicationClient();
                    var feed = await client.RetrieveFeedAsync(group.RssUri);

                    group.Title = feed.Title.Text;

                    retVal.Add(group);
                }
                catch (Exception ex)
                {
                    dialog = new MessageDialog(ex.Message);
                }

                if (dialog != null)
                {
                    await dialog.ShowAsync();
                }
            }

            return retVal;
        }

        public async Task LoadAllItems(BlogGroup group)
        {
            var cachedItems = await LoadCachedItems(group);
            group.ItemCount = cachedItems.Count;
            group.NewItemCount = 0;
            var newItems = await LoadLiveItems(group);
            foreach (var item in newItems.Where(i => !cachedItems.Contains(i, new BaseItemComparer())))
            {
                MessageDialog dialog = null;
                try
                {
                    var client = GetClient();
                    var page = await client.GetStringAsync(item.PageUri);                    
                    item.ImageUriList = new ObservableCollection<Uri>(BlogUtility.ExtractImagesFromPage(page));
                    foreach(var image in item.ImageUriList)
                    {
                        ImageUriManager.AddImage(item.Id, image);
                    }
                }
                catch (Exception ex)
                {
                    dialog = new MessageDialog(ex.Message);
                }

                if (dialog != null)
                {
                    await dialog.ShowAsync();
                }
                cachedItems.Add(item);
                group.NewItemCount++;
                await StorageUtility.SaveItem(group.Id.GetHashCode().ToString(), item);
            }

            foreach (var item in cachedItems.OrderByDescending(i => i.PostDate))
            {
                group.Items.Add(item);
            }
        }

        private static async Task<IList<BlogItem>> LoadCachedItems(BlogGroup group)
        {
            var retVal = new List<BlogItem>();

            var groupFolder = group.Id.GetHashCode().ToString();

            foreach (var item in await StorageUtility.ListItems(groupFolder))
            {
                try
                {
                    var post = await StorageUtility.RestoreItem<BlogItem>(groupFolder, item);
                    post.Group = group;
                    retVal.Add(post);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }

            return retVal;
        }

        private static async Task<List<BlogItem>> LoadLiveItems(BlogGroup group)
        {
            var info = NetworkInformation.GetInternetConnectionProfile();

            if (info == null || info.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.InternetAccess)
            {
                return new List<BlogItem>();
            }

            MessageDialog dialog = null;
            var retVal = new List<BlogItem>();

            try
            {
                var client = GetSyndicationClient();
                var feed = await client.RetrieveFeedAsync(group.RssUri);
                foreach (var item in feed.Items.OrderBy(i => i.PublishedDate))
                {
                    MessageDialog innerDialog = null;
                    try
                    {
                        var blogItem = new BlogItem {Group = @group};
                        var uri = string.Empty;

                        if (item.Links.Count > 0)
                        {
                            var link =
                                item.Links.FirstOrDefault(
                                    l => l.Relationship.Equals("alternate", StringComparison.OrdinalIgnoreCase)) ??
                                item.Links[0];
                            uri = link.Uri.AbsoluteUri;
                        }

                        blogItem.Id = uri;

                        blogItem.PageUri = new Uri(uri, UriKind.Absolute);
                        blogItem.Title = item.Title != null ? item.Title.Text : "(no title)";

                        blogItem.PostDate = item.PublishedDate.LocalDateTime;

                        var content = "(no content)";

                        if (item.Content != null)
                        {
                            content = BlogUtility.ParseHtml(item.Content.Text);
                        }
                        else if (item.Summary != null)
                        {
                            content = BlogUtility.ParseHtml(item.Summary.Text);
                        }

                        blogItem.Description = content;

                        retVal.Add(blogItem);
                    }
                    catch (Exception ex)
                    {
                        innerDialog = new MessageDialog(ex.Message);
                    }

                    if (innerDialog != null)
                    {
                        await innerDialog.ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                dialog = new MessageDialog(ex.Message);
            }

            if (dialog != null)
            {
                await dialog.ShowAsync();
            }
            return retVal;
        }

        private static SyndicationClient GetSyndicationClient()
        {
            var client = new SyndicationClient {BypassCacheOnRetrieve = false};
            client.SetRequestHeader("user-agent", USER_AGENT);
            return client;
        }

        private static HttpClient GetClient()
        {
            var retVal = new HttpClient
                             {
                                 MaxResponseContentBufferSize = 999999
                             };
            retVal.DefaultRequestHeaders.Add("user-agent", USER_AGENT);
            return retVal;
        }

        private void LinqExamples()
        {
            // obtain the data source
            IEnumerable<string> query = from g in GroupList select g.Title;
            IEnumerable<string> query2 = GroupList.Select(g => g.Title);

            // filter
            IEnumerable<BlogGroup> filter = from g in GroupList
                                            where g.Title.StartsWith("A")
                                            select g;
            IEnumerable<BlogGroup> filter2 = GroupList.Where(g => g.Title.StartsWith("A"));

            // order
            IOrderedEnumerable<BlogGroup> order = from g in GroupList
                                                  orderby g.Title
                                                  select g;
            IOrderedEnumerable<BlogGroup> order2 = GroupList.OrderBy(g => g.Title);

            // group (create group per letter of alphabet)
            IEnumerable<IGrouping<string, BlogGroup>> group = from g in GroupList
                                                              group g by g.Title.Substring(0, 1);
            IEnumerable<IGrouping<string, BlogGroup>> group2 = GroupList.GroupBy(g => g.Title.Substring(0, 1));

            // join and project
            var items = from i in GroupList[0].Items
                        join i2 in GroupList[1].Items
                            on i.PostDate equals i2.PostDate
                        select new
                                   {
                                       SourceTitle = i.Title,
                                       TargetTitle = i2.Title
                                   };

            var items2 = GroupList[0].Items.Join(
                GroupList[1].Items,
                g1 => g1.PostDate,
                g2 => g2.PostDate,
                (g1, g2) => new
                                {
                                    SourceTitle = g1.Title,
                                    TargetTitle = g2.Title
                                });
        }

        private void ConversionExamples()
        {
            const long bigNumber = 4523452345234523455L;
            byte[] bytes = BitConverter.GetBytes(bigNumber);
            long copyOfBigNumber = BitConverter.ToInt64(bytes, 0);
            Debug.Assert(bigNumber == copyOfBigNumber);
        }
    }
}