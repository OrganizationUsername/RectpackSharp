﻿using System;

namespace RectpackSharp
{
    /// <summary>
    /// Specifies hints that help optimize the rectangle packing algorithm. 
    /// </summary>
    [Flags]
    public enum PackingHint
    {
        /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by area.</summary>
        TryByArea = 1,

        /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by perimeter.</summary>
        TryByPerimeter = 2,

        /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by bigger side.</summary>
        TryByBiggerSide = 4,

        /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by width.</summary>
        TryByWidth = 8,

        /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by height.</summary>
        TryByHeight = 16,

        /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by a pathological multiplier.</summary>
        TryByPathologicalMultiplier = 32,

        /// <summary>Specifies to try all the possible hints, as to find the best packing configuration.</summary>
        FindBest = TryByArea | TryByPerimeter | TryByBiggerSide | TryByWidth | TryByHeight | TryByPathologicalMultiplier,

        /// <summary>Specifies hints to optimize for rectangles who have one side much bigger than the other.</summary>
        UnusualSizes = TryByPerimeter | TryByBiggerSide | TryByPathologicalMultiplier,

        /// <summary>Specifies hints to optimize for rectangles whose sides are relatively similar.</summary>
        MostlySquared = TryByArea | TryByBiggerSide | TryByWidth | TryByHeight,
    }

    /// <summary>
    /// Provides internal values and functions used by the rectangle packing algorithm.
    /// </summary>
    internal static class PackingHintExtensions
    {
        /// <summary>
        /// Represents a method for calculating a sort key from a <see cref="PackingRectangle"/>.
        /// </summary>
        /// <param name="rectangle">The <see cref="PackingRectangle"/> whose sort key to calculate.</param>
        /// <returns>The value that should be assigned to <see cref="PackingRectangle.SortKey"/>.</returns>
        private delegate uint GetSortKeyDelegate(in PackingRectangle rectangle);

        /// <summary>The maximum amount of hints that can be specified by a <see cref="PackingHint"/>.</summary>
        internal const int MaxHintCount = 6;

        public static uint GetArea(in PackingRectangle rectangle) => rectangle.Area;
        public static uint GetPerimeter(in PackingRectangle rectangle) => rectangle.Perimeter;
        public static uint GetBiggerSide(in PackingRectangle rectangle) => rectangle.BiggerSide;
        public static uint GetWidth(in PackingRectangle rectangle) => rectangle.Width;
        public static uint GetHeight(in PackingRectangle rectangle) => rectangle.Height;
        public static uint GetPathologicalMultiplier(in PackingRectangle rectangle) => rectangle.PathologicalMultiplier;

        /// <summary>
        /// Separates a <see cref="PackingHint"/> into the multiple options it contains,
        /// saving each of those separately onto a <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="packingHint">The <see cref="PackingHint"/> to separate.</param>
        /// <param name="span">The span in which to write the resulting hints. This span's excess will be sliced.</param>
        public static void GetFlagsFrom(PackingHint packingHint, ref Span<PackingHint> span)
        {
            int index = 0;
            if (packingHint.HasFlag(PackingHint.TryByArea))
                span[index++] = PackingHint.TryByArea;
            if (packingHint.HasFlag(PackingHint.TryByPerimeter))
                span[index++] = PackingHint.TryByPerimeter;
            if (packingHint.HasFlag(PackingHint.TryByBiggerSide))
                span[index++] = PackingHint.TryByBiggerSide;
            if (packingHint.HasFlag(PackingHint.TryByWidth))
                span[index++] = PackingHint.TryByWidth;
            if (packingHint.HasFlag(PackingHint.TryByHeight))
                span[index++] = PackingHint.TryByHeight;
            if (packingHint.HasFlag(PackingHint.TryByPathologicalMultiplier))
                span[index++] = PackingHint.TryByPathologicalMultiplier;
            span = span.Slice(0, index);
        }

        /// <summary>
        /// Sorts the given <see cref="PackingRectangle"/> array using the specified <see cref="PackingHint"/>.
        /// </summary>
        /// <param name="rectangles">The rectangles to sort.</param>
        /// <param name="packingHint">The hint to sort by. Must be a single bit value.</param>
        /// <remarks>
        /// The <see cref="PackingRectangle.SortKey"/> values will be modified.
        /// </remarks>
        public static void SortByPackingHint(PackingRectangle[] rectangles, PackingHint packingHint)
        {
            // We first get the appropiate delegate for getting a rectangle's sort key.
            GetSortKeyDelegate getKeyDelegate = packingHint switch
            {
                PackingHint.TryByArea => GetArea,
                PackingHint.TryByPerimeter => GetPerimeter,
                PackingHint.TryByBiggerSide => GetBiggerSide,
                PackingHint.TryByWidth => GetWidth,
                PackingHint.TryByHeight => GetHeight,
                PackingHint.TryByPathologicalMultiplier => GetPathologicalMultiplier,
                _ => throw new ArgumentException(nameof(packingHint))
            };

            // We use the getKeyDelegate to set the sort keys for all the rectangles.
            for (int i = 0; i < rectangles.Length; i++)
                rectangles[i].SortKey = getKeyDelegate(rectangles[i]);

            // We sort the array, using the default rectangle comparison (which compares sort keys).
            Array.Sort(rectangles);
        }
    }
}
