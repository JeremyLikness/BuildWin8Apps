using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortableWintellog.Contracts;
using PortableWintellog.Data;
using PortableWintellog.Utility;
using Windows.Data.Json;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Web.Syndication;

namespace WintellogMvvm.DataModel
{
    public class SyndicationHelper : ISyndicationHelper
    {
        private static readonly string Utf8ByteOrderMark =
           Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble(), 0, Encoding.UTF8.GetPreamble().Length);

        private const string USER_AGENT =
            "Wintellect/Wintellog (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";

        private readonly IDialog _dialog;

        public SyndicationHelper(IDialog dialog)
        {
            _dialog = dialog;
        }

        public async Task<IList<BlogGroup>> LoadBlogsAsync()
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
                var error = string.Empty;
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
                    error = ex.Message;
                }

                if (!string.IsNullOrEmpty(error))
                {
                    await _dialog.ShowDialogAsync(error);
                }
            }

            return retVal;
        }

        public async Task<IList<BlogItem>> LoadItemsAsync(BlogGroup group)
        {
            var info = NetworkInformation.GetInternetConnectionProfile();

            if (info == null || info.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.InternetAccess)
            {
                return new List<BlogItem>();
            }

            var retVal = new List<BlogItem>();
            var outerError = string.Empty;

            try
            {
                var client = GetSyndicationClient();
                var feed = await client.RetrieveFeedAsync(group.RssUri);
                foreach (var item in feed.Items.OrderBy(i => i.PublishedDate))
                {
                    var error = string.Empty;
                    try
                    {
                        var blogItem = new BlogItem { Group = group };
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
                        error = ex.Message;
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        await _dialog.ShowDialogAsync(error);
                    }
                }
            }
            catch (Exception ex)
            {
                outerError = ex.Message;
            }

            if (!string.IsNullOrEmpty(outerError))
            {
                await _dialog.ShowDialogAsync(outerError);
            }
            return retVal;
        }

        private static SyndicationClient GetSyndicationClient()
        {
            var client = new SyndicationClient
            {
                BypassCacheOnRetrieve = false
            };
            client.SetRequestHeader("user-agent", USER_AGENT);
            return client;
        }
    }
}
