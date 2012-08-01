using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PortableWintellog.Utility;
using PortableWintellog.Contracts;

namespace PortableWintellog.Data
{
    public class BlogDataSource
    {
        private readonly IStorageUtility _storage;
        private readonly IApplicationContext _context;
        private readonly IDialog _dialog;
        private readonly ISyndicationHelper _syndicationHelper;
        
        private const string GROUP_FOLDER = "Groups";

        private const string USER_AGENT =
            "Wintellect/Wintellog (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";

        private const string JEREMY_BLOG = "http://www.wintellect.com/CS/blogs/jlikness/default.aspx";

        private const string SAMPLE_BLOG =
            "http://wintellect.com/CS/blogs/jlikness/archive/2012/02/09/windows-8-the-facts-about-arm-metro-and-the-blue-stack.aspx";

        /// <summary>
        /// Design-time constructor
        /// </summary>
        public BlogDataSource()
        {
            GroupList = new ObservableCollection<BlogGroup>();            
            LoadTestGroups();
        }

        /// <summary>
        /// Run-time constructor
        /// </summary>
        /// <param name="storageUtility">Utility for storing information</param>
        /// <param name="context">Context for the application</param>
        /// <param name="dialog">Dialog to show errors</param>
        /// <param name="syndicationHelper">Syndication to help parse blog entries</param>
        public BlogDataSource(
            IStorageUtility storageUtility, 
            IApplicationContext context, 
            IDialog dialog,
            ISyndicationHelper syndicationHelper)
        {
            _context = context;
            _dialog = dialog;
            _syndicationHelper = syndicationHelper;
            _storage = storageUtility;

            GroupList = new ObservableCollection<BlogGroup>();                                    
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
            if (_context.IsTestMode)
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

            foreach (var liveGroup in live
                .Where(liveGroup => !existing.Contains(liveGroup, new BaseItemComparer())))
            {
                existing.Add(liveGroup);
                await _storage.SaveItem(GROUP_FOLDER, liveGroup);
            }

            foreach (var group in existing.OrderBy(e => e.Title))
            {
                GroupList.Add(group);
            }
        }

        private async Task<IList<BlogGroup>> LoadCachedGroups()
        {
            var retVal = new List<BlogGroup>();
            foreach (var item in await _storage.ListItems(GROUP_FOLDER))
            {
                try
                {
                    var group = await _storage.RestoreItem<BlogGroup>(GROUP_FOLDER, item);
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
            return await _syndicationHelper.LoadBlogsAsync();            
        }        

        public async Task LoadAllItems(BlogGroup group)
        {
            var cachedItems = await LoadCachedItems(group);
            group.ItemCount = cachedItems.Count;
            group.NewItemCount = 0;
            var newItems = await LoadLiveItems(group);
            foreach (var item in newItems.Where(i => !cachedItems.Contains(i, new BaseItemComparer())))
            {
                var error = string.Empty;
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
                    error = ex.Message;
                }

                if (!string.IsNullOrEmpty(error))
                {
                    await _dialog.ShowDialogAsync(error);
                }
                cachedItems.Add(item);
                group.NewItemCount++;
                await _storage.SaveItem(group.Id.GetHashCode().ToString(), item);
            }

            foreach (var item in cachedItems.OrderByDescending(i => i.PostDate))
            {
                foreach (var image in item.ImageUriList)
                {
                    ImageUriManager.AddImage(item.Id, image);
                }
                group.Items.Add(item);
            }

            group.ItemCount = group.Items.Count();
        }

        private async Task<IList<BlogItem>> LoadCachedItems(BlogGroup group)
        {
            var retVal = new List<BlogItem>();

            var groupFolder = group.Id.GetHashCode().ToString();

            foreach (var item in await _storage.ListItems(groupFolder))
            {
                try
                {
                    var post = await _storage.RestoreItem<BlogItem>(groupFolder, item);
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

        private async Task<IList<BlogItem>> LoadLiveItems(BlogGroup group)
        {
            return await _syndicationHelper.LoadItemsAsync(group);
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
    }
}