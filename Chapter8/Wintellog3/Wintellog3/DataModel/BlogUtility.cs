using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Wintellog3.DataModel
{
    public static class BlogUtility
    {
        public const string BLOGGER_PAGE = "http://www.wintellect.com/CS/blogs/Bloggers.aspx";
        public const string RSS_FEED = @".*\<link.*type=\""application\/rss\+xml\"".*href=\""(.*)\"".*";
        public const string IMAGE_TAG = @"<(img)\b[^>]*>";
        public const string IMAGE_SOURCE = @"src=[\""\']([^\""\']+)";

        private static readonly Regex Tags = new Regex(IMAGE_TAG, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static readonly Regex SourceAttribute = new Regex(IMAGE_SOURCE, RegexOptions.IgnoreCase);
        
        public static IEnumerable<Uri> ExtractImagesFromPage(string content)
        {
            var images = new List<Uri>();
            var matches = Tags.Matches(content);
            var max = matches.Count;

            for (var i = 0; i < max; i++)
            {
                var imageTags = SourceAttribute.Matches(matches[i].Value);

                if (imageTags.Count <= 0) continue;

                var tag = imageTags[0].Value;

                if (string.IsNullOrEmpty(tag)) continue;
                var startPos = tag.IndexOf("htt", StringComparison.Ordinal);

                if (startPos <= 0) continue;

                Uri image;

                if (Uri.TryCreate(tag.Substring(startPos), UriKind.Absolute, out image))
                {
                    images.Add(image);
                }
            }

            return images.Distinct();
        }

        public static string ParseHtml(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            var newLine = string.Format("{0}{0}", Environment.NewLine);

            // replace paragraphs and breaks with new lines 
            var formattedValue = Regex.Replace(
                Regex.Replace(
                    Regex.Replace(content, "<br/>", Environment.NewLine, RegexOptions.IgnoreCase),
                    "<br>", Environment.NewLine, RegexOptions.IgnoreCase),
                "<p>", Environment.NewLine, RegexOptions.IgnoreCase);

            // Remove HTML tags and empty newlines and spaces and leading spaces
            formattedValue = Regex.Replace(Regex.Replace(formattedValue, "<.*?>", string.Empty), @"\n+\s+", newLine);

            // decode HTML
            formattedValue = WebUtility.HtmlDecode(formattedValue).TrimStart(' ');

            return formattedValue;
        }
    }
}