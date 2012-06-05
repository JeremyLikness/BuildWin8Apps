using System;

namespace Panels.Data
{
    public class SimpleItem
    {
        private static readonly Random Random = new Random();

        public string Description { get; set; }
        public ItemType Type { get; set; }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        public static SimpleItem GenerateItem()
        {
            var enums = new[] { ItemType.Circle, ItemType.Ellipse,
                ItemType.Rectangle, ItemType.Square };
            var random = Random.Next(4);
            var red = Random.Next(256);
            var green = Random.Next(256);
            var blue = Random.Next(256);
            return new SimpleItem
            {
                Description = enums[random].ToString(),
                Type = enums[random],
                Red = red,
                Green = green,
                Blue = blue
            };
        }

    }

}
