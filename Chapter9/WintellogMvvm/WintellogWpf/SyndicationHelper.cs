using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using PortableWintellog.Contracts;
using PortableWintellog.Data;
using PortableWintellog.Utility;

namespace WintellogWpf
{
    public class SyndicationHelper : ISyndicationHelper
    {
        private static readonly Uri[] GroupsList = new[]
            {
                new Uri("http://feeds2.feedburner.com/CSharperImage", UriKind.Absolute),
                new Uri("http://www.wintellect.com/cs/blogs/jprosise/rss.aspx", UriKind.Absolute),
                new Uri("http://www.wintellect.com/cs/blogs/jeffreyr/rss.aspx", UriKind.Absolute),
                new Uri("http://www.wintellect.com/cs/blogs/jrobbins/rss.aspx", UriKind.Absolute),
                new Uri("http://www.wintellect.com/cs/blogs/jgarland/rss.aspx", UriKind.Absolute)
            };

        public Task<IList<BlogGroup>> LoadBlogsAsync()
        {
            return Task.Run(async () =>
                {
                    IList<BlogGroup> blogs = new List<BlogGroup>();
// ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var blog in GroupsList)
// ReSharper restore LoopCanBeConvertedToQuery
                    {
                        var feed = await GetFeed(blog);
                        var blogEntry = new BlogGroup
                            {
                                Id = blog.ToString(),
                                RssUri = blog,
                                Title = feed.Title.Text
                            };
                        blogs.Add(blogEntry);
                    }
                    return blogs;
                });
        }

        public Task<IList<BlogItem>> LoadItemsAsync(BlogGroup group)
        {
            return Task.Run(async () =>
                {
                    var feed = await GetFeed(group.RssUri);

                    IList<BlogItem> items = new List<BlogItem>();

                    foreach (var item in feed.Items)
                    {
                        try
                        {
                            var blogItem = new BlogItem {Group = group};
                            var uri = string.Empty;

                            if (item.Links.Count > 0)
                            {
                                var link =
                                    item.Links.FirstOrDefault(
                                        l => l.RelationshipType.Equals("alternate", StringComparison.OrdinalIgnoreCase)) ??
                                    item.Links[0];
                                uri = link.Uri.AbsoluteUri;
                            }

                            blogItem.Id = uri;

                            blogItem.PageUri = new Uri(uri, UriKind.Absolute);
                            blogItem.Title = item.Title != null ? item.Title.Text : "(no title)";

                            blogItem.PostDate = item.PublishDate.LocalDateTime;

                            var content = "(no content)";

                            if (item.Content != null)
                            {
                                content = BlogUtility.ParseHtml(((TextSyndicationContent) item.Content).Text);
                            }
                            else if (item.Summary != null)
                            {
                                content = BlogUtility.ParseHtml(item.Summary.Text);
                            }

                            blogItem.Description = content;

                            items.Add(blogItem);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }

                    return items;
                });
        }

        private static async Task<SyndicationFeed> GetFeed(Uri uri)
        {
            var client = new WebClient();
            var result = await client.DownloadStringTaskAsync(uri);
            using (var reader = new StringReader(result))
            {
                using (var xmlReader = XmlReader.Create(reader))
                {
                    return SyndicationFeed.Load(xmlReader);
                }
            }
        }
    }
}