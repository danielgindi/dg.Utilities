using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace dg.Utilities
{
    public static class Youtube
    {
        [Obsolete]
        public static string GetYoutubeId(string url)
        {
            return StringHelper.GetYoutubeId(url);
        }

        [Obsolete]
        public static string GetYoutubeMaxResDefaultImageUrl(string youtubeId, bool httpRooted = true)
        {
            return StringHelper.GetYoutubeMaxResDefaultImageUrl(youtubeId, httpRooted);
        }

        [Obsolete]
        public static string GetYoutubeThumbnail0ImageUrl(string youtubeId, bool httpRooted = true)
        {
            return StringHelper.GetYoutubeThumbnail0ImageUrl(youtubeId, httpRooted);
        }

        [Obsolete]
        public static string GetYoutubeThumbnail1ImageUrl(string youtubeId, bool httpRooted = true)
        {
            return StringHelper.GetYoutubeThumbnail1ImageUrl(youtubeId, httpRooted);
        }

        [Obsolete]
        public static string GetYoutubeThumbnail2ImageUrl(string youtubeId, bool httpRooted = true)
        {
            return StringHelper.GetYoutubeThumbnail2ImageUrl(youtubeId, httpRooted);
        }

        [Obsolete]
        public static string GetYoutubeThumbnail3ImageUrl(string youtubeId, bool httpRooted = true)
        {
            return StringHelper.GetYoutubeThumbnail3ImageUrl(youtubeId, httpRooted);
        }

        [Obsolete]
        public static string GetYoutubeHqDefaultImageUrl(string youtubeId, bool httpRooted = true)
        {
            return StringHelper.GetYoutubeHqDefaultImageUrl(youtubeId, httpRooted);
        }

        [Obsolete]
        public static string GetYoutubeMqDefaultImageUrl(string youtubeId, bool httpRooted = true)
        {
            return StringHelper.GetYoutubeMqDefaultImageUrl(youtubeId, httpRooted);
        }

        [Obsolete]
        public static string GetYoutubeSdDefaultImageUrl(string youtubeId, bool httpRooted = true)
        {
            return StringHelper.GetYoutubeSdDefaultImageUrl(youtubeId, httpRooted);
        }
    }
}
