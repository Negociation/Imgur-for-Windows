using Imgur.Api.Services.Models.Enum;
using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imgur.Constants
{
    public static class GalleryConstants
    {

        public static IReadOnlyList<Section> GetSections()
        {
            return new List<Section>
            {
                CreateViralSection(),
                CreateUserSubmittedSection(),
                CreateTopSection()
            }.AsReadOnly();
        }

        private static Section CreateViralSection()
        {
            var sorts = new List<Sort>
            {
                new Sort(GallerySort.Viral),  // Popular
                new Sort(GallerySort.Time),   // Newest
                new Sort(GallerySort.Top)     // Best
            };
            return new Section(GallerySection.Hot, sorts);
        }

        private static Section CreateUserSubmittedSection()
        {
            var sorts = new List<Sort>
            {
                new Sort(GallerySort.Viral),   // Popular
                new Sort(GallerySort.Rising),  // Rising
                new Sort(GallerySort.Time)     // Newest
            };
            return new Section(GallerySection.User, sorts);
        }

        private static Section CreateTopSection()
        {
            var sorts = new List<Sort>
            {
                new Sort(GalleryWindow.Day),
                new Sort(GalleryWindow.Week),
                new Sort(GalleryWindow.Month),
                new Sort(GalleryWindow.Year),
                new Sort(GalleryWindow.All)
            };
            return new Section(GallerySection.Top, sorts);
        }

        public static IReadOnlyList<Sort> GetDefaultSorts()
        {
            return new List<Sort>
            {
                new Sort(GallerySort.Viral),
                new Sort(GallerySort.Top),
                new Sort(GallerySort.Time)
            }.AsReadOnly();
        }
    }
}
