using System;
using System.Collections.Generic;
using System.Linq;

namespace Wintellog2.DataModel
{
    /// <summary>
    ///     This class handles unique images so that duplicates are stripped out.
    ///     This ensures each blog post only receives a collection of unique images to that post.
    /// </summary>
    public static class ImageUriManager
    {
        /// <summary>
        /// Dictionary that maps item Uris to an image
        /// </summary>
        private static readonly List<Tuple<string, Uri>> Images = new List<Tuple<string, Uri>>();

        /// <summary>
        /// Checks to see if the image exists universally 
        /// </summary>
        /// <param name="image">The image</param>
        /// <returns>True if it already exists</returns>
        private static bool ImageExists(Uri image)
        {
            return Images.Any(i => i.Item2.Equals(image));
        }

        /// <summary>
        /// Checks to see if the image exists in context of the item. If the
        /// image exists for that item, will return false so the image will
        /// still display.
        /// </summary>
        /// <param name="itemId">Id of the item</param>
        /// <param name="image">Image Uri</param>
        /// <returns>True if the image exists outside of that item</returns>
        private static bool ImageExists(string itemId, Uri image)
        {
            var match = Images.FirstOrDefault(i => i.Item2.Equals(image));
            return match != null && !match.Item1.Equals(itemId);
        }

        /// <summary>
        /// Adds an image to the collection
        /// </summary>
        /// <param name="itemId">The id of the item the image belongs to</param>
        /// <param name="image">The Uri of the image</param>
        public static void AddImage(string itemId, Uri image)
        {
            if (!ImageExists(image))
            {
                Images.Add(Tuple.Create(itemId,image));
            }
        }

        /// <summary>
        /// Provides the filtered set of images for an item, stripping out images
        /// that are duplicates to other items
        /// </summary>
        /// <param name="itemId">The id of the item to inspect</param>
        /// <param name="inputImageSet">The full list of images for the item</param>
        /// <returns>The list of unique images for that item</returns>
        public static IEnumerable<Uri> FilteredImageSet(string itemId, IEnumerable<Uri> inputImageSet)
        {
            return inputImageSet.Where(i => !ImageExists(itemId, i));
        }
    }
}
